-----------------------------------NETWORK SETTINGS----------------------------------
pcall(function() settings().Network.UseInstancePacketCache = true end)
pcall(function() settings().Network.UsePhysicsPacketCache = true end)
settings().Network.PhysicsSend = Enum.PhysicsSendMethod.TopNErrors
settings().Network.ExperimentalPhysicsEnabled = true
settings().Network.WaitingForCharacterLogRate = 100
pcall(function() settings().Diagnostics:LegacyScriptMode() end)

-----------------------------------CLOUD EDIT SERVER SETUP------------------------------

local placeId = tonumber(%placeId%)
local port = tonumber(%port%)
local url = "%baseUrl%"
local apiUrl = string.gsub(url, "www%.", "api.")
local jobId = "%gameId%"
local accessKey = "%accessKey%"

local scriptContext = game:GetService('ScriptContext')
pcall(function() scriptContext:AddStarterScript(37801172) end)
scriptContext.ScriptsDisabled = true

game:SetPlaceID(placeId, false)
game:GetService("ChangeHistoryService"):SetEnabled(false)

local ok, err = pcall(function()
    game:GetService("NetworkServer"):ConfigureAsCloudEditServer()
end)
if not ok then
    warn("[StartCloudEditServer] ConfigureAsCloudEditServer failed: " .. tostring(err))
end

if url ~= nil then
    pcall(function() game:GetService("ScriptInformationProvider"):SetAssetUrl(url .. "/Asset/") end)
    pcall(function() game:GetService("ContentProvider"):SetBaseUrl(url .. "/") end)
    game:GetService("BadgeService"):SetPlaceId(placeId)
    game:GetService("BadgeService"):SetAwardBadgeUrl(apiUrl .. "/badges/award?userId=%d&badgeId=%d&placeId=%d")
    game:GetService("BadgeService"):SetHasBadgeUrl(apiUrl .. "/badges/has-badge?userId=%d&badgeId=%d")
    game:GetService("BadgeService"):SetIsBadgeDisabledUrl(apiUrl .. "/badges/is-disabled?badgeId=%d&placeId=%d")
    game:GetService("BadgeService"):SetIsBadgeLegalUrl(apiUrl .. "/badges/is-legal?badgeId=%d&placeId=%d")
    game:GetService("InsertService"):SetBaseSetsUrl(url .. "/Game/Tools/InsertAsset.ashx?nsets=10&type=base")
    game:GetService("InsertService"):SetUserSetsUrl(url .. "/Game/Tools/InsertAsset.ashx?nsets=20&type=user&userid=%d")
    game:GetService("InsertService"):SetCollectionUrl(url .. "/Game/Tools/InsertAsset.ashx?sid=%d")
    game:GetService("InsertService"):SetAssetUrl(url .. "/Asset/?id=%d")
    game:GetService("InsertService"):SetAssetVersionUrl(url .. "/Asset/?assetversionid=%d")

    pcall(function() loadfile(url .. "/Game/LoadPlaceInfo.ashx?PlaceId=" .. placeId)() end)
end

pcall(function() game:GetService("NetworkServer"):SetIsPlayerAuthenticationRequired(false) end)
settings().Diagnostics.LuaRamLimit = 0

local maxPlayers = tonumber(%maxPlayers%)

wait(0.1)
pcall(function()
    game:GetService("Players").MaxPlayers = maxPlayers
end)

-- Track players for status reporting
game:GetService("Players").PlayerAdded:connect(function(player)
    print("[CloudEdit] Player " .. player.userId .. " connected")
    if player.userId > 0 then
        pcall(function()
            local postUrl = url .. "/Game/Joined"
            local postData = "userId=" .. tostring(player.userId) .. "&placeId=" .. tostring(placeId) .. "&jobId=" .. jobId .. "&token=" .. accessKey
            game:HttpPost(postUrl, postData, false, "application/x-www-form-urlencoded")
        end)
    end
end)

game:GetService("Players").PlayerRemoving:connect(function(player)
    print("[CloudEdit] Player " .. player.userId .. " disconnected")
    if player.userId > 0 then
        pcall(function()
            local postUrl = url .. "/Game/Left"
            local postData = "userId=" .. tostring(player.userId) .. "&placeId=" .. tostring(placeId) .. "&jobId=" .. jobId .. "&token=" .. accessKey
            game:HttpPost(postUrl, postData, false, "application/x-www-form-urlencoded")
        end)
    end
end)

if placeId ~= nil and url ~= nil then
    wait()
    game:Load(url .. "/asset/?id=" .. placeId)
end

local ns = game:GetService("NetworkServer")
ns:Start(port)

scriptContext:SetTimeout(10)
scriptContext.ScriptsDisabled = false

game:GetService("RunService"):Run()

return {
    status = "started",
    gameId = jobId,
    placeId = placeId,
    port = port,
    maxPlayers = maxPlayers,
    baseUrl = url,
    serverTime = workspace.DistributedGameTime,
    startTime = tick()
}
