// ~/viewapp/common/constants/localStorageNames.js
robloxApp.constant("localStorageNames", {
    friendsDict: Roblox && Roblox.CurrentUser ? "Roblox.FriendsDict.UserId" + Roblox.CurrentUser.userId : "Roblox.FriendsDict.UserId0"
});