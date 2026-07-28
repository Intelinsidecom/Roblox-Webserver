// ~/viewapp/pages/messages/directives/messagesNavDirective.js
"use strict";
messages.directive("rbxMessagesNav", ["messagesService", "$location", "$log", "$document", function(n, t, i, r) {
    return {
        restrict: "A",
        scope: !0,
        replace: !0,
        templateUrl: Roblox.websiteTemplates.messagesNavTemplate,
        link: function(u) {
            i.debug("message navigation"), u.markRead = function(t) {
                if (i.debug("========Read======="), u.messageContent.selectedMessageIndexs.length > 0) {
                    var f = [];
                    angular.forEach(u.messageContent.selectedMessageIndexs, function(n) {
                        u.messageContent.messages.data.Collection[n].IsRead = t, f.push(u.messageContent.messages.data.Collection[n].Id)
                    }), u.resetCheckboxStatusByMessageIndexes(), n.beginMarkMessagesRead(f, t).then(function() {
                        r.triggerHandler("Roblox.Messages.CountChanged")
                    })
                }
            }, u.markArchive = function(t) {
                i.debug("======= Archive========");
                var f = [];
                u.messageContent.selectedMessageIndexs.length > 0 ? (angular.forEach(u.messageContent.selectedMessageIndexs, function(n) {
                    f.push(u.messageContent.messages.data.Collection[n].Id)
                }), u.resetCheckboxStatusByMessageIndexes(), n.beginSetArchiveMessages(f, t).then(function() {
                    u.getMessages(u.currentStatus.activeTab, u.currentStatus.currentPage), u.toggleMessagesBox("list"), r.triggerHandler("Roblox.Messages.CountChanged")
                })) : u.messageContent.selectedMessage !== null && (f.push(u.messageContent.selectedMessage.Id), n.beginSetArchiveMessages(f, t).then(function() {
                    u.toggleMessagesBox(u.moduleState.list)
                }))
            }, u.pagination = function(n) {
                var i, r;
                switch (n) {
                    case "prev":
                        i = u.currentStatus.currentPage - 1;
                        break;
                    case "next":
                        i = u.currentStatus.currentPage + 1;
                        break;
                    case "end":
                        i = u.currentStatus.totalPages;
                        break;
                    case "start":
                        i = 1;
                        break;
                    default:
                        i = 1
                }
                r = i > 1 ? {
                    page: i
                } : {}, t.search(r)
            }, u.requestReply = function() {
                u.sendMessage.disableReplyBtn = !0, u.sendMessage.disableSendBtn = !1, u.sendMessage.sendComplete = !1, u.sendMessage.sendResult = {}
            }
        }
    }
}]);