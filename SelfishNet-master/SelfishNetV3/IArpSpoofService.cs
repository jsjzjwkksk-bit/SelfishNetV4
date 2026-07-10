using System;
using System.Net;

namespace SelfishNetv3
{
    /// <summary>
    /// Contract for ARP spoofing, network discovery, and packet redirection services.
    /// Extracted from <see cref="ArpSpoofService"/> to enable testability and DI.
    /// </summary>
    public interface IArpSpoofService : IDisposable
    {
        // ── Network addresses ──
        byte[] LocalIP { get; }
        byte[] LocalMAC { get; }
        byte[] Netmask { get; }
        byte[] RouterIP { get; }
        byte[]? RouterMAC { get; }

        // ── Lifecycle ──
        void StartArpListener();
        void StopArpListener();
        void StartDiscovery();
        void StopDiscovery();
        int StartRedirector();
        void StopRedirector();
        void Shutdown();

        // ── Spoofing ──
        void Spoof(IPAddress ip1, IPAddress ip2);
        void UnSpoof(IPAddress ip1, IPAddress ip2);
        void CompleteUnspoof();

        // ── ARP requests ──
        void SendArpRequest(string ip);
        void FindMacRouter();

        // ── Events ──
        /// <summary>
        /// Raised when a spoofing operation's status changes.
        /// </summary>
        event EventHandler<ArpStatusChangedEventArgs>? StatusChanged;
    }

    /// <summary>
    /// Event arguments for ARP spoofing status changes.
    /// </summary>
    public class ArpStatusChangedEventArgs : EventArgs
    {
        /// <summary>Target IP whose spoofing status changed.</summary>
        public required IPAddress TargetIp { get; init; }

        /// <summary>True if spoofing is now active for this target.</summary>
        public required bool IsActive { get; init; }

        /// <summary>UTC timestamp of the status change.</summary>
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        /// <summary>Optional message describing the change.</summary>
        public string? Message { get; init; }
    }
}
