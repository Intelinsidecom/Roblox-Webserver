var Roblox=Roblox|| {}

;

Roblox.Constants=function() {
    return {
        http: {
            contentType: "application/json; charset=utf-8", dataType:"json", successStatus:"Success"
        }

        ,
        realTimeNotifications: {
            friendshipNotifications: {

                name:"FriendshipNotifications",
                types: {
                    friendshipCreated: "FriendshipCreated", friendshipDestroyed:"FriendshipDestroyed", friendshipDeclined:"FriendshipDeclined", friendshipRequested:"FriendshipRequested"
                }
            }

            ,
            presenceNotifications: {

                name:"PresenceNotifications",
                types: {
                    presenceOffline: "UserOffline", presenceOnline:"UserOnline"
                }
            }
        }

        ,
        presenceTypes: {
            offline: 0, online:1, inGame:2, inStudio:3
        }
    }
}

();