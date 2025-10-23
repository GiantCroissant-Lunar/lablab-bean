using Terminal.Gui;

namespace LablabBean.Console.Views;

public class MainWindow : Window
{
    private readonly MenuBar _menuBar;
    private readonly StatusBar _statusBar;
    private readonly TextView _textView;

    public MainWindow()
    {
        Title = "Lablab Bean - Terminal.Gui (Ctrl+Q to quit)";

        // Create menu bar
        _menuBar = new MenuBar(new MenuBarItem[]
        {
            new MenuBarItem("_File", new MenuItem[]
            {
                new MenuItem("_New", "Create new", OnNew),
                new MenuItem("_Open", "Open file", OnOpen),
                new MenuItem("_Save", "Save file", OnSave),
                null!, // Separator
                new MenuItem("_Quit", "Exit application", OnQuit)
            }),
            new MenuBarItem("_Edit", new MenuItem[]
            {
                new MenuItem("_Copy", "Copy", OnCopy),
                new MenuItem("C_ut", "Cut", OnCut),
                new MenuItem("_Paste", "Paste", OnPaste)
            }),
            new MenuBarItem("_View", new MenuItem[]
            {
                new MenuItem("_Refresh", "Refresh view", OnRefresh)
            }),
            new MenuBarItem("_Help", new MenuItem[]
            {
                new MenuItem("_About", "About", OnAbout)
            })
        });

        // Create status bar
        _statusBar = new StatusBar(new StatusItem[]
        {
            new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", OnQuit),
            new StatusItem(Key.CtrlMask | Key.N, "~^N~ New", OnNew),
            new StatusItem(Key.CtrlMask | Key.O, "~^O~ Open", OnOpen)
        });

        // Create main text view
        _textView = new TextView
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            Text = "Welcome to Lablab Bean!\n\n" +
                   "This is a Terminal.Gui v2 application.\n\n" +
                   "Features:\n" +
                   "- Modern TUI framework\n" +
                   "- Reactive programming with ReactiveUI\n" +
                   "- Dependency injection\n" +
                   "- Logging with Serilog\n\n" +
                   "Press Ctrl+Q to quit."
        };

        // Add controls
        Add(_menuBar);
        Add(_textView);
        Add(_statusBar);
    }

    private void OnNew()
    {
        _textView.Text = string.Empty;
    }

    private void OnOpen()
    {
        var dialog = new OpenDialog("Open File", "Select a file to open");
        Application.Run(dialog);

        if (!dialog.Canceled && dialog.FilePath != null)
        {
            try
            {
                _textView.Text = File.ReadAllText(dialog.FilePath.ToString()!);
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", $"Failed to open file: {ex.Message}", "Ok");
            }
        }
    }

    private void OnSave()
    {
        var dialog = new SaveDialog("Save File", "Select location to save");
        Application.Run(dialog);

        if (!dialog.Canceled && dialog.FilePath != null)
        {
            try
            {
                File.WriteAllText(dialog.FilePath.ToString()!, _textView.Text.ToString()!);
                MessageBox.Query("Success", "File saved successfully!", "Ok");
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Error", $"Failed to save file: {ex.Message}", "Ok");
            }
        }
    }

    private void OnCopy()
    {
        _textView.Copy();
    }

    private void OnCut()
    {
        _textView.Cut();
    }

    private void OnPaste()
    {
        _textView.Paste();
    }

    private void OnRefresh()
    {
        Application.Refresh();
    }

    private void OnAbout()
    {
        MessageBox.Query("About",
            "Lablab Bean v0.1.0\n\n" +
            "A modern TUI application built with:\n" +
            "- Terminal.Gui v2\n" +
            "- .NET 8\n" +
            "- ReactiveUI\n" +
            "- Microsoft.Extensions.*",
            "Ok");
    }

    private void OnQuit()
    {
        Application.RequestStop();
    }
}
