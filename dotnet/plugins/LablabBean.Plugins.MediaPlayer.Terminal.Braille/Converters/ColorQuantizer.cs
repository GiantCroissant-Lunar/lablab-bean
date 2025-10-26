namespace LablabBean.Plugins.MediaPlayer.Terminal.Braille.Converters;

/// <summary>
/// Quantizes RGB colors to ANSI 16-color palette for terminal display
/// </summary>
public static class ColorQuantizer
{
    // ANSI 16 color palette (standard terminal colors)
    private static readonly (int r, int g, int b)[] Ansi16Palette = new[]
    {
        (0, 0, 0),       // 0: Black
        (128, 0, 0),     // 1: Red
        (0, 128, 0),     // 2: Green
        (128, 128, 0),   // 3: Yellow
        (0, 0, 128),     // 4: Blue
        (128, 0, 128),   // 5: Magenta
        (0, 128, 128),   // 6: Cyan
        (192, 192, 192), // 7: White
        (128, 128, 128), // 8: Bright Black (Gray)
        (255, 0, 0),     // 9: Bright Red
        (0, 255, 0),     // 10: Bright Green
        (255, 255, 0),   // 11: Bright Yellow
        (0, 0, 255),     // 12: Bright Blue
        (255, 0, 255),   // 13: Bright Magenta
        (0, 255, 255),   // 14: Bright Cyan
        (255, 255, 255)  // 15: Bright White
    };

    /// <summary>
    /// Quantize RGB color to nearest ANSI 16-color code
    /// </summary>
    /// <param name="r">Red component (0-255)</param>
    /// <param name="g">Green component (0-255)</param>
    /// <param name="b">Blue component (0-255)</param>
    /// <returns>ANSI color code (0-15)</returns>
    public static byte QuantizeToAnsi16(int r, int g, int b)
    {
        byte bestColor = 0;
        var minDistance = double.MaxValue;

        for (byte i = 0; i < Ansi16Palette.Length; i++)
        {
            var (pr, pg, pb) = Ansi16Palette[i];
            var distance = ColorDistance(r, g, b, pr, pg, pb);

            if (distance < minDistance)
            {
                minDistance = distance;
                bestColor = i;
            }
        }

        return bestColor;
    }

    /// <summary>
    /// Calculate perceptual color distance using weighted Euclidean distance
    /// </summary>
    private static double ColorDistance(int r1, int g1, int b1, int r2, int g2, int b2)
    {
        // Weighted Euclidean distance (gives more weight to green, like human vision)
        var dr = r1 - r2;
        var dg = g1 - g2;
        var db = b1 - b2;

        // Perceptual weights (approximate)
        return Math.Sqrt(2 * dr * dr + 4 * dg * dg + 3 * db * db);
    }

    /// <summary>
    /// Get ANSI escape code for foreground color
    /// </summary>
    public static string GetForegroundColorCode(byte colorIndex)
    {
        if (colorIndex < 8)
            return $"\x1b[{30 + colorIndex}m"; // Standard colors
        else
            return $"\x1b[{90 + (colorIndex - 8)}m"; // Bright colors
    }

    /// <summary>
    /// Get ANSI escape code for background color
    /// </summary>
    public static string GetBackgroundColorCode(byte colorIndex)
    {
        if (colorIndex < 8)
            return $"\x1b[{40 + colorIndex}m"; // Standard colors
        else
            return $"\x1b[{100 + (colorIndex - 8)}m"; // Bright colors
    }

    /// <summary>
    /// Reset ANSI color codes
    /// </summary>
    public static string Reset => "\x1b[0m";
}
