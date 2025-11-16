"use strict";

robloxAppService.factory("chatDispatchService", ["hybridService", "$document", "$log", function(n, t) {
        return {
            startChat:function(i, r) {
                if(r.androidApp.isEnabled&&r.androidApp.hybridRequired) {
                    var u= {
                        userIds:[]
                    }

                    ; u.userIds.push(i), n.startChatConversation(u)
                }

                else r.iOSApp.isEnabled&&r.iOSApp.hybridRequired?n.startWebChatConversation(i):r.uwpApp.isEnabled&&r.uwpApp.hybridRequired?n.startWebChatConversation(i):t.triggerHandler("Roblox.Chat.StartChat", {
                    userId:i
                })
        }

        , buildPermissionVerifier:function(n) {
            return {
                androidApp: {
                    isEnabled:n.inAndroidApp, hybridRequired: !0
                }

                , iOSApp: {
                    isEnabled:n.iniOSApp, hybridRequired: !0
                }

                , uwpApp: {
                    isEnabled:n.inUWPApp, hybridRequired: !1
                }
            }
        }
    }
}

]);