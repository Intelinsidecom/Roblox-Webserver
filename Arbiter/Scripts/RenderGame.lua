local format = "PNG"
local x = %x%
local y = %y%
local baseUrl = "http://www.freblx.com"
universeId = 1
local assetUrl = "http://www.freblx.com/asset/?id=%placeId%"
local HttpService = game:GetService('HttpService')
HttpService.HttpEnabled = true
pcall(function() game:GetService("ContentProvider"):SetBaseUrl(baseUrl) end)
if universeId ~= nil then
	pcall(function() game:SetUniverseId(universeId) end)
end

game:GetService("ScriptContext").ScriptsDisabled = true
game:GetService("StarterGui").ShowDevelopmentGui = false

game:Load(assetUrl)

-- Do this after again loading the place file to ensure that these values aren't changed when the place file is loaded.
game:GetService("ScriptContext").ScriptsDisabled = true
game:GetService("StarterGui").ShowDevelopmentGui = false

return game:GetService("ThumbnailGenerator"):Click(format, x, y, --[[hideSky = ]] false)