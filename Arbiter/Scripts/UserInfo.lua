-- Demonstrates using a parameter placeholder for userId
-- Usage: POST /run/UserInfo with JSON body { "userId": 123 }

local userId = {{userId}}
print("Running UserInfo for userId=", userId)

-- do something with userId in Lua...
-- return it so caller sees a value
return userId
