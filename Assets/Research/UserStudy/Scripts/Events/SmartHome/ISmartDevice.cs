using UnityEngine;

namespace UserStudy.SmartHome.Events
{
    /// <summary>
    /// Interface for smart home devices in Unity Events implementation.
    /// Provides common methods for device control and mode management.
    /// </summary>
    public interface ISmartDevice
    {
        /// <summary>
        /// Gets the GameObject this device is attached to.
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// Gets the device name.
        /// </summary>
        string DeviceName { get; }

        /// <summary>
        /// Sets the device active state (on/off).
        /// </summary>
        void SetDeviceActive(bool active);

        /// <summary>
        /// Sets the device mode (Home/Away/Sleep).
        /// </summary>
        void SetMode(string modeName);

        /// <summary>
        /// Gets the device type category.
        /// </summary>
        DeviceType DeviceType { get; }
    }

    /// <summary>
    /// Device type categories for filtering.
    /// </summary>
    public enum DeviceType
    {
        Light,
        Climate,
        Entertainment
    }
}
