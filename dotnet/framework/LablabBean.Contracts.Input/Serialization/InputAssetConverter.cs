using System.Text.Json;

namespace LablabBean.Contracts.Input.Serialization;

/// <summary>
/// Converts between InputAsset objects and Unity-compatible JSON format.
/// </summary>
public static class InputAssetConverter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Convert InputAsset to Unity-compatible JSON string.
    /// </summary>
    /// <param name="asset">Input asset to convert.</param>
    /// <returns>JSON string representation.</returns>
    public static string ToJson(InputAsset asset)
    {
        if (asset == null) throw new ArgumentNullException(nameof(asset));

        var jsonAsset = ToJsonModel(asset);
        return JsonSerializer.Serialize(jsonAsset, JsonOptions);
    }

    /// <summary>
    /// Create InputAsset from Unity-compatible JSON string.
    /// </summary>
    /// <param name="json">JSON string to parse.</param>
    /// <returns>InputAsset object.</returns>
    public static InputAsset FromJson(string json)
    {
        if (string.IsNullOrEmpty(json)) throw new ArgumentException("JSON cannot be null or empty", nameof(json));

        var jsonAsset = JsonSerializer.Deserialize<InputAssetJson>(json, JsonOptions);
        if (jsonAsset == null) throw new InvalidOperationException("Failed to deserialize JSON");

        return FromJsonModel(jsonAsset);
    }

    /// <summary>
    /// Load InputAsset from JSON file.
    /// </summary>
    /// <param name="filePath">Path to JSON file.</param>
    /// <returns>InputAsset object.</returns>
    public static async Task<InputAsset> LoadFromFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        var json = await File.ReadAllTextAsync(filePath);
        return FromJson(json);
    }

    /// <summary>
    /// Save InputAsset to JSON file.
    /// </summary>
    /// <param name="asset">Input asset to save.</param>
    /// <param name="filePath">Path to save JSON file.</param>
    public static async Task SaveToFileAsync(InputAsset asset, string filePath)
    {
        if (asset == null) throw new ArgumentNullException(nameof(asset));
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        var json = ToJson(asset);
        await File.WriteAllTextAsync(filePath, json);
    }

    private static InputAssetJson ToJsonModel(InputAsset asset)
    {
        var jsonAsset = new InputAssetJson
        {
            Name = asset.Name
        };

        // Convert action maps
        foreach (var (mapName, actionMap) in asset.ActionMaps)
        {
            var jsonMap = new ActionMapJson
            {
                Name = actionMap.Name,
                Id = GenerateId(actionMap.Name)
            };

            // Convert actions and bindings
            foreach (var (actionName, actionDef) in actionMap.Actions)
            {
                var actionId = GenerateId(actionName);

                // Add action
                jsonMap.Actions.Add(new ActionJson
                {
                    Name = actionDef.Name,
                    Type = actionDef.ActionType.ToString(),
                    Id = actionId,
                    ExpectedControlType = GetExpectedControlType(actionDef.ActionType)
                });

                // Add bindings for this action
                var hasComposite = actionDef.Bindings.Any(b => b.CompositeType != CompositeType.None);

                if (hasComposite)
                {
                    // Add composite root binding
                    var compositeType = actionDef.Bindings.First(b => b.CompositeType != CompositeType.None).CompositeType;
                    jsonMap.Bindings.Add(new BindingJson
                    {
                        Id = GenerateId($"{actionName}_composite"),
                        Name = compositeType.ToString(),
                        Path = "",
                        Action = actionName,
                        IsComposite = true,
                        IsPartOfComposite = false
                    });
                }

                foreach (var binding in actionDef.Bindings)
                {
                    var jsonBinding = new BindingJson
                    {
                        Id = GenerateId($"{actionName}_{binding.Path}"),
                        Path = ConvertToUnityPath(binding.Path),
                        Action = actionName,
                        Groups = "", // Could map from control schemes
                        IsComposite = false,
                        IsPartOfComposite = !string.IsNullOrEmpty(binding.CompositePart)
                    };

                    // Handle composite part bindings
                    if (!string.IsNullOrEmpty(binding.CompositePart))
                    {
                        jsonBinding.Name = binding.CompositePart;
                    }

                    jsonMap.Bindings.Add(jsonBinding);
                }
            }

            jsonAsset.Maps.Add(jsonMap);
        }

        // Convert control schemes
        foreach (var (schemeName, scheme) in asset.ControlSchemes)
        {
            var jsonScheme = new ControlSchemeJson
            {
                Name = scheme.Name,
                BindingGroup = scheme.Name.Replace(" ", "").Replace("&", "")
            };

            foreach (var device in scheme.DeviceRequirements)
            {
                jsonScheme.Devices.Add(new DeviceRequirementJson
                {
                    DevicePath = $"<{device.DeviceType}>",
                    IsOptional = !device.IsRequired,
                    IsOR = device.IsOrRequired
                });
            }

            jsonAsset.ControlSchemes.Add(jsonScheme);
        }

        return jsonAsset;
    }

    private static InputAsset FromJsonModel(InputAssetJson jsonAsset)
    {
        var actionMaps = new List<InputActionMap>();
        var controlSchemes = new List<ControlScheme>();

        // Convert action maps
        foreach (var jsonMap in jsonAsset.Maps)
        {
            var actions = new List<InputActionDefinition>();

            // Group bindings by action
            var bindingsByAction = jsonMap.Bindings
                .Where(b => !string.IsNullOrEmpty(b.Action))
                .GroupBy(b => b.Action)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Convert actions
            foreach (var jsonAction in jsonMap.Actions)
            {
                var actionType = Enum.TryParse<InputActionType>(jsonAction.Type, out var type)
                    ? type
                    : InputActionType.Button;

                var bindings = new List<InputBinding>();

                // Get bindings for this action
                if (bindingsByAction.TryGetValue(jsonAction.Name, out var actionBindings))
                {
                    foreach (var jsonBinding in actionBindings)
                    {
                        var compositeType = CompositeType.None;
                        var compositePart = "";

                        if (jsonBinding.IsComposite)
                        {
                            Enum.TryParse<CompositeType>(jsonBinding.Name, out compositeType);
                        }
                        else if (jsonBinding.IsPartOfComposite)
                        {
                            compositePart = jsonBinding.Name;
                            compositeType = CompositeType.Axis2D; // Default for composite parts
                        }

                        bindings.Add(new InputBinding(
                            jsonBinding.Path,
                            "", // Modifiers could be extracted from processors
                            compositeType,
                            compositePart
                        ));
                    }
                }

                actions.Add(new InputActionDefinition(
                    jsonAction.Name,
                    actionType,
                    bindings.AsReadOnly()
                ));
            }

            actionMaps.Add(InputActionMap.Create(jsonMap.Name, actions));
        }

        // Convert control schemes
        foreach (var jsonScheme in jsonAsset.ControlSchemes)
        {
            var devices = jsonScheme.Devices.Select(d => new DeviceRequirement(
                ExtractDeviceType(d.DevicePath),
                !d.IsOptional,
                d.IsOR
            )).ToList();

            controlSchemes.Add(new ControlScheme(jsonScheme.Name, devices.AsReadOnly()));
        }

        return InputAsset.Create(jsonAsset.Name, actionMaps, controlSchemes);
    }

    private static string GenerateId(string name)
    {
        // Generate a simple GUID-like ID based on name
        var hash = name.GetHashCode();
        return $"{Math.Abs(hash):x8}-{Math.Abs(hash >> 8):x4}-{Math.Abs(hash >> 16):x4}-{Math.Abs(hash >> 24):x4}-{Math.Abs(hash):x12}";
    }

    private static string GetExpectedControlType(InputActionType actionType)
    {
        return actionType switch
        {
            InputActionType.Button => "Button",
            InputActionType.Value => "Vector2",
            InputActionType.PassThrough => "Any",
            _ => "Button"
        };
    }

    private static string ExtractDeviceType(string devicePath)
    {
        // Extract device type from path like "<Keyboard>" -> "Keyboard"
        return devicePath.Trim('<', '>');
    }

    private static string ConvertToUnityPath(string lablabPath)
    {
        // Convert LablabBean path format to Unity format
        // "Keyboard/w" -> "<Keyboard>/w"
        // "Mouse/leftButton" -> "<Mouse>/leftButton"

        if (string.IsNullOrEmpty(lablabPath)) return lablabPath;

        var parts = lablabPath.Split('/');
        if (parts.Length >= 2)
        {
            var device = parts[0];
            var control = parts[1];
            return $"<{device}>/{control}";
        }

        return lablabPath;
    }
}
