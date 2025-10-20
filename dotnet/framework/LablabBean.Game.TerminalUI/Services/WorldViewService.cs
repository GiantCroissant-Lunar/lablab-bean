using System.Text;
using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using LablabBean.Game.Core.Maps;
using Microsoft.Extensions.Logging;
using Terminal.Gui;
using TGuiColor = Terminal.Gui.Color;
using TGuiAttribute = Terminal.Gui.Attribute;
using SadColor = SadRogue.Primitives.Color;
using SadPoint = SadRogue.Primitives.Point;

namespace LablabBean.Game.TerminalUI.Services;

/// <summary>
/// Service for rendering the game world/scene in Terminal.Gui
/// Displays the dungeon map, entities, and handles the game viewport
/// </summary>
public class WorldViewService
{
    private readonly ILogger<WorldViewService> _logger;
    private readonly FrameView _worldFrame;
    private readonly View _renderView;
    private int _viewWidth;
    private int _viewHeight;

    public View WorldView => _worldFrame;

    public WorldViewService(ILogger<WorldViewService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Create the main world frame (left side)
        _worldFrame = new FrameView("Dungeon")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(30), // Leave space for HUD
            Height = Dim.Fill()
        };

        // Create a view for rendering
        _renderView = new View
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            CanFocus = false
        };

        _worldFrame.Add(_renderView);

        // Subscribe to layout changes to update view dimensions
        _renderView.LayoutComplete += (sender, args) =>
        {
            _viewWidth = _renderView.Bounds.Width;
            _viewHeight = _renderView.Bounds.Height;
        };
    }

    /// <summary>
    /// Renders the game world
    /// </summary>
    public void Render(World world, DungeonMap map)
    {
        if (_viewWidth <= 0 || _viewHeight <= 0)
            return;

        // Get player position for camera centering
        var playerPos = GetPlayerPosition(world);
        if (playerPos == null)
            return;

        // Calculate camera offset to center on player
        int cameraX = playerPos.Value.X - _viewWidth / 2;
        int cameraY = playerPos.Value.Y - _viewHeight / 2;

        // Clamp camera to map bounds
        cameraX = Math.Max(0, Math.Min(cameraX, map.Width - _viewWidth));
        cameraY = Math.Max(0, Math.Min(cameraY, map.Height - _viewHeight));

        // Clear the render view
        _renderView.Clear();

        // Render map tiles
        RenderMap(_renderView, map, cameraX, cameraY);

        // Render entities
        RenderEntities(_renderView, world, map, cameraX, cameraY);

        // Request redraw using Terminal.Gui v2 pattern
        _renderView.SetNeedsDisplay();
    }

    /// <summary>
    /// Renders the map tiles
    /// </summary>
    private void RenderMap(View view, DungeonMap map, int cameraX, int cameraY)
    {
        for (int y = 0; y < _viewHeight && y + cameraY < map.Height; y++)
        {
            for (int x = 0; x < _viewWidth && x + cameraX < map.Width; x++)
            {
                var worldPos = new SadPoint(x + cameraX, y + cameraY);

                char glyph;
                TGuiAttribute attr;

                // Check if position is in FOV (currently visible)
                if (map.IsInFOV(worldPos))
                {
                    // Currently visible tiles - bright
                    if (map.IsWalkable(worldPos))
                    {
                        glyph = '.';
                        attr = new TGuiAttribute(TGuiColor.Gray, TGuiColor.Black);
                    }
                    else
                    {
                        glyph = '#';
                        attr = new TGuiAttribute(TGuiColor.White, TGuiColor.Black);
                    }
                }
                else if (map.FogOfWar.IsExplored(worldPos))
                {
                    // Explored but not currently visible - darker
                    if (map.IsWalkable(worldPos))
                    {
                        glyph = '.';
                        attr = new TGuiAttribute(TGuiColor.DarkGray, TGuiColor.Black);
                    }
                    else
                    {
                        glyph = '#';
                        attr = new TGuiAttribute(TGuiColor.DarkGray, TGuiColor.Black);
                    }
                }
                else
                {
                    // Not explored - completely dark
                    glyph = ' ';
                    attr = new TGuiAttribute(TGuiColor.Black, TGuiColor.Black);
                }

                // Draw the tile using Terminal.Gui v2 pattern
                view.AddRune(x, y, new Rune(glyph));
            }
        }
    }

    /// <summary>
    /// Renders all visible entities
    /// </summary>
    private void RenderEntities(View view, World world, DungeonMap map, int cameraX, int cameraY)
    {
        var query = new QueryDescription().WithAll<Position, Renderable, Visible>();

        // Collect entities to render, sorted by Z-order
        var entitiesToRender = new List<(int x, int y, char glyph, SadColor foreground, SadColor background, int zOrder)>();

        world.Query(in query, (Entity entity, ref Position pos, ref Renderable renderable, ref Visible visible) =>
        {
            if (!visible.IsVisible)
                return;

            // Check if entity is in FOV
            if (!map.IsInFOV(pos.Point))
                return;

            // Convert world position to screen position
            int screenX = pos.X - cameraX;
            int screenY = pos.Y - cameraY;

            // Check if on screen
            if (screenX >= 0 && screenX < _viewWidth && screenY >= 0 && screenY < _viewHeight)
            {
                entitiesToRender.Add((
                    screenX,
                    screenY,
                    renderable.Glyph,
                    renderable.Foreground,
                    renderable.Background,
                    renderable.ZOrder
                ));
            }
        });

        // Sort by Z-order (lower values drawn first)
        entitiesToRender.Sort((a, b) => a.zOrder.CompareTo(b.zOrder));

        // Render entities using Terminal.Gui v2 pattern
        foreach (var (x, y, glyph, foreground, background, _) in entitiesToRender)
        {
            var tguiForeground = ConvertColor(foreground);
            var tguiBackground = ConvertColor(background);
            var attr = new TGuiAttribute(tguiForeground, tguiBackground);

            view.AddRune(x, y, new Rune(glyph));
        }
    }

    /// <summary>
    /// Converts SadRogue color to Terminal.Gui color
    /// </summary>
    private TGuiColor ConvertColor(SadColor color)
    {
        // Map to closest Terminal.Gui color
        // This is a simplified mapping - you might want to enhance this

        if (color == SadColor.White)
            return TGuiColor.White;
        if (color == SadColor.Black)
            return TGuiColor.Black;
        if (color == SadColor.Red)
            return TGuiColor.Red;
        if (color == SadColor.Green)
            return TGuiColor.Green;
        if (color == SadColor.Blue)
            return TGuiColor.Blue;
        if (color == SadColor.Yellow)
            return TGuiColor.BrightYellow;
        if (color == SadColor.Cyan)
            return TGuiColor.Cyan;
        if (color == SadColor.Magenta)
            return TGuiColor.Magenta;
        if (color == SadColor.Gray)
            return TGuiColor.Gray;
        if (color == SadColor.DarkGray)
            return TGuiColor.DarkGray;
        if (color == SadColor.Brown)
            return TGuiColor.Brown;

        // Default to gray
        return TGuiColor.Gray;
    }

    /// <summary>
    /// Gets the player's position
    /// </summary>
    private SadPoint? GetPlayerPosition(World world)
    {
        var query = new QueryDescription().WithAll<Player, Position>();

        SadPoint? result = null;

        world.Query(in query, (Entity entity, ref Player player, ref Position pos) =>
        {
            result = pos.Point;
        });

        return result;
    }
}
