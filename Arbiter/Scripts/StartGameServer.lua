-----------------------------------"CUSTOM" SHARED CODE----------------------------------
	
	pcall(function() settings().Network.UseInstancePacketCache = true end)
	pcall(function() settings().Network.UsePhysicsPacketCache = true end)
	--pcall(function() settings()["Task Scheduler"].PriorityMethod = Enum.PriorityMethod.FIFO end)
	pcall(function() settings()["Task Scheduler"].PriorityMethod = Enum.PriorityMethod.AccumulatedError end)
	
	--settings().Network.PhysicsSend = 1 -- 1==RoundRobin
	--settings().Network.PhysicsSend = Enum.PhysicsSendMethod.ErrorComputation2
	settings().Network.PhysicsSend = Enum.PhysicsSendMethod.TopNErrors
	settings().Network.ExperimentalPhysicsEnabled = true
	settings().Network.WaitingForCharacterLogRate = 100
	pcall(function() settings().Diagnostics:LegacyScriptMode() end)
	
	-----------------------------------START GAME SHARED SCRIPT------------------------------
	
	local placeId = tonumber(%placeId%)
	local port = tonumber(%port%)
	local url = "%baseUrl%"
	local accessKey = "%accessKey%"
	local jobId = "%gameId%"
	
	local assetId = placeId -- might be able to remove this now

	local scriptContext = game:GetService('ScriptContext')
	pcall(function() scriptContext:AddStarterScript(37801172) end)
	scriptContext.ScriptsDisabled = true
	
	game:SetPlaceID(assetId, false)
	game:GetService("ChangeHistoryService"):SetEnabled(false)
	
	-- establish this peer as Server
	local ns = game:GetService("NetworkServer")
	
	if url~=nil then
		pcall(function() game:GetService("Players"):SetAbuseReportUrl(url .. "/AbuseReport/InGameChatHandler.ashx") end)
		pcall(function() game:GetService("ScriptInformationProvider"):SetAssetUrl(url .. "/Asset/") end)
		pcall(function() game:GetService("ContentProvider"):SetBaseUrl(url .. "/") end)
		--pcall(function() game:GetService("Players"):SetChatFilterUrl(url .. "/Game/ChatFilter.ashx") end)
	
		game:GetService("BadgeService"):SetPlaceId(placeId)
	
		game:GetService("BadgeService"):SetIsBadgeLegalUrl("")
		game:GetService("InsertService"):SetBaseSetsUrl(url .. "/Game/Tools/InsertAsset.ashx?nsets=10&type=base")
		game:GetService("InsertService"):SetUserSetsUrl(url .. "/Game/Tools/InsertAsset.ashx?nsets=20&type=user&userid=%d")
		game:GetService("InsertService"):SetCollectionUrl(url .. "/Game/Tools/InsertAsset.ashx?sid=%d")
		game:GetService("InsertService"):SetAssetUrl(url .. "/Asset/?id=%d")
		game:GetService("InsertService"):SetAssetVersionUrl(url .. "/Asset/?assetversionid=%d")
		
		pcall(function() loadfile(url .. "/Game/LoadPlaceInfo.ashx?PlaceId=" .. placeId)() end)
		
		-- pcall(function() 
		--			if access then
		--				loadfile(url .. "/Game/PlaceSpecificScript.ashx?PlaceId=" .. placeId .. "&" .. access)()
		--			end
		--		end)
	end
	
	pcall(function() game:GetService("NetworkServer"):SetIsPlayerAuthenticationRequired(false) end) -- REMEMBER TO ENABLE!!
	settings().Diagnostics.LuaRamLimit = 0
	--settings().Network:SetThroughputSensitivity(0.08, 0.01)
	--settings().Network.SendRate = 35
	--settings().Network.PhysicsSend = 0  -- 1==RoundRobin
	
	local maxPlayers = tonumber(%maxPlayers%)
	
	wait(0.1)
	pcall(function()
		game:GetService("Players").MaxPlayers = maxPlayers
	end)
	
	game:GetService("Players").PlayerAdded:connect(function(player)
		print("Player " .. player.userId .. " added")
		
		if player.userId > 0 then
			pcall(function()
				local postUrl = url .. "/Game/Joined"
				local postData = "userId=" .. tostring(player.userId) .. "&placeId=" .. tostring(placeId) .. "&jobId=" .. jobId .. "&token=" .. accessKey
				game:HttpPost(postUrl, postData, false, "application/x-www-form-urlencoded")
			end)
		end
	end)

	game:GetService("Players").PlayerRemoving:connect(function(player)
		print("Player " .. player.userId .. " leaving")
		
		if player.userId > 0 then
			pcall(function()
				local postUrl = url .. "/Game/Left"
				local postData = "userId=" .. tostring(player.userId) .. "&placeId=" .. tostring(placeId) .. "&jobId=" .. jobId .. "&token=" .. accessKey
				game:HttpPost(postUrl, postData, false, "application/x-www-form-urlencoded")
			end)
		end
	end)
	
	if placeId~=nil and url~=nil then
		-- yield so that file load happens in heartbeat thread
		wait()
		
		-- load game
		game:Load(url .. "/asset/?id=" .. placeId)
	end
	
	-- Now start connection
	ns:Start(port) 
	
	
	scriptContext:SetTimeout(10)
	scriptContext.ScriptsDisabled = false
	
	
	
	------------------------------END START GAME SHARED SCRIPT--------------------------
	
	
	
	-- StartGame -- 
	game:GetService("RunService"):Run()
	
	-- Return server information
	return {
		status = "started",
		gameId = tostring(game.PlaceId),
		placeId = placeId,
		port = port,
		maxPlayers = maxPlayers,
		baseUrl = url,
		serverTime = workspace.DistributedGameTime,
		startTime = tick()
	}
