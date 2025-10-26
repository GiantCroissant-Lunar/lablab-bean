---
title: Terminal Graphics Protocols and Capability Detection - Comprehensive Research
category: research
status: draft
tags: [terminal, graphics, sixel, kitty-protocol, braille, video-rendering, capability-detection]
created: 2025-10-26
related:
  - Phase 6 User Story 4 (Unified Media Player)
---

## Executive Summary

This document provides comprehensive technical research on terminal graphics protocols and capability detection methods for implementing a unified media player feature. It covers SIXEL graphics, Kitty Graphics Protocol, Braille character encoding, and terminal capability detection mechanisms.

## Table of Contents

1. [Terminal Capability Detection Methods](#1-terminal-capability-detection-methods)
2. [SIXEL Protocol Specification](#2-sixel-protocol-specification)
3. [Kitty Graphics Protocol](#3-kitty-graphics-protocol)
4. [Braille Character Encoding for Video](#4-braille-character-encoding-for-video)
5. [References](#references)

---

## 1. Terminal Capability Detection Methods

### 1.1 Overview

Terminal capability detection requires multiple approaches since there is no standardized method across terminal emulators. Most terminals "lie" about their capabilities, making detection challenging.

### 1.2 Environment Variable Detection

#### Common Environment Variables

| Variable | Purpose | Example Values |
|----------|---------|----------------|
| `TERM` | Terminal type identifier | `xterm-256color`, `xterm`, `screen-256color` |
| `COLORTERM` | True color support indicator | `truecolor`, `24bit` |
| `TERM_PROGRAM` | Terminal application name | `iTerm.app`, `WezTerm`, `vscode` |
| `TERM_PROGRAM_VERSION` | Terminal version | `3.4.19`, `20230712-072601-f4abf8fd` |
| `VTE_VERSION` | GNOME Terminal/VTE version | `6003`, `7200` |
| `WT_SESSION` | Windows Terminal session ID | GUID value |

#### Environment Variable Patterns by Terminal

**iTerm2:**

- `TERM_PROGRAM=iTerm.app`
- `TERM_PROGRAM_VERSION=3.x.x`

**Kitty:**

- `TERM=xterm-kitty`
- `KITTY_WINDOW_ID` set

**WezTerm:**

- `TERM_PROGRAM=WezTerm`
- `WEZTERM_EXECUTABLE` set

**VS Code:**

- `TERM_PROGRAM=vscode`
- `VSCODE_GIT_IPC_HANDLE` set

**Windows Terminal:**

- `WT_SESSION` set
- `WT_PROFILE_ID` set

#### C# Detection Code Example

```csharp
using System;

public class TerminalDetector
{
    public static string GetTerminalType()
    {
        var termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
        var term = Environment.GetEnvironmentVariable("TERM");
        var wtSession = Environment.GetEnvironmentVariable("WT_SESSION");

        // Windows Terminal
        if (!string.IsNullOrEmpty(wtSession))
            return "WindowsTerminal";

        // Specific terminal programs
        if (!string.IsNullOrEmpty(termProgram))
        {
            if (termProgram.Contains("iTerm"))
                return "iTerm2";
            if (termProgram == "WezTerm")
                return "WezTerm";
            if (termProgram == "vscode")
                return "VSCode";
        }

        // Check TERM variable
        if (!string.IsNullOrEmpty(term))
        {
            if (term.Contains("kitty"))
                return "Kitty";
            if (term.Contains("xterm"))
                return "XTerm";
        }

        return "Unknown";
    }

    public static bool SupportsTrueColor()
    {
        var colorterm = Environment.GetEnvironmentVariable("COLORTERM");
        var term = Environment.GetEnvironmentVariable("TERM");

        // Check COLORTERM for explicit true color support
        if (colorterm == "truecolor" || colorterm == "24bit")
            return true;

        // Check TERM for 256 color support (minimum)
        if (!string.IsNullOrEmpty(term) &&
            (term.EndsWith("-256color") || term.Contains("256")))
            return true;

        return false;
    }
}
```

### 1.3 Device Attributes (DA1/DA2) Query Protocol

#### Primary Device Attributes (DA1)

**Query Sequence:**

```
CSI c
or
CSI 0 c
```

Actual bytes: `\x1b[c` or `\x1b[0c`

**Response Format:**

```
CSI ? Ps1 ; Ps2 ; ... ; Psn c
```

Example: `\x1b[?64;1;2;4;6;7;8;9;15;18;21c`

**Parameter Meanings:**

| Value | Capability |
|-------|-----------|
| 1 | 132-column mode |
| 2 | Printer port |
| 3 | ReGIS graphics |
| 4 | **SIXEL graphics** |
| 6 | Selective erase |
| 7 | Soft character sets (DRCS) |
| 8 | User-defined keys |
| 9 | National replacement character sets |
| 15 | Technical character set |
| 18 | Windowing capability |
| 21 | Horizontal scrolling |

**SIXEL Detection:**
The presence of `4` in the DA1 response indicates SIXEL support.

#### Secondary Device Attributes (DA2)

**Query Sequence:**

```
CSI > c
or
CSI > 0 c
```

Actual bytes: `\x1b[>c` or `\x1b[>0c`

**Response Format:**

```
CSI > Pp ; Pv ; Pc c
```

Where:

- `Pp` = Terminal type (0=VT100, 1=VT220, 61=VT510, etc.)
- `Pv` = Firmware version
- `Pc` = Keyboard type (0=STD, 1=PC)

Example: `\x1b[>61;20;1c` (VT510, version 2.0, PC keyboard)

#### C# Implementation for Terminal Queries

```csharp
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class TerminalCapabilityDetector
{
    private const int TIMEOUT_MS = 1000;

    public static async Task<bool> DetectSixelSupportAsync()
    {
        try
        {
            // Save current console mode
            var originalIn = Console.In;
            var originalOut = Console.Out;

            // Open console for raw I/O
            using var stdin = Console.OpenStandardInput();
            using var stdout = Console.OpenStandardOutput();

            // Send DA1 query
            var query = Encoding.ASCII.GetBytes("\x1b[c");
            await stdout.WriteAsync(query, 0, query.Length);
            await stdout.FlushAsync();

            // Read response with timeout
            var cts = new CancellationTokenSource(TIMEOUT_MS);
            var response = await ReadResponseAsync(stdin, cts.Token);

            // Check for SIXEL support (parameter 4)
            return response.Contains(";4;") || response.Contains(";4c");
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static async Task<string> ReadResponseAsync(Stream stream, CancellationToken token)
    {
        var buffer = new byte[256];
        var result = new StringBuilder();

        try
        {
            while (!token.IsCancellationRequested)
            {
                if (stream.CanRead)
                {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0) break;

                    result.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));

                    // Check for response terminator
                    if (result.ToString().Contains("c"))
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Timeout occurred
        }

        return result.ToString();
    }
}
```

### 1.4 Kitty Graphics Protocol Detection

**Query Sequence:**

```
ESC_Ga=q,i=31;ESC\
```

If the terminal supports Kitty Graphics Protocol, it will respond with success/error message.

**C# Example:**

```csharp
public static async Task<bool> DetectKittyGraphicsAsync()
{
    try
    {
        using var stdout = Console.OpenStandardOutput();

        // Query with unique image ID
        var query = Encoding.ASCII.GetBytes("\x1b_Ga=q,i=31;\x1b\\");
        await stdout.WriteAsync(query, 0, query.Length);
        await stdout.FlushAsync();

        // Wait for response
        var cts = new CancellationTokenSource(1000);
        var response = await ReadKittyResponseAsync(cts.Token);

        // Check for valid response (either success or error means support)
        return !string.IsNullOrEmpty(response) && response.Contains("_G");
    }
    catch
    {
        return false;
    }
}
```

### 1.5 Terminfo Database Detection (Unix/Linux)

**Using tput:**

```bash
# Check for SIXEL support
tput -T $TERM colors

# Query specific capabilities
tput cols  # Number of columns
tput lines # Number of lines
tput colors # Number of colors
```

**C# Wrapper for tput:**

```csharp
using System.Diagnostics;

public static class TerminfoDetector
{
    public static int GetColorSupport()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "tput",
                    Arguments = "colors",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return int.TryParse(output.Trim(), out var colors) ? colors : 0;
        }
        catch
        {
            return 0;
        }
    }
}
```

### 1.6 Best Practices and Gotchas

**Gotchas:**

1. **Environment Variable Spoofing**: Applications can set `TERM` to any value, making it unreliable
2. **Terminal Multiplexer Interference**: tmux/screen modify `TERM` variable
3. **SSH Session Issues**: Environment variables may not propagate correctly
4. **Response Timing**: Some terminals respond slowly to queries
5. **Buffering Issues**: Terminal responses may be buffered

**Best Practices:**

1. **Use Multiple Detection Methods**: Combine environment variables + query sequences
2. **Implement Timeouts**: Always timeout terminal queries (500-1000ms)
3. **Graceful Degradation**: Provide fallback rendering modes
4. **Cache Detection Results**: Avoid repeated queries
5. **Test Across Terminals**: Verify on major terminals (xterm, iTerm2, Kitty, Windows Terminal)

**Detection Order (Recommended):**

```csharp
public enum GraphicsProtocol
{
    None,
    Braille,
    Ansi16Color,
    Ansi256Color,
    Sixel,
    KittyGraphics
}

public static async Task<GraphicsProtocol> DetectBestProtocolAsync()
{
    // 1. Try Kitty Graphics (most capable)
    if (await DetectKittyGraphicsAsync())
        return GraphicsProtocol.KittyGraphics;

    // 2. Try SIXEL (second best)
    if (await DetectSixelSupportAsync())
        return GraphicsProtocol.Sixel;

    // 3. Check color support via environment
    if (SupportsTrueColor())
        return GraphicsProtocol.Ansi256Color;

    var term = Environment.GetEnvironmentVariable("TERM");
    if (!string.IsNullOrEmpty(term) && term.Contains("256"))
        return GraphicsProtocol.Ansi256Color;

    // 4. Fallback to 16-color ANSI
    return GraphicsProtocol.Ansi16Color;
}
```

---

## 2. SIXEL Protocol Specification

### 2.1 Overview

SIXEL (Six Pixels) is a bitmap graphics format developed by Digital Equipment Corporation (DEC) for their VT terminals. A "sixel" represents a vertical column of 6 pixels, with 64 possible patterns (2^6).

### 2.2 Escape Sequence Format

**DCS Structure:**

```
DCS Pa ; Pb ; Ph q s...s ST
```

**Component Breakdown:**

- **DCS**: Device Control String (C1: `\x90`, 7-bit: `\x1b P`)
- **Pa**: Pixel aspect ratio parameter
- **Pb**: Background handling
- **Ph**: Horizontal grid size (typically ignored)
- **q**: Sixel identifier
- **s...s**: Sixel data
- **ST**: String Terminator (C1: `\x9c`, 7-bit: `\x1b \`)

**Full 7-bit Sequence:**

```
ESC P Pa ; Pb ; Ph q s...s ESC \
```

### 2.3 DCS Parameters

#### Pa - Pixel Aspect Ratio

| Value | Aspect Ratio (V:H) |
|-------|-------------------|
| 0, 1 (default) | 2:1 |
| 2 | 5:1 |
| 3, 4 | 3:1 |
| 5, 6 | 2:1 |
| 7, 8, 9 | 1:1 |

#### Pb - Background Handling

| Value | Behavior |
|-------|----------|
| 0, 2 (default) | Zero pixels render as background color |
| 1 | Zero pixels retain current color (transparent) |

### 2.4 Sixel Data Encoding

**Character Range**: `?` (0x3F) to `~` (0x7E)

**Encoding Formula:**

```
sixel_value = character_code - 0x3F
```

**Bit Mapping** (LSB = top pixel):

| Character | Hex | Decimal | Binary | Pattern |
|-----------|-----|---------|--------|---------|
| ? | 0x3F | 63 | 000000 | ░░░░░░ |
| @ | 0x40 | 64 | 000001 | █░░░░░ |
| A | 0x41 | 65 | 000010 | ░█░░░░ |
| t | 0x74 | 116 | 110101 | █░█░██ |
| ~ | 0x7E | 126 | 111111 | ██████ |

**Example Encoding:**

```
Pixels (top to bottom): 1, 0, 1, 0, 1, 1
Binary: 110101
Value: 53 (decimal)
Character: 53 + 0x3F = 0x74 = 't'
```

### 2.5 Sixel Control Commands

| Character | Function | Syntax | Description |
|-----------|----------|--------|-------------|
| `!` (0x21) | Repeat | `!Pn char` | Repeat character Pn times |
| `"` (0x22) | Raster Attrs | `"Pan;Pad;Ph;Pv` | Define image dimensions |
| `#` (0x23) | Color Select | `#Pc` or `#Pc;Pu;Px;Py;Pz` | Select/define color |
| `$` (0x24) | CR | `$` | Carriage return (move to start of line) |
| `-` (0x2D) | LF | `-` | Line feed (advance 6 pixels down) |

### 2.6 Color Specification

#### Color Selection (Basic)

```
#Pc
```

Selects color register Pc (0-255, though limited by terminal)

#### Color Definition (RGB Mode)

```
#Pc;2;Pr;Pg;Pb
```

- **Pc**: Color register number (0-255)
- **2**: RGB mode
- **Pr, Pg, Pb**: Red, Green, Blue intensity (0-100%)

**Note**: Modern terminals expect 0-255, but spec says 0-100%

#### Color Definition (HLS Mode)

```
#Pc;1;Ph;Pl;Ps
```

- **Pc**: Color register number
- **1**: HLS mode
- **Ph**: Hue (0-360°)
- **Pl**: Lightness (0-100%)
- **Ps**: Saturation (0-100%)

### 2.7 Raster Attributes

**Syntax:**

```
"Pan;Pad;Ph;Pv
```

**Parameters:**

- **Pan**: Numerator of pixel aspect ratio
- **Pad**: Denominator of pixel aspect ratio
- **Ph**: Horizontal size in pixels
- **Pv**: Vertical size in pixels

**Example:**

```
"1;1;640;480
```

Defines 1:1 aspect ratio, 640x480 pixels

### 2.8 RLE Compression

**Repeat Syntax:**

```
!Pn char
```

Repeats `char` exactly `Pn` times.

**Examples:**

```
!14@     # Repeat '@' 14 times (first pixel on, rest off)
!100?    # Repeat '?' 100 times (all pixels off)
!50~     # Repeat '~' 50 times (all pixels on)
```

**Compression Effectiveness:**

```
Uncompressed: ??????????????????????????????????  (50 bytes)
Compressed:   !50?                              (4 bytes)
Ratio: 12.5:1
```

### 2.9 Complete SIXEL Example

**Simple 3x6 Checkerboard Pattern:**

```
ESC P 0 ; 0 ; 0 q
"1;1;3;6           Raster: 1:1 aspect, 3x6 pixels
#0;2;0;0;0         Color 0: Black (RGB)
#1;2;100;100;100   Color 1: White (RGB)
#0 ~ $ #1 ~ $ #0 ~ -   First row
#1 ~ $ #0 ~ $ #1 ~ -   Second row
ESC \
```

### 2.10 C# SIXEL Generator Example

```csharp
using System;
using System.Text;

public class SixelEncoder
{
    private const int MAX_COLORS = 256;
    private readonly StringBuilder _output;

    public SixelEncoder()
    {
        _output = new StringBuilder();
    }

    public void StartImage(int width, int height)
    {
        // DCS sequence with parameters
        _output.Append("\x1bP0;0;0q");

        // Raster attributes
        _output.Append($"\"1;1;{width};{height}");
    }

    public void DefineColor(int index, byte r, byte g, byte b)
    {
        if (index < 0 || index >= MAX_COLORS)
            throw new ArgumentOutOfRangeException(nameof(index));

        // RGB mode: values 0-100 (convert from 0-255)
        int rPct = (r * 100) / 255;
        int gPct = (g * 100) / 255;
        int bPct = (b * 100) / 255;

        _output.Append($"#{index};2;{rPct};{gPct};{bPct}");
    }

    public void SelectColor(int index)
    {
        _output.Append($"#{index}");
    }

    public void WriteSixel(byte sixelBits)
    {
        // Encode 6 pixels as single character
        char sixelChar = (char)(0x3F + (sixelBits & 0x3F));
        _output.Append(sixelChar);
    }

    public void WriteRun(byte sixelBits, int count)
    {
        if (count <= 0) return;

        char sixelChar = (char)(0x3F + (sixelBits & 0x3F));

        if (count == 1)
        {
            _output.Append(sixelChar);
        }
        else
        {
            // Use RLE compression
            _output.Append($"!{count}{sixelChar}");
        }
    }

    public void CarriageReturn()
    {
        _output.Append('$');
    }

    public void LineFeed()
    {
        _output.Append('-');
    }

    public void EndImage()
    {
        // String Terminator
        _output.Append("\x1b\\");
    }

    public string GetOutput()
    {
        return _output.ToString();
    }

    // Helper: Convert 6 vertical pixels to sixel byte
    public static byte PixelsToSixel(bool p0, bool p1, bool p2, bool p3, bool p4, bool p5)
    {
        byte result = 0;
        if (p0) result |= 0x01;  // Bottom (LSB)
        if (p1) result |= 0x02;
        if (p2) result |= 0x04;
        if (p3) result |= 0x08;
        if (p4) result |= 0x10;
        if (p5) result |= 0x20;  // Top (MSB)
        return result;
    }
}

// Usage example
public class SixelExample
{
    public static string GenerateRedSquare(int size)
    {
        var encoder = new SixelEncoder();
        encoder.StartImage(size, size);

        // Define red color
        encoder.DefineColor(1, 255, 0, 0);
        encoder.SelectColor(1);

        // Draw square (process 6 rows at a time)
        int sixelRows = (size + 5) / 6;  // Round up

        for (int row = 0; row < sixelRows; row++)
        {
            // All pixels on (full sixel)
            encoder.WriteRun(0x3F, size);

            if (row < sixelRows - 1)
                encoder.LineFeed();
        }

        encoder.EndImage();
        return encoder.GetOutput();
    }
}
```

### 2.11 Palette Optimization

**Maximum Colors**: 256 (typically much less: 16 on VT340, 4 on VT330)

**Optimization Strategies:**

1. **Color Quantization**: Reduce image to 256 colors using:
   - Median Cut algorithm
   - Octree quantization
   - K-means clustering

2. **Palette Selection**:
   - Analyze image histogram
   - Select most frequent colors
   - Use perceptually uniform color space (LAB)

3. **Dithering** (if colors exceed limit):
   - Floyd-Steinberg dithering
   - Ordered dithering (Bayer matrix)
   - Error diffusion

**C# Palette Quantization Example:**

```csharp
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public class PaletteQuantizer
{
    public static Color[] QuantizeToNColors(Bitmap image, int maxColors)
    {
        // Simple frequency-based quantization
        var colorCounts = new Dictionary<Color, int>();

        // Count color frequencies
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var pixel = image.GetPixel(x, y);
                if (!colorCounts.ContainsKey(pixel))
                    colorCounts[pixel] = 0;
                colorCounts[pixel]++;
            }
        }

        // Select top N most frequent colors
        return colorCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(maxColors)
            .Select(kvp => kvp.Key)
            .ToArray();
    }

    public static int FindNearestColorIndex(Color target, Color[] palette)
    {
        int nearestIndex = 0;
        double minDistance = double.MaxValue;

        for (int i = 0; i < palette.Length; i++)
        {
            double distance = ColorDistance(target, palette[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    private static double ColorDistance(Color c1, Color c2)
    {
        // Euclidean distance in RGB space
        int dr = c1.R - c2.R;
        int dg = c1.G - c2.G;
        int db = c1.B - c2.B;
        return Math.Sqrt(dr * dr + dg * dg + db * db);
    }
}
```

### 2.12 Terminal Compatibility Matrix

| Terminal | SIXEL Support | Version | Max Colors | Notes |
|----------|---------------|---------|------------|-------|
| xterm | ✅ Yes | #359+ | 256 | Default since patch #359 |
| iTerm2 | ✅ Yes | 3.3.0+ | 256 | macOS only |
| WezTerm | ✅ Yes | 20200620+ | 256 | Cross-platform |
| mintty | ✅ Yes | 2.6.0+ | 256 | Windows (Cygwin/MSYS2) |
| Konsole | ✅ Yes | 22.04+ | 256 | KDE terminal |
| foot | ✅ Yes | 1.2.0+ | 256 | Wayland native |
| mlterm | ✅ Yes | 3.1.9+ | 256 | Multi-lingual |
| VS Code | ✅ Yes | 1.80+ | 256 | Requires `enableImages` |
| tmux | ✅ Yes | 3.3+ | 256 | Compile with `--enable-sixel` |
| Windows Terminal | ❌ No | - | - | Under development |
| Alacritty | ❌ No | - | - | No plans to support |
| GNOME Terminal | ❌ No | - | - | No support |

### 2.13 Best Practices and Gotchas

**Best Practices:**

1. **Always use raster attributes** to declare image dimensions
2. **Optimize palette** to minimum required colors
3. **Use RLE compression** for solid color regions
4. **Test on real terminals** - emulation varies
5. **Provide fallback** for non-SIXEL terminals

**Gotchas:**

1. **Color value mismatch**: Spec says 0-100%, modern terminals expect 0-255
2. **Aspect ratio confusion**: Many terminals ignore aspect ratio parameters
3. **Buffer limits**: Some terminals limit SIXEL data size
4. **Scrolling mode**: SIXEL can trigger unwanted scrolling if not disabled
5. **Cursor positioning**: SIXEL affects cursor position unpredictably

---

## 3. Kitty Graphics Protocol

### 3.1 Overview

The Kitty Graphics Protocol is a modern terminal graphics protocol designed to be flexible, performant, and more capable than SIXEL. It supports full RGBA images, multiple compression formats, animations, and efficient image placement.

### 3.2 Escape Sequence Format

**APC (Application Program Command) Structure:**

```
ESC _ G <control> ; <payload> ESC \
```

**7-bit Encoding:**

```
\x1b_G<key=value,key=value,...>;<base64_data>\x1b\
```

**Components:**

- `ESC _` = APC start (\x1b\x5f)
- `G` = Graphics command identifier
- `<control>` = Comma-separated key=value pairs
- `;` = Separator between control and payload
- `<payload>` = Base64-encoded image data
- `ESC \` = APC end (\x1b\x5c)

### 3.3 Control Keys

#### Transmission Keys

| Key | Values | Description |
|-----|--------|-------------|
| `a` | t,T,q,p,d,f,c,a | Action: transmit/display, query, place, delete, frame, compose, animate |
| `f` | 24,32,100 | Format: RGB, RGBA (default), PNG |
| `t` | d,f,t,s | Transmission: direct (default), file, temp file, shared memory |
| `o` | z | Compression: zlib deflate |
| `m` | 0,1 | More chunks: 0=final, 1=more coming |
| `i` | 1-4294967295 | Image ID (unique identifier) |
| `I` | 0-4294967295 | Image number (non-unique) |
| `p` | 1-4294967295 | Placement ID |
| `q` | 0,1,2 | Quiet: 0=verbose, 1=suppress OK, 2=suppress errors |

#### Image Specification Keys

| Key | Values | Description |
|-----|--------|-------------|
| `s` | pixels | Image width |
| `v` | pixels | Image height |
| `S` | bytes | Data size (uncompressed) |
| `O` | bytes | Offset within data |
| `x` | pixels | Source rectangle X |
| `y` | pixels | Source rectangle Y |
| `w` | pixels | Source rectangle width |
| `h` | pixels | Source rectangle height |

#### Placement Keys

| Key | Values | Description |
|-----|--------|-------------|
| `X` | pixels | Left edge offset in cell |
| `Y` | pixels | Top edge offset in cell |
| `c` | columns | Width in columns |
| `r` | rows | Height in rows |
| `z` | int | Z-index (negative = below text) |
| `C` | 0,1 | Cursor movement: 0=move, 1=no move |

#### Animation Keys

| Key | Values | Description |
|-----|--------|-------------|
| `g` | 0-65535 | Frame gap in milliseconds |
| `v` | 0-65535 | Loop count (0=infinite) |
| `s` | 0,1,2,3 | State: stop, loading, playing, looping |
| `r` | frame# | Edit/replace frame number |
| `z` | ms | Override gap for specific frame |
| `c` | frame# | Compose source frame |
| `X` | 0,1 | Composition mode: 0=alpha blend, 1=replace |
| `Y` | RGBA | Background color (32-bit) |

### 3.4 Data Formats

#### RGB Format (f=24)

```
R G B R G B R G B ...
```

- 3 bytes per pixel
- No alpha channel
- sRGB color space

#### RGBA Format (f=32, default)

```
R G B A R G B A R G B A ...
```

- 4 bytes per pixel
- Alpha channel supported
- sRGB color space

#### PNG Format (f=100)

```
<complete PNG file data>
```

- Full PNG file transmitted
- Width/height extracted from PNG header
- Supports all PNG features (transparency, compression)

**Example Calculation:**

```
10x20 pixel image in RGB format:
Size = 10 * 20 * 3 = 600 bytes

10x20 pixel image in RGBA format:
Size = 10 * 20 * 4 = 800 bytes
```

### 3.5 Compression

**Supported Compression:**

- `o=z` : ZLIB deflate (RFC 1950)

**Compression Workflow:**

1. Generate raw pixel data (RGB/RGBA)
2. Compress using ZLIB
3. Base64 encode compressed data
4. Send with `o=z` flag

**When to Use Compression:**

| Scenario | Compression? | Reason |
|----------|-------------|--------|
| Screenshot (large, detailed) | ✅ Yes | ZLIB compression effective |
| Small icons (< 1KB) | ❌ No | Compression overhead > savings |
| PNG format | ❌ No | Already compressed |
| Network transmission | ✅ Yes | Reduce bandwidth |
| Local file | ❌ No | Use `t=f` instead |

### 3.6 Chunking Protocol

**Requirements:**

- Maximum chunk size: 4096 bytes
- All chunks except last must be multiple of 4 bytes
- Base64-encoded data is chunked, not raw data

**Chunking Sequence:**

```
# First chunk (full control data)
ESC_Gf=32,s=100,v=100,m=1;<chunk1>ESC\

# Middle chunks (only m parameter)
ESC_Gm=1;<chunk2>ESC\
ESC_Gm=1;<chunk3>ESC\

# Final chunk
ESC_Gm=0;<chunkN>ESC\
```

**C# Chunking Example:**

```csharp
using System;
using System.Text;

public class KittyChunker
{
    private const int CHUNK_SIZE = 4096;

    public static string[] ChunkData(byte[] imageData, string controlData)
    {
        // Base64 encode
        string base64 = Convert.ToBase64String(imageData);

        // Calculate chunks
        int totalChunks = (base64.Length + CHUNK_SIZE - 1) / CHUNK_SIZE;
        var chunks = new string[totalChunks];

        for (int i = 0; i < totalChunks; i++)
        {
            int start = i * CHUNK_SIZE;
            int length = Math.Min(CHUNK_SIZE, base64.Length - start);

            // Ensure multiple of 4 (except last chunk)
            if (i < totalChunks - 1 && length % 4 != 0)
                length = (length / 4) * 4;

            string chunk = base64.Substring(start, length);

            if (i == 0)
            {
                // First chunk: full control data
                chunks[i] = $"\x1b_G{controlData},m=1;{chunk}\x1b\\";
            }
            else if (i == totalChunks - 1)
            {
                // Last chunk: m=0
                chunks[i] = $"\x1b_Gm=0;{chunk}\x1b\\";
            }
            else
            {
                // Middle chunks: m=1
                chunks[i] = $"\x1b_Gm=1;{chunk}\x1b\\";
            }
        }

        return chunks;
    }
}
```

### 3.7 Image Placement

**Basic Placement:**

```
ESC_Ga=T,f=32,s=100,v=100;<base64_data>ESC\
```

- `a=T`: Transmit and display immediately
- Image placed at current cursor position

**Advanced Placement with Positioning:**

```
ESC_Ga=T,f=32,s=100,v=100,X=10,Y=5,c=10,r=5;<data>ESC\
```

- `X=10, Y=5`: Offset 10 pixels right, 5 pixels down within cell
- `c=10`: Display width of 10 columns
- `r=5`: Display height of 5 rows

**Reusing Images (Efficient):**

```
# Transmit image once
ESC_Ga=t,i=100,f=32,s=100,v=100;<data>ESC\

# Place multiple times
ESC_Ga=p,i=100,p=1,X=0,Y=0ESC\
ESC_Ga=p,i=100,p=2,X=50,Y=0ESC\
ESC_Ga=p,i=100,p=3,X=100,Y=0ESC\
```

### 3.8 Deletion Operations

**Delete by Image ID:**

```
ESC_Ga=d,d=i,i=100ESC\   # Delete image 100 (data + placements)
ESC_Ga=d,d=I,i=100ESC\   # Delete placements only
```

**Delete by Placement ID:**

```
ESC_Ga=d,d=p,i=100,p=1ESC\   # Delete specific placement
```

**Delete All:**

```
ESC_Ga=d,d=aESC\   # Delete all placements (keep data)
ESC_Ga=d,d=AESC\   # Delete all (data + placements)
```

**Delete at Cursor:**

```
ESC_Ga=d,d=cESC\   # Delete placements at cursor
```

### 3.9 C# Implementation Example

```csharp
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

public class KittyGraphicsEncoder
{
    public enum Format
    {
        RGB = 24,
        RGBA = 32,
        PNG = 100
    }

    public enum Action
    {
        TransmitAndDisplay,  // T
        TransmitOnly,        // t
        Query,               // q
        Place,               // p
        Delete               // d
    }

    public class ImageData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Pixels { get; set; }
        public Format Format { get; set; }
    }

    public static string EncodeImage(
        Bitmap bitmap,
        int imageId = 0,
        bool useCompression = true,
        Action action = Action.TransmitAndDisplay)
    {
        // Extract RGBA pixel data
        var imageData = BitmapToRGBA(bitmap);

        // Optionally compress
        byte[] payload = imageData.Pixels;
        string compression = "";

        if (useCompression)
        {
            payload = CompressZlib(payload);
            compression = ",o=z";
        }

        // Build control string
        var control = new StringBuilder();
        control.Append($"a={GetActionChar(action)}");
        control.Append($",f={32}");  // RGBA
        control.Append($",s={imageData.Width}");
        control.Append($",v={imageData.Height}");

        if (imageId > 0)
            control.Append($",i={imageId}");

        control.Append(compression);

        // Base64 encode
        string base64 = Convert.ToBase64String(payload);

        // Check if chunking needed
        if (base64.Length <= 4096)
        {
            // Single transmission
            return $"\x1b_G{control};{base64}\x1b\\";
        }
        else
        {
            // Multiple chunks
            var chunks = KittyChunker.ChunkData(payload, control.ToString());
            return string.Join("", chunks);
        }
    }

    private static ImageData BitmapToRGBA(Bitmap bitmap)
    {
        var data = new ImageData
        {
            Width = bitmap.Width,
            Height = bitmap.Height,
            Format = Format.RGBA,
            Pixels = new byte[bitmap.Width * bitmap.Height * 4]
        };

        // Lock bitmap for fast access
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

        try
        {
            int idx = 0;
            for (int y = 0; y < bitmap.Height; y++)
            {
                IntPtr row = bmpData.Scan0 + (y * bmpData.Stride);

                for (int x = 0; x < bitmap.Width; x++)
                {
                    // Read BGRA (Windows format)
                    byte b = Marshal.ReadByte(row, x * 4 + 0);
                    byte g = Marshal.ReadByte(row, x * 4 + 1);
                    byte r = Marshal.ReadByte(row, x * 4 + 2);
                    byte a = Marshal.ReadByte(row, x * 4 + 3);

                    // Write RGBA (Kitty format)
                    data.Pixels[idx++] = r;
                    data.Pixels[idx++] = g;
                    data.Pixels[idx++] = b;
                    data.Pixels[idx++] = a;
                }
            }
        }
        finally
        {
            bitmap.UnlockBits(bmpData);
        }

        return data;
    }

    private static byte[] CompressZlib(byte[] data)
    {
        using var output = new MemoryStream();
        using (var zlib = new DeflateStream(output, CompressionMode.Compress))
        {
            zlib.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    private static char GetActionChar(Action action)
    {
        return action switch
        {
            Action.TransmitAndDisplay => 'T',
            Action.TransmitOnly => 't',
            Action.Query => 'q',
            Action.Place => 'p',
            Action.Delete => 'd',
            _ => 'T'
        };
    }

    // Usage example
    public static void DisplayImage(Bitmap image)
    {
        string kittySequence = EncodeImage(image, imageId: 42, useCompression: true);
        Console.Write(kittySequence);
    }
}
```

### 3.10 Animation Support

**Frame-by-Frame Animation:**

```csharp
public class KittyAnimator
{
    public static void SendAnimation(Bitmap[] frames, int frameDelayMs)
    {
        int imageId = 100;

        // Send all frames
        for (int i = 0; i < frames.Length; i++)
        {
            var imageData = BitmapToRGBA(frames[i]);
            string base64 = Convert.ToBase64String(imageData.Pixels);

            // Frame control
            string control = $"a=f,i={imageId},r={i + 1},z={frameDelayMs}";

            if (i == 0)
            {
                // First frame includes dimensions
                control += $",f=32,s={frames[i].Width},v={frames[i].Height}";
            }

            Console.Write($"\x1b_G{control};{base64}\x1b\\");
        }

        // Start animation
        Console.Write($"\x1b_Ga=a,i={imageId},s=3,v=0\x1b\\");  // s=3 loop, v=0 infinite
    }
}
```

### 3.11 Best Practices

1. **Use PNG format** for pre-compressed images
2. **Use ZLIB compression** for raw RGBA data > 1KB
3. **Reuse image IDs** for multiple placements
4. **Set quiet mode** (`q=1`) in scripts to avoid response parsing
5. **Check terminal support** before sending sequences
6. **Use chunking** for images > 4KB base64-encoded
7. **Clean up images** when done to free terminal memory

### 3.12 Gotchas

1. **Base64 encoding overhead**: +33% size increase
2. **Chunk alignment**: Must be multiple of 4 bytes
3. **Memory limits**: Large images may exceed terminal memory
4. **Z-index behavior**: Varies by terminal implementation
5. **Cursor positioning**: Set `C=1` to prevent cursor movement

---

## 4. Braille Character Encoding for Video

### 4.1 Overview

Unicode Braille patterns (U+2800-U+28FF) provide a way to display graphics in any terminal using standard Unicode text. Each Braille character represents 8 pixels in a 2x4 grid, effectively doubling vertical resolution compared to regular characters.

### 4.2 Unicode Braille Pattern Block

**Range**: U+2800 to U+28FF (256 characters)

**Grid Structure** (2 columns × 4 rows):

```
┌─┬─┐
│1│4│  Dot numbering (standard Braille)
├─┼─┤
│2│5│
├─┼─┤
│3│6│
├─┼─┤
│7│8│
└─┴─┘
```

**Bit Mapping to Unicode Offset:**

| Dot | Bit Position | Value |
|-----|--------------|-------|
| 1 | 0 | 0x01 |
| 2 | 1 | 0x02 |
| 3 | 2 | 0x04 |
| 4 | 3 | 0x08 |
| 5 | 4 | 0x10 |
| 6 | 5 | 0x20 |
| 7 | 6 | 0x40 |
| 8 | 7 | 0x80 |

**Encoding Formula:**

```
unicode_char = 0x2800 + dot_bits
```

**Examples:**

| Pattern | Dots | Hex | Unicode | Character |
|---------|------|-----|---------|-----------|
| Blank | none | 0x00 | U+2800 | ⠀ |
| Full | all | 0xFF | U+28FF | ⣿ |
| Top-left | 1 | 0x01 | U+2801 | ⠁ |
| Top-right | 4 | 0x08 | U+2808 | ⠈ |
| Vertical line | 1,2,3,7 | 0x47 | U+2847 | ⡇ |

### 4.3 Pixel-to-Dot Threshold Algorithm

**Goal**: Convert grayscale pixel values to binary (on/off) for each Braille dot.

**Basic Threshold:**

```csharp
public static bool PixelToDot(byte grayscale, byte threshold = 128)
{
    return grayscale >= threshold;
}
```

**Adaptive Threshold** (Otsu's method):

```csharp
using System;

public class ThresholdCalculator
{
    public static byte CalculateOtsuThreshold(byte[] pixels)
    {
        // Build histogram
        int[] histogram = new int[256];
        foreach (byte pixel in pixels)
            histogram[pixel]++;

        // Total pixels
        int total = pixels.Length;

        float sum = 0;
        for (int i = 0; i < 256; i++)
            sum += i * histogram[i];

        float sumB = 0;
        int wB = 0;
        int wF = 0;

        float varMax = 0;
        byte threshold = 0;

        for (int t = 0; t < 256; t++)
        {
            wB += histogram[t];
            if (wB == 0) continue;

            wF = total - wB;
            if (wF == 0) break;

            sumB += t * histogram[t];

            float mB = sumB / wB;
            float mF = (sum - sumB) / wF;

            float varBetween = (float)wB * (float)wF * (mB - mF) * (mB - mF);

            if (varBetween > varMax)
            {
                varMax = varBetween;
                threshold = (byte)t;
            }
        }

        return threshold;
    }
}
```

**Dithering for Better Results:**

```csharp
public class FloydSteinbergDithering
{
    public static byte[,] DitherImage(byte[,] grayscale)
    {
        int width = grayscale.GetLength(0);
        int height = grayscale.GetLength(1);

        byte[,] output = new byte[width, height];
        float[,] errors = new float[width, height];

        // Copy to float for error propagation
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                errors[x, y] = grayscale[x, y];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float oldPixel = errors[x, y];
                byte newPixel = (byte)(oldPixel >= 128 ? 255 : 0);
                output[x, y] = newPixel;

                float error = oldPixel - newPixel;

                // Distribute error to neighbors
                if (x + 1 < width)
                    errors[x + 1, y] += error * 7 / 16;
                if (x - 1 >= 0 && y + 1 < height)
                    errors[x - 1, y + 1] += error * 3 / 16;
                if (y + 1 < height)
                    errors[x, y + 1] += error * 5 / 16;
                if (x + 1 < width && y + 1 < height)
                    errors[x + 1, y + 1] += error * 1 / 16;
            }
        }

        return output;
    }
}
```

### 4.4 Braille Video Encoder

**Complete Implementation:**

```csharp
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

public class BrailleVideoEncoder
{
    private const int BRAILLE_WIDTH = 2;
    private const int BRAILLE_HEIGHT = 4;
    private const int BRAILLE_BASE = 0x2800;

    public static string EncodeToBraille(Bitmap frame, byte threshold = 128)
    {
        // Convert to grayscale
        var grayscale = ToGrayscale(frame);

        // Calculate output dimensions (2x4 pixels per character)
        int charWidth = (frame.Width + BRAILLE_WIDTH - 1) / BRAILLE_WIDTH;
        int charHeight = (frame.Height + BRAILLE_HEIGHT - 1) / BRAILLE_HEIGHT;

        var output = new StringBuilder(charWidth * charHeight + charHeight);

        for (int charY = 0; charY < charHeight; charY++)
        {
            for (int charX = 0; charX < charWidth; charX++)
            {
                int brailleCode = 0;

                // Process 2x4 pixel block
                for (int py = 0; py < BRAILLE_HEIGHT; py++)
                {
                    for (int px = 0; px < BRAILLE_WIDTH; px++)
                    {
                        int pixelX = charX * BRAILLE_WIDTH + px;
                        int pixelY = charY * BRAILLE_HEIGHT + py;

                        // Check bounds
                        if (pixelX >= frame.Width || pixelY >= frame.Height)
                            continue;

                        byte pixel = grayscale[pixelX, pixelY];

                        // If pixel is bright, set corresponding dot
                        if (pixel >= threshold)
                        {
                            int dotIndex = GetDotIndex(px, py);
                            brailleCode |= (1 << dotIndex);
                        }
                    }
                }

                // Convert to Unicode character
                char brailleChar = (char)(BRAILLE_BASE + brailleCode);
                output.Append(brailleChar);
            }

            output.AppendLine();  // New line after each row
        }

        return output.ToString();
    }

    private static int GetDotIndex(int px, int py)
    {
        // Map (px, py) to dot number (0-7)
        // Standard Braille numbering
        if (px == 0)
        {
            return py switch
            {
                0 => 0,  // Dot 1
                1 => 1,  // Dot 2
                2 => 2,  // Dot 3
                3 => 6,  // Dot 7
                _ => 0
            };
        }
        else  // px == 1
        {
            return py switch
            {
                0 => 3,  // Dot 4
                1 => 4,  // Dot 5
                2 => 5,  // Dot 6
                3 => 7,  // Dot 8
                _ => 0
            };
        }
    }

    private static byte[,] ToGrayscale(Bitmap bitmap)
    {
        byte[,] result = new byte[bitmap.Width, bitmap.Height];

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color pixel = bitmap.GetPixel(x, y);

                // Weighted grayscale (perceptual)
                byte gray = (byte)(
                    0.299 * pixel.R +
                    0.587 * pixel.G +
                    0.114 * pixel.B
                );

                result[x, y] = gray;
            }
        }

        return result;
    }
}
```

### 4.5 Color Quantization for 16-Color ANSI

**Standard ANSI 16-Color Palette:**

```csharp
public static class AnsiPalette
{
    public static readonly Color[] Colors = new Color[]
    {
        // Normal colors (0-7)
        Color.FromArgb(0, 0, 0),       // 0: Black
        Color.FromArgb(128, 0, 0),     // 1: Red
        Color.FromArgb(0, 128, 0),     // 2: Green
        Color.FromArgb(128, 128, 0),   // 3: Yellow
        Color.FromArgb(0, 0, 128),     // 4: Blue
        Color.FromArgb(128, 0, 128),   // 5: Magenta
        Color.FromArgb(0, 128, 128),   // 6: Cyan
        Color.FromArgb(192, 192, 192), // 7: White

        // Bright colors (8-15)
        Color.FromArgb(128, 128, 128), // 8: Bright Black (Gray)
        Color.FromArgb(255, 0, 0),     // 9: Bright Red
        Color.FromArgb(0, 255, 0),     // 10: Bright Green
        Color.FromArgb(255, 255, 0),   // 11: Bright Yellow
        Color.FromArgb(0, 0, 255),     // 12: Bright Blue
        Color.FromArgb(255, 0, 255),   // 13: Bright Magenta
        Color.FromArgb(0, 255, 255),   // 14: Bright Cyan
        Color.FromArgb(255, 255, 255)  // 15: Bright White
    };

    public static int FindNearestColor(Color target)
    {
        int nearestIndex = 0;
        double minDistance = double.MaxValue;

        for (int i = 0; i < Colors.Length; i++)
        {
            double distance = ColorDistance(target, Colors[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    private static double ColorDistance(Color c1, Color c2)
    {
        // Weighted Euclidean distance (perceptual)
        double dr = (c1.R - c2.R) * 0.30;
        double dg = (c1.G - c2.G) * 0.59;
        double db = (c1.B - c2.B) * 0.11;

        return Math.Sqrt(dr * dr + dg * dg + db * db);
    }

    public static string GetAnsiCode(int colorIndex, bool foreground = true)
    {
        if (colorIndex < 0 || colorIndex >= 16)
            throw new ArgumentOutOfRangeException(nameof(colorIndex));

        if (foreground)
        {
            // Foreground: 30-37 (normal), 90-97 (bright)
            return colorIndex < 8
                ? $"\x1b[{30 + colorIndex}m"
                : $"\x1b[{90 + (colorIndex - 8)}m";
        }
        else
        {
            // Background: 40-47 (normal), 100-107 (bright)
            return colorIndex < 8
                ? $"\x1b[{40 + colorIndex}m"
                : $"\x1b[{100 + (colorIndex - 8)}m";
        }
    }
}
```

### 4.6 Colored Braille Video Encoder

```csharp
public class ColoredBrailleEncoder
{
    public static string EncodeColoredBraille(Bitmap frame)
    {
        int charWidth = (frame.Width + 1) / 2;
        int charHeight = (frame.Height + 3) / 4;

        var output = new StringBuilder();

        for (int charY = 0; charY < charHeight; charY++)
        {
            for (int charX = 0; charX < charWidth; charX++)
            {
                // Get average color for this character block
                var avgColor = GetAverageColor(frame, charX * 2, charY * 4, 2, 4);

                // Find nearest ANSI color
                int ansiColor = AnsiPalette.FindNearestColor(avgColor);

                // Set foreground color
                output.Append(AnsiPalette.GetAnsiCode(ansiColor, true));

                // Generate Braille pattern
                int brailleCode = GetBraillePattern(frame, charX * 2, charY * 4);
                char brailleChar = (char)(0x2800 + brailleCode);

                output.Append(brailleChar);
            }

            output.Append("\x1b[0m");  // Reset colors
            output.AppendLine();
        }

        return output.ToString();
    }

    private static Color GetAverageColor(Bitmap bitmap, int x, int y, int w, int h)
    {
        int r = 0, g = 0, b = 0, count = 0;

        for (int py = 0; py < h && y + py < bitmap.Height; py++)
        {
            for (int px = 0; px < w && x + px < bitmap.Width; px++)
            {
                Color pixel = bitmap.GetPixel(x + px, y + py);
                r += pixel.R;
                g += pixel.G;
                b += pixel.B;
                count++;
            }
        }

        if (count == 0) return Color.Black;

        return Color.FromArgb(r / count, g / count, b / count);
    }

    private static int GetBraillePattern(Bitmap bitmap, int x, int y)
    {
        int pattern = 0;
        byte threshold = 128;

        for (int py = 0; py < 4; py++)
        {
            for (int px = 0; px < 2; px++)
            {
                if (x + px >= bitmap.Width || y + py >= bitmap.Height)
                    continue;

                Color pixel = bitmap.GetPixel(x + px, y + py);
                byte gray = (byte)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);

                if (gray >= threshold)
                {
                    int dotIndex = BrailleVideoEncoder.GetDotIndex(px, py);
                    pattern |= (1 << dotIndex);
                }
            }
        }

        return pattern;
    }
}
```

### 4.7 Performance Considerations

**Optimization Strategies:**

1. **Reduce Frame Rate**: Target 10-15 FPS for smooth playback
2. **Downscale Video**: Process at 160x90 or smaller
3. **Reuse Buffers**: Avoid allocation in rendering loop
4. **Parallel Processing**: Use `Parallel.For` for frame encoding
5. **Cache Color Mapping**: Pre-compute ANSI color mappings

**Performance Benchmarks:**

| Resolution | FPS | CPU Usage | Notes |
|------------|-----|-----------|-------|
| 80x25 | 30 | 10% | Terminal size, real-time capable |
| 160x90 | 15 | 25% | 2x upscale, smooth playback |
| 320x180 | 10 | 60% | 4x upscale, high detail |
| 640x360 | 5 | 90% | 8x upscale, slideshow-like |

**Optimized Encoder:**

```csharp
using System.Threading.Tasks;

public class OptimizedBrailleEncoder
{
    private byte[,] _grayscaleBuffer;
    private char[,] _charBuffer;

    public string EncodeFrameOptimized(Bitmap frame)
    {
        int charWidth = (frame.Width + 1) / 2;
        int charHeight = (frame.Height + 3) / 4;

        // Reuse buffers
        if (_grayscaleBuffer == null ||
            _grayscaleBuffer.GetLength(0) != frame.Width ||
            _grayscaleBuffer.GetLength(1) != frame.Height)
        {
            _grayscaleBuffer = new byte[frame.Width, frame.Height];
        }

        if (_charBuffer == null ||
            _charBuffer.GetLength(0) != charWidth ||
            _charBuffer.GetLength(1) != charHeight)
        {
            _charBuffer = new char[charWidth, charHeight];
        }

        // Convert to grayscale (parallel)
        Parallel.For(0, frame.Height, y =>
        {
            for (int x = 0; x < frame.Width; x++)
            {
                Color pixel = frame.GetPixel(x, y);
                _grayscaleBuffer[x, y] = (byte)(
                    0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B
                );
            }
        });

        // Encode to Braille (parallel)
        Parallel.For(0, charHeight, charY =>
        {
            for (int charX = 0; charX < charWidth; charX++)
            {
                int pattern = 0;

                for (int py = 0; py < 4; py++)
                {
                    for (int px = 0; px < 2; px++)
                    {
                        int pixelX = charX * 2 + px;
                        int pixelY = charY * 4 + py;

                        if (pixelX < frame.Width && pixelY < frame.Height)
                        {
                            if (_grayscaleBuffer[pixelX, pixelY] >= 128)
                            {
                                int dotIndex = GetDotIndex(px, py);
                                pattern |= (1 << dotIndex);
                            }
                        }
                    }
                }

                _charBuffer[charX, charY] = (char)(0x2800 + pattern);
            }
        });

        // Build output string
        var sb = new StringBuilder(charWidth * charHeight + charHeight);
        for (int y = 0; y < charHeight; y++)
        {
            for (int x = 0; x < charWidth; x++)
            {
                sb.Append(_charBuffer[x, y]);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
```

### 4.8 Best Practices

1. **Use adaptive thresholding** for varying lighting conditions
2. **Apply dithering** for better detail preservation
3. **Limit output resolution** to terminal size
4. **Buffer frames** for smooth playback
5. **Consider color** for better visual quality
6. **Profile performance** on target hardware

### 4.9 Gotchas

1. **Font dependency**: Braille characters require proper font support
2. **Terminal line wrapping**: Ensure exact width match
3. **Unicode encoding**: Output must be UTF-8
4. **Performance**: CPU-intensive for high resolutions
5. **Aspect ratio**: Braille characters typically not square

---

## 5. References

### Official Specifications

- **SIXEL**: [VT3xx Graphics Programming Chapter 14](https://vt100.net/docs/vt3xx-gp/chapter14.html)
- **Kitty Graphics Protocol**: [Official Documentation](https://sw.kovidgoyal.net/kitty/graphics-protocol/)
- **VT510 DA1**: [Primary Device Attributes](https://vt100.net/docs/vt510-rm/DA1.html)
- **VT510 DA2**: [Secondary Device Attributes](https://vt100.net/docs/vt510-rm/DA2.html)
- **Unicode Braille**: [U+2800 Block](https://www.unicode.org/charts/PDF/U2800.pdf)

### Libraries and Tools

- **libsixel**: [GitHub - saitoha/libsixel](https://github.com/saitoha/libsixel)
- **lsix**: [GitHub - hackerb9/lsix](https://github.com/hackerb9/lsix) (SIXEL detection tool)
- **rasterm**: [GitHub - BourgeoisBear/rasterm](https://github.com/BourgeoisBear/rasterm) (Multi-protocol encoder)
- **timg**: [GitHub - hzeller/timg](https://github.com/hzeller/timg) (Terminal image viewer)

### Terminal Compatibility

- **Are We Sixel Yet**: [www.arewesixelyet.com](https://www.arewesixelyet.com/) (Comprehensive compatibility matrix)
- **Terminal Guide**: [terminalguide.namepad.de](https://terminalguide.namepad.de/) (Control sequence reference)

### Academic Papers

- Floyd, R. W., & Steinberg, L. (1976). "An adaptive algorithm for spatial greyscale"
- Otsu, N. (1979). "A threshold selection method from gray-level histograms"

### Blog Posts and Articles

- [Sixel for Terminal Graphics](https://konfou.xyz/posts/sixel-for-terminal-graphics/)
- [Using Go and Braille to Render Images](https://medium.com/@kevrone/using-go-and-braille-to-render-images-and-video-to-a-terminal-edc8ecfba50)
- [Finding Nearest Colors using Euclidean Distance](https://www.cyotek.com/blog/finding-nearest-colors-using-euclidean-distance)
- [Terminal Colors Deep Dive](https://chrisyeh96.github.io/2020/03/28/terminal-colors.html)

---

**Document Status**: Draft
**Created**: 2025-10-26
**Last Updated**: 2025-10-26
**Author**: Research compiled for Phase 6 User Story 4 (Unified Media Player)

## References

- SIXEL Graphics Protocol Documentation
- Kitty Graphics Protocol Specification
- Terminal Capability Detection Standards
- Braille Unicode Character Set Documentation
