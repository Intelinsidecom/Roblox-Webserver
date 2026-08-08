// ~/viewapp/pages/chat/services/chatHybridService.js
"use strict";
chat.factory("chatHybridService", ["$log", function() {
    function t() {
        return Roblox && Roblox.Hybrid && Roblox.Hybrid.Chat
    }
    return {
        setNewMessageNotification: function(n, i) {
            t() && (angular.isUndefined(i) && (i = function() {}), Roblox.Hybrid.Chat.newMessageNotification(n, i))
        },
        joinGame: function(n, i) {
            t() && (angular.isUndefined(i) && (i = function() {}), Roblox.Hybrid.Game.launchPartyForPlaceId(n, i))
        },
        getTopBarHeight: function(n) {
            t() && Roblox.Hybrid.Chat.getTopBarHeight(n)
        },
        openDialog: function(n) {
            t() && (angular.isUndefined(n) && (n = function() {}), Roblox.Hybrid.Chat.enterConversation(n))
        },
        closeDialog: function(n) {
            t() && (angular.isUndefined(n) && (n = function() {}), Roblox.Hybrid.Chat.leaveConversation(n))
        }
    }
}]);