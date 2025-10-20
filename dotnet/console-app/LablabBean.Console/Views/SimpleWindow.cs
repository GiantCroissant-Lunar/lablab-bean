using Terminal.Gui;

namespace LablabBean.Console.Views;

public class SimpleWindow : Window
{
    private readonly TextView _textView;
    private readonly StatusBar _statusBar;

    public SimpleWindow()
    {
        Title = "Lablab Bean - Interactive TUI (Press ESC to quit)";

        // Create main text view
        _textView = new TextView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            Text = "╔══════════════════════════════════════════════════════════╗\n" +
                   "║           Welcome to Lablab Bean Interactive TUI        ║\n" +
                   "╚══════════════════════════════════════════════════════════╝\n\n" +
                   "This is a Terminal.Gui application running in a PTY!\n\n" +
                   "Features:\n" +
                   "• Runs in browser via xterm.js\n" +
                   "• Full keyboard support\n" +
                   "• Mouse support\n" +
                   "• Real-time updates\n\n" +
                   "Commands:\n" +
                   "  ESC - Exit application\n" +
                   "  F1  - Show help\n" +
                   "  F5  - Refresh\n\n" +
                   "This TUI is managed by PM2 and connected through node-pty.\n" +
                   "You can interact with it directly from your browser!\n\n" +
                   "Try typing or clicking around...\n"
        };

        // Create status bar
        _statusBar = new StatusBar(new StatusItem[]
        {
            new StatusItem(Key.Esc, "~ESC~ Quit", OnQuit),
            new StatusItem(Key.F1, "~F1~ Help", OnHelp),
            new StatusItem(Key.F5, "~F5~ Refresh", OnRefresh)
        });

        Add(_textView);
        Add(_statusBar);
    }

    private void OnQuit()
    {
        Application.RequestStop();
    }

    private void OnHelp()
    {
        MessageBox.Query("Help", 
            "Lablab Bean Interactive TUI\n\n" +
            "Keyboard Shortcuts:\n" +
            "  ESC - Quit application\n" +
            "  F1  - Show this help\n" +
            "  F5  - Refresh view\n\n" +
            "This application runs in:\n" +
            "• Browser (via xterm.js)\n" +
            "• PTY session (node-pty)\n" +
            "• Managed by PM2\n\n" +
            "Version: 0.1.0", 
            "OK");
    }

    private void OnRefresh()
    {
        var currentText = _textView.Text.ToString();
        _textView.Text = currentText + $"\n[{DateTime.Now:HH:mm:ss}] View refreshed!\n";
        Application.Refresh();
    }
}
