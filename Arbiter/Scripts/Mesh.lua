local fileExtension= "PNG"
local x = %x%
local y = %y%
local baseUrl = "http://www.freblx.xyz"
local assetUrl =%assetUrl%

pcall(function() game:GetService("ContentProvider"):SetBaseUrl(baseUrl) end)

game:GetService("ScriptContext").ScriptsDisabled = true

local part = Instance.new("Part")
part.Parent = workspace

local specialMesh = Instance.new("SpecialMesh")
specialMesh.MeshId = assetUrl
specialMesh.Parent = part

return game:GetService("ThumbnailGenerator"):Click(fileExtension, x, y, --[[hideSky = ]] true, --[[crop = ]] true)