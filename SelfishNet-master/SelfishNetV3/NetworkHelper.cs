using System;
using System.Net;

namespace SelfishNetv3
{
    /// <summary>
    /// Static utility methods for network byte comparisons and IP parsing.
    /// </summary>
    public static class NetworkHelper
    {
        /// <summary>
        /// Compares two byte arrays for value equality.
        /// Uses Span-based comparison for optimal performance.
        /// </summary>
        public static bool AreValuesEqual(byte[] a, byte[] b)
        {
            if (a is null || b is null)
                return false;

            return a.AsSpan().SequenceEqual(b.AsSpan());
        }

        /// <summary>
        /// Parses an IP address string into an <see cref="IPAddress"/>.
        /// </summary>
        public static IPAddress ParseIpAddress(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new ArgumentException("IP address string cannot be null or empty.", nameof(ip));

            return IPAddress.Parse(ip);
        }
    }
}
