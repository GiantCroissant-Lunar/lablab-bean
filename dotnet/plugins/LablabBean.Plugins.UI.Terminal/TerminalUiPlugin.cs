using LablabBean.Plugins.Contracts;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Reflection;
using TGui = global::Terminal.Gui;

namespace LablabBean.Plugins.UI.Terminal;

/// <summary>
/// Terminal UI plugin that hosts Terminal.Gui and provides a minimal UI shell.
/// Loads last by depending on core gameplay and diagnostics plugins.
/// </summary>
public class TerminalUiPlugin : IPlugin
{
    private ILogger? _logger;
    private bool _initialized;
    private bool _running;

    public string Id => "ui-terminal";
    public string Name => "Terminal UI";
    public string Version => "1.0.0";

    public Task InitializeAsync(IPluginContext context, CancellationToken ct = default)
    {
        _logger = context.Logger;
        _logger.LogInformation("Initializing Terminal UI plugin");
        _initialized = true;
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        if (!_initialized)
            throw new InvalidOperationException("UI plugin not initialized");

        _logger?.LogInformation("Starting Terminal.Gui from UI plugin");

        try
        {
            // Skip UI when running CLI plugin commands to avoid blocking
            var args = Environment.GetCommandLineArgs();
            if (args.Any(a => string.Equals(a, "plugins", StringComparison.OrdinalIgnoreCase)))
            {
                _logger?.LogInformation("Detected 'plugins' CLI mode; skipping UI startup.");
                return Task.CompletedTask;
            }

            // ConfigurationManager.Enabled removed in newer Terminal.Gui versions

            TGui.Application.Init();

            var window = new TGui.Window
            {
                Title = "LablabBean - UI Plugin",
                X = 0,
                Y = 0,
                Width = TGui.Dim.Fill(),
                Height = TGui.Dim.Fill(),
                BorderStyle = TGui.LineStyle.Single
            };

            var label = new TGui.Label
            {
                Text = "TUI initialized via plugin. Press Q to quit.",
                X = TGui.Pos.Center(),
                Y = TGui.Pos.Center()
            };
            window.Add(label);

            // Handle quit
            window.KeyDown += (s, e) =>
            {
                if (e?.KeyCode == TGui.KeyCode.Q)
                {
                    TGui.Application.RequestStop();
                    e.Handled = true;
                }
            };

            if (TGui.Application.Top != null)
            {
                TGui.Application.Top.Add(window);
            }

            _running = true;
            TGui.Application.Run(window);
            _running = false;
        }
        catch (ReflectionTypeLoadException)
        {
            _logger?.LogWarning("Terminal.Gui initialization failed due to type loading issues. UI plugin will not start.");
        }
        catch (TypeLoadException)
        {
            _logger?.LogWarning("Terminal.Gui initialization failed due to missing types. UI plugin will not start.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Terminal.Gui failed to start");
        }
        finally
        {
            try { TGui.Application.Shutdown(); } catch { }
            _logger?.LogInformation("Terminal UI plugin stopped");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        if (_running)
        {
            try { TGui.Application.RequestStop(); } catch { }
        }
        return Task.CompletedTask;
    }
}
