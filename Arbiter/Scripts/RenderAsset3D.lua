local jobId = %jobId%
local assetId = %assetId%
local x = %x%
local y = %y%
local baseUrl = "%baseUrl%"

print(("[%s] RenderAsset3D started for assetId=%d, x=%d, y=%d"):format(jobId, assetId, x, y))

local HttpService = game:GetService('HttpService')
HttpService.HttpEnabled = true

game:GetService("InsertService"):SetAssetUrl(baseUrl .. "/asset/?id=%d")
game:GetService("InsertService"):SetAssetVersionUrl(baseUrl .. "/Asset/?assetversionid=%d")
game:GetService("ContentProvider"):SetBaseUrl(baseUrl)
game:GetService("ScriptContext").ScriptsDisabled = true

local Player = game.Players:CreateLocalPlayer(0)
Player:LoadCharacter(false)

local character = Player.Character or Player.CharacterAdded:wait()
print(("[%s] Character loaded: %s"):format(jobId, character and character.Name or "nil"))

local assetUrl = "rbxassetid://" .. assetId
local ok, loaded = pcall(function()
    return game:GetObjects(assetUrl)
end)

local clothingApplied = false
local assetInserted = false

if ok and loaded and #loaded > 0 then
    local asset = loaded[1]
    print(("[%s] Loaded asset: %s %s"):format(jobId, asset.ClassName, asset.Name))

    local function findChildOfClass(parent, className)
        local children = parent:GetChildren()
        for i = 1, #children do
            if children[i].ClassName == className then
                return children[i]
            end
        end
        return nil
    end

    -- Try applying clothing: Pants, Shirt, ShirtGraphic
    if asset.ClassName == "Pants" then
        local pants = findChildOfClass(character, "Pants")
        if not pants then
            pants = Instance.new("Pants")
            pants.Name = "RenderedPants"
            pants.Parent = character
        end
        pants.PantsTemplate = asset.PantsTemplate
        clothingApplied = true
    elseif asset.ClassName == "Shirt" then
        local shirt = findChildOfClass(character, "Shirt")
        if not shirt then
            shirt = Instance.new("Shirt")
            shirt.Name = "RenderedShirt"
            shirt.Parent = character
        end
        shirt.ShirtTemplate = asset.ShirtTemplate
        clothingApplied = true
    elseif asset.ClassName == "ShirtGraphic" then
        local tshirt = findChildOfClass(character, "ShirtGraphic")
        if not tshirt then
            tshirt = Instance.new("ShirtGraphic")
            tshirt.Name = "RenderedTShirt"
            tshirt.Parent = character
        end
        tshirt.Graphic = asset.Graphic
        clothingApplied = true
    end

    -- For non-clothing assets, try to equip as a child of the character
    if not clothingApplied then
        local success = pcall(function()
            asset.Parent = character
        end)
        if success then
            assetInserted = true
            print(("[%s] Inserted asset %s into character"):format(jobId, asset.ClassName))
        else
            print(("[%s] Could not insert %s into character, trying standalone"):format(jobId, asset.ClassName))
            local success2 = pcall(function()
                asset.Parent = workspace
                asset:MoveTo(Vector3.new(0, 0, 0))
            end)
            if success2 then
                assetInserted = true
            end
        end
    end
end

if not clothingApplied and not assetInserted then
    print(("[%s] No asset applied, rendering plain character"):format(jobId))
end

game:GetService("RunService"):Run()

local animate = character:FindFirstChild("Animate")
if animate then
    animate.Disabled = true
end
local torso = character:FindFirstChild("Torso") or character:FindFirstChild("UpperTorso")
if torso then
    torso.Anchored = true
end

print(("[%s] Rendering OBJ ..."):format(jobId))
local result = game:GetService("ThumbnailGenerator"):Click("OBJ", x, y, true)
print(("[%s] Done!"):format(jobId))

return result
