namespace LablabBean.Contracts.Game.UI.Models;

public sealed class ActivityLogOptions
{
    public int Capacity { get; set; } = 1000;
    public bool EnableTimestamps { get; set; } = true;
    public bool EnableIcons { get; set; } = true;
}
