// ~/viewapp/pages/chat/controllers/chatController.js
"use strict";
chat.controller("chatController", ["$scope", "chatService", "partyService", "messageService", "chatUtility", "cookieService", "localStorageService", "chatHybridService", "performanceService", "googleAnalyticsEventsService", "$window", "$document", "$log", function(n, t, i, r, u, f, e, o, s, h, c, l, a) {
    var y = function(t) {
            n.chatLibrary.dialogDict[t] && (n.chatLibrary.dialogDict[t].isUpdated = !0, n.chatLibrary.dialogDict[t].updateStatus = u.dialogStatus.REFRESH)
        },
        ht = function() {
            var t = c.innerWidth,
                i = t - u.chatLayout.widthOfChat - u.chatLayout.widthOfDialogMinimize,
                r = u.chatLayout.widthOfDialog + u.chatLayout.spaceOfDialog;
            n.chatLibrary.chatLayout.availableNumberOfDialogs = Math.floor(i / r), n.chatLibrary.chatLayout.numberOfDialogs = n.chatLibrary.dialogIdList.length, a.debug(" -------------numberOfDialogs = -------------- " + n.chatLibrary.chatLayout.numberOfDialogs), a.debug(" -------------availableNumberOfDialogs = -------------- " + n.chatLibrary.chatLayout.availableNumberOfDialogs)
        },
        st = function() {
            var t = c.innerWidth;
            return n.chatLibrary.chatLayout.numberOfDialogs >= n.chatLibrary.chatLayout.availableNumberOfDialogs && t > u.chatLayout.thresholdMobile
        },
        gt = function() {
            var t = c.innerWidth;
            return n.chatLibrary.chatLayout.numberOfDialogs < n.chatLibrary.chatLayout.availableNumberOfDialogs && t > u.chatLayout.thresholdMobile
        },
        d = function(t, i) {
            var r, f, e;
            if (n.chatLibrary.deviceType === u.deviceType.PHONE && (n.chatLibrary.dialogIdList.forEach(function(t) {
                    n.closeDialog(t)
                }), n.chatLibrary.dialogIdList = []), ht(), st() && n.chatLibrary.deviceType === u.deviceType.COMPUTER)
                while (n.chatLibrary.dialogIdList.length >= n.chatLibrary.chatLayout.availableNumberOfDialogs) {
                    if (r = n.chatLibrary.dialogIdList.pop(), angular.isUndefined(r)) break;
                    n.chatLibrary.dialogDict[r].isUpdated = !0, n.chatLibrary.dialogDict[r].updateStatus = u.dialogStatus.MINIMIZE, f = n.chatLibrary.minimizedDialogIdList.indexOf(t), f > -1 && (n.chatLibrary.minimizedDialogIdList.splice(f, 1), delete n.chatLibrary.minimizedDialogData[t])
                }
            n.chatLibrary.dialogIdList.push(t), e = angular.copy(u.dialogInitValue), angular.isDefined(i) && i && (e.autoOpen = !0), n.chatLibrary.dialogDict[t] = e
        },
        lt = function(t, i) {
            n.chatLibrary.dialogDict[t] && (n.chatLibrary.dialogDict[t].markAsActive = !0, n.chatLibrary.dialogDict[t].activeType = i)
        },
        w = function(t) {
            n.chatLibrary.dialogDict[t.layoutId] && y(t.layoutId), n.chatLibrary.deviceType === u.deviceType.COMPUTER && !n.chatLibrary.dialogDict[t.layoutId] && (t.dialogType === u.dialogType.PENDINGPARTY || t.DisplayMessage && t.DisplayMessage.Content) && n.launchDialog(t.layoutId, !0)
        },
        nt = function(t, i) {
            n.chatLibrary.partyIds.indexOf(t) < 0 && (n.chatLibrary.partyIds.push(t), n.chatLibrary.partiesDict[t] = {
                conversationId: i.Id,
                layoutId: i.layoutId
            }), n.chatLibrary.partyIds.length > 0 && (n.chatLibrary.party.isPartyExisted = !0)
        },
        pt = function(i) {
            i.length > 0 && t.multiGetLatestMessages(i, u.chatApiParams.pageSizeOfDisplayMessages).then(function(t) {
                angular.forEach(t, function(t) {
                    var i = t.ChatMessages,
                        e = i.length > 0 ? i[0] : {},
                        f;
                    n.chatLibrary.conversationsDict[t.ConversationId] && (f = n.chatLibrary.conversationsDict[t.ConversationId].layoutId, u.sanitizeMessage(e), n.chatUserDict[f].DisplayMessage = e, r.resetConversationUnreadStatus(n.chatUserDict[f], i))
                })
            }, function() {
                a.debug("----- multiGetLatestMessages request is failed ! ------")
            })
        },
        k = function() {
            (n.chatLibrary.chatLayout.collapsed || n.chatLibrary.inApp) && t.getUnreadConversationCount().then(function(t) {
                n.chatViewModel.conversationCount = t, n.chatLibrary.inApp && o.setNewMessageNotification(t)
            }, function() {
                a.debug("----- getUnreadConversationCount request is failed ! ------")
            })
        },
        rt = function(i) {
            var r = [];
            r.push(i), t.getConversations(r).then(function(t) {
                n.buildChatUserListByUnreadConversations(t)
            }, function() {
                a.debug(" -------- fetchConversation request failed ------ ")
            })
        },
        ut = function(i, f) {
            var o = n.getLayoutId(i, u.dialogType.CHAT),
                e;
            angular.isUndefined(n.chatUserDict[o]) ? rt(i) : (e = n.chatUserDict[o], t.getMessages(i, null, u.dialogParams.pageSizeOfGetMessages).then(function(i) {
                if (i && i.length > 0 && (r.appendMessages(e, i), n.updateChatViewModel(e, !0), w(e), !f)) {
                    lt(e.layoutId, u.activeType.NEWMESSAGE);
                    var o = u.getDataForMarkingSeen(n.chatUserDict);
                    o.length > 0 && t.markAsSeen(o)
                }
            }, function() {}))
        },
        ft = function(i, r, f, e) {
            var o, s;
            i.party = r, i.dialogType = !i.IsGroupChat && f ? u.dialogType.CHAT : u.dialogType.PENDINGPARTY, i.incomingPartyInvite = !0, i.pendingPartyMsg = u.party.partyInviteMsg, n.getUserInfoForConversation(i), o = i.layoutId, n.chatLibrary.dialogDict[o] && y(o), e === r.Id && (w(i), f || (lt(i.layoutId, u.activeType.PARTYINVITE), s = u.getDataForMarkingSeen(n.chatUserDict), s.length > 0 && t.markAsSeen(s))), n.chatLibrary.pendingPartiesDict[r.Id] = {
                conversationId: i.Id,
                layoutId: i.layoutId
            }
        },
        p = function(r, f) {
            i.getInvitedParties(n.partyApiParams.pageNumberOfPartyInvites, n.partyApiParams.pageSizeOfPartyInvites).then(function(i) {
                var s = n.chatUserDict,
                    e = i,
                    o = !1;
                e.length > 0 ? (e.forEach(function(t) {
                    t && (angular.equals({}, s) || angular.forEach(s, function(n) {
                        n.isConversation && t.ConversationId === n.Id && (ft(n, t, f, r), o = !0)
                    }), o || (n.convIdsForInvitedParties.push(t.ConversationId), n.invitedParties[t.ConversationId] = t))
                }), e.length === n.partyApiParams.pageSizeOfPartyInvites && (n.partyApiParams.pageNumberOfPartyInvites++, n.partyApiParams.loadMoreInvitedParties = !0, p(r, f)), o || (t.getConversations(n.convIdsForInvitedParties).then(function(t) {
                    var i = [];
                    t.forEach(function(t) {
                        var e = n.invitedParties[t.Id],
                            s = n.chatLibrary.userId === e.LeaderUser.Id ? u.dialogType.PARTY : u.dialogType.PENDINGPARTY,
                            o = n.getLayoutId(t.Id, u.dialogType.PENDINGPARTY);
                        n.chatUserDict[o] ? (ft(n.chatUserDict[o], e, f, r), n.updateChatViewModel(n.chatUserDict[o], !0)) : (t.party = e, t.dialogType = s, i.push(t))
                    }), i.length > 0 && n.buildChatUserListByConversations(i, !0), n.convIdsForInvitedParties = [], n.invitedParties = {}
                }), n.partyApiParams.loadMoreInvitedParties = !1)) : n.partyApiParams.loadMoreInvitedParties = !1
            })
        },
        et = function(n) {
            n.party = null, n.placeThumbnail = null, n.dialogType = n.IsGroupChat ? u.dialogType.GROUPCHAT : u.dialogType.CHAT, y(n.layoutId)
        },
        dt = function(t) {
            var i = !1,
                r, f, e;
            u.deleteParty(n.chatLibrary, n.chatUserDict, t), n.chatLibrary.pendingPartiesDict[t] && (i = !0, r = n.chatLibrary.pendingPartiesDict[t].layoutId, f = n.chatUserDict[r], et(f)), n.chatLibrary.cleanPartyFromConversationEnabled && !i && (e = n.chatUserDict, angular.forEach(e, function(n) {
                n.isConversation && n.party && n.party.Id === t && (i = !0, et(n))
            }))
        },
        kt = function(n, t) {
            var i, r;
            if (angular.isDefined(n.party))
                for (n.newJoinedUsernames = "", i = 0; i < t.MemberUsers.length; i++)
                    if (r = t.MemberUsers[i], !n.membersDict[r.Id] || n.membersDict[r.Id].memberStatus === u.memberStatus.PENDING) {
                        n.newJoinedUsernames = r.Name;
                        break
                    } n.party = t
        },
        b = function(f) {
            i.getCurrentParty().then(function(e) {
                var s;
                if (e) {
                    u.cleanPartyList(n.chatLibrary, n.chatUserDict, e.Id);
                    var c = e.ConversationId,
                        h = n.getLayoutId(c, u.dialogType.PARTY),
                        o = n.chatUserDict[h];
                    n.chatLibrary.partiesDict[e.Id] || nt(e.Id, o), o.dialogType = u.dialogType.PARTY, kt(o, e), o.party.LeaderUser.Id === n.chatLibrary.userId && (n.chatLibrary.party.isPartyLeader = !0), o.party.GamePlaceId && o.party.LeaderUser.Id !== n.chatLibrary.userId && i.joinGame(o, n.chatLibrary.inApp), o.party.GamePlaceId && !o.placeThumbnail && i.getPlace(o.party.GamePlaceId).then(function(n) {
                        o.placeThumbnail = n
                    }), y(h), n.getUserInfoForConversation(o), n.updateChatViewModel(o, !0), angular.isDefined(f) && r.buildSystemMessage(f, o), s = u.getDataForMarkingSeen(n.chatUserDict), s.length > 0 && t.markAsSeen(s)
                } else u.cleanPartyList(n.chatLibrary, n.chatUserDict, null)
            }, function() {
                a.debug(" -------- getCurrentParty request failed ------ ")
            })
        },
        bt = function() {
            angular.isDefined(n.preSetChatLibrary) && angular.isDefined(n.preSetChatLibrary.dialogIdList) && (n.chatLibrary.dialogIdList = n.preSetChatLibrary.dialogIdList, angular.forEach(n.preSetChatLibrary.dialogDict, function(t, i) {
                t.isUpdated || (t.isUpdated = !0), i === u.newGroup.layoutId && (n.chatUserDict[u.newGroup.layoutId] = n.newGroup), n.chatLibrary.dialogDict[i] = t, n.chatLibrary.dialogsLayout[i] = n.preSetChatLibrary.dialogsLayout[i]
            }))
        },
        ct = function() {
            return c.innerWidth >= u.chatLayout.thresholdChatBarOpen && !n.chatLibrary.isTakeOverOn && !angular.element(document.querySelector("#GamesPageLeftColumn")).length
        },
        wt = function() {
            if (!n.chatLibrary.inApp) {
                var t = {
                    collapsed: n.chatLibrary.chatLayout.collapsed
                };
                f.updateCookie(u.cookies.chatBarLayout, t, n.chatLibrary.cookieOption)
            }
        },
        it = function() {
            n.chatUserDict = {}, n.dialogType = angular.copy(u.dialogType), n.deviceType = angular.copy(u.deviceType), n.memberStatus = angular.copy(u.memberStatus), n.userPresenceTypes = angular.copy(u.userPresenceTypes), n.newGroup = angular.copy(u.newGroup), n.newParty = angular.copy(u.newParty), n.party = angular.copy(u.party), n.selectedFriendIds = [], n.chatLibrary = {
                partyIds: [],
                partiesDict: {},
                pendingPartiesDict: {},
                conversationsDict: {},
                userConversationsDict: {},
                friendIds: [],
                friendLayoutIds: [],
                friendsDict: {},
                chatLayout: angular.copy(u.chatLayout),
                chatLayoutIds: [],
                layoutIdList: [],
                dialogIdList: [],
                dialogDict: {},
                dialogsLayout: {},
                party: u.party,
                minimizedDialogIdList: [],
                minimizedDialogData: {},
                isTakeOverOn: angular.element(document.querySelector("#wrap")).data("gutter-ads-enabled"),
                currentTabTitle: c.document.title
            }, n.convIdsForInvitedParties = [], n.invitedParties = {}
        },
        at = function() {
            n.preSetChatLibrary = {}, n.chatLibrary.inApp || (f.isCookieDefined(u.cookies.dialogDict) && (n.preSetChatLibrary = {
                dialogIdList: f.isCookieDefined(u.cookies.dialogIdList) ? f.retrieveCookie(u.cookies.dialogIdList) : [],
                dialogDict: f.retrieveCookie(u.cookies.dialogDict),
                dialogsLayout: f.retrieveCookie(u.cookies.dialogsLayout)
            }), f.isCookieDefined(u.cookies.chatBarLayout) && (n.preSetChatLibrary.chatBarLayout = f.retrieveCookie(u.cookies.chatBarLayout))), n.chatApiParams = angular.copy(u.chatApiParams), n.partyApiParams = angular.copy(u.partyApiParams)
        },
        ni = function() {
            n.chatLibrary.inApp ? (n.chatLibrary.chatLayout.collapsed = !1, n.chatLibrary.chatLayout.chatBarInitialized = !0) : n.chatLibrary.inApp || n.chatLibrary.chatLayout.chatBarInitialized || (ct() && !n.preSetChatLibrary.chatBarLayout ? n.chatLibrary.chatLayout.collapsed = !1 : n.preSetChatLibrary.chatBarLayout ? n.chatLibrary.chatLayout.collapsed = n.preSetChatLibrary.chatBarLayout.collapsed : ct() || (n.chatLibrary.chatLayout.collapsed = !0), n.chatLibrary.chatLayout.chatBarInitialized = !0), s.logSinglePerformanceMark(u.performanceMarkLabels.chatPageDataLoaded)
        },
        yt = function(t) {
            if (n.chatLibrary.friendIds.indexOf(t.Id) < 0 && n.chatLibrary.friendIds.push(t.Id), n.chatLibrary.friendsDict[t.Id] || (n.chatLibrary.friendsDict[t.Id] = angular.copy(t)), n.chatViewModel.friendsHasConversation.indexOf(t.Id) < 0) {
                var i = n.getLayoutId(t.Id, u.dialogType.FRIEND);
                t.layoutId = i, t.isConversation = !1, t.dialogType = u.dialogType.FRIEND, n.chatLibrary.friendLayoutIds.indexOf(i) < 0 && n.chatLibrary.friendLayoutIds.push(i), n.updateChatViewModel(t, !1), n.chatLibrary.chatLayout.chatLandingEnabled && (n.chatLibrary.chatLayout.chatLandingEnabled = !1)
            }
        },
        v = function(t, i) {
            var r = n.chatLibrary.layoutIdList.indexOf(t);
            i && r > -1 ? n.chatLibrary.layoutIdList.splice(r, 1) : !i && r < 0 && n.chatLibrary.layoutIdList.push(t)
        },
        ot = function(t) {
            return n.chatLibrary.userConversationsDict[t] ? n.chatLibrary.userConversationsDict[t] : null
        },
        vt = function(t) {
            var i = ot(t),
                r, e, f, o;
            i || (i = n.getLayoutId(t, u.dialogType.FRIEND)), r = n.chatUserDict[i], r.isConversation ? (e = n.chatUserDict[i].Id, n.chatLibrary.conversationsDict[e].remove = !0) : r && (f = n.chatLibrary.chatLayoutIds.indexOf(i), f > -1 && (n.chatLibrary.chatLayoutIds.splice(f, 1), delete n.chatUserDict[i], v(i, !0))), n.chatViewModel.friendsHasConversation.indexOf(t) > -1 && (o = n.chatViewModel.friendsHasConversation.indexOf(t), n.chatViewModel.friendsHasConversation.splice(o, 1)), n.chatLibrary.chatLayoutIds.indexOf(i) > -1 && n.closeDialog(i), n.chatLibrary.chatLayoutIds.length === 0 && (n.chatLibrary.chatLayout.chatLandingEnabled = !0)
        },
        g = function() {
            n.buildChatUserListByFriends(n.chatApiParams.startIndexOfFriendList, n.chatApiParams.pageSizeOfFriendList).then(function(t) {
                n.chatApiParams.startIndexOfFriendList !== 0 || t && t.length !== 0 || (n.chatLibrary.chatLayout.chatLandingEnabled = !0), t && t.length > 0 && n.filterFriends(t), t && t.length === n.chatApiParams.pageSizeOfFriendList ? (n.chatApiParams.startIndexOfFriendList = +n.chatApiParams.startIndexOfFriendList + n.chatApiParams.pageSizeOfFriendList, g()) : (n.chatApiParams.startIndexOfFriendList = 0, n.chatApiParams.loadMoreFriends = !1)
            })
        },
        tt = function() {
            n.chatUserDict && n.chatLibrary || it(), s.logSinglePerformanceMark(u.performanceMarkLabels.chatConversationsLoading), k(), t.getUserConversations(n.chatApiParams.pageNumberOfConversations, n.chatApiParams.pageSizeOfConversations).then(function(t) {
                s.logSinglePerformanceMark(u.performanceMarkLabels.chatConversationsLoaded), t && t.length > 0 && (n.refreshFriendsDict(), n.buildChatUserListByConversations(t, !1), n.chatApiParams.pageNumberOfConversations++, n.chatApiParams.pageNumberOfPartyInvites++, bt()), !t || t.length < n.chatApiParams.pageSizeOfConversations ? (n.chatApiParams.pageNumberOfConversations = 1, g(), t.length === 0 && (n.chatLibrary.chatLayout.chatLandingEnabled = !0)) : n.chatApiParams.loadMoreConversations = !0, n.chatLibrary.chatLayout.pageDataLoading && (n.chatLibrary.chatLayout.pageDataLoading = !1)
            }, function() {
                a.debug("--getConversations-error---")
            })
        },
        ti = function() {
            if (angular.isDefined(Roblox.UserNotifications)) {
                s.logSinglePerformanceMark(u.performanceMarkLabels.chatSignalRInitializing);
                Roblox.UserNotifications.onConnected(n.handleSignalRSuccess);
                Roblox.UserNotifications.onReconnected(n.handleSignalRSuccess);
                Roblox.UserNotifications.onDisconnected(n.handleSignalRError);
                Roblox.UserNotifications.subscribe("ChatNotifications", function(t) {
                    a.debug("--------- this is ChatNotifications subscription -----------" + t.Type);
                    switch (t.Type) {
                        case u.notificationType.newMessage:
                            ut(t.ConversationId);
                            break;
                        case u.notificationType.newMessageBySelf:
                            ut(t.ConversationId, !0);
                            break;
                        case u.notificationType.newConversation:
                        case u.notificationType.addedToConversation:
                        case u.notificationType.participantAdded:
                        case u.notificationType.participantLeft:
                            rt(t.ConversationId);
                            break;
                        case u.notificationType.removedFromConversation:
                            var i = n.chatLibrary.conversationsDict[t.ConversationId].layoutId;
                            n.chatLibrary.conversationsDict[t.ConversationId].remove = !0, n.closeDialog(i)
                    }
                }), n.chatLibrary.inApp || Roblox.UserNotifications.subscribe("PartyNotifications", function(t) {
                    a.debug("--------- this is PartyNotifications subscription -----------" + t.Type);
                    switch (t.Type) {
                        case u.notificationType.invitedToParty:
                            n.partyApiParams.loadMoreInvitedParties || p(t.PartyId);
                            break;
                        case u.notificationType.iLeftParty:
                            n.partyApiParams.loadMoreInvitedParties || p(t.PartyId, !0);
                            break;
                        case u.notificationType.partyUserLeft:
                        case u.notificationType.partyLeftGame:
                            n.partyApiParams.loadMoreCurrentParty || b();
                            break;
                        case u.notificationType.partyJoinedGame:
                            n.partyApiParams.loadMoreCurrentParty || b();
                            break;
                        case u.notificationType.partyDeleted:
                            dt(t.PartyId);
                            break;
                        case u.notificationType.partyUserJoined:
                            n.partyApiParams.loadMoreInvitedParties || p(t.PartyId);
                        case u.notificationType.iCreatedParty:
                        case u.notificationType.iJoinedParty:
                            n.partyApiParams.loadMoreCurrentParty || b(t.Type)
                    }
                }), Roblox.UserNotifications.subscribe("FriendshipNotifications", function(t) {
                    a.debug("--------- this is FriendshipNotifications subscription -----------" + t.Type);
                    switch (t.Type) {
                        case u.notificationType.friendshipDestroyed:
                            var i = t.EventArgs;
                            angular.forEach(i, function(t) {
                                t !== n.chatLibrary.userId && n.$digest(vt(t))
                            }), l.triggerHandler("Roblox.Friends.CountChanged");
                            break;
                        case u.notificationType.friendshipCreated:
                            g(), l.triggerHandler("Roblox.Friends.CountChanged")
                    }
                }), Roblox.UserNotifications.subscribe("PresenceNotifications", function(i) {
                    a.debug("--------- this is PresenceNotifications subscription -----------" + i.Type);
                    switch (i.Type) {
                        case u.notificationType.presenceOnline:
                        case u.notificationType.presenceOffline:
                            if (n.chatLibrary.friendsDict[i.UserId]) {
                                var r = n.chatLibrary.friendsDict[i.UserId];
                                t.getUserPresence(r).then(function(t) {
                                    n.chatLibrary.friendsDict[r.Id].UserPresenceType = t.UserPresenceType
                                }, function() {
                                    a.debug(" ----- getUserPresence failed -----")
                                })
                            }
                    }
                })
            }
        };
    n.handleSignalRSuccess = function() {
        if (a.debug(" -------- Signal R is connected ------ "), n.chatLibrary.chatLayout.errorMaskEnable = !1, n.chatLibrary.chatLayout.pageInitializing) {
            s.logSinglePerformanceMark(u.performanceMarkLabels.chatSignalRSucceeded), n.chatLibrary.chatLayout.pageInitializing = !1;
            return
        }
        try {
            at(), tt()
        } catch (t) {
            h.fireEvent(n.chatLibrary.googleAnalyticsEvent.category, n.chatLibrary.googleAnalyticsEvent.action, t.message)
        }
    }, n.handleSignalRError = function() {
        a.debug(" -------- Signal R is disconnected ------ "), n.chatLibrary.chatLayout.errorMaskEnable = !0
    }, n.isPartyLeader = function() {
        return n.chatLibrary.party.isPartyLeader
    }, n.onResize = function() {
        var t;
        if (n.chatLibrary.chatLayout.numberOfDialogs > n.chatLibrary.chatLayout.availableNumberOfDialogs)
            while (n.chatLibrary.dialogIdList.length > n.chatLibrary.chatLayout.availableNumberOfDialogs) {
                if (a.debug(" -------------overflow ------ $scope.chatLibrary.dialogIdList.length ------------- " + n.chatLibrary.dialogIdList.length), t = n.chatLibrary.dialogIdList.pop(), angular.isUndefined(t)) break;
                t && n.chatLibrary.dialogDict[t] && (n.chatLibrary.dialogDict[t].isUpdated = !0, n.chatLibrary.dialogDict[t].updateStatus = u.dialogStatus.MINIMIZE)
            } else if (n.chatLibrary.chatLayout.numberOfDialogs < n.chatLibrary.chatLayout.availableNumberOfDialogs)
                while (n.chatLibrary.dialogIdList.length < n.chatLibrary.chatLayout.availableNumberOfDialogs) {
                    if (a.debug(" -------------fit ------ $scope.chatLibrary.dialogIdList.length ------------- " + n.chatLibrary.dialogIdList.length), t = n.chatLibrary.minimizedDialogIdList.pop(), angular.isUndefined(t)) break;
                    t && n.chatLibrary.minimizedDialogData[t] && (delete n.chatLibrary.minimizedDialogData[t], n.chatLibrary.dialogIdList.push(t), n.chatLibrary.dialogDict[t] = angular.copy(u.dialogInitValue))
                }
        n.chatLibrary.chatLayout.resizing = !1
    }, n.toggleChatContainer = function() {
        n.chatLibrary.togglechatbarenabled && (n.chatLibrary.chatLayout.collapsed = !n.chatLibrary.chatLayout.collapsed, k(), n.chatLibrary.chatLayout.chatBarInitialized = !0, wt())
    }, n.getLayoutId = function(n, t) {
        switch (t) {
            case u.dialogType.FRIEND:
                return "friend_" + n;
            case u.dialogType.CHAT:
            case u.dialogType.GROUPCHAT:
            case u.dialogType.PENDINGPARTY:
            case u.dialogType.ADDFRIENDS:
            case u.dialogType.PARTY:
                return "conv_" + n;
            case u.dialogType.NEWPARTY:
                return u.newParty.dialogType;
            case u.dialogType.NEWGROUPCHAT:
                return u.newGroup.dialogType
        }
    }, n.getUserInfoForConversation = function(i) {
        if (i.ParticipantUsers) {
            if (i.userIds = [], i.dialogType === n.dialogType.PARTY || i.dialogType === n.dialogType.PENDINGPARTY) {
                var r = i.party;
                i.membersDict = {}, angular.forEach(r.MemberUsers, function(n) {
                    i.membersDict[n.Id] || (i.membersDict[n.Id] = n.Id !== r.LeaderUser.Id ? {
                        memberStatus: u.memberStatus.MEMBER,
                        statusTooltip: u.party.partyMemberTooltip
                    } : {
                        memberStatus: u.memberStatus.LEADER,
                        statusTooltip: u.party.partyLeaderTooltip
                    })
                })
            }
            i.groupName = "", i.ParticipantUsers.forEach(function(r) {
                switch (i.dialogType) {
                    case n.dialogType.PENDINGPARTY:
                    case n.dialogType.PARTY:
                        i.membersDict[r.Id] ? i.membersDict[r.Id].memberStatus === u.memberStatus.LEADER || i.userIds.length === 0 ? i.userIds.unshift(r.Id) : i.membersDict[r.Id].memberStatus === u.memberStatus.MEMBER ? i.userIds.splice(1, 0, r.Id) : i.userIds.push(r.Id) : (i.userIds.push(r.Id), i.membersDict[r.Id] = {
                            memberStatus: u.memberStatus.PENDING,
                            statusTooltip: n.chatLibrary.party.pendingMemberTooltip
                        }), r.Id !== n.chatLibrary.userId && (i.groupName = i.groupName ? i.groupName + ", " + r.Username : r.Username), i.IsGroupChat || r.Id === n.chatLibrary.userId ? i.IsGroupChat && (i.partyName = u.newParty.partyName + i.groupName) : i.partyName = u.newParty.partyName + r.Username;
                        break;
                    case n.dialogType.GROUPCHAT:
                        i.userIds.push(r.Id), r.Id !== n.chatLibrary.userId && (i.groupName = i.groupName ? i.groupName + ", " + r.Username : r.Username);
                        break;
                    case n.dialogType.CHAT:
                        r.Id !== n.chatLibrary.userId && (i.userIds.push(r.Id), i.displayUserId = r.Id, i.Username = r.Username);
                        break;
                    default:
                        i.userIds.push(r.Id)
                }
                n.chatLibrary.friendsDict[r.Id] || t.getUserInfo(r).then(function(t) {
                    t && r.Id === t.Id && (n.chatLibrary.friendsDict[r.Id] = t)
                }, function() {
                    a.debug(" ----- getUserInfo failed -----")
                }), !i.IsGroupChat && n.chatViewModel.friendsHasConversation.indexOf(r.Id) < 0 && n.chatViewModel.friendsHasConversation.push(r.Id)
            })
        }
    }, n.updateChatViewModel = function(t, i) {
        var r, f, e;
        t.IsGroupChat || t.dialogType !== u.dialogType.CHAT || t.ParticipantUsers.forEach(function(i) {
            if (i.Id !== n.chatViewModel.userId) {
                var r = n.getLayoutId(i.Id, u.dialogType.FRIEND),
                    f = n.chatLibrary.chatLayoutIds.indexOf(r);
                f > -1 && (n.chatLibrary.chatLayoutIds.splice(f, 1), delete n.chatUserDict[r]), v(r, !0), n.chatLibrary.userConversationsDict[i.Id] = t.layoutId
            }
        });
        switch (t.dialogType) {
            case n.dialogType.PENDINGPARTY:
                t.pendingPartyMsg = u.party.partyInviteMsg, t.incomingPartyInvite = !1, t.name = t.groupName;
                break;
            case n.dialogType.PARTY:
                nt(t.party.Id, t), t.incomingPartyInvite = !1, t.name = t.partyName;
                break;
            case n.dialogType.GROUPCHAT:
                t.name = t.groupName;
                break;
            case n.dialogType.CHAT:
                t.name = t.Username;
                break;
            default:
                angular.isDefined(t.Username) && (t.name = t.Username)
        }
        n.chatUserDict[t.layoutId] = t, r = n.chatLibrary.chatLayoutIds.indexOf(t.layoutId), r > -1 && n.chatLibrary.chatLayoutIds.splice(r, 1), t.isConversation && (i ? n.chatLibrary.chatLayoutIds.length > 0 && t.dialogType !== u.dialogType.PARTY ? (f = n.chatLibrary.chatLayoutIds[0], e = n.chatUserDict[f], e.dialogType === u.dialogType.PARTY ? n.chatLibrary.chatLayoutIds.splice(1, 0, t.layoutId) : n.chatLibrary.chatLayoutIds.unshift(t.layoutId)) : n.chatLibrary.chatLayoutIds.unshift(t.layoutId) : t.dialogType && t.dialogType === u.dialogType.PARTY ? n.chatLibrary.chatLayoutIds.unshift(t.layoutId) : n.chatLibrary.chatLayoutIds.push(t.layoutId)), v(t.layoutId), t.isConversation && !n.chatLibrary.conversationsDict[t.Id] && (n.chatLibrary.conversationsDict[t.Id] = angular.copy(u.conversationInitStatus), n.chatLibrary.conversationsDict[t.Id].layoutId = t.layoutId)
    }, n.refreshFriendsDict = function() {
        var i = t.getFriendsDict();
        angular.forEach(i, function(t) {
            n.chatLibrary.friendsDict[t.Id] = t
        })
    }, n.filterFriends = function(t) {
        var i = [],
            r;
        t = u.sortFriendList(n.chatLibrary, t), n.chatLibrary.friendIds = [], n.chatLibrary.friendLayoutIds = [], t.forEach(function(n) {
            yt(n)
        }), n.chatLibrary.chatLayoutIds.forEach(function(t) {
            n.chatUserDict[t].isConversation && i.push(t)
        }), r = i.concat(n.chatLibrary.friendLayoutIds), n.chatLibrary.chatLayoutIds = r
    }, n.buildChatUserListByFriends = function(i, r) {
        return n.chatApiParams.loadMoreConversations = !1, n.chatApiParams.loadMoreFriends = !0, t.getFriends(n.chatLibrary.userId, i, r)
    }, n.buildChatUserListByUnreadConversations = function(t) {
        t.forEach(function(t) {
            var e = n.getLayoutId(t.Id, u.dialogType.CHAT),
                f;
            n.chatUserDict[e] ? (f = n.chatUserDict[e], t.HasUnreadMessages && t.ChatMessages && t.ChatMessages.length > 0 && (f.HasUnreadMessages = !0, n.chatLibrary.dialogDict[e] && n.chatLibrary.dialogIdList.indexOf(e) > -1 && r.manipulateMessages(f, t.ChatMessages, n.chatLibrary.friendsDict), u.sanitizeMessage(t.ChatMessages[0]), f.DisplayMessage = t.ChatMessages[0], n.updateChatViewModel(f, !0)), f.ParticipantUsers = t.ParticipantUsers, f.dialogType === u.dialogType.PARTY ? i.getCurrentParty().then(function(t) {
                t && (f.party = t, n.getUserInfoForConversation(f))
            }) : n.getUserInfoForConversation(f), w(f)) : (t.layoutId = e, t.isConversation = !0, t.dialogType = t.IsGroupChat ? u.dialogType.GROUPCHAT : u.dialogType.CHAT, n.getUserInfoForConversation(t), n.updateChatViewModel(t, !0), w(t))
        })
    }, n.buildChatUserListByConversations = function(t, i) {
        var r = [];
        t.forEach(function(t) {
            var f = n.getLayoutId(t.Id, t.dialogType);
            r.push(t.Id), t.layoutId = f, t.isConversation = !0, n.getUserInfoForConversation(t), n.updateChatViewModel(t, i), t.dialogType === u.dialogType.PARTY && t.party.LeaderUser.Id === n.chatLibrary.userId && (n.chatLibrary.party.isPartyLeader = !0)
        }), pt(r)
    }, n.cancelSearch = function() {
        n.chatViewModel.searchTerm = "", n.chatLibrary.chatLayout.searchFocus = !1
    }, n.launchDialog = function(i, r) {
        if (n.chatLibrary.tabletInApp && n.chatLibrary.dialogIdList.length > 0)
            for (var f = 0; f < n.chatLibrary.dialogIdList.length; f++) n.closeDialog(n.chatLibrary.dialogIdList[f]);
        n.chatLibrary.dialogIdList.indexOf(i) < 0 && i === u.newGroup.layoutId ? (d(i, r), n.chatUserDict[u.newGroup.layoutId] = n.newGroup) : n.chatLibrary.dialogIdList.indexOf(i) < 0 && n.chatUserDict[i] && (n.chatLibrary.inApp && !n.chatLibrary.tabletInApp && o.openDialog(), n.chatUserDict[i].isConversation ? d(i, r) : t.startOneToOneConversation(n.chatUserDict[i].Id).then(function(t) {
            var o = n.chatLibrary.chatLayoutIds.indexOf(i),
                f, e;
            n.chatLibrary.chatLayoutIds.splice(o, 1), delete n.chatUserDict[i], v(i, !0), f = t.Conversation, e = n.getLayoutId(f.Id, u.dialogType.CHAT), f.layoutId = e, f.isConversation = !0, f.dialogType = u.dialogType.CHAT, f.ChatMessages = [], n.getUserInfoForConversation(f), n.updateChatViewModel(f, !0), d(e, r)
        }, function() {
            a.debug(" ---- startOneToOneConversation ---- failed!")
        }))
    }, n.destroyDialogLayout = function(n) {
        angular.element(document.querySelector("#" + n)).empty()
    }, n.closeDialog = function(t) {
        var c = n.chatLibrary.dialogIdList.indexOf(t),
            l = u.getScrollBarSelector(n.chatUserDict[t]),
            e = angular.element(document.querySelector(l)),
            a = angular.element(document.querySelector("#chat-main")),
            s, h, i;
        c > -1 && (n.chatLibrary.dialogIdList.splice(c, 1), delete n.chatLibrary.dialogDict[t]), n.chatUserDict[t].dialogType === u.dialogType.NEWPARTY && (n.chatLibrary.party.isPartyExisted = !1), n.chatUserDict[t].dialogType === u.dialogType.NEWGROUPCHAT && (n.chatUserDict[t].selectedUserIds = [], n.chatUserDict[t].selectedUsersDict = {}, n.chatUserDict[t].numberOfSelected = 0), e && e.length > 0 && e.mCustomScrollbar("destroy"), n.$broadcast("Roblox.Chat.MarkDialogInactive", {
            layoutId: t
        }), s = n.chatUserDict[t].Id, n.chatLibrary.conversationsDict[s] && n.chatLibrary.conversationsDict[s].remove ? (h = n.chatLibrary.chatLayoutIds.indexOf(t), h > -1 && n.chatUserDict[t] && (n.chatLibrary.chatLayoutIds.splice(h, 1), delete n.chatUserDict[t], angular.equals(n.chatUserDict, {}) && (n.chatLibrary.chatLayout.chatLandingEnabled = !0)), v(t, !0), n.chatLibrary.party.isPartyExisted = !1) : r.manipulateMessages(n.chatUserDict[t], null), n.chatLibrary.deviceType === u.deviceType.PHONE && a.removeClass("hidden"), n.destroyDialogLayout(t), n.chatLibrary.minimizedDialogIdList.length > 0 && (i = n.chatLibrary.minimizedDialogIdList.shift(), delete n.chatLibrary.minimizedDialogData[i], n.chatLibrary.dialogIdList.push(i), n.chatLibrary.dialogDict[i].isUpdated = !0, n.chatLibrary.dialogDict[i].updateStatus = u.dialogStatus.REPLACE), f.updateCookie(u.cookies.dialogIdList, n.chatLibrary.dialogIdList, n.chatLibrary.cookieOption), f.updateCookie(u.cookies.dialogDict, n.chatLibrary.dialogDict, n.chatLibrary.cookieOption), n.chatLibrary.inApp || (delete n.chatLibrary.dialogsLayout[t], f.updateCookie(u.cookies.dialogsLayout, n.chatLibrary.dialogsLayout, n.chatLibrary.cookieOption)), n.chatLibrary.inApp && !n.chatLibrary.tabletInApp && o.closeDialog()
    }, it(), n.$watch(function() {
        return n.chatViewModel
    }, function(t, i) {
        if (!angular.isUndefined(t) && !angular.equals(t, i)) {
            n.chatLibrary.chatLayout.pageInitializing = !0, n.chatLibrary.chatLayout.pageDataLoading = !0, at();
            try {
                ni(), tt(), ti()
            } catch (r) {
                h.fireEvent(n.chatLibrary.googleAnalyticsEvent.category, n.chatLibrary.googleAnalyticsEvent.action, r.message)
            }
        }
    }), angular.element(c).bind("resize", function() {
        !n.chatLibrary.chatLayout.resizing && n.chatLibrary.dialogIdList.length > 0 && (n.chatLibrary.chatLayout.resizing = !0, ht(), st() || gt() ? (a.debug(" ------- need to resize -------------- "), n.onResize()) : n.chatLibrary.chatLayout.resizing = !1)
    }), n.$on("Roblox.Chat.destroyChatCookie", function() {
        f.destroyCookie(u.cookies.dialogIdList, n.chatLibrary.cookieOption), f.destroyCookie(u.cookies.dialogDict, n.chatLibrary.cookieOption), f.destroyCookie(u.cookies.dialogsLayout, n.chatLibrary.cookieOption), f.destroyCookie(u.cookies.chatBarLayout, n.chatLibrary.cookieOption), e.removeLocalStorage(n.chatLibrary.dialogLocalStorageName)
    }), n.$on("Roblox.Chat.LoadUnreadConversationCount", function() {
        k()
    }), l.bind("Roblox.Chat.StartChat", function(t, i) {
        var r = ot(i.userId);
        r || (r = n.getLayoutId(i.userId, u.dialogType.FRIEND)), n.launchDialog(r, !0)
    })
}]);