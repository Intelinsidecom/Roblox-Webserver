
local characterAppearanceUrl = "https://www.freblx.com/Asset/CharacterFetch.ashx?userId=1"
local baseUrl = "http://www.freblx.com"
local x = 100
local y = 100
local format = "PNG"
local userId = 1
local ContentProvider = game:GetService("ContentProvider")
game:GetService('StarterGui'):SetCoreGuiEnabled(Enum.CoreGuiType.All, false);

game:GetService("RunService"):Run()

local Player = game.Players:CreateLocalPlayer(0)
Player.CharacterAppearance = ("https://api.freblx.com/v1/avatar-fetch?placeId=15&userId=1")
print(Player.CharacterAppearance)
Player:LoadCharacterBlocking()
--[[Player:LoadCharacter(false)--]]

Player.Character.Animate.Disabled = true
Player.Character.Torso.Anchored = true

local result = game:GetService("ThumbnailGenerator"):Click(format, x, y, true)
print(result)

return result