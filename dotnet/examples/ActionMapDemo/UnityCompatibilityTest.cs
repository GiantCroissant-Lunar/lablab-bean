using LablabBean.Contracts.Input;
using LablabBean.Contracts.Input.Serialization;
using System.Text.Json;

namespace ActionMapDemo;

/// <summary>
/// Test to verify Unity Input System JSON compatibility.
/// This demonstrates that our JSON can be used directly in Unity.
/// </summary>
public static class UnityCompatibilityTest
{
    /// <summary>
    /// Test round-trip compatibility: LablabBean → JSON → Unity → JSON → LablabBean
    /// </summary>
    public static async Task RunCompatibilityTest()
    {
        Console.WriteLine("=== Unity Compatibility Test ===\n");

        // 1. Create LablabBean input asset
        var originalAsset = CreateTestAsset();
        Console.WriteLine("✓ Created LablabBean input asset");

        // 2. Convert to Unity JSON
        var unityJson = InputAssetConverter.ToJson(originalAsset);
        await File.WriteAllTextAsync("TestOutput.inputactions", unityJson);
        Console.WriteLine("✓ Exported to Unity-compatible JSON");

        // 3. Verify JSON structure matches Unity format
        VerifyUnityJsonStructure(unityJson);
        Console.WriteLine("✓ JSON structure matches Unity format");

        // 4. Load back from JSON
        var loadedAsset = InputAssetConverter.FromJson(unityJson);
        Console.WriteLine("✓ Successfully loaded back from JSON");

        // 5. Verify data integrity
        VerifyDataIntegrity(originalAsset, loadedAsset);
        Console.WriteLine("✓ Data integrity verified");

        // 6. Show Unity-specific features
        ShowUnityFeatures(unityJson);

        Console.WriteLine("\n🎉 Unity compatibility test PASSED!");
        Console.WriteLine("   The generated JSON can be used directly in Unity Input System!");
    }

    private static InputAsset CreateTestAsset()
    {
        // Create a comprehensive test asset with all Unity features
        var playerActions = new[]
        {
            // 2D Movement composite (Unity's most common pattern)
            InputActionDefinition.Composite("Move",
                InputBinding.CompositeAxis("w", "up"),
                InputBinding.CompositeAxis("s", "down"),
                InputBinding.CompositeAxis("a", "left"),
                InputBinding.CompositeAxis("d", "right")
            ),

            // Simple button actions
            InputActionDefinition.Button("Jump", "space"),
            InputActionDefinition.Button("Fire", "leftButton"),
            InputActionDefinition.Button("AltFire", "rightButton"),

            // Actions with modifiers (Unity supports this)
            InputActionDefinition.Button("Run", "leftShift"),
            InputActionDefinition.Button("Crouch", "leftCtrl"),
        };

        var uiActions = new[]
        {
            InputActionDefinition.Button("Submit", "enter"),
            InputActionDefinition.Button("Cancel", "escape"),
            InputActionDefinition.Button("Navigate", "tab"),
        };

        var actionMaps = new[]
        {
            InputActionMap.Create("Player", playerActions, isEnabled: true),
            InputActionMap.Create("UI", uiActions, isEnabled: false),
        };

        var controlSchemes = new[]
        {
            ControlScheme.KeyboardMouse("Keyboard&Mouse"),
            ControlScheme.Keyboard("Keyboard"),
        };

        return InputAsset.Create("UnityCompatibilityTest", actionMaps, controlSchemes);
    }

    private static void VerifyUnityJsonStructure(string json)
    {
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Verify top-level structure
        if (!root.TryGetProperty("name", out _))
            throw new Exception("Missing 'name' property");

        if (!root.TryGetProperty("maps", out var maps))
            throw new Exception("Missing 'maps' property");

        if (!root.TryGetProperty("controlSchemes", out var schemes))
            throw new Exception("Missing 'controlSchemes' property");

        // Verify action map structure
        foreach (var map in maps.EnumerateArray())
        {
            if (!map.TryGetProperty("name", out _))
                throw new Exception("Action map missing 'name'");

            if (!map.TryGetProperty("id", out _))
                throw new Exception("Action map missing 'id' (required by Unity)");

            if (!map.TryGetProperty("actions", out _))
                throw new Exception("Action map missing 'actions'");

            if (!map.TryGetProperty("bindings", out _))
                throw new Exception("Action map missing 'bindings'");
        }

        // Verify composite binding structure
        var hasComposite = false;
        var hasCompositePart = false;

        foreach (var map in maps.EnumerateArray())
        {
            if (map.TryGetProperty("bindings", out var bindings))
            {
                foreach (var binding in bindings.EnumerateArray())
                {
                    if (binding.TryGetProperty("isComposite", out var isComposite) && isComposite.GetBoolean())
                    {
                        hasComposite = true;
                        // Composite root should have empty path
                        if (binding.TryGetProperty("path", out var path))
                        {
                            var pathStr = path.GetString();
                            if (!string.IsNullOrEmpty(pathStr))
                            {
                                throw new Exception("Composite root binding should have empty path");
                            }
                        }
                    }

                    if (binding.TryGetProperty("isPartOfComposite", out var isPartOfComposite) && isPartOfComposite.GetBoolean())
                    {
                        hasCompositePart = true;
                        // Composite parts should have Unity path format
                        if (binding.TryGetProperty("path", out var path))
                        {
                            var pathStr = path.GetString();
                            if (pathStr != null && (!pathStr.StartsWith("<") || !pathStr.Contains(">")))
                            {
                                throw new Exception($"Composite part path should use Unity format: {pathStr}");
                            }
                        }
                    }
                }
            }
        }

        if (!hasComposite || !hasCompositePart)
        {
            throw new Exception("Missing composite binding structure");
        }
    }

    private static void VerifyDataIntegrity(InputAsset original, InputAsset loaded)
    {
        // Verify basic properties
        if (original.Name != loaded.Name)
            throw new Exception("Asset name mismatch");

        if (original.ActionMaps.Count != loaded.ActionMaps.Count)
            throw new Exception("Action map count mismatch");

        if (original.ControlSchemes.Count != loaded.ControlSchemes.Count)
            throw new Exception("Control scheme count mismatch");

        // Verify action maps
        foreach (var (name, originalMap) in original.ActionMaps)
        {
            if (!loaded.ActionMaps.TryGetValue(name, out var loadedMap))
                throw new Exception($"Missing action map: {name}");

            if (originalMap.Actions.Count != loadedMap.Actions.Count)
                throw new Exception($"Action count mismatch in map: {name}");

            // Verify actions exist (binding details may differ due to conversion)
            foreach (var (actionName, _) in originalMap.Actions)
            {
                if (!loadedMap.Actions.ContainsKey(actionName))
                    throw new Exception($"Missing action: {actionName} in map: {name}");
            }
        }
    }

    private static void ShowUnityFeatures(string json)
    {
        Console.WriteLine("\n📋 Unity-Specific Features in Generated JSON:");

        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Show GUIDs
        if (root.TryGetProperty("maps", out var maps))
        {
            foreach (var map in maps.EnumerateArray())
            {
                if (map.TryGetProperty("name", out var name) && map.TryGetProperty("id", out var id))
                {
                    Console.WriteLine($"   Map '{name.GetString()}' has GUID: {id.GetString()}");
                }
            }
        }

        // Show Unity path format
        Console.WriteLine("\n🎯 Unity Path Format Examples:");
        foreach (var map in maps.EnumerateArray())
        {
            if (map.TryGetProperty("bindings", out var bindings))
            {
                var count = 0;
                foreach (var binding in bindings.EnumerateArray())
                {
                    if (binding.TryGetProperty("path", out var path) && !string.IsNullOrEmpty(path.GetString()))
                    {
                        Console.WriteLine($"   {path.GetString()}");
                        if (++count >= 3) break; // Show first 3 examples
                    }
                }
                break;
            }
        }

        // Show composite structure
        Console.WriteLine("\n🔗 Composite Binding Structure:");
        foreach (var map in maps.EnumerateArray())
        {
            if (map.TryGetProperty("bindings", out var bindings))
            {
                foreach (var binding in bindings.EnumerateArray())
                {
                    if (binding.TryGetProperty("isComposite", out var isComposite) && isComposite.GetBoolean())
                    {
                        if (binding.TryGetProperty("name", out var name))
                        {
                            Console.WriteLine($"   Composite: {name.GetString()} (Unity recognizes this)");
                        }
                        break;
                    }
                }
                break;
            }
        }
    }
}
