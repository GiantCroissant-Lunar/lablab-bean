namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// GPU information.
/// </summary>
public class GpuInfo
{
    /// <summary>
    /// GPU name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// GPU vendor.
    /// </summary>
    public string Vendor { get; set; } = string.Empty;

    /// <summary>
    /// Driver version.
    /// </summary>
    public string DriverVersion { get; set; } = string.Empty;

    /// <summary>
    /// API version.
    /// </summary>
    public string ApiVersion { get; set; } = string.Empty;

    /// <summary>
    /// Total GPU memory in bytes.
    /// </summary>
    public long TotalMemory { get; set; }

    /// <summary>
    /// Maximum texture size.
    /// </summary>
    public int MaxTextureSize { get; set; }

    /// <summary>
    /// Whether compute shaders are supported.
    /// </summary>
    public bool SupportsComputeShaders { get; set; }

    /// <summary>
    /// Whether geometry shaders are supported.
    /// </summary>
    public bool SupportsGeometryShaders { get; set; }
}
