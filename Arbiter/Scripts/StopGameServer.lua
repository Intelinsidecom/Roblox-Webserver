local gameId = "%gameId%"

-- Get the network server
local NetworkServer = game:GetService("NetworkServer")
local Players = game:GetService("Players")

-- Get current player count before shutdown
local playerCount = #Players:GetPlayers()

-- Disconnect all players gracefully
for _, player in pairs(Players:GetPlayers()) do
    pcall(function()
        player:Kick("Server shutting down")
    end)
end

-- Wait a moment for disconnections to process
wait(0.5)

-- Stop the network server
local success, error = pcall(function()
    NetworkServer:Stop()
end)

-- Return shutdown status
return {
    status = success and "stopped" or "error",
    gameId = gameId,
    playersDisconnected = playerCount,
    error = success and nil or tostring(error),
    shutdownTime = tick()
}
