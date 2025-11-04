// ~/viewapp/common/services/hybridService.js
"use strict";
robloxAppService.factory("hybridService", ["$log", function() {
    function t() {
        return Roblox && Roblox.Hybrid
    }
    return {
        startChatConversation: function(n, i) {
            t() && Roblox.Hybrid.Chat && (angular.isUndefined(i) && (i = function() {}), Roblox.Hybrid.Chat.startChatConversation(n, i))
        },
        startWebChatConversation: function(n, i) {
            t() && Roblox.Hybrid.Navigation && (angular.isUndefined(i) && (i = function() {}), Roblox.Hybrid.Navigation.startWebChatConversation(n, i))
        },
        navigateToFeature: function(n, i) {
            t() && Roblox.Hybrid.Navigation && (angular.isUndefined(i) && (i = function() {}), Roblox.Hybrid.Navigation.navigateToFeature(n, i))
        },
        openUserProfile: function(n, i) {
            t() && Roblox.Hybrid.Navigation && (angular.isUndefined(i) && (i = function() {}), Roblox.Hybrid.Navigation.openUserProfile(n, i))
        }
    }
}]);