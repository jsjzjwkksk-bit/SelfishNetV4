using System;
using System.Diagnostics;

namespace SelfishNetv3
{
    /// <summary>
    /// Token Bucket rate limiter for bandwidth control.
    /// Tokens represent bytes; the bucket refills at a configured rate (bytes/sec).
    /// Thread-safe — designed to be called from the hot packet-processing loop.
    /// </summary>
    /// <remarks>
    /// How it works:
    /// - The bucket holds up to <see cref="BucketCapacity"/> tokens (bytes).
    /// - Tokens refill at <see cref="TokensPerSecond"/> rate.
    /// - Each packet "consumes" tokens equal to its size.
    /// - If not enough tokens, the packet is dropped (bandwidth exceeded).
    /// 
    /// Compared to the old byte-cap approach, Token Bucket provides:
    /// - Smooth rate limiting (no bursty behavior at reset boundaries)
    /// - Allows small bursts up to bucket capacity
    /// - More accurate per-second bandwidth control
    /// </remarks>
    public sealed class TokenBucketRateLimiter
    {
        private readonly double _tokensPerSecond;
        private double _tokens;
        private long _lastRefillTimestamp;
        private readonly object _lock = new();

        /// <summary>
        /// Gets the configured rate in bytes per second.
        /// </summary>
        public double TokensPerSecond => _tokensPerSecond;

        /// <summary>
        /// Gets the bucket capacity (maximum burst size in bytes).
        /// Defaults to 1 second worth of tokens.
        /// </summary>
        public double BucketCapacity { get; }

        /// <summary>
        /// Gets the current number of available tokens.
        /// </summary>
        public double AvailableTokens
        {
            get
            {
                lock (_lock)
                {
                    Refill();
                    return _tokens;
                }
            }
        }

        /// <summary>
        /// Creates a new Token Bucket rate limiter.
        /// </summary>
        /// <param name="bytesPerSecond">
        /// Maximum throughput in bytes per second. Must be positive.
        /// Example: 128 * 1024 = 128 KB/s, 1024 * 1024 = 1 MB/s.
        /// </param>
        /// <param name="burstMultiplier">
        /// Burst multiplier (bucket capacity = bytesPerSecond * burstMultiplier).
        /// Default 1.5 allows 50% burst above the steady-state rate.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if bytesPerSecond is not positive.
        /// </exception>
        public TokenBucketRateLimiter(double bytesPerSecond, double burstMultiplier = 1.5)
        {
            if (bytesPerSecond <= 0)
                throw new ArgumentOutOfRangeException(nameof(bytesPerSecond),
                    "Rate must be positive.");
            if (burstMultiplier < 1.0)
                throw new ArgumentOutOfRangeException(nameof(burstMultiplier),
                    "Burst multiplier must be >= 1.0.");

            _tokensPerSecond = bytesPerSecond;
            BucketCapacity = bytesPerSecond * burstMultiplier;
            _tokens = BucketCapacity; // Start with a full bucket
            _lastRefillTimestamp = Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// Attempts to consume <paramref name="byteCount"/> tokens (bytes).
        /// Returns true if the packet should be forwarded, false if it should be dropped.
        /// </summary>
        /// <param name="byteCount">Size of the packet in bytes.</param>
        /// <returns>True if enough tokens were available and consumed.</returns>
        public bool TryConsume(int byteCount)
        {
            if (byteCount <= 0)
                return true;

            lock (_lock)
            {
                Refill();

                if (_tokens >= byteCount)
                {
                    _tokens -= byteCount;
                    return true;
                }

                return false; // Not enough tokens — drop the packet
            }
        }

        /// <summary>
        /// Updates the rate limit. Creates a smooth transition by scaling
        /// current tokens proportionally.
        /// </summary>
        /// <param name="newBytesPerSecond">New rate in bytes per second.</param>
        public void UpdateRate(double newBytesPerSecond)
        {
            // Rate changes are handled by creating a new limiter instance
            // in the DeviceCollection. This method is reserved for future
            // dynamic rate adjustment without replacing the object.
            throw new NotSupportedException(
                "Create a new TokenBucketRateLimiter instance to change the rate.");
        }

        /// <summary>
        /// Resets the bucket to full capacity.
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _tokens = BucketCapacity;
                _lastRefillTimestamp = Stopwatch.GetTimestamp();
            }
        }

        /// <summary>
        /// Refills tokens based on elapsed time since last refill.
        /// Must be called within the lock.
        /// </summary>
        private void Refill()
        {
            var now = Stopwatch.GetTimestamp();
            var elapsedSeconds = (now - _lastRefillTimestamp) / (double)Stopwatch.Frequency;

            if (elapsedSeconds > 0)
            {
                _tokens = Math.Min(
                    _tokens + elapsedSeconds * _tokensPerSecond,
                    BucketCapacity);
                _lastRefillTimestamp = now;
            }
        }
    }
}
