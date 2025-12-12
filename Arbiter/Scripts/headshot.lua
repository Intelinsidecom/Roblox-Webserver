local jobId = %jobId%
local type = %type%
local format = "PNG"
local x = %x%
local y = %y%
local baseUrl = "http://www.freblx.xyz"
local userId = %userId%
print(("[%s] Started RenderJob for type '%s' with userId %d ..."):format(jobId, type, userId))



local HttpService = game:GetService('HttpService')
HttpService.HttpEnabled = true

game:GetService("InsertService"):SetAssetUrl(baseUrl .. "/asset/?id=%d")
game:GetService("InsertService"):SetAssetVersionUrl(baseUrl .. "/Asset/?assetversionid=%d")
game:GetService("ContentProvider"):SetBaseUrl(baseUrl)
game:GetService("ScriptContext").ScriptsDisabled = true

local Player = game.Players:CreateLocalPlayer(0)
Player.CharacterAppearance = ("%s/Asset/CharacterFetch.ashx?userId=%d"):format(baseUrl, userId)
print(Player.CharacterAppearance)
Player:LoadCharacter(false)



game:GetService("RunService"):Run()

Player.Character.Animate.Disabled = true
Player.Character.Torso.Anchored = true

local gear = Player.Backpack:GetChildren()[1]
if gear then
    gear.Parent = Player.Character
    Player.Character.Torso["Right Shoulder"].CurrentAngle = math.rad(90)
end

-- Headshot Camera
local FOV = 52.5
local AngleOffsetX = 0
local AngleOffsetY = 0
local AngleOffsetZ = 0

local CameraAngle = Player.Character.Head.CFrame * CFrame.new(AngleOffsetX, AngleOffsetY, AngleOffsetZ)
local CameraPosition = Player.Character.Head.CFrame + Vector3.new(0, 0, 0) + (CFrame.Angles(0, -0.2, 0).lookVector.unit * 3)

local Camera = Instance.new("Camera", Player.Character)
Camera.Name = "ThumbnailCamera"
Camera.CameraType = Enum.CameraType.Scriptable

Camera.CoordinateFrame = CFrame.new(CameraPosition.p, CameraAngle.p)
Camera.FieldOfView = FOV
workspace.CurrentCamera = Camera

print(("[%s] Rendering ..."):format(jobId))
local result = game:GetService("ThumbnailGenerator"):Click(format, x, y, true)
print(("[%s] Done!"):format(jobId))

return result