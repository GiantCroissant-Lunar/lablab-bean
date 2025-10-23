using System;

namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Comprehensive system information.
/// </summary>
public class SystemInfo
{
    /// <summary>
    /// Memory information.
    /// </summary>
    public MemoryInfo Memory { get; set; } = new();

    /// <summary>
    /// CPU information.
    /// </summary>
    public CpuInfo Cpu { get; set; } = new();

    /// <summary>
    /// GPU information.
    /// </summary>
    public GpuInfo Gpu { get; set; } = new();

    /// <summary>
    /// Device information.
    /// </summary>
    public DeviceInfo Device { get; set; } = new();

    /// <summary>
    /// When the system information was collected.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
