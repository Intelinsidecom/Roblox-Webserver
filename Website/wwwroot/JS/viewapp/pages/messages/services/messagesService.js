// ~/viewapp/pages/messages/services/messagesService.js
"use strict";
messages.factory("messagesService", ["httpService", "$log", function(n, t) {
    var i = {
        MAXROWS: 20,
        HTTPCALLFAILS: "fails ",
        currentTab: null,
        currentPage: 0,
        sentMessageComplete: !1,
        sendMessageError: {
            hasError: !1,
            details: ""
        },
        tabNameToId: function(n) {
            switch (n) {
                case "inbox":
                    return 0;
                case "sent":
                    return 1;
                case "notifications":
                    return 2;
                case "archive":
                    return 3;
                default:
                    return 0
            }
        },
        setTab: function(n) {
            var r = i.tabNameToId(n);
            return r >= 0 && r < 4 ? (i.currentTab = r, !0) : (i.currentTab = 0, t.debug("Invalid attempt to change tab to non MESSAGETABS value"), !1)
        },
        setPage: function(n) {
            return n < 0 ? (t.debug("Invalid attempt to set page to page " + n), !1) : (i.currentPage = n, !0)
        },
        beginUpdateMessages: function() {
            var t, r, u;
            return i.currentTab === 2 ? (t = Roblox.websiteLinks.GetFormattedNotificationsJsonLink, r = {}) : (t = Roblox.websiteLinks.GetFormattedMessagesJsonLink, r = {
                pageNumber: i.currentPage,
                pageSize: i.MAXROWS,
                messageTab: i.currentTab
            }), u = {
                url: t,
                noCache: !0
            }, n.httpGet(u, r)
        },
        beginMarkMessagesRead: function(t, i) {
            var r = i ? Roblox.websiteLinks.MarkMessagesReadLink : Roblox.websiteLinks.MarkMessagesUnreadLink,
                u = {
                    messageIds: t
                },
                f = {
                    url: r
                };
            return n.httpPost(f, u)
        },
        beginSetArchiveMessages: function(t, i) {
            var r = i ? Roblox.websiteLinks.ArchiveMessagesLink : Roblox.websiteLinks.UnarchiveMessagesLink,
                u = {
                    messageIds: t
                },
                f = {
                    url: r
                };
            return n.httpPost(f, u)
        },
        beginSendMessage: function(t, r, u, f, e) {
            i.sentMessageComplete = !1;
            var o = Roblox.websiteLinks.SendMessageJsonResultLink,
                s = {
                    subject: t,
                    body: r,
                    recipientId: u,
                    replyMessageId: f,
                    includePreviousMessage: e
                },
                h = {
                    url: o
                };
            return n.httpPost(h, s, !0).then(i.endSendMessageSuccess, i.endSendMessageError)
        },
        endSendMessageSuccess: function(n) {
            var t = n.data;
            i.sendMessageError = {
                hasError: !t.success,
                details: t.message
            }, i.sentMessageComplete = !0
        },
        endSendMessageError: function() {
            i.sendMessageError = {
                hasError: !0,
                details: "Unknown error"
            }, i.sentMessageComplete = !0
        },
        getMessageDetailById: function(t) {
            var i = Roblox.websiteLinks.GetMessageDetailByIdLink + t,
                r = {
                    url: i
                };
            return n.httpGet(r)
        }
    };
    return i
}]);