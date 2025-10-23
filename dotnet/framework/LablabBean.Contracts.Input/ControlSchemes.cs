namespace LablabBean.Contracts.Input;

/// <summary>
/// Control scheme defining device-specific input bindings.
/// Inspired by Unity Input System's Control Scheme concept.
/// </summary>
/// <param name="Name">Unique name of the control scheme.</param>
/// <param name="DeviceRequirements">Required devices for this scheme.</param>
/// <param name="IsActive">Whether this scheme is currently active.</param>
public record ControlScheme(
    string Name,
    IReadOnlyList<DeviceRequirement> DeviceRequirements,
    bool IsActive = false
)
{
    /// <summary>
    /// Create a keyboard-only control scheme.
    /// </summary>
    /// <param name="name">Scheme name.</param>
    public static ControlScheme Keyboard(string name = "Keyboard")
    {
        return new ControlScheme(name, [DeviceRequirement.Required("Keyboard")]);
    }

    /// <summary>
    /// Create a keyboard and mouse control scheme.
    /// </summary>
    /// <param name="name">Scheme name.</param>
    public static ControlScheme KeyboardMouse(string name = "Keyboard & Mouse")
    {
        return new ControlScheme(name, [
            DeviceRequirement.Required("Keyboard"),
            DeviceRequirement.Required("Mouse")
        ]);
    }

    /// <summary>
    /// Create a gamepad control scheme.
    /// </summary>
    /// <param name="name">Scheme name.</param>
    public static ControlScheme Gamepad(string name = "Gamepad")
    {
        return new ControlScheme(name, [DeviceRequirement.Required("Gamepad")]);
    }
}

/// <summary>
/// Device requirement for a control scheme.
/// </summary>
/// <param name="DeviceType">Type of device (e.g., "Keyboard", "Mouse", "Gamepad").</param>
/// <param name="IsRequired">Whether this device is required or optional.</param>
/// <param name="IsOrRequired">Whether this device is part of an OR group.</param>
public record DeviceRequirement(
    string DeviceType,
    bool IsRequired = true,
    bool IsOrRequired = false
)
{
    /// <summary>
    /// Create a required device requirement.
    /// </summary>
    /// <param name="deviceType">Type of device.</param>
    public static DeviceRequirement Required(string deviceType)
    {
        return new DeviceRequirement(deviceType, IsRequired: true);
    }

    /// <summary>
    /// Create an optional device requirement.
    /// </summary>
    /// <param name="deviceType">Type of device.</param>
    public static DeviceRequirement Optional(string deviceType)
    {
        return new DeviceRequirement(deviceType, IsRequired: false);
    }

    /// <summary>
    /// Create an OR device requirement (any one of multiple devices).
    /// </summary>
    /// <param name="deviceType">Type of device.</param>
    public static DeviceRequirement Or(string deviceType)
    {
        return new DeviceRequirement(deviceType, IsRequired: true, IsOrRequired: true);
    }
}

/// <summary>
/// Input asset containing action maps and control schemes.
/// This is the top-level container for all input configuration.
/// </summary>
/// <param name="Name">Name of the input asset.</param>
/// <param name="ActionMaps">Collection of action maps.</param>
/// <param name="ControlSchemes">Available control schemes.</param>
public record InputAsset(
    string Name,
    IReadOnlyDictionary<string, InputActionMap> ActionMaps,
    IReadOnlyDictionary<string, ControlScheme> ControlSchemes
)
{
    /// <summary>
    /// Create an input asset from collections.
    /// </summary>
    /// <param name="name">Asset name.</param>
    /// <param name="actionMaps">Action maps to include.</param>
    /// <param name="controlSchemes">Control schemes to include.</param>
    public static InputAsset Create(
        string name,
        IEnumerable<InputActionMap> actionMaps,
        IEnumerable<ControlScheme> controlSchemes)
    {
        var mapDict = actionMaps.ToDictionary(m => m.Name, m => m);
        var schemeDict = controlSchemes.ToDictionary(s => s.Name, s => s);
        return new InputAsset(name, mapDict, schemeDict);
    }

    /// <summary>
    /// Get an action map by name.
    /// </summary>
    /// <param name="mapName">Name of the action map.</param>
    public InputActionMap? GetActionMap(string mapName)
    {
        return ActionMaps.TryGetValue(mapName, out var map) ? map : null;
    }

    /// <summary>
    /// Get a control scheme by name.
    /// </summary>
    /// <param name="schemeName">Name of the control scheme.</param>
    public ControlScheme? GetControlScheme(string schemeName)
    {
        return ControlSchemes.TryGetValue(schemeName, out var scheme) ? scheme : null;
    }

    /// <summary>
    /// Get all enabled action maps.
    /// </summary>
    public IEnumerable<InputActionMap> GetEnabledActionMaps()
    {
        return ActionMaps.Values.Where(m => m.IsEnabled);
    }

    /// <summary>
    /// Get the active control scheme.
    /// </summary>
    public ControlScheme? GetActiveControlScheme()
    {
        return ControlSchemes.Values.FirstOrDefault(s => s.IsActive);
    }
}
