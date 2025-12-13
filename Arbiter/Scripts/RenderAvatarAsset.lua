-- Pants v1.1.0

local assetId = %assetId%

local assetUrl = "rbxassetid://" .. assetId
local baseUrl = "https://freblx.xyz"
local fileExtension = "PNG"
local x, y = 1024, 1024

local ThumbnailGenerator = game:GetService("ThumbnailGenerator")

pcall(function() game:GetService("ContentProvider"):SetBaseUrl(baseUrl) end)
game:GetService("ScriptContext").ScriptsDisabled = true

-- Create a local test player and load a default character
local Player = game.Players:CreateLocalPlayer(0)
Player:LoadCharacter(false)

local character = Player.Character or Player.CharacterAdded:wait()
print("[RenderAvatarAsset] Player:", Player.Name)
print("[RenderAvatarAsset] Character:", character and character.Name or "nil")

-- Load the pants asset model
local ok, loaded = pcall(function()
    return game:GetObjects(assetUrl)
end)

if not ok or not loaded or #loaded == 0 then
    warn("[RenderAvatarAsset] Failed to load pants asset from " .. assetUrl)
else
    local pantsAsset = loaded[1]
    print("[RenderAvatarAsset] Loaded asset:", pantsAsset.ClassName, pantsAsset.Name)

    -- Helper: find a child of given class among direct children
    local function findChildOfClass(parent, className)
        local children = parent:GetChildren()
        for i = 1, #children do
            if children[i].ClassName == className then
                return children[i]
            end
        end
        return nil
    end

    -- Helper: depth-first search for a child of given class
    local function findDescendantOfClass(root, className)
        local children = root:GetChildren()
        for i = 1, #children do
            local child = children[i]
            if child.ClassName == className then
                return child
            end
            local found = findDescendantOfClass(child, className)
            if found then return found end
        end
        return nil
    end

    -- Find an existing Pants instance or create one on the character
    local pants = findChildOfClass(character, "Pants")
    if not pants then
        pants = Instance.new("Pants")
        pants.Name = "RenderedPants"
        pants.Parent = character
    end

    -- If the loaded asset is a "Pants" instance use its template; if it's a model, try to locate Pants child
    if pantsAsset.ClassName == "Pants" then
        pants.PantsTemplate = pantsAsset.PantsTemplate
    else
        local assetPants = findDescendantOfClass(pantsAsset, "Pants")
        if assetPants then
            pants.PantsTemplate = assetPants.PantsTemplate
        else
            warn("[RenderAvatarAsset] No Pants instance found inside loaded asset; cannot set PantsTemplate")
        end
    end

    -- Print useful pants properties
    print("[RenderAvatarAsset] Pants instance:", pants)
    print("[RenderAvatarAsset]   Name          =", pants.Name)
    print("[RenderAvatarAsset]   ClassName     =", pants.ClassName)
    print("[RenderAvatarAsset]   Parent        =", pants.Parent and pants.Parent:GetFullName() or "nil")
    print("[RenderAvatarAsset]   PantsTemplate =", pants.PantsTemplate)
end

local result, requestedUrls = ThumbnailGenerator:Click(fileExtension, x, y, true)

return result