namespace LablabBean.Rendering.Terminal.Kitty;

/// <summary>
/// Configuration options for Kitty graphics protocol encoding.
/// </summary>
public class KittyOptions
{
    /// <summary>
    /// Transmission mode: 'd' = direct, 'f' = file, 't' = temporary file, 's' = shared memory
    /// </summary>
    public char TransmissionMode { get; set; } = 'd';

    /// <summary>
    /// Enable chunked transmission for large images
    /// </summary>
    public bool ChunkedTransmission { get; set; } = false;

    /// <summary>
    /// Placement ID for efficient re-rendering (optional)
    /// </summary>
    public int? PlacementId { get; set; }
}
