namespace LablabBean.Plugins.MediaPlayer.Terminal.Braille.Converters;

/// <summary>
/// Converts image pixels to braille characters for terminal display.
/// Uses Unicode braille patterns (U+2800 to U+28FF) to represent 2x4 pixel grids.
/// </summary>
public static class BrailleConverter
{
    // Braille pattern base (U+2800)
    private const int BrailleBase = 0x2800;

    // Braille dot positions (2x4 grid):
    // 1 4
    // 2 5
    // 3 6
    // 7 8
    private static readonly int[] DotBits = { 0x01, 0x02, 0x04, 0x40, 0x08, 0x10, 0x20, 0x80 };

    /// <summary>
    /// Convert RGB24 image data to braille characters.
    /// Each braille character represents a 2x4 pixel block.
    /// </summary>
    /// <param name="rgbData">RGB24 pixel data (width * height * 3 bytes)</param>
    /// <param name="width">Image width in pixels</param>
    /// <param name="height">Image height in pixels</param>
    /// <param name="threshold">Brightness threshold (0-255) for dot visibility</param>
    /// <returns>2D array of braille characters</returns>
    public static char[,] ConvertToBraille(byte[] rgbData, int width, int height, byte threshold = 128)
    {
        // Calculate output dimensions (each braille char = 2x4 pixels)
        var outWidth = (width + 1) / 2;
        var outHeight = (height + 3) / 4;
        var result = new char[outHeight, outWidth];

        for (int row = 0; row < outHeight; row++)
        {
            for (int col = 0; col < outWidth; col++)
            {
                result[row, col] = GetBrailleChar(rgbData, width, height, col * 2, row * 4, threshold);
            }
        }

        return result;
    }

    /// <summary>
    /// Get braille character for a 2x4 pixel block
    /// </summary>
    private static char GetBrailleChar(byte[] rgbData, int width, int height, int x, int y, byte threshold)
    {
        int pattern = 0;

        // Map 2x4 pixel grid to braille dots
        for (int dy = 0; dy < 4; dy++)
        {
            for (int dx = 0; dx < 2; dx++)
            {
                var px = x + dx;
                var py = y + dy;

                if (px < width && py < height)
                {
                    var brightness = GetPixelBrightness(rgbData, width, px, py);

                    if (brightness >= threshold)
                    {
                        var dotIndex = dy * 2 + dx;
                        pattern |= DotBits[dotIndex];
                    }
                }
            }
        }

        return (char)(BrailleBase + pattern);
    }

    /// <summary>
    /// Calculate pixel brightness using luminance formula
    /// </summary>
    private static byte GetPixelBrightness(byte[] rgbData, int width, int x, int y)
    {
        var index = (y * width + x) * 3;

        if (index + 2 >= rgbData.Length)
            return 0;

        var r = rgbData[index];
        var g = rgbData[index + 1];
        var b = rgbData[index + 2];

        // Standard luminance formula (ITU-R BT.709)
        return (byte)(0.2126 * r + 0.7152 * g + 0.0722 * b);
    }

    /// <summary>
    /// Convert RGB24 image with ANSI color codes for colored braille output
    /// </summary>
    public static (char character, byte colorCode)[,] ConvertToBrailleWithColor(
        byte[] rgbData, int width, int height, byte threshold = 128)
    {
        var outWidth = (width + 1) / 2;
        var outHeight = (height + 3) / 4;
        var result = new (char, byte)[outHeight, outWidth];

        for (int row = 0; row < outHeight; row++)
        {
            for (int col = 0; col < outWidth; col++)
            {
                var brailleChar = GetBrailleChar(rgbData, width, height, col * 2, row * 4, threshold);
                var color = GetBlockAverageColor(rgbData, width, height, col * 2, row * 4);
                result[row, col] = (brailleChar, color);
            }
        }

        return result;
    }

    /// <summary>
    /// Get average color of a 2x4 pixel block
    /// </summary>
    private static byte GetBlockAverageColor(byte[] rgbData, int width, int height, int x, int y)
    {
        int totalR = 0, totalG = 0, totalB = 0, count = 0;

        for (int dy = 0; dy < 4; dy++)
        {
            for (int dx = 0; dx < 2; dx++)
            {
                var px = x + dx;
                var py = y + dy;

                if (px < width && py < height)
                {
                    var index = (py * width + px) * 3;
                    if (index + 2 < rgbData.Length)
                    {
                        totalR += rgbData[index];
                        totalG += rgbData[index + 1];
                        totalB += rgbData[index + 2];
                        count++;
                    }
                }
            }
        }

        if (count == 0)
            return 0;

        var avgR = totalR / count;
        var avgG = totalG / count;
        var avgB = totalB / count;

        return ColorQuantizer.QuantizeToAnsi16(avgR, avgG, avgB);
    }
}
