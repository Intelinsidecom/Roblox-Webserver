// ~/viewapp/pages/chat/services/messageService.js
"use strict";
chat.factory("messageService", ["chatService", "chatUtility", "$rootScope", "$filter", "$log", function(n, t, i, r, u) {
    function f(n) {
        n.Sent && !n.parsedTimestamp && (n.parsedTimestamp = parseInt(typeof n.Sent == "string" && n.Sent.search("Date") > -1 ? n.Sent.slice(6, -2) : n.Sent))
    }

    function e(n, t, i) {
        var f = angular.isDefined(i) ? i : new Date,
            u = new Date(t),
            e = u.toDateString(),
            s = Math.round(Math.abs(f.getTime() - u.getTime()) / o),
            h = u.getDay();
        n.displayTimeStamp = f.toDateString() === e ? r("date")(t, "h:mm a") : s <= h ? r("date")(t, "EEE | h:mm a") : r("date")(t, "MMM d, yyyy | h:mm a")
    }
    var o = 864e5;
    return {
        setFallbackClusterMaster: function(n, t) {
            angular.isUndefined(n.ChatMessages) && (n.ChatMessages = []);
            var r = n.ChatMessages.length - 1;
            t.displayTimeStamp && (t.isClusterMaster = !0), n.ChatMessages.length > 0 && n.ChatMessages[r].SenderUserId !== t.SenderUserId && (n.ChatMessages[r].isClusterMaster = !0), n.ChatMessages.push(t)
        },
        setClusterMaster: function(n, t) {
            angular.isUndefined(n.ChatMessages) && (n.ChatMessages = []), (n.ChatMessages.length > 0 && n.ChatMessages[0].SenderUserId !== t.SenderUserId || t.displayTimeStamp) && (t.isClusterMaster = !0), n.ChatMessages.unshift(t)
        },
        buildFallbackTimeStamp: function(n, i, r) {
            var o, u;
            if (!n.Sent) return !1;
            o = parseInt(t.parytChromeDisplayTimeStampInterval), f(n), u = n.parsedTimestamp, (!i.startTimeStamp || u + o < i.startTimeStamp) && (e(n, u, r), i.startTimeStamp = u)
        },
        buildTimeStamp: function(n, i, r) {
            var o, u;
            return n.Sent ? (o = parseInt(t.parytChromeDisplayTimeStampInterval), f(n), u = n.parsedTimestamp, i.previousTimeStamp || (i.startTimeStamp = u), (!i.previousTimeStamp || u - o > i.previousTimeStamp) && (e(n, u, r), i.previousTimeStamp = u), !0) : !1
        },
        manipulateMessages: function(r, f, e) {
            var l, h, s, o, c;
            if (f || (r.messagesDict = {}, r.unreadMessageIds = [], r.unreadMessageTimestamps = []), angular.isUndefined(r.messagesDict) && (r.messagesDict = {}), angular.isUndefined(r.unreadMessageIds) && (r.unreadMessageIds = [], r.unreadMessageTimestamps = []), f && f.length > 0) {
                for (l = f.length, h = [], r.previousTimeStamp = null, s = l - 1; s >= 0; s--) o = f[s], this.buildTimeStamp(o, r), r.messagesDict[o.Id] || (t.sanitizeMessage(o), r.messagesDict[o.Id] = o, this.setClusterMaster(r, o), o.Read || (r.unreadMessageIds.push(o.Id), r.unreadMessageTimestamps.push(o.parsedTimestamp))), e && !e[o.SenderUserId] && h.indexOf(o.SenderUserId) < 0 && (u.debug(" ----- new friend information for this message, trying to get now -----" + o.SenderUserId), c = {
                    Id: o.SenderUserId
                }, h.push(o.SenderUserId), n.getUserInfo(c).then(function(n) {
                    n && (e[c.Id] = n)
                }, function() {
                    u.debug(" ----- getUserInfo failed -----")
                }));
                r.unreadMessageIds.length > 0 && i.$broadcast("Roblox.Chat.LoadUnreadConversationCount")
            }
        },
        appendMessages: function(n, r) {
            var e, s, o, u, h, c;
            if (angular.isUndefined(n.unreadMessageIds) && (n.unreadMessageIds = [], n.unreadMessageTimestamps = []), t.sanitizeMessages(r), n.ChatMessages && n.ChatMessages.length !== 0) {
                if (n.ChatMessages) {
                    for (e = {}, o = 0; o < n.ChatMessages.length; o++)
                        if (!n.ChatMessages[o].isSystemMessage) {
                            e = n.ChatMessages[o], f(e);
                            break
                        } for (s = r.length, o = s - 1; o >= 0; o--) u = r[o], f(u), h = u.Id === e.Id || e.Id && typeof e.Id != "string" && e.Id.toString() === u.Id, c = !angular.isUndefined(n.messagesDict) && !angular.isUndefined(u.Id) && !angular.isUndefined(n.messagesDict[u.Id]), (angular.equals({}, e) || u.parsedTimestamp > e.parsedTimestamp) && !h && !c && (this.buildTimeStamp(u, n), this.setClusterMaster(n, u)), u.Read || (n.HasUnreadMessages = !0, n.unreadMessageIds.push(u.Id), n.unreadMessageTimestamps.push(u.parsedTimestamp))
                }
            } else n.ChatMessages = r;
            n.DisplayMessage = r[0], n.unreadMessageIds.length > 0 && i.$broadcast("Roblox.Chat.LoadUnreadConversationCount")
        },
        markMessagesAsRead: function(t) {
            var u, f, e, r;
            t.ChatMessages && t.unreadMessageIds && (u = t.ChatMessages, f = u.length, t.unreadMessageTimestamps.length > 0 && t.unreadMessageTimestamps[0] >= u[f - 1].parsedTimestamp && (e = t.unreadMessageIds.length, r = t.unreadMessageIds[e - 1], angular.isUndefined(t.pendingUnreadMessageId) && (t.pendingUnreadMessageId = []), t.pendingUnreadMessageId.indexOf(r) < 0 && (t.pendingUnreadMessageId.push(r), n.markAsRead(t.Id, r).then(function(n) {
                n.Success && (t.HasUnreadMessages = !1, t.unreadMessageIds = [], t.unreadMessageTimestamps = [], t.pendingUnreadMessageId.splice(r, 1), i.$broadcast("Roblox.Chat.LoadUnreadConversationCount"))
            }, function() {}))))
        },
        buildSystemMessage: function(n, i) {
            var r = angular.copy(t.systemMessage),
                e = new Date,
                u;
            r.Sent = e.getTime().toString();
            switch (n) {
                case t.notificationType.iCreatedParty:
                    r.Content = "<a class='xsmall text-link' href='" + t.party.gamesPageLink + "'>Find Games</a>" + t.party.createPartyText;
                    break;
                case t.notificationType.iJoinedParty:
                    r.Content = t.party.joinPartyText;
                    break;
                case t.notificationType.partyUserJoined:
                    u = i.newJoinedUsernames.length > 0 ? i.newJoinedUsernames : "Member", r.Content = u + t.party.memberJoinText
            }
            angular.isUndefined(i.ChatMessages) && (i.ChatMessages = []), f(r), this.setClusterMaster(i, r)
        },
        resetConversationUnreadStatus: function(t, r) {
            r.length === 0 && t.HasUnreadMessages && n.markAsRead(t.Id, null).then(function(n) {
                n.Success && (t.HasUnreadMessages = !1, i.$broadcast("Roblox.Chat.LoadUnreadConversationCount"))
            }, function() {})
        }
    }
}]);