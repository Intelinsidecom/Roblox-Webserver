local gameId = "%gameId%"

local NetworkServer = game:GetService("NetworkServer")
local Players = game:GetService("Players")
local Workspace = game:GetService("Workspace")
local StatsService = game:GetService("Stats")

print("=== GetGameServerStatus Debug ===")
print("GameId:", gameId)
print("Players service:", Players)
print("NetworkServer:", NetworkServer)

local playerList = {}
local totalPing = 0
for _, player in pairs(Players:GetPlayers()) do
    print("Found player:", player.Name, "UserId:", player.UserId)
    local ping = player:GetNetworkPing() or 0
    totalPing = totalPing + ping
    table.insert(playerList, {
        Name = player.Name,
        UserId = player.UserId,
        Ping = ping,
        CharacterAdded = player.Character ~= nil
    })
end

print("Total players found:", #playerList)
print("Player list:", playerList)

local networkStats = {}
if NetworkServer then
    local averagePing = #playerList > 0 and (totalPing / #playerList) or 0
    networkStats = {
        AveragePing = averagePing,
        IncomingBytes = StatsService and StatsService:GetNetworkStats().IncomingBytes or 0,
        OutgoingBytes = StatsService and StatsService:GetNetworkStats().OutgoingBytes or 0
    }
end

local performanceStats = {
    FPS = Workspace:GetRealFPS(),
    ServerTime = Workspace.DistributedGameTime,
    MemoryUsage = StatsService and StatsService:GetTotalMemoryUsage() or 0
}

local serverConfig = {
    MaxPlayers = Players.MaxPlayers,
    PlaceId = game.PlaceId,
    JobId = game.JobId,
    ServerInstanceId = game.JobId,
    PrivateServerId = game.PrivateServerId,
    PrivateServerOwnerId = game.PrivateServerOwnerId
}

local result = {
    gameId = gameId,
    status = "running",
    timestamp = tick(),
    players = {
        count = #playerList,
        maxPlayers = serverConfig.MaxPlayers,
        list = playerList
    },

    network = networkStats,
    performance = performanceStats,
    configuration = serverConfig,
    placeId = serverConfig.PlaceId,
    serverAge = Workspace.DistributedGameTime,
    isPrivateServer = serverConfig.PrivateServerId ~= nil and serverConfig.PrivateServerId ~= ""
}

print("=== Final Result ===")
print("Result structure:", result)
print("Players count:", result.players.count)
print("Players list length:", #result.players.list)

return result
