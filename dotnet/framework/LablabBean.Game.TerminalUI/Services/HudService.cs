using LablabBean.Game.Core.Components;
using Microsoft.Extensions.Logging;
using Terminal.Gui;
using Arch.Core;
using Arch.Core.Extensions;

namespace LablabBean.Game.TerminalUI.Services;

/// <summary>
/// Service for managing the HUD (Heads-Up Display) in Terminal.Gui
/// Displays player stats, inventory, messages, etc.
/// </summary>
public class HudService
{
    private readonly ILogger<HudService> _logger;
    private readonly FrameView _hudFrame;
    private readonly Label _healthLabel;
    private readonly Label _statsLabel;

    public View HudView => _hudFrame;

    public HudService(ILogger<HudService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Create the main HUD frame (on the right side)
        _hudFrame = new FrameView("HUD")
        {
            X = Pos.AnchorEnd(30),
            Y = 0,
            Width = 30,
            Height = Dim.Fill(),
            CanFocus = false  // HUD should not steal focus from game
        };

        // Health display
        _healthLabel = new Label
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),  // Leave margin for frame border
            Height = 3,
            Text = "Health: --/--"
        };

        // Stats display
        _statsLabel = new Label
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill(2),  // Leave margin for frame border
            Height = Dim.Fill(2),  // Fill remaining space
            Text = "Stats:\n  ATK: --\n  DEF: --\n  SPD: --"
        };

        _hudFrame.Add(_healthLabel, _statsLabel);
    }

    /// <summary>
    /// Updates the HUD with current player information
    /// </summary>
    public void Update(World world)
    {
        var query = new QueryDescription().WithAll<Player, Health, Combat, Actor>();

        world.Query(in query, (Entity entity, ref Player player, ref Health health, ref Combat combat, ref Actor actor) =>
        {
            UpdatePlayerStats(player.Name, health, combat, actor);
        });
    }

    /// <summary>
    /// Updates player stats display
    /// </summary>
    private void UpdatePlayerStats(string playerName, Health health, Combat combat, Actor actor)
    {
        // Update health
        _healthLabel.Text = $"Health: {health.Current}/{health.Maximum}\n" +
                           $"HP%: {health.Percentage:P0}\n" +
                           $"{GetHealthBar(health.Percentage)}";

        // Update stats
        _statsLabel.Text = $"Stats:\n" +
                          $"  ATK: {combat.Attack}\n" +
                          $"  DEF: {combat.Defense}\n" +
                          $"  SPD: {actor.Speed}\n" +
                          $"  NRG: {actor.Energy}";
    }

    /// <summary>
    /// Creates a visual health bar
    /// </summary>
    private string GetHealthBar(float percentage)
    {
        int barLength = 20;
        int filled = (int)(barLength * percentage);
        int empty = barLength - filled;

        return "[" + new string('=', filled) + new string(' ', empty) + "]";
    }
}
