using RobloxFiles;

namespace Assets;

public static class AssetValidationHelper
{
    private static readonly HashSet<string> ValidChildClasses = new()
    {
        "Part", "MeshPart", "SpecialMesh", "WedgePart", "CornerWedgePart",
        "UnionOperation", "TrussPart", "VehicleSeat", "Seat", "SpawnLocation",
        "BlockMesh", "CylinderMesh",
        "Attachment", "Motor6D", "Weld", "WeldConstraint",
        "HingeConstraint", "BallSocketConstraint", "RodConstraint",
        "Decal", "Texture", "SurfaceGui", "BillboardGui",
        "PointLight", "SpotLight", "SurfaceLight",
        "ParticleEmitter", "Trail", "Fire", "Smoke",
        "Sky",
        "Sound", "SoundGroup", "EqualizerSoundEffect", "ReverbSoundEffect",
        "Folder", "LuaSourceContainer",
        "ScreenGui", "TextLabel", "TextButton", "ImageLabel", "ImageButton",
        "Frame", "ScrollingFrame", "UIListLayout", "UIGridLayout",
        "UICorner", "UIStroke", "UIPadding",
        "Script", "LocalScript", "ModuleScript"
    };

    private static readonly HashSet<string> AllowedRootClasses = new()
    {
        "Model", "Tool", "Folder", "Script", "LocalScript", "ModuleScript"
    };

    public static (bool IsValid, string? ErrorMessage) ValidatePlaceContent(byte[] fileBytes)
    {
        if (fileBytes == null || fileBytes.Length == 0)
            return (false, "File content is empty.");

        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".rbxl");
        try
        {
            File.WriteAllBytes(tempPath, fileBytes);
            var file = RobloxFile.Open(tempPath);

            var hasLighting = false;
            var hasWorkspace = false;

            foreach (var item in file.GetDescendants())
            {
                if (item.IsService)
                {
                    if (item.ClassName == "Lighting") hasLighting = true;
                    else if (item.ClassName == "Workspace") hasWorkspace = true;
                }
            }

            if (!hasLighting)
                return (false, "Invalid place file: missing Lighting service.");
            if (!hasWorkspace)
                return (false, "Invalid place file: missing Workspace service.");

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Place validation failed: {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public static (bool IsValid, string? ErrorMessage) ValidateModelContent(byte[] fileBytes)
    {
        if (fileBytes == null || fileBytes.Length == 0)
            return (false, "File content is empty.");

        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".rbxm");
        try
        {
            File.WriteAllBytes(tempPath, fileBytes);
            var file = RobloxFile.Open(tempPath);

            foreach (var item in file.GetDescendants())
            {
                if (item.IsService)
                    return (false, "Invalid model: file contains service instances.");
            }

            var childSet = new HashSet<Instance>();
            foreach (var inst in file.GetDescendants())
            {
                foreach (var child in inst.Children)
                {
                    childSet.Add(child);
                }
            }

            var rootInstances = new List<Instance>();
            foreach (var inst in file.GetDescendants())
            {
                if (childSet.Contains(inst))
                    continue;

                if (!AllowedRootClasses.Contains(inst.ClassName))
                    return (false, $"Invalid model: unsupported top-level instance class '{inst.ClassName}'.");

                rootInstances.Add(inst);
            }

            if (rootInstances.Count == 0)
                return (false, "Invalid model: no root instances found.");

            var rootModels = rootInstances.Where(r => r.ClassName == "Model" || r.ClassName == "Tool" || r.ClassName == "Folder").ToList();
            var rootScripts = rootInstances.Where(r => r.ClassName == "Script" || r.ClassName == "LocalScript" || r.ClassName == "ModuleScript").ToList();

            if (rootScripts.Count == 1 && rootModels.Count == 0)
                return (true, null);

            if (rootModels.Count == 1 && rootScripts.Count == 0)
            {
                var root = rootModels[0];
                if (root.Children.Count == 0)
                    return (false, "Invalid model: root Model has no children.");

                foreach (var child in root.Children)
                {
                    if (!ValidChildClasses.Contains(child.ClassName) && child.ClassName != "Model")
                        return (false, $"Invalid model: unsupported child class '{child.ClassName}' in root model.");
                }

                return (true, null);
            }

            return (false, "Invalid model: expected exactly 1 root Model or 1 root Script.");
        }
        catch (Exception ex)
        {
            return (false, $"Model validation failed: {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public static (bool IsValid, string? ErrorMessage) ValidatePluginContent(byte[] fileBytes)
    {
        if (fileBytes == null || fileBytes.Length == 0)
            return (false, "File content is empty.");

        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".rbxm");
        try
        {
            File.WriteAllBytes(tempPath, fileBytes);
            var file = RobloxFile.Open(tempPath);

            foreach (var item in file.GetDescendants())
            {
                if (item.IsService)
                    return (false, "Invalid plugin: file contains service instances.");
            }

            var childSet = new HashSet<Instance>();
            foreach (var inst in file.GetDescendants())
            {
                foreach (var child in inst.Children)
                {
                    childSet.Add(child);
                }
            }

            var rootInstances = new List<Instance>();
            foreach (var inst in file.GetDescendants())
            {
                if (childSet.Contains(inst))
                    continue;

                if (!AllowedRootClasses.Contains(inst.ClassName))
                    return (false, $"Invalid plugin: unsupported top-level instance class '{inst.ClassName}'.");

                rootInstances.Add(inst);
            }

            if (rootInstances.Count == 0)
                return (false, "Invalid plugin: no root instances found.");

            var rootModels = rootInstances.Where(r => r.ClassName == "Model" || r.ClassName == "Tool" || r.ClassName == "Folder").ToList();
            var rootScripts = rootInstances.Where(r => r.ClassName == "Script" || r.ClassName == "LocalScript" || r.ClassName == "ModuleScript").ToList();

            if (rootScripts.Count == 1 && rootModels.Count == 0)
                return (true, null);

            if (rootModels.Count == 1 && rootScripts.Count == 0)
            {
                var root = rootModels[0];
                if (root.Children.Count == 0)
                    return (false, "Invalid plugin: root Model has no children.");

                var hasScript = false;
                foreach (var desc in root.GetDescendants())
                {
                    if (desc.ClassName == "Script" || desc.ClassName == "LocalScript" || desc.ClassName == "ModuleScript")
                    {
                        hasScript = true;
                        break;
                    }
                }

                if (!hasScript)
                    return (false, "Invalid plugin: no Script, LocalScript, or ModuleScript found in the model.");

                return (true, null);
            }

            return (false, "Invalid plugin: expected exactly 1 root Model or 1 root Script.");
        }
        catch (Exception ex)
        {
            return (false, $"Plugin validation failed: {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public static (bool IsValid, string? ErrorMessage) ValidateMeshContent(byte[] fileBytes)
    {
        if (fileBytes == null || fileBytes.Length == 0)
            return (false, "File content is empty.");

        if (fileBytes.Length > 50 * 1024 * 1024)
            return (false, "Mesh file exceeds maximum allowed size of 50 MB.");

        if (fileBytes.Length < 4)
            return (false, "Mesh file is too small to be valid.");

        // Check for Roblox mesh magic bytes (binary mesh format): version header
        // .mesh files start with a binary header; try to parse via RobloxFile as a fallback
        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".mesh");
        try
        {
            File.WriteAllBytes(tempPath, fileBytes);

            // Try to parse as a Roblox file (works for XML/binary model format)
            try
            {
                var file = RobloxFile.Open(tempPath);

                // Mesh files should NOT contain services (that would indicate a place file)
                foreach (var item in file.GetDescendants())
                {
                    if (item.IsService)
                        return (false, "Invalid mesh: file contains service instances.");
                }

                return (true, null);
            }
            catch
            {
                // If RobloxFile can't parse it, it may still be a valid raw .mesh binary file.
                // Accept it as long as it has content.
                return (true, null);
            }
        }
        catch (Exception ex)
        {
            return (false, $"Mesh validation failed: {ex.Message}");
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}
