// ~/viewapp/pages/messages/controllers/messagesController.js
"use strict";
messages.controller("messagesController", ["$scope", "messagesService", "tabs", "moduleState", "$log", "$document", "$http", function(n, t, i, r, u, f, e) {
    u.debug("scope from messagesController "), n.tabs = i, n.moduleState = r, n.MESSAGETABS = [{
        name: n.tabs.inbox.name,
        label: n.tabs.inbox.label,
        count: Roblox.messagesModel.totalUnreadMessages
    }, n.tabs.sent, {
        name: n.tabs.news.name,
        label: n.tabs.news.label,
        count: Roblox.messagesModel.totalAnnouncements
    }, n.tabs.archive], n.MESSAGEDEFAULTS = {
        robloxUserId: Roblox.messageDefaults.robloxUserId,
        systemUserId: 1,
        robloxUserName: Roblox.messageDefaults.robloxUserName,
        robloxUserThumbnail: {
            Url: Roblox.messageDefaults.robloxUserThumbnail,
            Final: !0
        },
        robloxSystemThumbnail: {
            Url: Roblox.messagesModel.adminIconUrl,
            Final: !0
        }
    }, n.currentStatus = n.currentStatus ? n.currentStatus : {
        activeTab: n.MESSAGETABS[0].name,
        currentPage: 1,
        totalPages: 1,
        moduleState: r.list,
        messageIdx: null,
        conversationId: null,
        loadMessages: !0,
        isSingleMessageDetail: !1
    }, n.messageContent = {
        messages: {},
        selectedMessageIndexs: [],
        selectedMessage: null,
        selectedAll: !1,
        loadingComplete: !1,
        messageDict: {}
    }, n.resetCurrentStatus = function() {
        n.currentStatus.isSingleMessageDetail && (n.currentStatus.isSingleMessageDetail = !1)
    }, n.resetMessageContent = function() {
        n.messageContent && (n.messageContent.messages = {}, n.messageContent.selectedMessageIndexs = [], n.messageContent.selectedMessage = null), n.resetCurrentStatus()
    }, n.getMessageDetailById = function() {
        n.resetMessageContent(), t.setPage(1), n.messageContent.loadingComplete = !1, n.currentStatus.loadMessages = !1, n.currentStatus.isSingleMessageDetail = !0, t.getMessageDetailById(n.currentStatus.conversationId).then(function(i) {
            i && (n.messageContent.loadingComplete = !0, n.currentStatus.loadMessages = !0, n.currentStatus.conversationId = null, n.currentStatus.moduleState = r.detail, n.messageContent.selectedMessage = i, n.messageContent.selectedMessage.IsRead || t.beginMarkMessagesRead([n.messageContent.selectedMessage.Id], !0, !1), f.triggerHandler("Roblox.Messages.CountChanged"))
        }, function(t) {
            n.currentStatus.currentPage = 1, n.currentStatus.totalPages = 1, n.messageContent.loadingComplete = !0, n.messageContent.messages = {
                data: t,
                hasError: !0
            }
        })
    }, n.getMessages = function(i, u) {
        n.messageContent.loadingComplete = !1, t.setTab(i), t.setPage(u - 1), t.beginUpdateMessages().then(function(i) {
            var e = i.PageNumber ? +i.PageNumber + 1 : 1,
                u;
            n.currentStatus.currentPage = e, n.currentStatus.totalPages = i.TotalPages ? i.TotalPages : 1, n.messageContent.loadingComplete = !0, n.messageContent.messages = {
                data: i,
                hasError: !1
            }, angular.forEach(i.Collection, function(t) {
                n.messageContent.messageDict[t.Id] = t
            }), u = n.currentStatus.messageIdx, u != null ? (n.currentStatus.moduleState = r.detail, n.messageContent.selectedMessage = i.Collection[u], n.messageContent.selectedMessage.IsRead || t.beginMarkMessagesRead([n.messageContent.selectedMessage.Id], !0, !1)) : n.currentStatus.moduleState = r.list, f.triggerHandler("Roblox.Messages.CountChanged")
        }, function(t) {
            n.currentStatus.currentPage = 1, n.currentStatus.totalPages = 1, n.messageContent.loadingComplete = !0, n.messageContent.messages = {
                data: t,
                hasError: !0
            }
        })
    }, n.refreshCounts = function() {
        e.get(Roblox.websiteLinks.GetMyUnreadMessagesCountLink).then(function(e) {
            n.MESSAGETABS[0].count = e.data.count || 0
        }), e.get("/v2/stream-notifications/unread-count").then(function(e) {
            n.MESSAGETABS[2].count = e.data.unreadNotifications || 0
        })
    }, f.on("Roblox.Messages.CountChanged", function() {
        n.$applyAsync(function() {
            n.refreshCounts()
        })
    }), n.$on("$stateChangeSuccess", function(t, i, r) {
        if (u.debug(" --- receiving route is changing --- "), n.currentStatus.loadMessages) {
            var o = r.page ? r.page : 1,
                s = angular.isDefined(r.messageIdx) ? r.messageIdx : null;
            n.currentStatus.activeTab = i.name, n.currentStatus.messageIdx = s, angular.isDefined(r.conversationId) ? (n.currentStatus.conversationId = r.conversationId, n.getMessageDetailById(i.name, o)) : n.getMessages(i.name, o)
        } else n.currentStatus.loadMessages = !0
    })
}]);