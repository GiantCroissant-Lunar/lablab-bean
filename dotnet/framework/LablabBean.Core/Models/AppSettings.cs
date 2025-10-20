namespace LablabBean.Core.Models;

public class AppSettings
{
    public string ApplicationName { get; set; } = "Lablab Bean";
    public string Version { get; set; } = "0.1.0";
    public LoggingSettings Logging { get; set; } = new();
}

public class LoggingSettings
{
    public string MinimumLevel { get; set; } = "Information";
    public bool EnableConsole { get; set; } = true;
    public bool EnableFile { get; set; } = false;
    public string? FilePath { get; set; }
}
