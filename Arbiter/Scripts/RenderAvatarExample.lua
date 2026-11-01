local jobId = 360
local type = "avatar"
local format = "PNG"
local x = 1080
local y = 1080
local baseUrl = "https://www.freblx.com"
local userId = 1
print(("[%s] Started RenderJob for type '%s' with userId %d ..."):format(jobId, type, userId))
local characterAppearanceUrl = "https://api.freblx.com/v1.0/avatar-fetch/?userId=1"
pcall(function() game:GetService("ContentProvider"):SetBaseUrl(baseUrl) end)
game:GetService("ScriptContext").ScriptsDisabled = true
print("1")
local player = game:GetService("Players"):CreateLocalPlayer(0)
print("2")
player.CharacterAppearance = characterAppearanceUrl
print("3")
player:LoadCharacterBlocking()
print("4")
print("5")
local result = game:GetService("ThumbnailGenerator"):Click(format, x, y, --[[hideSky = ]] true)
print(result)
return result