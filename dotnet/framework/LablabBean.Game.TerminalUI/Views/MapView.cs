using System.Text;
using Terminal.Gui;

namespace LablabBean.Game.TerminalUI.Views;

/// <summary>
/// Custom view for rendering the dungeon map
/// </summary>
public class MapView : View
{
    private char[,]? _buffer;
    private int _bufferWidth;
    private int _bufferHeight;

    public MapView()
    {
        CanFocus = false;
    }

    /// <summary>
    /// Updates the buffer with new content
    /// </summary>
    public void UpdateBuffer(char[,] buffer)
    {
        _buffer = buffer;
        _bufferHeight = buffer.GetLength(0);
        _bufferWidth = buffer.GetLength(1);
        SetNeedsDisplay();
    }

    /// <summary>
    /// Renders the buffer to screen - called by Terminal.Gui rendering system
    /// </summary>
    public override void OnDrawContent(Rect viewport)
    {
        base.OnDrawContent(viewport);
        
        if (_buffer == null)
            return;

        // Draw character by character using AddRune
        for (int row = 0; row < _bufferHeight && row < viewport.Height; row++)
        {
            for (int col = 0; col < _bufferWidth && col < viewport.Width; col++)
            {
                char ch = _buffer[row, col];
                AddRune(col, row, new Rune(ch));
            }
        }
    }
}
