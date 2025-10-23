using System.Text.Json.Serialization;

namespace LablabBean.Contracts.Input.Serialization;

/// <summary>
/// JSON representation of an Input Asset, compatible with Unity Input System format.
/// </summary>
public class InputAssetJson
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("maps")]
    public List<ActionMapJson> Maps { get; set; } = new();

    [JsonPropertyName("controlSchemes")]
    public List<ControlSchemeJson> ControlSchemes { get; set; } = new();
}

/// <summary>
/// JSON representation of an Action Map.
/// </summary>
public class ActionMapJson
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("actions")]
    public List<ActionJson> Actions { get; set; } = new();

    [JsonPropertyName("bindings")]
    public List<BindingJson> Bindings { get; set; } = new();
}

/// <summary>
/// JSON representation of an Action.
/// </summary>
public class ActionJson
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "Button"; // "Button", "Value", "PassThrough"

    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("expectedControlType")]
    public string ExpectedControlType { get; set; } = "";

    [JsonPropertyName("processors")]
    public string Processors { get; set; } = "";

    [JsonPropertyName("interactions")]
    public string Interactions { get; set; } = "";

    [JsonPropertyName("initialStateCheck")]
    public bool InitialStateCheck { get; set; } = false;
}

/// <summary>
/// JSON representation of an Input Binding.
/// </summary>
public class BindingJson
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("path")]
    public string Path { get; set; } = "";

    [JsonPropertyName("interactions")]
    public string Interactions { get; set; } = "";

    [JsonPropertyName("processors")]
    public string Processors { get; set; } = "";

    [JsonPropertyName("groups")]
    public string Groups { get; set; } = "";

    [JsonPropertyName("action")]
    public string Action { get; set; } = "";

    [JsonPropertyName("isComposite")]
    public bool IsComposite { get; set; } = false;

    [JsonPropertyName("isPartOfComposite")]
    public bool IsPartOfComposite { get; set; } = false;
}

/// <summary>
/// JSON representation of a Control Scheme.
/// </summary>
public class ControlSchemeJson
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("bindingGroup")]
    public string BindingGroup { get; set; } = "";

    [JsonPropertyName("devices")]
    public List<DeviceRequirementJson> Devices { get; set; } = new();
}

/// <summary>
/// JSON representation of a Device Requirement.
/// </summary>
public class DeviceRequirementJson
{
    [JsonPropertyName("devicePath")]
    public string DevicePath { get; set; } = "";

    [JsonPropertyName("isOptional")]
    public bool IsOptional { get; set; } = false;

    [JsonPropertyName("isOR")]
    public bool IsOR { get; set; } = false;
}
