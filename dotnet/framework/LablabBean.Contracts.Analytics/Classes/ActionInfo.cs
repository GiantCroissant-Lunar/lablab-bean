using System.Collections.Generic;

namespace LablabBean.Contracts.Analytics;

public class ActionInfo
{
    public string ActionName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] ParameterNames { get; set; } = System.Array.Empty<string>();
    public bool HasReturnValue { get; set; }
}
