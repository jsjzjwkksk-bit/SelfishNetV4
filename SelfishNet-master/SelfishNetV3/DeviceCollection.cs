using System;
using System.Collections.Generic;
using System.Linq;

namespace SelfishNetv3
{
    /// <summary>
    /// Thread-safe collection of discovered <see cref="NetworkDevice"/> instances.
    /// Fires events when devices are added or removed.
    /// </summary>
    public sealed class DeviceCollection : IDisposable
    {
        private readonly object _syncRoot = new object();
        private readonly List<NetworkDevice> _devices = new List<NetworkDevice>();

        /// <summary>Raised when a new device is added to the collection.</summary>
        public event Action<NetworkDevice>? DeviceAdded;

        /// <summary>Raised when a device is removed from the collection.</summary>
        public event Action<NetworkDevice>? DeviceRemoved;

        /// <summary>
        /// Gets a snapshot of all devices in the collection.
        /// </summary>
        public List<NetworkDevice> GetAll()
        {
            lock (_syncRoot)
            {
                return new List<NetworkDevice>(_devices);
            }
        }

        /// <summary>
        /// Gets the number of devices in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _devices.Count;
                }
            }
        }

        /// <summary>
        /// Adds a device to the collection. If a device with the same IP already exists,
        /// updates its <see cref="NetworkDevice.LastArpReplyTime"/> and returns false.
        /// </summary>
        /// <returns>True if the device was newly added; false if already existed.</returns>
        public bool AddDevice(NetworkDevice device)
        {
            lock (_syncRoot)
            {
                var existing = _devices.FirstOrDefault(d =>
                    d.Ip.Equals(device.Ip));

                if (existing is not null)
                {
                    existing.LastArpReplyTime = DateTime.Now;
                    return false;
                }

                _devices.Add(device);
            }

            // Fire event outside lock to prevent deadlocks
            DeviceAdded?.Invoke(device);
            return true;
        }

        /// <summary>
        /// Removes a device from the collection by IP address match.
        /// </summary>
        /// <returns>True if a device was found and removed.</returns>
        public bool RemoveDevice(NetworkDevice device)
        {
            bool removed;
            lock (_syncRoot)
            {
                removed = _devices.RemoveAll(d =>
                    d.Ip.Equals(device.Ip)) > 0;
            }

            if (removed)
                DeviceRemoved?.Invoke(device);

            return removed;
        }

        /// <summary>
        /// Finds the gateway (router) device.
        /// </summary>
        public NetworkDevice? GetRouter()
        {
            lock (_syncRoot)
            {
                return _devices.FirstOrDefault(d => d.IsGateway);
            }
        }

        /// <summary>
        /// Finds the local PC device.
        /// </summary>
        public NetworkDevice? GetLocalPC()
        {
            lock (_syncRoot)
            {
                return _devices.FirstOrDefault(d => d.IsLocalPc);
            }
        }

        /// <summary>
        /// Finds a device by its IP address bytes.
        /// </summary>
        public NetworkDevice? GetDeviceByIp(byte[] ip)
        {
            lock (_syncRoot)
            {
                return _devices.FirstOrDefault(d =>
                    NetworkHelper.AreValuesEqual(d.Ip.GetAddressBytes(), ip));
            }
        }

        /// <summary>
        /// Finds a device by its MAC address bytes.
        /// </summary>
        public NetworkDevice? GetDeviceByMac(byte[] mac)
        {
            lock (_syncRoot)
            {
                return _devices.FirstOrDefault(d =>
                    NetworkHelper.AreValuesEqual(d.Mac.GetAddressBytes(), mac));
            }
        }

        /// <summary>
        /// Resets the per-interval byte counters for all devices.
        /// </summary>
        public void ResetAllByteCounts()
        {
            lock (_syncRoot)
            {
                foreach (var device in _devices)
                {
                    device.BytesSentSinceLastReset = 0;
                    device.BytesReceivedSinceLastReset = 0;
                }
            }
        }

        public void Dispose()
        {
            lock (_syncRoot)
            {
                _devices.Clear();
            }
        }
    }
}
