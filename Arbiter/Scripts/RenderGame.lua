-- Place v1.0.2

--[[assetUrl, fileExtension, x, y, baseUrl, universeId = ... --]]
local assetUrl = "http://www.freblx.xyz/asset/?id=%placeId%"
local fileExtension = "PNG"
local x = %x%
local y = %y%
local baseUrl = "http://www.freblx.xyz"
local universeId = 4

pcall(function() game:GetService("ContentProvider"):SetBaseUrl(baseUrl) end)
if universeId ~= nil then
	pcall(function() game:SetUniverseId(universeId) end)
end

local HttpService = game:GetService('HttpService')
HttpService.HttpEnabled = true

game:GetService("InsertService"):SetAssetUrl(baseUrl .. "/asset/?id=%d")
game:GetService("InsertService"):SetAssetVersionUrl(baseUrl .. "/Asset/?assetversionid=%d")
game:GetService("ContentProvider"):SetBaseUrl(baseUrl)
game:GetService("ScriptContext").ScriptsDisabled = true

game:GetService("ScriptContext").ScriptsDisabled = true
game:GetService("StarterGui").ShowDevelopmentGui = false

game:Load(assetUrl)

game:GetService("RunService"):Run()

-- Do this after again loading the place file to ensure that these values aren't changed when the place file is loaded.
game:GetService("ScriptContext").ScriptsDisabled = true
game:GetService("StarterGui").ShowDevelopmentGui = false
local game = game:GetService("ThumbnailGenerator"):Click(fileExtension, x, y, false)
return game