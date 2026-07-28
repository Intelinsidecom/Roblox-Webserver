// ~/viewapp/pages/messages/directives/messagesTabsDirective.js
"use strict";
messages.directive("rbxTabs", ["$log", function(n) {
    return {
        restrict: "A",
        scope: !0,
        replace: !0,
        templateUrl: Roblox.websiteTemplates.tabsTemplate,
        link: function(t) {
            n.debug("..start roblox tabs .."), t.tabs = t.MESSAGETABS, t.currentStatus.moduleState = t.moduleState.list, t.onClickTab = function(n) {
                t.currentStatus.moduleState === t.moduleState.detail && (t.currentStatus.isSingleMessageDetail || (t.currentStatus.loadMessages = !1), t.currentStatus.moduleState = t.moduleState.list), n.name !== t.currentStatus.activeTab && t.resetMessageContent();
                var r = new Date(Number(Roblox.messagesModel.minimumAdRefreshInterval)),
                    i = new Date;
                i > Roblox.messagesModel.lastAdRefresh.getTime() + r.getTime() && (googletag.cmd.push(function() {
                    googletag.pubads().refresh()
                }), Roblox.messagesModel.lastAdRefresh = i)
            }
        }
    }
}]);