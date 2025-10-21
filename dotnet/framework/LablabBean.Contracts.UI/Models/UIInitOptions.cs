namespace LablabBean.Contracts.UI.Models;

/// <summary>
/// UI initialization options.
/// </summary>
public record UIInitOptions(
    int ViewportWidth,
    int ViewportHeight,
    bool EnableMouse,
    string Theme
);
