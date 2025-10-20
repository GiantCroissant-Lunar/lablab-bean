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
    private readonly ListView _messageList;
    private readonly List<string> _messages;

    public View HudView => _hudFrame;

    public HudService(ILogger<HudService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messages = new List<string>();

        // Create the main HUD frame (on the right side)
        _hudFrame = new FrameView("HUD")
        {
            X = Pos.AnchorEnd(30),
            Y = 0,
            Width = 30,
            Height = Dim.Fill()
        };

        // Health display
        _healthLabel = new Label
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(1),
            Height = 3,
            Text = "Health: --/--"
        };

        // Stats display
        _statsLabel = new Label
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill(1),
            Height = 5,
            Text = "Stats:\n  ATK: --\n  DEF: --\n  SPD: --"
        };

        // Message log
        var messageFrame = new FrameView("Messages")
        {
            X = 1,
            Y = 12,
            Width = Dim.Fill(1),
            Height = Dim.Fill(1)
        };

        _messageList = new ListView(_messages)
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        messageFrame.Add(_messageList);

        _hudFrame.Add(_healthLabel, _statsLabel, messageFrame);
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

    /// <summary>
    /// Adds a message to the message log
    /// </summary>
    public void AddMessage(string message)
    {
        _messages.Add(message);

        // Keep only last 100 messages
        if (_messages.Count > 100)
        {
            _messages.RemoveAt(0);
        }

        // Scroll to bottom
        _messageList.SelectedItem = _messages.Count - 1;

        _logger.LogDebug("Message added: {Message}", message);
    }

    /// <summary>
    /// Clears all messages
    /// </summary>
    public void ClearMessages()
    {
        _messages.Clear();
        _messageList.SetNeedsDisplay();
    }
}
