local gameId = "%gameId%"

local NetworkServer = game:GetService("NetworkServer")
local Players = game:GetService("Players")
local Workspace = game:GetService("Workspace")
local StatsService = game:GetService("Stats")

local playerList = {}
for _, player in pairs(Players:GetPlayers()) do
    table.insert(playerList, {
        Name = player.Name,
        UserId = player.UserId,
        Ping = player:GetNetworkPing() or 0,
        CharacterAdded = player.Character ~= nil
    })
end

local networkStats = {}
if NetworkServer then
    networkStats = {
        AveragePing = NetworkServer:GetAveragePing() or 0,
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

return {
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
