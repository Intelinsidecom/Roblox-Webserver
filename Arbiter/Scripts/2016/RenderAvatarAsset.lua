-- Avatar clothing renderer v1.2.0

local assetId = %assetId%

local assetUrl = "rbxassetid://" .. assetId
local baseUrl = "https://freblx.com"
local fileExtension = "PNG"
local x, y = 1024, 1024

local ThumbnailGenerator = game:GetService("ThumbnailGenerator")

pcall(function() game:GetService("ContentProvider"):SetBaseUrl(baseUrl) end)
game:GetService("ScriptContext").ScriptsDisabled = true

-- Create a local test player and load a default character
local Player = game.Players:CreateLocalPlayer(0)
--[[Player:LoadCharacter(false)--]]
Player:LoadCharacterBlocking()

local character = Player.Character or Player.CharacterAdded:wait()
print("[RenderAvatarAsset] Player:", Player.Name)
print("[RenderAvatarAsset] Character:", character and character.Name or "nil")

-- Load the asset model (Pants, Shirt, or ShirtGraphic/T-Shirt)
local ok, loaded = pcall(function()
    return game:GetObjects(assetUrl)
end)

if not ok or not loaded or #loaded == 0 then
    warn("[RenderAvatarAsset] Failed to load avatar asset from " .. assetUrl)
else
    local asset = loaded[1]
    print("[RenderAvatarAsset] Loaded asset:", asset.ClassName, asset.Name)

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

    ----------------------------------------------------------------------
    -- 1) Try Pants
    ----------------------------------------------------------------------
    local clothingApplied = false

    local function applyPants()
        local pants = findChildOfClass(character, "Pants")
        if not pants then
            pants = Instance.new("Pants")
            pants.Name = "RenderedPants"
            pants.Parent = character
        end

        if asset.ClassName == "Pants" then
            pants.PantsTemplate = asset.PantsTemplate
            clothingApplied = true
        else
            local assetPants = findDescendantOfClass(asset, "Pants")
            if assetPants then
                pants.PantsTemplate = assetPants.PantsTemplate
                clothingApplied = true
            end
        end

        if clothingApplied then
            print("[RenderAvatarAsset] Applied Pants:", pants)
            print("[RenderAvatarAsset]   PantsTemplate =", pants.PantsTemplate)
        end
    end

    ----------------------------------------------------------------------
    -- 2) Try Shirt
    ----------------------------------------------------------------------
    local function applyShirt()
        local shirt = findChildOfClass(character, "Shirt")
        if not shirt then
            shirt = Instance.new("Shirt")
            shirt.Name = "RenderedShirt"
            shirt.Parent = character
        end

        if asset.ClassName == "Shirt" then
            shirt.ShirtTemplate = asset.ShirtTemplate
            clothingApplied = true
        else
            local assetShirt = findDescendantOfClass(asset, "Shirt")
            if assetShirt then
                shirt.ShirtTemplate = assetShirt.ShirtTemplate
                clothingApplied = true
            end
        end

        if clothingApplied then
            print("[RenderAvatarAsset] Applied Shirt:", shirt)
            print("[RenderAvatarAsset]   ShirtTemplate =", shirt.ShirtTemplate)
        end
    end

    ----------------------------------------------------------------------
    -- 3) Try T-Shirt (ShirtGraphic)
    ----------------------------------------------------------------------
    local function applyTShirt()
        local tshirt = findChildOfClass(character, "ShirtGraphic")
        if not tshirt then
            tshirt = Instance.new("ShirtGraphic")
            tshirt.Name = "RenderedTShirt"
            tshirt.Parent = character
        end

        if asset.ClassName == "ShirtGraphic" then
            tshirt.Graphic = asset.Graphic
            clothingApplied = true
        else
            local assetGraphic = findDescendantOfClass(asset, "ShirtGraphic")
            if assetGraphic then
                tshirt.Graphic = assetGraphic.Graphic
                clothingApplied = true
            end
        end

        if clothingApplied then
            print("[RenderAvatarAsset] Applied T-Shirt (ShirtGraphic):", tshirt)
            print("[RenderAvatarAsset]   Graphic =", tshirt.Graphic)
        end
    end

    -- Prefer Pants, then Shirt, then T-Shirt
    applyPants()
    if not clothingApplied then
        applyShirt()
    end
    if not clothingApplied then
        applyTShirt()
    end

    if not clothingApplied then
        warn("[RenderAvatarAsset] No supported clothing instance (Pants, Shirt, ShirtGraphic) found inside loaded asset;")
    end
end

local result, requestedUrls = ThumbnailGenerator:Click(fileExtension, x, y, true)

return result