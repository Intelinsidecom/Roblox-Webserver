using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobloxFiles;

namespace RobloxAssetValidation
{
    interface Validation
    {
        bool ValidateAsset(string File);
    }

    public class ValidClassesClass
    {
        public string STRING { get; set; }
        public bool BOOL { get; set; }
    }


    // this is just code ported from AssetValidationServiceV2 Go lang project to c#. most parts ported by AI cuz im too dumb to find replacements myself
    class IValidatePlace : Validation
    {
        public bool ValidateAsset(string File)
        {
            if (File == null)
                return false;

            var file = RobloxFile.Open(File);
            
            var services = new Dictionary<string, Instance>();
            foreach (var item in file.GetDescendants())
            {
                if (item.IsService)
                {
                    services[item.ClassName] = item;
                }
            }

            if (services["Lighting"] == null)
            {
                return false;
            }

            if (services["Workspace"] == null)
            {
                return false;
            }

            return true;
        }
    }

    class IValidateModel : Validation
    {
        public bool ValidateAsset(string File)
        {
            if (File == null)
                return false;

            var file = RobloxFile.Open(File);

            var services = new Dictionary<string, Instance>();
            foreach (var item in file.GetDescendants())
            {
                if (item.IsService)
                {
                    // Invalid model: contains service
                    return false;
                }
            }

            var childSet = new HashSet<Instance>();
            foreach (var inst in file.GetDescendants())
            {
                foreach (var child in inst.Children)
                {
                    childSet.Add(child);
                }
            }

            var rootModels = new List<Instance>();
            var rootScripts = new List<Instance>();

            foreach (var inst in file.GetDescendants())
            {
                if (childSet.Contains(inst))
                {
                    continue;
                }

                switch (inst.ClassName)
                {
                    case "Model": case "Tool": case "Folder":
                        rootModels.Add(inst);
                        break;
                    case "Script": case "LocalScript": case "ModuleScript":
                        rootScripts.Add(inst);
                        break;
                    default:
                        // Invalid model: unsupported top-level instance class
                        return false;
                }
            }

            if (rootScripts.Count() == 1 && rootModels.Count() == 0) {
                return true;
             }

            if (rootModels.Count() == 1 && rootScripts.Count() == 0)
            {
                var root = rootModels[0];
                if ((root.Children).Count() == 0)
                {
                    // Invalid model: root Model has no children.
                    return false;
                }


                List<ValidClassesClass> validClasses = new List<ValidClassesClass>();

                // Base building parts
                validClasses.Add(new ValidClassesClass { STRING = "Part", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "MeshPart", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "SpecialMesh", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "WedgePart", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "CornerWedgePart", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "UnionOperation", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "TrussPart", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "VehicleSeat", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "Seat", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "SpawnLocation", BOOL = true });

                // Meshes
                validClasses.Add(new ValidClassesClass { STRING = "BlockMesh", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "CylinderMesh", BOOL = true });

                // Constraints & joints
                validClasses.Add(new ValidClassesClass { STRING = "Attachment", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "Motor6D", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "Weld", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "WeldConstraint", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "HingeConstraint", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "BallSocketConstraint", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "RodConstraint", BOOL = true });

                // Visual effects
                validClasses.Add(new ValidClassesClass { STRING = "Decal", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "Texture", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "SurfaceGui", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "BillboardGui", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "PointLight", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "SpotLight", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "SurfaceLight", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "ParticleEmitter", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "Trail", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "Fire", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "Smoke", BOOL = true });

                // Skybox
                validClasses.Add(new ValidClassesClass { STRING = "Sky", BOOL = true });

                // Sounds
                validClasses.Add(new ValidClassesClass { STRING = "Sound", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "SoundGroup", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "EqualizerSoundEffect", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "ReverbSoundEffect", BOOL = true });

                // Containers
                validClasses.Add(new ValidClassesClass { STRING = "Folder", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "LuaSourceContainer", BOOL = true });

                // UI elements
                validClasses.Add(new ValidClassesClass { STRING = "ScreenGui", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "TextLabel", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "TextButton", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "ImageLabel", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "ImageButton", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "Frame", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "ScrollingFrame", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "UIListLayout", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "UIGridLayout", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "UICorner", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "UIStroke", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "UIPadding", BOOL = true });

                // Scripts
                validClasses.Add(new ValidClassesClass { STRING = "Script", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "LocalScript", BOOL = true });
                validClasses.Add(new ValidClassesClass { STRING = "ModuleScript", BOOL = true });

                foreach (var child in root.Children)
                {
                    var isValid = validClasses.Any(vc => vc.STRING == child.ClassName && vc.BOOL);
                    if (!isValid && child.ClassName != "Model")
                    {
                        // Invalid model: unsupported child class in root model.
                        return false;
                    }
                }
                return true;
            }
            // Invalid model: expected exactly 1 root Model or 1 root Script
            return false;
        }
    }


}
