using System;
using System.Text;

namespace LablabBean.Rendering.Terminal.Kitty;

/// <summary>
/// Encodes image data into Kitty graphics protocol escape sequences.
/// </summary>
public class KittyGraphicsProtocol
{
    /// <summary>
    /// Maximum dimension supported by Kitty graphics protocol.
    /// Based on typical terminal limits and protocol specifications.
    /// </summary>
    private const int MaxKittyDimension = 10000;

    /// <summary>
    /// Encodes RGBA pixel data into a Kitty graphics protocol escape sequence.
    /// </summary>
    /// <param name="rgba">RGBA pixel data (4 bytes per pixel)</param>
    /// <param name="width">Image width in pixels</param>
    /// <param name="height">Image height in pixels</param>
    /// <param name="options">Optional encoding options</param>
    /// <returns>Kitty graphics escape sequence string</returns>
    public string Encode(byte[] rgba, int width, int height, KittyOptions? options = null)
    {
        if (rgba == null || rgba.Length == 0)
            throw new ArgumentException("RGBA data cannot be null or empty", nameof(rgba));

        if (width <= 0 || height <= 0)
            throw new ArgumentException($"Image dimensions must be positive: {width}x{height}");

        if (width > MaxKittyDimension || height > MaxKittyDimension)
            throw new ArgumentException($"Image dimensions exceed Kitty protocol limits ({MaxKittyDimension}x{MaxKittyDimension}): {width}x{height}");

        if (rgba.Length != width * height * 4)
            throw new ArgumentException($"RGBA data length mismatch. Expected {width * height * 4} bytes, got {rgba.Length}", nameof(rgba));

        options ??= new KittyOptions();

        var base64Data = Convert.ToBase64String(rgba);
        var sb = new StringBuilder();

        // Start escape sequence: ESC _G
        sb.Append("\x1b_G");

        // Add control data
        sb.Append($"a=T"); // Action: transmit and display
        sb.Append($",f=32"); // Format: RGBA (32-bit)
        sb.Append($",s={width}"); // Width
        sb.Append($",v={height}"); // Height

        if (options.PlacementId.HasValue)
        {
            sb.Append($",i={options.PlacementId.Value}");
        }

        if (options.ChunkedTransmission && base64Data.Length > 4096)
        {
            // Chunked transmission for large images
            return EncodeChunked(sb.ToString(), base64Data, options);
        }
        else
        {
            // Direct transmission
            sb.Append($",m=0"); // m=0: last (or only) chunk
            sb.Append(";");
            sb.Append(base64Data);
            sb.Append("\x1b\\"); // String terminator: ESC \
        }

        return sb.ToString();
    }

    private string EncodeChunked(string controlData, string base64Data, KittyOptions options)
    {
        var sb = new StringBuilder();
        const int chunkSize = 4096;
        int offset = 0;

        while (offset < base64Data.Length)
        {
            int length = Math.Min(chunkSize, base64Data.Length - offset);
            string chunk = base64Data.Substring(offset, length);
            bool isLast = (offset + length >= base64Data.Length);

            sb.Append("\x1b_G");
            sb.Append(controlData);
            sb.Append($",m={(isLast ? 0 : 1)}"); // m=1: more chunks, m=0: last chunk
            sb.Append(";");
            sb.Append(chunk);
            sb.Append("\x1b\\");

            offset += length;
        }

        return sb.ToString();
    }
}
