// ~/viewapp/common/services/hybridService.js
"use strict";
freebloxiaAppService.factory("hybridService", ["$log", function() {
    function t() {
        return Freebloxia && Freebloxia.Hybrid
    }
    return {
        startChatConversation: function(n, i) {
            t() && Freebloxia.Hybrid.Chat && (angular.isUndefined(i) && (i = function() {}), Freebloxia.Hybrid.Chat.startChatConversation(n, i))
        },
        startWebChatConversation: function(n, i) {
            t() && Freebloxia.Hybrid.Navigation && (angular.isUndefined(i) && (i = function() {}), Freebloxia.Hybrid.Navigation.startWebChatConversation(n, i))
        },
        navigateToFeature: function(n, i) {
            t() && Freebloxia.Hybrid.Navigation && (angular.isUndefined(i) && (i = function() {}), Freebloxia.Hybrid.Navigation.navigateToFeature(n, i))
        },
        openUserProfile: function(n, i) {
            t() && Freebloxia.Hybrid.Navigation && (angular.isUndefined(i) && (i = function() {}), Freebloxia.Hybrid.Navigation.openUserProfile(n, i))
        }
    }
}]);