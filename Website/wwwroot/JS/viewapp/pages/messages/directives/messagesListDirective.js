// ~/viewapp/pages/messages/directives/messagesListDirective.js
"use strict";
messages.directive("rbxMessagesList", ["messagesService", "$location", "$log", function(n, t, i) {
    return {
        restrict: "A",
        scope: !0,
        replace: !0,
        templateUrl: Roblox.websiteTemplates.messagesListTemplate,
        link: function(n) {
            i.debug("============== message list =============="), n.$watch(function() {
                return n.messageContent.messages
            }, function(r) {
                var f, e;
                !r.hasError && n.messageContent.loadingComplete && (i.debug("  ========== roblox message page =========== "), f = r.data ? +r.data.PageNumber + 1 : 1, n.currentStatus.activeTab == "notifications" ? n.notifications = r.data : n.messages = r.data, t.search().page != f && (e = f > 1 ? {
                    page: f
                } : {}, t.search(e)))
            }, !1)
        }
    }
}]);