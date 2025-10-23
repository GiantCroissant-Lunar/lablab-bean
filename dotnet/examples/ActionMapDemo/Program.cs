using LablabBean.Contracts.Input;
using LablabBean.Contracts.Input.ActionMap;
using LablabBean.Plugins.Contracts;
using LablabBean.Plugins.InputActionMap;
using Microsoft.Extensions.Logging;

namespace ActionMapDemo;

/// <summary>
/// Demonstration of Unity-inspired Action Map input system.
/// Shows how to create action maps, register callbacks, and handle input.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== LablabBean Action Map Demo ===\n");

        // Create mock services
        var eventBus = new MockEventBus();
        var logger = new MockLogger();

        // Create action map service
        var actionMapService = new ActionMapService(eventBus, logger);

        // Demo 1: Load from Unity-compatible JSON file
        await DemoJsonLoading(actionMapService);

        // Demo 2: Create programmatically and save to JSON
        await DemoProgrammaticCreation(actionMapService);

        // Register action callbacks
        RegisterCallbacks(actionMapService);

        // Simulate input events
        await SimulateGameplay(actionMapService);

        // Run Unity compatibility test
        await UnityCompatibilityTest.RunCompatibilityTest();

        Console.WriteLine("\nDemo completed. Press any key to exit...");
        Console.ReadKey();
    }

    static async Task DemoJsonLoading(ActionMapService actionMapService)
    {
        Console.WriteLine("--- Demo 1: Loading from Unity JSON ---");

        try
        {
            // Load from Unity-compatible JSON file
            await actionMapService.LoadAssetFromJsonAsync("GameInput.inputactions");
            Console.WriteLine("✓ Successfully loaded input asset from JSON file");

            // Switch to keyboard & mouse control scheme
            actionMapService.SwitchControlScheme("Keyboard & Mouse");
            Console.WriteLine("✓ Switched to Keyboard & Mouse control scheme\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to load JSON: {ex.Message}");

            // Fallback to programmatic creation
            Console.WriteLine("Falling back to programmatic creation...");
            var inputAsset = CreateGameInputAsset();
            await actionMapService.LoadAssetAsync(inputAsset);
            actionMapService.SwitchControlScheme("Keyboard & Mouse");
        }
    }

    static async Task DemoProgrammaticCreation(ActionMapService actionMapService)
    {
        Console.WriteLine("--- Demo 2: Programmatic Creation & JSON Export ---");

        // Create input asset programmatically
        var inputAsset = CreateGameInputAsset();

        // Save to JSON file
        try
        {
            await actionMapService.LoadAssetAsync(inputAsset);
            await actionMapService.SaveAssetToJsonAsync("GeneratedInput.inputactions");
            Console.WriteLine("✓ Created input asset programmatically");
            Console.WriteLine("✓ Saved to GeneratedInput.inputactions");

            // Show JSON content
            if (File.Exists("GeneratedInput.inputactions"))
            {
                var jsonContent = await File.ReadAllTextAsync("GeneratedInput.inputactions");
                Console.WriteLine("\n📄 Generated JSON (first 300 chars):");
                Console.WriteLine(jsonContent.Length > 300 ? jsonContent[..300] + "..." : jsonContent);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to save JSON: {ex.Message}");
        }

        Console.WriteLine();
    }

    static InputAsset CreateGameInputAsset()
    {
        Console.WriteLine("Creating input asset with action maps...");

        // Player action map
        var playerActions = new[]
        {
            // Movement composite (WASD)
            InputActionDefinition.Composite("Move",
                InputBinding.CompositeAxis("w", "up"),
                InputBinding.CompositeAxis("s", "down"),
                InputBinding.CompositeAxis("a", "left"),
                InputBinding.CompositeAxis("d", "right")
            ),

            // Simple actions
            InputActionDefinition.Button("Jump", "space"),
            InputActionDefinition.Button("Attack", "leftButton"),
            InputActionDefinition.Button("Block", "rightButton"),
            InputActionDefinition.Button("Interact", "e"),
            InputActionDefinition.Button("Sprint", "leftShift"),
        };

        // UI action map
        var uiActions = new[]
        {
            InputActionDefinition.Button("Submit", "enter"),
            InputActionDefinition.Button("Cancel", "escape"),
            InputActionDefinition.Button("Navigate", "tab"),
            InputActionDefinition.Button("OpenInventory", "i"),
            InputActionDefinition.Button("Pause", "escape"),
        };

        // Menu action map
        var menuActions = new[]
        {
            InputActionDefinition.Button("Select", "enter"),
            InputActionDefinition.Button("Back", "escape"),
            InputActionDefinition.Button("Up", "w"),
            InputActionDefinition.Button("Down", "s"),
        };

        // Create action maps
        var actionMaps = new[]
        {
            InputActionMap.Create("Player", playerActions, isEnabled: true),
            InputActionMap.Create("UI", uiActions, isEnabled: false),
            InputActionMap.Create("Menu", menuActions, isEnabled: false),
        };

        // Create control schemes
        var controlSchemes = new[]
        {
            ControlScheme.KeyboardMouse("Keyboard & Mouse"),
            ControlScheme.Keyboard("Keyboard Only"),
        };

        var asset = InputAsset.Create("GameInput", actionMaps, controlSchemes);
        Console.WriteLine($"✓ Created input asset with {actionMaps.Length} action maps\n");

        return asset;
    }

    static void RegisterCallbacks(IService actionMapService)
    {
        Console.WriteLine("Registering action callbacks...");

        // Player movement
        actionMapService.RegisterActionCallback("Move", context =>
        {
            Console.WriteLine($"🏃 Move: {context.RawInput.Key} ({context.Phase})");
        });

        // Player actions
        actionMapService.RegisterActionCallback("Jump", context =>
        {
            if (context.Phase == InputActionPhase.Performed)
                Console.WriteLine("🦘 Player jumped!");
        });

        actionMapService.RegisterActionCallback("Attack", context =>
        {
            if (context.Phase == InputActionPhase.Performed)
                Console.WriteLine("⚔️ Player attacked!");
        });

        actionMapService.RegisterActionCallback("Interact", context =>
        {
            if (context.Phase == InputActionPhase.Performed)
                Console.WriteLine("🤝 Player interacted!");
        });

        // UI actions
        actionMapService.RegisterActionCallback("UI", "Pause", context =>
        {
            if (context.Phase == InputActionPhase.Performed)
            {
                Console.WriteLine("⏸️ Game paused! Switching to UI mode...");
                actionMapService.DisableActionMap("Player");
                actionMapService.EnableActionMap("UI");
            }
        });

        actionMapService.RegisterActionCallback("UI", "Cancel", context =>
        {
            if (context.Phase == InputActionPhase.Performed)
            {
                Console.WriteLine("▶️ Resuming game! Switching to Player mode...");
                actionMapService.DisableActionMap("UI");
                actionMapService.EnableActionMap("Player");
            }
        });

        // Menu actions
        actionMapService.RegisterActionCallback("Menu", "Select", context =>
        {
            if (context.Phase == InputActionPhase.Performed)
                Console.WriteLine("📋 Menu item selected!");
        });

        Console.WriteLine("✓ Action callbacks registered\n");
    }

    static async Task SimulateGameplay(ActionMapService actionMapService)
    {
        Console.WriteLine("Simulating gameplay input...\n");

        // Simulate player movement
        Console.WriteLine("--- Player Movement ---");
        actionMapService.ProcessInput(new RawKeyEvent("w", ""), InputActionPhase.Performed);
        await Task.Delay(100);
        actionMapService.ProcessInput(new RawKeyEvent("a", ""), InputActionPhase.Performed);
        await Task.Delay(100);

        // Simulate player actions
        Console.WriteLine("\n--- Player Actions ---");
        actionMapService.ProcessInput(new RawKeyEvent("space", ""), InputActionPhase.Performed);
        await Task.Delay(100);
        actionMapService.ProcessInput(new RawKeyEvent("leftButton", ""), InputActionPhase.Performed);
        await Task.Delay(100);
        actionMapService.ProcessInput(new RawKeyEvent("e", ""), InputActionPhase.Performed);
        await Task.Delay(100);

        // Simulate pause (switch to UI mode)
        Console.WriteLine("\n--- Pause Game ---");
        actionMapService.ProcessInput(new RawKeyEvent("escape", ""), InputActionPhase.Performed);
        await Task.Delay(500);

        // Try player action while in UI mode (should not trigger)
        Console.WriteLine("\n--- Try Player Action in UI Mode ---");
        Console.WriteLine("(This should not trigger player actions)");
        actionMapService.ProcessInput(new RawKeyEvent("space", ""), InputActionPhase.Performed);
        await Task.Delay(100);

        // Resume game
        Console.WriteLine("\n--- Resume Game ---");
        actionMapService.ProcessInput(new RawKeyEvent("escape", ""), InputActionPhase.Performed);
        await Task.Delay(500);

        // Test player actions again
        Console.WriteLine("\n--- Player Actions After Resume ---");
        actionMapService.ProcessInput(new RawKeyEvent("space", ""), InputActionPhase.Performed);
        await Task.Delay(100);

        // Switch to menu mode
        Console.WriteLine("\n--- Switch to Menu ---");
        actionMapService.DisableActionMap("Player");
        actionMapService.EnableActionMap("Menu");
        actionMapService.ProcessInput(new RawKeyEvent("enter", ""), InputActionPhase.Performed);
        await Task.Delay(100);
    }
}

// Mock implementations for demo
public class MockEventBus : IEventBus
{
    public Task PublishAsync<T>(T eventData) where T : class
    {
        // Console.WriteLine($"Event: {typeof(T).Name}");
        return Task.CompletedTask;
    }

    public void Subscribe<T>(Func<T, Task> handler) where T : class { }
}

public class MockLogger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
        {
            var message = formatter(state, exception);
            var prefix = logLevel switch
            {
                LogLevel.Information => "ℹ️",
                LogLevel.Warning => "⚠️",
                LogLevel.Error => "❌",
                LogLevel.Debug => "🔍",
                _ => "📝"
            };
            Console.WriteLine($"{prefix} {message}");
        }
    }
}
