using Arch.Core;
using Arch.Core.Extensions;
using LablabBean.Game.Core.Components;
using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;

namespace LablabBean.Game.SadConsole.Renderers;

/// <summary>
/// Renders the HUD using SadConsole
/// </summary>
public class HudRenderer
{
    private readonly ControlsConsole _console;
    private readonly Label _healthLabel;
    private readonly Label _statsLabel;
    private readonly ListBox _messageList;
    private readonly List<string> _messages;

    public ControlsConsole Console => _console;

    public HudRenderer(int width, int height)
    {
        _console = new ControlsConsole(width, height);
        _messages = new List<string>();

        // Health label
        _healthLabel = new Label(width - 2)
        {
            Position = new Point(1, 1),
            Text = "Health: --/--"
        };

        // Stats label
        _statsLabel = new Label(width - 2)
        {
            Position = new Point(1, 4),
            Text = "Stats:\n  ATK: --\n  DEF: --\n  SPD: --"
        };

        // Message list
        _messageList = new ListBox(width - 2, height - 12)
        {
            Position = new Point(1, 10)
        };

        _console.Controls.Add(_healthLabel);
        _console.Controls.Add(_statsLabel);
        _console.Controls.Add(_messageList);

        // Draw border
        _console.Surface.DrawBox(new Rectangle(0, 0, width, height),
            ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin,
            new ColoredGlyph(Color.White, Color.Black)));
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
        _healthLabel.DisplayText = $"Health: {health.Current}/{health.Maximum}\n" +
                                   $"HP%: {health.Percentage:P0}";

        _statsLabel.DisplayText = $"Stats:\n" +
                                  $"  ATK: {combat.Attack}\n" +
                                  $"  DEF: {combat.Defense}\n" +
                                  $"  SPD: {actor.Speed}\n" +
                                  $"  NRG: {actor.Energy}";
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

        // Update list box
        _messageList.Items.Clear();
        foreach (var msg in _messages)
        {
            _messageList.Items.Add(msg);
        }

        // Scroll to bottom
        if (_messageList.Items.Count > 0)
        {
            _messageList.SelectedIndex = _messageList.Items.Count - 1;
        }
    }

    /// <summary>
    /// Clears all messages
    /// </summary>
    public void ClearMessages()
    {
        _messages.Clear();
        _messageList.Items.Clear();
    }
}
