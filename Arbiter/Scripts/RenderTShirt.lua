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
Player.CharacterAppearance = ("http://www.freblx.xyz/Asset/CharacterFetch.ashx?userId=1")
print(Player.CharacterAppearance)
Player:LoadCharacter(false)
while not Player.AppearanceDidLoad do
    wait(0.1)
end



Player.Character.Animate.Disabled = true
Player.Character.Torso.Anchored = true

local gear = Player.Backpack:GetChildren()[1]
if gear and char then
    gear.Parent = char
    local torso = char:FindFirstChild("Torso")
    if torso then
        local rs = torso:FindFirstChild("Right Shoulder")
        if rs then
            rs.CurrentAngle = math.rad(90)
        end
    end
end

local result = game:GetService("ThumbnailGenerator"):Click(format, x, y, true)
print(result)

return result