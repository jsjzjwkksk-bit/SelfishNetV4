using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace SelfishNetv3
{
    /// <summary>
    /// Represents a network device discovered on the local network.
    /// </summary>
    public sealed class NetworkDevice
    {
        /// <summary>IP address of the device.</summary>
        public IPAddress Ip { get; set; } = IPAddress.None;

        /// <summary>MAC (physical) address of the device.</summary>
        public PhysicalAddress Mac { get; set; } = PhysicalAddress.None;

        /// <summary>Resolved hostname, or empty if not yet resolved.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>True if this device is the default gateway / router.</summary>
        public bool IsGateway { get; set; }

        /// <summary>True if this device is the local PC running SelfishNet.</summary>
        public bool IsLocalPc { get; set; }

        /// <summary>Download bandwidth cap in bytes/sec. 0 = unlimited.</summary>
        public long CapDown { get; set; }

        /// <summary>Upload bandwidth cap in bytes/sec. 0 = unlimited.</summary>
        public long CapUp { get; set; }

        /// <summary>Whether packet redirection is enabled for this device.</summary>
        public bool Redirect { get; set; } = true;

        /// <summary>Total bytes sent (lifetime).</summary>
        public long TotalPacketSent { get; set; }

        /// <summary>Total bytes received (lifetime).</summary>
        public long TotalPacketReceived { get; set; }

        // Backing fields for Interlocked operations in the packet redirector.
        // Use the properties below for normal reads; use Interlocked.Add on these fields
        // from the hot packet-processing loop for thread-safe counting.
        internal long bytesSentField;
        internal long bytesReceivedField;

        /// <summary>Bytes sent since last stats reset (for rate calculation).</summary>
        public long BytesSentSinceLastReset
        {
            get => Interlocked.Read(ref bytesSentField);
            set => Interlocked.Exchange(ref bytesSentField, value);
        }

        /// <summary>Bytes received since last stats reset (for rate calculation).</summary>
        public long BytesReceivedSinceLastReset
        {
            get => Interlocked.Read(ref bytesReceivedField);
            set => Interlocked.Exchange(ref bytesReceivedField, value);
        }

        /// <summary>Timestamp of the last ARP reply seen from this device.</summary>
        public DateTime LastArpReplyTime { get; set; } = DateTime.Now;
    }
}
