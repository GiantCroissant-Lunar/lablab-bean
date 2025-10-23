namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// CPU information.
/// </summary>
public class CpuInfo
{
    /// <summary>
    /// CPU name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Number of CPU cores.
    /// </summary>
    public int CoreCount { get; set; }

    /// <summary>
    /// Number of logical processors.
    /// </summary>
    public int ProcessorCount { get; set; }

    /// <summary>
    /// CPU frequency in MHz.
    /// </summary>
    public int Frequency { get; set; }

    /// <summary>
    /// CPU architecture (e.g., x64, ARM64).
    /// </summary>
    public string Architecture { get; set; } = string.Empty;
}
