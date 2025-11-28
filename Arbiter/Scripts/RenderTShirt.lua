local jobId = 1
local type = headshot
local format = "PNG"
local x = 400
local y = 400
local baseUrl = "http://www.freblx.xyz"
local userId = 1



local HttpService = game:GetService('HttpService')
HttpService.HttpEnabled = true

game:GetService("InsertService"):SetAssetUrl(baseUrl .. "/asset/?id=%d")
game:GetService("InsertService"):SetAssetVersionUrl(baseUrl .. "/Asset/?assetversionid=%d")
game:GetService("ContentProvider"):SetBaseUrl(baseUrl)
game:GetService("ScriptContext").ScriptsDisabled = true

local Player = game.Players:CreateLocalPlayer(0)
Player.CharacterAppearance = ("http://www.freblx.xyz/asset/?id=4")
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


print(("[%s] Rendering ..."):format(jobId))
local result = game:GetService("ThumbnailGenerator"):Click(format, x, y, true)
print(("[%s] Done!"):format(jobId))

return result