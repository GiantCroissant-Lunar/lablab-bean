namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Device information.
/// </summary>
public class DeviceInfo
{
    /// <summary>
    /// Device name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Device model.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Operating system name.
    /// </summary>
    public string OperatingSystem { get; set; } = string.Empty;

    /// <summary>
    /// Operating system version.
    /// </summary>
    public string OsVersion { get; set; } = string.Empty;

    /// <summary>
    /// Device type (e.g., Desktop, Mobile, Console).
    /// </summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// Unique device identifier (if available).
    /// </summary>
    public string UniqueIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Whether GPS is supported.
    /// </summary>
    public bool SupportsGps { get; set; }

    /// <summary>
    /// Whether accelerometer is supported.
    /// </summary>
    public bool SupportsAccelerometer { get; set; }

    /// <summary>
    /// Whether gyroscope is supported.
    /// </summary>
    public bool SupportsGyroscope { get; set; }

    /// <summary>
    /// Whether vibration is supported.
    /// </summary>
    public bool SupportsVibration { get; set; }

    /// <summary>
    /// Battery level (0-1, -1 if unknown).
    /// </summary>
    public float BatteryLevel { get; set; } = -1f;

    /// <summary>
    /// Battery status (e.g., Charging, Full, Discharging).
    /// </summary>
    public string BatteryStatus { get; set; } = "Unknown";

    /// <summary>
    /// Network reachability status.
    /// </summary>
    public string NetworkReachability { get; set; } = "Unknown";

    /// <summary>
    /// Screen DPI.
    /// </summary>
    public float ScreenDpi { get; set; }

    /// <summary>
    /// Screen resolution (e.g., "1920x1080").
    /// </summary>
    public string ScreenResolution { get; set; } = string.Empty;

    /// <summary>
    /// Whether render textures are supported.
    /// </summary>
    public bool SupportsRenderTextures { get; set; }

    /// <summary>
    /// Whether shadows are supported.
    /// </summary>
    public bool SupportsShadows { get; set; }
}
