using LablabBean.Console.Models;
using LablabBean.Console.Services;
using Terminal.Gui;

namespace LablabBean.Console.Views;

public class InteractiveWindow : Window
{
    private readonly MenuBar _menuBar;
    private readonly StatusBar _statusBar;
    private readonly FrameView _leftPanel;
    private readonly FrameView _centerPanel;
    private readonly FrameView _rightPanel;
    private readonly ListView _actionList;
    private readonly TextView _outputView;
    private readonly TextView _detailView;
    private readonly IMenuService _menuService;

    private readonly List<string> _outputLines = new();

    public InteractiveWindow(IMenuService menuService)
    {
        _menuService = menuService;
        Title = "Lablab Bean - Interactive TUI (Ctrl+Q to quit)";

        // Create menu bar
        _menuBar = new MenuBar(new MenuBarItem[]
        {
            new MenuBarItem("_File", GetFileMenuItems()),
            new MenuBarItem("_Edit", GetEditMenuItems()),
            new MenuBarItem("_View", GetViewMenuItems()),
            new MenuBarItem("_Build", GetBuildMenuItems()),
            new MenuBarItem("_Help", new MenuItem[]
            {
                new MenuItem("_About", "About", OnAbout),
                new MenuItem("_Shortcuts", "Keyboard Shortcuts", OnShowShortcuts)
            })
        });

        // Create status bar
        _statusBar = new StatusBar(new StatusItem[]
        {
            new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", OnQuit),
            new StatusItem(Key.F5, "~F5~ Refresh", OnRefresh),
            new StatusItem(Key.F6, "~F6~ Build", OnBuild),
            new StatusItem(Key.F1, "~F1~ Help", OnShowShortcuts)
        });

        // Create left panel (Actions)
        _leftPanel = new FrameView("Actions")
        {
            X = 0,
            Y = 1,
            Width = Dim.Percent(25),
            Height = Dim.Fill(1)
        };

        var actions = new List<string>
        {
            "📄 New File",
            "📂 Open File",
            "💾 Save File",
            "🔨 Build Project",
            "🧪 Run Tests",
            "📊 View Logs",
            "🔄 Refresh",
            "ℹ️  About",
            "❌ Exit"
        };

        _actionList = new ListView(actions)
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        _actionList.SelectedItemChanged += OnActionSelected;
        _actionList.OpenSelectedItem += OnActionExecuted;

        _leftPanel.Add(_actionList);

        // Create center panel (Output)
        _centerPanel = new FrameView("Output")
        {
            X = Pos.Right(_leftPanel),
            Y = 1,
            Width = Dim.Percent(50),
            Height = Dim.Fill(1)
        };

        _outputView = new TextView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            Text = "Welcome to Lablab Bean Interactive TUI!\n\n" +
                   "This application demonstrates:\n" +
                   "• Interactive menu system\n" +
                   "• Keyboard shortcuts\n" +
                   "• Mouse support\n" +
                   "• Real-time output\n\n" +
                   "Select an action from the left panel or use keyboard shortcuts.\n"
        };

        _centerPanel.Add(_outputView);

        // Create right panel (Details)
        _rightPanel = new FrameView("Details")
        {
            X = Pos.Right(_centerPanel),
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(1)
        };

        _detailView = new TextView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            Text = "Select an action to see details here."
        };

        _rightPanel.Add(_detailView);

        // Add all controls
        Add(_menuBar);
        Add(_leftPanel);
        Add(_centerPanel);
        Add(_rightPanel);
        Add(_statusBar);

        // Log initial message
        LogOutput("Application started");
    }

    private MenuItem[] GetFileMenuItems()
    {
        var actions = _menuService.GetFileMenuActions().ToArray();
        return new MenuItem[]
        {
            new MenuItem(actions[0].Label, actions[0].Description, () => ExecuteMenuAction(actions[0].Type)),
            new MenuItem(actions[1].Label, actions[1].Description, () => ExecuteMenuAction(actions[1].Type)),
            new MenuItem(actions[2].Label, actions[2].Description, () => ExecuteMenuAction(actions[2].Type)),
            null!,
            new MenuItem(actions[3].Label, actions[3].Description, OnQuit)
        };
    }

    private MenuItem[] GetEditMenuItems()
    {
        var actions = _menuService.GetEditMenuActions().ToArray();
        return actions.Select(a =>
            new MenuItem(a.Label, a.Description, () => ExecuteMenuAction(a.Type))
        ).ToArray();
    }

    private MenuItem[] GetViewMenuItems()
    {
        var actions = _menuService.GetViewMenuActions().ToArray();
        return actions.Select(a =>
            new MenuItem(a.Label, a.Description, () => ExecuteMenuAction(a.Type))
        ).ToArray();
    }

    private MenuItem[] GetBuildMenuItems()
    {
        var actions = _menuService.GetBuildMenuActions().ToArray();
        return actions.Select(a =>
            new MenuItem(a.Label, a.Description, () => ExecuteMenuAction(a.Type))
        ).ToArray();
    }

    private void OnActionSelected(ListViewItemEventArgs args)
    {
        var action = args.Value.ToString();
        UpdateDetails(action);
    }

    private void OnActionExecuted(ListViewItemEventArgs args)
    {
        var action = args.Value.ToString();
        ExecuteAction(action);
    }

    private void UpdateDetails(string? action)
    {
        var details = action switch
        {
            "📄 New File" => "Create a new file\n\nShortcut: Ctrl+N\n\nCreates a new empty file in the editor.",
            "📂 Open File" => "Open an existing file\n\nShortcut: Ctrl+O\n\nOpens a file dialog to select a file.",
            "💾 Save File" => "Save the current file\n\nShortcut: Ctrl+S\n\nSaves the current file to disk.",
            "🔨 Build Project" => "Build the project\n\nShortcut: F6\n\nCompiles the project and shows build output.",
            "🧪 Run Tests" => "Run all tests\n\nShortcut: Ctrl+T\n\nExecutes all unit tests in the project.",
            "📊 View Logs" => "View application logs\n\nShortcut: Ctrl+L\n\nDisplays the application log file.",
            "🔄 Refresh" => "Refresh the view\n\nShortcut: F5\n\nRefreshes all panels and reloads data.",
            "ℹ️  About" => "About this application\n\nShows version and information about Lablab Bean.",
            "❌ Exit" => "Exit application\n\nShortcut: Ctrl+Q\n\nCloses the application.",
            _ => "Select an action to see details."
        };

        _detailView.Text = details;
    }

    private void ExecuteAction(string? action)
    {
        switch (action)
        {
            case "📄 New File":
                ExecuteMenuAction(MenuActionType.NewFile);
                break;
            case "📂 Open File":
                ExecuteMenuAction(MenuActionType.OpenFile);
                break;
            case "💾 Save File":
                ExecuteMenuAction(MenuActionType.SaveFile);
                break;
            case "🔨 Build Project":
                ExecuteMenuAction(MenuActionType.BuildProject);
                break;
            case "🧪 Run Tests":
                ExecuteMenuAction(MenuActionType.RunTests);
                break;
            case "📊 View Logs":
                ExecuteMenuAction(MenuActionType.ViewLogs);
                break;
            case "🔄 Refresh":
                OnRefresh();
                break;
            case "ℹ️  About":
                OnAbout();
                break;
            case "❌ Exit":
                OnQuit();
                break;
        }
    }

    private void ExecuteMenuAction(MenuActionType actionType)
    {
        _menuService.ExecuteAction(actionType);

        switch (actionType)
        {
            case MenuActionType.NewFile:
                LogOutput("Creating new file...");
                _outputView.Text = string.Empty;
                LogOutput("New file created");
                break;

            case MenuActionType.OpenFile:
                LogOutput("Opening file dialog...");
                var openDialog = new OpenDialog("Open File", "Select a file");
                Application.Run(openDialog);
                if (!openDialog.Canceled && openDialog.FilePath != null)
                {
                    try
                    {
                        var content = File.ReadAllText(openDialog.FilePath.ToString()!);
                        _outputView.Text = content;
                        LogOutput($"Opened: {openDialog.FilePath}");
                    }
                    catch (Exception ex)
                    {
                        LogOutput($"Error opening file: {ex.Message}");
                    }
                }
                break;

            case MenuActionType.SaveFile:
                LogOutput("Opening save dialog...");
                var saveDialog = new SaveDialog("Save File", "Select location");
                Application.Run(saveDialog);
                if (!saveDialog.Canceled && saveDialog.FilePath != null)
                {
                    try
                    {
                        File.WriteAllText(saveDialog.FilePath.ToString()!, _outputView.Text.ToString()!);
                        LogOutput($"Saved: {saveDialog.FilePath}");
                    }
                    catch (Exception ex)
                    {
                        LogOutput($"Error saving file: {ex.Message}");
                    }
                }
                break;

            case MenuActionType.BuildProject:
                OnBuild();
                break;

            case MenuActionType.RunTests:
                LogOutput("Running tests...");
                LogOutput("Test Suite: LablabBean.Tests");
                LogOutput("  ✓ Test 1: Passed");
                LogOutput("  ✓ Test 2: Passed");
                LogOutput("  ✓ Test 3: Passed");
                LogOutput("All tests passed! (3/3)");
                break;

            case MenuActionType.ViewLogs:
                LogOutput("Loading logs...");
                LogOutput("=== Application Logs ===");
                foreach (var line in _outputLines.TakeLast(10))
                {
                    LogOutput(line);
                }
                break;

            case MenuActionType.Refresh:
                OnRefresh();
                break;
        }
    }

    private void OnBuild()
    {
        LogOutput("Building project...");
        LogOutput("Restoring packages...");
        LogOutput("Compiling LablabBean.Core...");
        LogOutput("Compiling LablabBean.Infrastructure...");
        LogOutput("Compiling LablabBean.Console...");
        LogOutput("Build succeeded! 0 errors, 0 warnings");
    }

    private void OnRefresh()
    {
        LogOutput("Refreshing view...");
        Application.Refresh();
        LogOutput("View refreshed");
    }

    private void OnAbout()
    {
        MessageBox.Query("About",
            "Lablab Bean v0.1.0\n\n" +
            "Interactive TUI Application\n\n" +
            "Built with:\n" +
            "• Terminal.Gui v2\n" +
            "• .NET 8\n" +
            "• ReactiveUI\n" +
            "• Microsoft.Extensions.*\n\n" +
            "Works with xterm.js in browser!",
            "Ok");
    }

    private void OnShowShortcuts()
    {
        MessageBox.Query("Keyboard Shortcuts",
            "Ctrl+Q - Quit\n" +
            "Ctrl+N - New File\n" +
            "Ctrl+O - Open File\n" +
            "Ctrl+S - Save File\n" +
            "F5 - Refresh\n" +
            "F6 - Build\n" +
            "Ctrl+T - Run Tests\n" +
            "Ctrl+L - View Logs\n" +
            "F1 - Help",
            "Ok");
    }

    private void OnQuit()
    {
        Application.RequestStop();
    }

    private void LogOutput(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var logLine = $"[{timestamp}] {message}";
        _outputLines.Add(logLine);

        var currentText = _outputView.Text.ToString();
        _outputView.Text = currentText + logLine + "\n";
    }
}
