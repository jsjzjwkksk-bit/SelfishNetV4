using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;
using Serilog;

namespace SelfishNetv3
{
    /// <summary>
    /// Main application form — hosts the device grid and controls ARP spoofing.
    /// </summary>
    public partial class ArpForm : Form
    {
        // ── Column indices (named constants replace magic numbers) ──
        private const int COL_ROLE = 0;
        private const int COL_NAME = 1;
        private const int COL_IP = 2;
        private const int COL_MAC = 3;
        private const int COL_DOWNLOAD = 4;
        private const int COL_UPLOAD = 5;
        private const int COL_DOWN_CAP = 6;
        private const int COL_UP_CAP = 7;
        private const int COL_BLOCK = 8;

        // ── State ──
        internal DeviceCollection? devices;
        internal ArpSpoofService? arpService;
        private AdapterSelectionForm? adapterForm;
        private NetworkInterface? nicNet;
        private bool isSpoofing;

        private static readonly int WM_QUERYENDSESSION = 0x11;
        private static bool systemShutdown;

        public static ArpForm? Instance { get; private set; }

        public ArpForm()
        {
            InitializeComponent();
            Instance = this;
        }

        // ───── Form Events ─────

        private void ArpForm_Load(object sender, EventArgs e)
        {
            Text = $"SelfishNet v{Application.ProductVersion}";

            // Check that Npcap is installed
            try
            {
                var deviceCount = SharpPcap.CaptureDeviceList.Instance.Count;
                if (deviceCount == 0)
                {
                    MessageBox.Show(
                        "No capture devices found. Please install Npcap from https://npcap.com\n\n" +
                        "Ensure you check 'Install Npcap in WinPcap API-compatible Mode' during installation.",
                        "Npcap Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Environment.Exit(1);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to initialize packet capture library:\n{ex.Message}\n\n" +
                    "Please install Npcap from https://npcap.com",
                    "Npcap Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1);
                return;
            }

            // Show adapter selection dialog
            ShowAdapterSelection();
        }

        private void ArpForm_Shown(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && args[1] == "minimize")
                WindowState = FormWindowState.Minimized;

            Opacity = 1.0;
        }

        private void ArpForm_Resize(object sender, EventArgs e)
        {
            // Normal minimize — stays in the taskbar like any other app.
            // No Hide() call, no system tray magic.
        }

        /// <summary>
        /// Restores the form from system tray (used by tray icon).
        /// </summary>
        private void RestoreFromTray()
        {
            Show();
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private void ArpForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!systemShutdown)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to close SelfishNet?",
                    "Confirm Exit",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question);

                if (result != DialogResult.OK)
                {
                    e.Cancel = true;
                    return;
                }
            }

            PerformCleanShutdown();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_QUERYENDSESSION)
                systemShutdown = true;

            base.WndProc(ref m);
        }

        // ───── Adapter Selection ─────

        private void ShowAdapterSelection()
        {
            adapterForm = new AdapterSelectionForm();
            adapterForm.ShowDialog(this);
        }

        /// <summary>
        /// Called by <see cref="AdapterSelectionForm"/> when the user selects a NIC.
        /// </summary>
        public void NicIsSelected(NetworkInterface nic)
        {
            nicNet = nic;
            devices = new DeviceCollection();
            devices.DeviceAdded += OnDeviceAdded;
            devices.DeviceRemoved += OnDeviceRemoved;

            arpService = new ArpSpoofService(nic, devices);
            arpService.StartArpListener();

            // Add local PC as a device
            var localPc = new NetworkDevice
            {
                Ip = new IPAddress(arpService.LocalIP),
                Mac = new PhysicalAddress(arpService.LocalMAC),
                IsLocalPc = true,
                Name = "Your PC",
                Redirect = false,
                IsGateway = false
            };
            devices.AddDevice(localPc);

            // Give the ARP listener thread time to start capturing,
            // then discover the router and sweep the full subnet.
            // Runs on a background thread so the UI stays responsive.
            var svc = arpService;
            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                // Wait for the ARP listener loop to begin receiving packets
                await System.Threading.Tasks.Task.Delay(200);
                svc.FindMacRouter();

                // Brief pause so the router reply arrives before we flood the wire
                await System.Threading.Tasks.Task.Delay(300);
                svc.StartDiscovery();
            });

            // Start cleanup timer
            timerCleanup.Interval = 5000;
            timerCleanup.Start();
        }

        // ───── Device Added/Removed Callbacks ─────

        private void OnDeviceAdded(NetworkDevice device)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<NetworkDevice>(OnDeviceAdded), device);
                return;
            }

            AddDeviceToGrid(device);

            // Resolve hostname asynchronously
            _ = ResolveHostnameAsync(device);
        }

        private void OnDeviceRemoved(NetworkDevice device)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<NetworkDevice>(OnDeviceRemoved), device);
                return;
            }

            RemoveDeviceFromGrid(device);
        }

        // ───── Grid Management ─────

        private void AddDeviceToGrid(NetworkDevice device)
        {
            string role = device.IsGateway ? "Gateway" : device.IsLocalPc ? "Local" : "Device";
            bool isEditable = !device.IsGateway && !device.IsLocalPc;

            int rowIndex = deviceGridView.Rows.Add(
                role,
                device.Name,
                device.Ip.ToString(),
                device.Mac.ToString(),
                "",  // Download
                "",  // Upload
                isEditable ? "0" : "",   // Down Cap
                isEditable ? "0" : "",   // Up Cap
                isEditable && device.Redirect  // Block checkbox
            );

            var row = deviceGridView.Rows[rowIndex];

            // Make non-editable cells read-only
            row.Cells[COL_DOWN_CAP].ReadOnly = !isEditable;
            row.Cells[COL_UP_CAP].ReadOnly = !isEditable;
            row.Cells[COL_BLOCK].ReadOnly = !isEditable;
        }

        private void RemoveDeviceFromGrid(NetworkDevice device)
        {
            string ip = device.Ip.ToString();
            for (int i = deviceGridView.Rows.Count - 1; i >= 0; i--)
            {
                if (deviceGridView.Rows[i].Cells[COL_IP].Value?.ToString() == ip)
                {
                    deviceGridView.Rows.RemoveAt(i);
                    break;
                }
            }
        }

        private DataGridViewRow? FindRowByIp(string ip)
        {
            return deviceGridView.Rows
                .Cast<DataGridViewRow>()
                .FirstOrDefault(r => r.Cells[COL_IP].Value?.ToString() == ip);
        }

        // ───── Hostname Resolution ─────

        private async System.Threading.Tasks.Task ResolveHostnameAsync(NetworkDevice device)
        {
            try
            {
                // BUG #8 FIX: Add 3-second timeout to prevent UI hangs
                // on non-responsive DNS servers.
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                var entry = await System.Net.Dns.GetHostEntryAsync(device.Ip.ToString(), cts.Token);
                string hostname = entry.HostName ?? "unknown";

                if (InvokeRequired)
                    Invoke(new Action(() => UpdateDeviceName(device, hostname)));
                else
                    UpdateDeviceName(device, hostname);
            }
            catch (OperationCanceledException)
            {
                // DNS lookup timed out after 3 seconds — leave name as-is
            }
            catch
            {
                // DNS resolution failed; leave name as-is
            }
        }

        private void UpdateDeviceName(NetworkDevice device, string name)
        {
            device.Name = name;
            var row = FindRowByIp(device.Ip.ToString());
            if (row is not null)
                row.Cells[COL_NAME].Value = name;
        }

        // ───── Toolbar Buttons ─────

        private void BtnDiscover_Click(object sender, EventArgs e)
        {
            arpService?.StartDiscovery();
        }

        private void BtnStartSpoof_Click(object sender, EventArgs e)
        {
            if (isSpoofing || arpService is null) return;

            Log.Information("Starting ARP spoofing and packet redirection");

            arpService.StartRedirector();
            isSpoofing = true;

            timerStats.Interval = 1000;
            timerStats.Start();

            timerSpoof.Interval = 2000;
            timerSpoof.Start();

            btnStartSpoof.Enabled = false;
        }

        private void BtnStopSpoof_Click(object sender, EventArgs e)
        {
            if (!isSpoofing || arpService is null) return;

            Log.Information("Stopping ARP spoofing and restoring ARP tables");

            arpService.StopRedirector();
            arpService.CompleteUnspoof();

            timerStats.Stop();
            timerSpoof.Stop();

            // Clear speed display
            foreach (DataGridViewRow row in deviceGridView.Rows)
            {
                row.Cells[COL_DOWNLOAD].Value = "";
                row.Cells[COL_UPLOAD].Value = "";
            }

            isSpoofing = false;
            btnStartSpoof.Enabled = true;
        }

        // ───── Timer Callbacks ─────

        /// <summary>
        /// Updates download/upload speed display in the grid.
        /// </summary>
        private void TimerStats_Tick(object sender, EventArgs e)
        {
            if (devices is null) return;

            double intervalSeconds = timerStats.Interval / 1000.0;

            foreach (DataGridViewRow row in deviceGridView.Rows)
            {
                try
                {
                    string? ipStr = row.Cells[COL_IP].Value?.ToString();
                    if (string.IsNullOrEmpty(ipStr)) continue;

                    var device = devices.GetDeviceByIp(IPAddress.Parse(ipStr).GetAddressBytes());
                    if (device is null) continue;

                    double downKBps = device.BytesReceivedSinceLastReset / 1024.0 / intervalSeconds;
                    double upKBps = device.BytesSentSinceLastReset / 1024.0 / intervalSeconds;

                    row.Cells[COL_DOWNLOAD].Value = downKBps.ToString("F1");
                    row.Cells[COL_UPLOAD].Value = upKBps.ToString("F1");
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Stats update error");
                }
            }

            devices.ResetAllByteCounts();
        }

        /// <summary>
        /// Removes stale devices that haven't responded to ARP in a while,
        /// and sends keep-alive ARP requests to active devices.
        /// </summary>
        private void TimerCleanup_Tick(object sender, EventArgs e)
        {
            if (devices is null || arpService is null) return;

            var allDevices = devices.GetAll();

            // Remove stale devices (no ARP reply for > 350 seconds)
            foreach (var device in allDevices)
            {
                if (device.IsGateway || device.IsLocalPc) continue;

                if ((DateTime.Now - device.LastArpReplyTime).TotalSeconds > 350)
                {
                    devices.RemoveDevice(device);
                }
            }

            // Send ARP keep-alive to active devices (last seen > 20 seconds ago)
            allDevices = devices.GetAll();
            foreach (var device in allDevices)
            {
                if ((DateTime.Now - device.LastArpReplyTime).TotalSeconds > 20)
                {
                    arpService.SendArpRequest(device.Ip.ToString());
                }
            }
        }

        /// <summary>
        /// Periodically re-sends ARP spoofing packets to maintain poisoned ARP caches.
        /// </summary>
        private void TimerSpoof_Tick(object sender, EventArgs e)
        {
            if (devices is null || arpService is null) return;

            // Guard against null RouterIP before using it.
            if (arpService.RouterIP is null)
                return;

            timerSpoof.Interval = 5000;

            foreach (DataGridViewRow row in deviceGridView.Rows)
            {
                try
                {
                    // Check if "Block" checkbox is checked
                    if (row.Cells[COL_BLOCK].Value is true)
                    {
                        string? ipStr = row.Cells[COL_IP].Value?.ToString();
                        if (string.IsNullOrEmpty(ipStr)) continue;

                        var device = devices.GetDeviceByIp(IPAddress.Parse(ipStr).GetAddressBytes());
                        if (device is not null && !device.IsLocalPc)
                        {
                            arpService.Spoof(device.Ip, new IPAddress(arpService.RouterIP));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Spoof timer error");
                }
            }
        }

        // ───── Grid Cell Editing ─────

        private void DeviceGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // Immediately commit checkbox changes
            if (deviceGridView.CurrentCell is DataGridViewCheckBoxCell && deviceGridView.IsCurrentCellDirty)
            {
                deviceGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DeviceGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || devices is null || arpService is null) return;

            try
            {
                var row = deviceGridView.Rows[e.RowIndex];
                string? ipStr = row.Cells[COL_IP].Value?.ToString();
                if (string.IsNullOrEmpty(ipStr)) return;

                var device = devices.GetDeviceByIp(IPAddress.Parse(ipStr).GetAddressBytes());
                if (device is null) return;

                switch (e.ColumnIndex)
                {
                    // Validate bandwidth cap input values and create Token Bucket limiters.
                    // Reject negative numbers and values that would overflow when
                    // multiplied by 1024 to convert KB → bytes.
                    case COL_DOWN_CAP:
                        if (long.TryParse(row.Cells[COL_DOWN_CAP].Value?.ToString(), out long downCap))
                        {
                            const long MAX_CAP_KB = long.MaxValue / 1024;
                            if (downCap < 0)
                            {
                                MessageBox.Show("Download cap cannot be negative.", "Invalid Input",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                row.Cells[COL_DOWN_CAP].Value = "0";
                            }
                            else if (downCap > MAX_CAP_KB)
                            {
                                MessageBox.Show($"Download cap too large (max {MAX_CAP_KB} KB).", "Invalid Input",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                row.Cells[COL_DOWN_CAP].Value = "0";
                            }
                            else
                            {
                                device.CapDown = downCap * 1024;
                                // Create/update Token Bucket limiter for download
                                device.DownloadLimiter = downCap > 0
                                    ? new TokenBucketRateLimiter(downCap * 1024)
                                    : null;
                                Log.Debug("Download cap set to {CapKB} KB/s for {Ip}", downCap, device.Ip);
                            }
                        }
                        else
                        {
                            row.Cells[COL_DOWN_CAP].Value = "0";
                        }
                        break;

                    case COL_UP_CAP:
                        if (long.TryParse(row.Cells[COL_UP_CAP].Value?.ToString(), out long upCap))
                        {
                            const long MAX_CAP_KB = long.MaxValue / 1024;
                            if (upCap < 0)
                            {
                                MessageBox.Show("Upload cap cannot be negative.", "Invalid Input",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                row.Cells[COL_UP_CAP].Value = "0";
                            }
                            else if (upCap > MAX_CAP_KB)
                            {
                                MessageBox.Show($"Upload cap too large (max {MAX_CAP_KB} KB).", "Invalid Input",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                row.Cells[COL_UP_CAP].Value = "0";
                            }
                            else
                            {
                                device.CapUp = upCap * 1024;
                                // Create/update Token Bucket limiter for upload
                                device.UploadLimiter = upCap > 0
                                    ? new TokenBucketRateLimiter(upCap * 1024)
                                    : null;
                                Log.Debug("Upload cap set to {CapKB} KB/s for {Ip}", upCap, device.Ip);
                            }
                        }
                        else
                        {
                            row.Cells[COL_UP_CAP].Value = "0";
                        }
                        break;

                    case COL_BLOCK:
                        bool blocked = row.Cells[COL_BLOCK].Value is true;
                        device.Redirect = blocked;
                        Log.Information("Device {Ip} redirect set to {Blocked}", device.Ip, blocked);

                        // Guard against null RouterIP before un-spoofing.
                        if (!blocked && arpService.RouterIP is not null)
                        {
                            for (int i = 0; i < 35; i++)
                                arpService.UnSpoof(device.Ip, new IPAddress(arpService.RouterIP));
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Cell value changed error");
            }
        }

        // ───── Emergency Stop ─────

        /// <summary>
        /// Emergency Stop: immediately stops all spoofing, restores ARP tables,
        /// and clears all bandwidth caps.
        /// </summary>
        internal void EmergencyStop()
        {
            Log.Warning("EMERGENCY STOP activated");

            // Stop spoofing
            if (isSpoofing)
            {
                BtnStopSpoof_Click(this, EventArgs.Empty);
            }

            // Clear all bandwidth caps
            if (devices is not null)
            {
                foreach (var device in devices.GetAll())
                {
                    device.CapDown = 0;
                    device.CapUp = 0;
                    device.DownloadLimiter = null;
                    device.UploadLimiter = null;
                    device.Redirect = false;
                }
            }

            // Reset grid
            foreach (DataGridViewRow row in deviceGridView.Rows)
            {
                if (!row.Cells[COL_DOWN_CAP].ReadOnly)
                    row.Cells[COL_DOWN_CAP].Value = "0";
                if (!row.Cells[COL_UP_CAP].ReadOnly)
                    row.Cells[COL_UP_CAP].Value = "0";
                if (!row.Cells[COL_BLOCK].ReadOnly)
                    row.Cells[COL_BLOCK].Value = false;
            }

            StatusMessage("Emergency Stop — all devices restored");
        }

        /// <summary>
        /// Updates the status bar message (if StatusStrip exists) or the title bar.
        /// </summary>
        internal void StatusMessage(string message)
        {
            if (statusLabel is not null)
                statusLabel.Text = message;
            else
                Text = $"SelfishNet v{Application.ProductVersion} — {message}";
        }

        // ───── Clean Shutdown ─────

        private void PerformCleanShutdown()
        {
            timerStats.Stop();
            timerCleanup.Stop();
            timerSpoof.Stop();

            if (isSpoofing)
            {
                arpService?.StopRedirector();
                arpService?.CompleteUnspoof();
            }

            arpService?.Dispose();
            devices?.Dispose();
            selfishNetTrayIcon.Dispose();

            Environment.Exit(0);
        }
    }
}