// ~/viewapp/pages/chat/services/chatService.js
"use strict";
chat.factory("chatService", ["$http", "$q", "partyService", "chatUtility", "$log", function(n, t, i, r, u) {
    var o = {},
        h = 100,
        s = 100,
        e = function(i, r, f) {
            var e = t.defer();
            return n({
                method: "GET",
                url: i.url,
                params: r,
                withCredentials: f,
                retryable: i.retryable
            }).success(function(n) {
                e.resolve(n)
            }).error(function(n) {
                u.debug("Error: unable to send " + i.url + " request."), e.reject(n)
            }), e.promise
        },
        f = function(i, r, f) {
            var e = t.defer();
            return n({
                method: "POST",
                url: i.url,
                data: r,
                withCredentials: f,
                retryable: i.retryable
            }).success(function(n) {
                e.resolve(n)
            }).error(function(n) {
                u.debug("Error: unable to send " + i.url + " request."), e.reject(n)
            }), e.promise
        },
        a = function(t) {
            var i = "/thumbnail/avatar-headshots",
                r = {
                    userIds: t
                };
            return n({
                method: "GET",
                url: i,
                params: r,
                withCredentials: !0,
                retryable: !0
            })
        },
        v = function(t) {
            var i = "/presence/users",
                r = {
                    userIds: t
                };
            return n({
                method: "GET",
                url: i,
                params: r,
                withCredentials: !0,
                retryable: !0
            })
        },
        c = function(n) {
            var u = [];
            angular.forEach(n, function(n) {
                u.push(n.Id)
            });
            for (var t = 0, i = h >= s ? parseInt(h) : parseInt(s), r = u.slice(t, i), f = []; r.length > 0;) f.push(a(r)), f.push(v(r)), t++, r = u.slice(t * i, t * i + i);
            return f
        },
        y = function(n, t) {
            for (var u = [], f = [], r = 0, e = n.length, i; r < e;) u = u.concat(n[r].data), f = f.concat(n[r + 1].data), r = r + 2;
            for (i in t) u[i] && (t[i].AvatarThumb = u[i]), f[i] && (t[i].UserPresenceType = f[i].UserPresenceType), o[t[i].Id] = t[i], Roblox && Roblox.Endpoints && (o[t[i].Id].UserProfileLink = Roblox.Endpoints.generateAbsoluteUrl("/users/{id}/profile", {
                id: t[i].Id
            }, !0));
            return t
        },
        p = function(t) {
            if (t.length === 0) return [];
            var r = {
                conversationIds: t
            };
            return n({
                method: "GET",
                url: i.apiSets.getPartiesForConversationsApi.url,
                params: r,
                withCredentials: !0,
                retryable: !0
            })
        },
        w = function(n, i) {
            var u = [];
            return r.isInApp() || (u = u.concat(p(n))), u = u.concat(c(i)), t.all(u).then(function(t) {
                var u = {
                        parties: [],
                        users: []
                    },
                    f, e;
                return t && t.length > 0 && (f = n.length > 0 && !r.isInApp() ? t[0].data : [], e = r.isInApp() ? t : t.slice(1), i = y(e, i), u = {
                    parties: f,
                    users: i
                }), u
            })
        },
        l = function(n) {
            return o[n] && angular.isDefined(o[n].AvatarThumb) && angular.isDefined(o[n].UserPresenceType)
        },
        b = function(n) {
            var i = c(n);
            return t.all(i).then(function(t) {
                for (var f = [], e = [], u = 0, s = i.length, r; u < s;) f = f.concat(t[u].data), e = e.concat(t[u + 1].data), u = u + 2;
                for (r in n) f[r] && (n[r].AvatarThumb = f[r]), e[r] && (n[r].UserPresenceType = e[r].UserPresenceType), o[n[r].Id] = n[r];
                return n
            })
        };
    return {
        invitedParties: [],
        apiSets: {
            getFriendListUrl: {
                url: "/friends/list",
                retryable: !0
            },
            getUserAvatarUrl: {
                url: "/thumbnail/avatar-headshot",
                retryable: !0
            },
            getPresenceUserUrl: {
                url: "/presence/user",
                retryable: !0
            }
        },
        setParams: function(n) {
            this.apiSets.markAsReadApi = {
                url: n + "/v1.0/mark-as-read",
                retryable: !1
            }, this.apiSets.markAsSeenApi = {
                url: n + "/v1.0/mark-as-seen",
                retryable: !1
            }, this.apiSets.sendMessageApi = {
                url: n + "/v1.0/send-message",
                retryable: !1
            }, this.apiSets.conversationsApi = {
                url: n + "/v1.0/get-conversations",
                retryable: !0
            }, this.apiSets.userConversationsApi = {
                url: n + "/v1.0/get-user-conversations",
                retryable: !0
            }, this.apiSets.unreadConversationsApi = {
                url: n + "/v1.0/get-unread-conversations",
                retryable: !0
            }, this.apiSets.getMessagesApi = {
                url: n + "/v1.0/get-messages",
                retryable: !0
            }, this.apiSets.multiGetLatestMessagesApi = {
                url: n + "/v1.0/multi-get-latest-messages",
                retryable: !0
            }, this.apiSets.unreadMessagesApi = {
                url: n + "/v1.0/get-unread-messages",
                retryable: !0
            }, this.apiSets.getUnreadConversationCountApi = {
                url: n + "/v1.0/get-unread-conversation-count",
                retryable: !0
            }, this.apiSets.startOneToOneConversationApi = {
                url: n + "/v1.0/start-one-to-one-conversation",
                retryable: !0
            }, this.apiSets.startGroupConversationApi = {
                url: n + "/v1.0/start-group-conversation",
                retryable: !1
            }, this.apiSets.addToConversationApi = {
                url: n + "/v1.0/add-to-conversation",
                retryable: !0
            }, this.apiSets.removeFromConversationApi = {
                url: n + "/v1.0/remove-from-conversation",
                retryable: !0
            }
        },
        setAvatarHeadshotsMultigetLimit: function(n) {
            h = n
        },
        setUserPresenceMultigetLimit: function(n) {
            s = n
        },
        getFriendsDict: function() {
            return o
        },
        getUnreadConversationCount: function() {
            return e(this.apiSets.getUnreadConversationCountApi, null, !0)
        },
        getUnreadConversations: function(n, i, f) {
            var o = this,
                s = {
                    pageNumber: n,
                    pageSize: i
                };
            return e(this.apiSets.unreadConversationsApi, s, !0).then(function(n) {
                var i = n;
                if (i.length > 0) {
                    var e = {},
                        h = [],
                        s = [];
                    return i.forEach(function(n) {
                        h.push(n.Id), e[n.Id] = n, s.push(o.getMessages(n.Id, null, f).then(function(t) {
                            return {
                                messages: t,
                                conversationId: n.Id
                            }
                        }))
                    }), t.all(s).then(function(n) {
                        var t = [];
                        return angular.forEach(n, function(n) {
                            var o = n.conversationId,
                                u = n.messages,
                                f = e[o];
                            f.ChatMessages = u, u.length > 0 ? (r.sanitizeMessage(u[0]), f.DisplayMessage = u[0]) : f.DisplayMessage = [], t.push(f)
                        }), t
                    }, function() {
                        u.debug("------------ get messages request failed -------------")
                    })
                }
                return null
            })
        },
        getUserConversations: function(f, e) {
            var s = {
                    pageNumber: f,
                    pageSize: e
                },
                o = [n({
                    method: "GET",
                    url: this.apiSets.userConversationsApi.url,
                    params: s,
                    retryable: this.apiSets.userConversationsApi.retryable,
                    withCredentials: !0
                })];
            return r.isInApp() || o.push(n({
                method: "GET",
                url: i.apiSets.getCurrentPartyApi.url,
                withCredentials: !0,
                retryable: i.apiSets.getCurrentPartyApi.retryable
            })), t.all(o).then(function(n) {
                var i = n[0].data ? n[0].data : [],
                    u = n[1] && n[1].data ? n[1].data : [],
                    f = [],
                    e = [],
                    o = [],
                    t = {};
                return i.forEach(function(n) {
                    angular.forEach(n.ParticipantUsers, function(n) {
                        f.indexOf(n.Id) < 0 && !l(n.Id) && (f.push(n.Id), e.push(n))
                    }), u && u.ConversationId === n.Id && (n.party = u, n.dialogType = r.dialogType.PARTY), n.dialogType || (n.dialogType = n.IsGroupChat ? r.dialogType.GROUPCHAT : r.dialogType.CHAT), o.push(n.Id), t[n.Id] = n
                }), w(o, e).then(function(n) {
                    var u = n.parties;
                    return u && u.length > 0 && u.forEach(function(n) {
                        t[n.ConversationId] && !t[n.ConversationId].party && (t[n.ConversationId].party = n, t[n.ConversationId].dialogType = r.dialogType.PENDINGPARTY)
                    }), i
                }, function() {
                    return i
                })
            }, function() {
                u.debug("------------ get user conversation request failed -------------")
            })
        },
        getUserInfo: function(i) {
            var r = {
                userId: i.Id
            };
            return t.all([n({
                method: "GET",
                url: this.apiSets.getUserAvatarUrl.url,
                params: r,
                retryable: this.apiSets.getUserAvatarUrl.retryable
            }), n({
                method: "GET",
                url: this.apiSets.getPresenceUserUrl.url,
                params: r,
                retryable: this.apiSets.getPresenceUserUrl.retryable
            })]).then(function(n) {
                var t = n[0].data,
                    r = n[1].data;
                return i.AvatarThumb = t, i.UserPresenceType = r.UserPresenceType, Roblox && Roblox.Endpoints && (i.UserProfileLink = Roblox.Endpoints.generateAbsoluteUrl("/users/{id}/profile", {
                    id: i.Id
                }, !0)), i
            })
        },
        getUserPresence: function(n) {
            var t = {
                userId: n.Id
            };
            return e(this.apiSets.getPresenceUserUrl, t, !0)
        },
        getFriends: function(n, t, i) {
            var r = {
                userId: n,
                startIndex: t,
                pageSize: i
            };
            return e(this.apiSets.getFriendListUrl, r, !0).then(function(n) {
                return angular.isDefined(n.Friends) && n.Friends.length > 0 ? b(n.Friends) : null
            })
        },
        getConversations: function(n) {
            var t = {
                conversationIds: n
            };
            return e(this.apiSets.conversationsApi, t, !0)
        },
        addToConversation: function(n, t) {
            var i = {
                participantUserIds: n,
                conversationId: t
            };
            return f(this.apiSets.addToConversationApi, i, !0)
        },
        removeFromConversation: function(n, t) {
            var i = {
                participantUserId: n,
                conversationId: t
            };
            return f(this.apiSets.removeFromConversationApi, i, !0)
        },
        startOneToOneConversation: function(n) {
            var t = {
                participantUserId: n
            };
            return f(this.apiSets.startOneToOneConversationApi, t, !0)
        },
        startGroupConversation: function(n) {
            var t = {
                participantUserIds: n
            };
            return f(this.apiSets.startGroupConversationApi, t, !0)
        },
        getMessages: function(n, t, i) {
            var r = {
                conversationId: n,
                exclusiveStartMessageId: t,
                pageSize: i
            };
            return e(this.apiSets.getMessagesApi, r, !0)
        },
        multiGetLatestMessages: function(n, t) {
            var i = {
                conversationIds: n,
                pageSize: t
            };
            return e(this.apiSets.multiGetLatestMessagesApi, i, !0)
        },
        markAsRead: function(n, t) {
            var i = {
                conversationId: n,
                endMessageId: t
            };
            return f(this.apiSets.markAsReadApi, i, !0)
        },
        markAsSeen: function(n) {
            return f(this.apiSets.markAsSeenApi, n, !0)
        },
        sendMessage: function(n, t) {
            var i = {
                conversationId: n,
                message: t
            };
            return f(this.apiSets.sendMessageApi, i, !0)
        }
    }
}]);