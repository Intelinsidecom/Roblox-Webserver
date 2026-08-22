--[[
    CollectGameServerStatus.lua
    
    Script Used By Roblox-Webserver Arbiter to collect info from an game server
--]]

local reportType = "%reportType%"
local callbackToken = "%callbackToken%"
local arbiterUrl = "%arbiterUrl%"

local Players = game:GetService("Players")
local Workspace = game:GetService("Workspace")
local StatsService = game:GetService("Stats")
local HttpService = game:GetService("HttpService")

print("[CollectGameServerStatus] Executing in job", game.JobId, "Type:", reportType)

local jobId = ""
local placeId = 0
local idSuccess, idResult = pcall(function()
    return {
        job = game.JobId,
        place = game.PlaceId
    }
end)
if idSuccess and idResult then
    jobId = idResult.job or ""
    placeId = idResult.place or 0
end

local allPlayers = Players:GetPlayers()
local totalPlayers = #allPlayers

local authenticatedPlayers = {}
local guestPlayers = {}
local authenticatedCount = 0
local guestCount = 0

for _, player in ipairs(allPlayers) do
    local userId = player.UserId
    local isGuest = false
    if userId < 0 or userId == 0 then
        isGuest = true
    elseif string.sub(player.Name, 1, 6) == "Guest " then
        isGuest = true
    end
    
    local displayName = player.Name
    local dnSuccess, dnResult = pcall(function()
        return player.DisplayName
    end)
    if dnSuccess and dnResult then
        displayName = dnResult
    end
    
    local ping = 0
    local pingSuccess, pingResult = pcall(function()
        if player.GetNetworkPing then
            return player:GetNetworkPing()
        end
        return 0
    end)
    if pingSuccess then
        ping = pingResult or 0
    end
    
    local playerData = {
        Name = player.Name,
        UserId = userId,
        DisplayName = displayName,
        CharacterLoaded = player.Character ~= nil,
        Ping = ping
    }
    
    if isGuest then
        table.insert(guestPlayers, playerData)
        guestCount = guestCount + 1
    else
        table.insert(authenticatedPlayers, playerData)
        authenticatedCount = authenticatedCount + 1
    end
end

local authIdList = {}
for _, p in ipairs(authenticatedPlayers) do
    table.insert(authIdList, tostring(p.UserId))
end

local guestNameList = {}
for _, p in ipairs(guestPlayers) do
    table.insert(guestNameList, p.Name)
end

local serverUptime = 0
if Workspace.DistributedGameTime then
    serverUptime = Workspace.DistributedGameTime
end

local fps = 0
local success, fpsResult = pcall(function()
    if Workspace.GetRealFPS then
        return Workspace:GetRealFPS()
    end
    return 0
end)
if success then
    fps = fpsResult or 0
end

local memoryUsage = 0
local incomingBytes = 0
local outgoingBytes = 0

if StatsService then
    local memSuccess, memResult = pcall(function()
        if StatsService.GetTotalMemoryUsage then
            return StatsService:GetTotalMemoryUsage()
        end
        return 0
    end)
    if memSuccess then
        memoryUsage = memResult or 0
    end
    
    local netSuccess, netResult = pcall(function()
        if StatsService.GetNetworkStats then
            return StatsService:GetNetworkStats()
        end
        return nil
    end)
    if netSuccess and netResult then
        incomingBytes = netResult.IncomingBytes or 0
        outgoingBytes = netResult.OutgoingBytes or 0
    end
end

local avgPing = 0
if totalPlayers > 0 then
    local totalPing = 0
    for _, p in ipairs(allPlayers) do
        local playerPing = 0
        local pSuccess, pResult = pcall(function()
            if p.GetNetworkPing then
                return p:GetNetworkPing()
            end
            return 0
        end)
        if pSuccess then
            playerPing = pResult or 0
        end
        totalPing = totalPing + playerPing
    end
    avgPing = totalPing / totalPlayers
end

local privateServerId = ""
local privateServerOwnerId = 0
local psSuccess, psResult = pcall(function()
    return {
        id = game.PrivateServerId,
        ownerId = game.PrivateServerOwnerId
    }
end)
if psSuccess and psResult then
    privateServerId = psResult.id or ""
    privateServerOwnerId = psResult.ownerId or 0
end

pcall(function()
    if not callbackToken or callbackToken == "" then
        print("[CollectGameServerStatus] ERROR: No callback token provided!")
        return
    end
    
    if not arbiterUrl or arbiterUrl == "" then
        print("[CollectGameServerStatus] ERROR: No arbiter URL provided!")
        return
    end
    local callbackUrl = arbiterUrl .. "/gs/callback"
    local postData = {
        CallbackToken = callbackToken,
        ServerId = jobId,
        PlaceId = placeId,
        PlayerCount = totalPlayers,
        Timestamp = tick()
    }
    
    postData.FPS = fps
    postData.Memory = memoryUsage
    postData.Ping = avgPing
    
    local authPlayerJson = HttpService:JSONEncode(authenticatedPlayers)
    local guestPlayerJson = HttpService:JSONEncode(guestPlayers)
    
    local postData = string.format(
        "CallbackToken=%s&ServerId=%s&PlaceId=%d&PlayerCount=%d&AuthenticatedCount=%d&GuestCount=%d&FPS=%f&Memory=%d&Ping=%f&Timestamp=%f",
        callbackToken,
        jobId,
        placeId,
        totalPlayers,
        authenticatedCount,
        guestCount,
        fps or 0,
        memoryUsage or 0,
        avgPing or 0,
        tick()
    )
    
    postData = postData .. "&AuthenticatedPlayers=" .. HttpService:UrlEncode(authPlayerJson)
    postData = postData .. "&GuestPlayers=" .. HttpService:UrlEncode(guestPlayerJson)
    
    local httpSuccess, httpResult = pcall(function()
        return game:HttpPost(callbackUrl, postData, false, "application/x-www-form-urlencoded")
    end)
end)

local result = {
    gameId = jobId,
    placeId = placeId,
    jobId = jobId,
    reportType = reportType,
    timestamp = tick(),
    serverTime = serverUptime,
    players = {
        total = totalPlayers,
        authenticatedCount = authenticatedCount,
        guestCount = guestCount,
        maxPlayers = Players.MaxPlayers,
        authenticated = authenticatedPlayers,
        guests = guestPlayers,
        authenticatedIds = table.concat(authIdList, ","),
        guestNames = table.concat(guestNameList, ",")
    },
    
    performance = {
        fps = fps,
        memoryUsage = memoryUsage,
        averagePing = avgPing,
        uptime = serverUptime
    },
    
    network = {
        incomingBytes = incomingBytes,
        outgoingBytes = outgoingBytes
    },
    
    serverInfo = {
        privateServerId = privateServerId,
        privateServerOwnerId = privateServerOwnerId
    },
    
    isAlive = true
}

print("[CollectGameServerStatus] Collection complete:")
print("  Total:", totalPlayers, "| Auth:", authenticatedCount, "| Guests:", guestCount)
print("  FPS:", fps > 0 and fps or "N/A", "| Ping:", avgPing)

return result
