game:GetService("ContentProvider"):SetBaseUrl("http://www.freblx.xyz")
game:GetService("ScriptContext").ScriptsDisabled = true
game.Players:CreateLocalPlayer(0)
game.Players.Player1:LoadCharacter()
print(game:GetService("ThumbnailGenerator"):Click("PNG", 500, 500, true))