using System;

namespace LablabBean.Contracts.Diagnostic;

/// <summary>
/// Memory information container.
/// </summary>
public class MemoryInfo
{
    /// <summary>
    /// Total physical memory in bytes.
    /// </summary>
    public long TotalPhysicalMemory { get; set; }

    /// <summary>
    /// Available physical memory in bytes.
    /// </summary>
    public long AvailablePhysicalMemory { get; set; }

    /// <summary>
    /// Used physical memory in bytes.
    /// </summary>
    public long UsedPhysicalMemory { get; set; }

    /// <summary>
    /// Total virtual memory in bytes.
    /// </summary>
    public long TotalVirtualMemory { get; set; }

    /// <summary>
    /// Used virtual memory in bytes.
    /// </summary>
    public long UsedVirtualMemory { get; set; }

    /// <summary>
    /// Managed heap memory in bytes.
    /// </summary>
    public long ManagedHeapSize { get; set; }

    /// <summary>
    /// Number of garbage collections for generation 0.
    /// </summary>
    public int GCGen0Collections { get; set; }

    /// <summary>
    /// Number of garbage collections for generation 1.
    /// </summary>
    public int GCGen1Collections { get; set; }

    /// <summary>
    /// Number of garbage collections for generation 2.
    /// </summary>
    public int GCGen2Collections { get; set; }

    /// <summary>
    /// Time of the memory snapshot.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
