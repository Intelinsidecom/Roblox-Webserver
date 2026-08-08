// ~/viewapp/pages/chat/services/chatUtilityService.js
"use strict";
chat.factory("chatUtility", ["$filter", "$log", function(n) {
    function c(n, t, i) {
        return t.maxHeightOfInput = i.tabletInApp ? parseInt(t.maxHeightOfTextInput) + 12 : parseInt(t.maxHeightOfTextInput) + 17, t.maxHeightOfInput < n ? t.maxHeightOfInput : n
    }
    var e, i = {
            FRIEND: -1,
            CHAT: 0,
            GROUPCHAT: 1,
            NEWGROUPCHAT: 2,
            PARTY: 3,
            NEWPARTY: 4,
            PENDINGPARTY: 5,
            ADDFRIENDS: 6
        },
        r = {
            INIT: 0,
            OPEN: 1,
            REPLACE: 2,
            MINIMIZE: 3,
            COLLAPSE: 4,
            REMOVE: 5,
            REFRESH: 6
        },
        s = {
            dialogType: i.NEWGROUPCHAT,
            layoutId: "newGroup",
            title: "Create Group"
        },
        u = {
            MESSAGE: 0,
            FRIENDSELECTION: 1
        },
        h = {
            PENDING: 0,
            MEMBER: 1,
            LEADER: 2
        },
        o = angular.element("#chat-data-model"),
        f = o.data("smallerchatenabled");
    return {
        parytChromeDisplayTimeStampInterval: o.data("parytchromedisplaytimestampinterval"),
        chatLayout: {
            scrollbarClassName: "#chat-friend-list",
            collapsed: !0,
            pageInitializing: !1,
            pageDataLoading: !1,
            chatBarInitialized: !1,
            isChatLoading: !1,
            widthOfChatCollapsed: 112,
            widthOfChat: f ? 256 : 262,
            widthOfDialog: f ? 250 : 300,
            spaceOfDialog: f ? 6 : 12,
            widthOfDialogMinimize: 200,
            numberOfDialogOpen: 0,
            defaultChatZIndex: 1030,
            errorMaskEnable: !1,
            isFriendListEmpty: !1,
            isUserConversationEmpty: !1,
            chatLandingEnabled: !1,
            thresholdMobile: 768,
            thresholdChatBarOpen: 1738,
            resizing: !1,
            defaultTitleForMessage: " says ...",
            defaultTitleForPartyInvite: "Party invite from ",
            heightOfMobileInput: 50,
            heightOfTabletInput: 80
        },
        chatApiParams: {
            pageNumberOfUnreadConversations: 1,
            pageSizeOfUnreadConversations: 30,
            pageNumberOfPartyInvites: 1,
            pageSizeOfPartyInvites: 30,
            pageNumberOfConversations: 1,
            pageSizeOfConversations: 30,
            pageSizeOfDisplayMessages: 1,
            pageSizeOfUnreadMessages: 30,
            pageSizeOfGetMessages: 30,
            startIndexOfFriendList: 0,
            pageSizeOfFriendList: 50,
            loadMoreUnreadConversations: !1,
            loadMoreConversations: !1,
            loadMoreFriends: !1,
            loadMoreInvitedParties: !1,
            loadMoreCurrentParty: !1
        },
        partyApiParams: {
            pageNumberOfPartyInvites: 1,
            pageSizeOfPartyInvites: 30,
            loadMoreInvitedParties: !1,
            loadMoreCurrentParty: !1
        },
        dialogParams: {
            loadMoreMessages: !0,
            sendingMessage: !1,
            sendMessageHasError: !1,
            loadMoreFriends: !1,
            startIndexOfFriendList: 0,
            pageSizeOfFriendList: 50,
            pageSizeOfGetMessages: 30
        },
        dialogLayout: {
            lookUpMembers: !1,
            focusMeEnabled: !0,
            hasFocus: !1,
            active: !1,
            isChatLoading: !1,
            collapsed: !1,
            isConfirmationOn: !1,
            isMembersOverloaded: !1,
            scrollToBottom: !1,
            IsdialogContainerVisible: !1,
            inviteBtnDisabled: !0,
            limitMemberDisplay: 6,
            heightOfInput: 40,
            maxHeightOfTextInput: 56 * 1.2,
            maxHeightOfInput: 56 * 1.2 + 10,
            limitCharacterCount: 160,
            templateUrl: Roblox.ChatTemplates.DialogTemplate,
            scrollbarElm: null
        },
        userPresenceTypes: [{
            className: "",
            title: "Offline"
        }, {
            className: "online",
            title: "Online"
        }, {
            className: "game",
            title: "In Game"
        }, {
            className: "studio",
            title: "In Studio"
        }],
        dialogType: i,
        newGroup: s,
        scrollBarType: u,
        newParty: {
            dialogType: i.NEWPARTY,
            layoutId: "newParty",
            title: "Create Party",
            isCreated: !1,
            partyName: "Party : "
        },
        party: {
            partyName: "Party",
            partyInviteMsg: "PARTY INVITE!",
            isPartyExisted: !1,
            isPartyLeader: !1,
            partyLeaderTooltip: " is the party leader",
            partyMemberTooltip: " is in the party",
            pendingMemberTooltip: " is not in the party",
            memberJoinText: " joined the party",
            joinPartyText: "The party leader is finding a game to play.",
            createPartyText: " to play with your friends!"
        },
        memberStatus: h,
        dialogInitValue: {
            isUpdated: !0,
            updateStatus: r.INIT,
            markAsActive: !1,
            activeType: null,
            autoOpen: !1
        },
        dialogStatus: r,
        conversationInitStatus: {
            remove: !1
        },
        notificationType: {
            newMessage: "NewMessage",
            newMessageBySelf: "NewMessageBySelf",
            newConversation: "NewConversation",
            addedToConversation: "AddedToConversation",
            removedFromConversation: "RemovedFromConversation",
            participantAdded: "ParticipantAdded",
            participantLeft: "ParticipantLeft",
            invitedToParty: "InvitedToParty",
            partyUserJoined: "PartyUserJoined",
            partyUserLeft: "PartyUserLeft",
            iLeftParty: "ILeftParty",
            partyJoinedGame: "PartyJoinedGame",
            partyLeftGame: "PartyLeftGame",
            partyDeleted: "PartyDeleted",
            iCreatedParty: "ICreatedParty",
            iJoinedParty: "IJoinedParty",
            friendshipDestroyed: "FriendshipDestroyed",
            friendshipCreated: "FriendshipCreated",
            presenceOffline: "UserOffline",
            presenceOnline: "UserOnline"
        },
        systemMessage: {
            isSystemMessage: !0
        },
        deviceType: {
            COMPUTER: "Computer",
            PHONE: "Phone",
            TABLET: "Tablet"
        },
        cookies: {
            dialogIdList: "dialogIdList",
            dialogDict: "dialogDict",
            dialogsLayout: "dialogsLayout",
            chatBarLayout: "chatBarLayout"
        },
        activeType: {
            NEWMESSAGE: "New message",
            PARTYINVITE: "Party Invite"
        },
        performanceMarkLabels: {
            chatPageDataLoaded: "chat_pageData_loaded",
            chatConversationsLoading: "chat_conversations_loading",
            chatConversationsLoaded: "chat_conversations_loaded",
            chatSignalRInitializing: "chat_signalR_initializing",
            chatSignalRSucceeded: "chat_signalR_succeeded"
        },
        buildScrollbar: function(n) {
            var t = angular.element(document.querySelector(n));
            t.mCustomScrollbar({
                autoExpandScrollbar: !1,
                scrollInertia: 1,
                contentTouchScroll: 1,
                mouseWheel: {
                    preventDefault: !0
                }
            })
        },
        updateScrollbar: function(n) {
            var t = angular.element(document.querySelector(n));
            t.mCustomScrollbar("update")
        },
        removeParty: function(n, t) {
            var u = t.layoutId,
                f, e;
            t.party ? (t.dialogType = i.PENDINGPARTY, f = t.party.Id, t.party.LeaderUser.Id === n.userId && t.party.MemberUsers.length === 1 && t.party.MemberUsers[0].Id === n.userId && (t.party = null, t.placeThumbnail = null, t.dialogType = t.IsGroupChat ? i.GROUPCHAT : i.CHAT), angular.isDefined(n.partyIds) && (e = n.partyIds.indexOf(f), e > -1 && n.partyIds.splice(e, 1)), angular.isDefined(n.partiesDict) && delete n.partiesDict[f]) : t.dialogType = t.IsGroupChat ? i.GROUPCHAT : i.CHAT, n.dialogDict[u] && n.dialogIdList.indexOf(u) > -1 && (n.dialogDict[u].isUpdated = !0, n.dialogDict[u].updateStatus = r.REFRESH)
        },
        deleteParty: function(n, t, i) {
            if (n.partyIds.length > 0) {
                var r = angular.copy(n.partyIds),
                    u = this;
                r.forEach(function(r) {
                    if (r === i) {
                        var f = n.partiesDict[r].layoutId,
                            e = t[f];
                        u.removeParty(n, e)
                    }
                })
            }
        },
        cleanPartyList: function(n, t, i) {
            if (n.partyIds.length > 0) {
                var r = angular.copy(n.partyIds),
                    u = this;
                r.forEach(function(r) {
                    if (r !== i) {
                        var f = n.partiesDict[r].layoutId,
                            e = t[f];
                        u.removeParty(n, e)
                    }
                })
            }
        },
        htmlEntities: function(n) {
            return String(n).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;")
        },
        linkify: function(n) {
            return angular.isDefined(Roblox) && angular.isDefined(Roblox.Linkify) && typeof Roblox.Linkify.String == "function" ? Roblox.Linkify.String(n) : n
        },
        sortFriendList: function(t, i) {
            var f = n("orderBy"),
                r = [],
                u = [],
                e = [];
            return t.friendIds.forEach(function(n) {
                var i = t.friendsDict[n];
                i.UserPresenceType > 0 ? r.push(i) : u.push(i), e.push(n)
            }), i.forEach(function(n) {
                e.indexOf(n.Id) < 0 && (n.UserPresenceType > 0 ? r.push(n) : u.push(n)), t.friendIds.indexOf(n.Id) < 0 && t.friendIds.push(n.Id)
            }), r = f(r, "+Username"), u = f(u, "+Username"), i = r.concat(u)
        },
        getScrollBarSelector: function(n, t) {
            var i = n.layoutId;
            angular.isUndefined(t) && (t = n.scrollBarType);
            switch (t) {
                case u.FRIENDSELECTION:
                    return "#scrollbar_friend_" + n.dialogType + "_" + i;
                case u.MESSAGE:
                default:
                    return "#scrollbar_" + n.dialogType + "_" + i
            }
        },
        sanitizeMessage: function(n) {
            n && n.Content && (n.Content = this.htmlEntities(n.Content), n.Content = this.linkify(n.Content))
        },
        sanitizeMessages: function(n) {
            var t, i;
            if (n && n.length > 0)
                for (t = 0; t < n.length; t++) i = n[t], this.sanitizeMessage(i)
        },
        getDataForMarkingSeen: function(n) {
            var t = [];
            return document.hasFocus && document.hasFocus() && angular.forEach(n, function(n) {
                n.isConversation && t.push(n.Id)
            }), t
        },
        updateDialogStyle: function(n, t, i) {
            if (i.inApp && t.inAppStyle) {
                var u = i.inAppLayout,
                    r = u.topBarHeight,
                    e = 0,
                    f = 0,
                    o = 0,
                    s = r / 2 - 9;
                t.inAppStyle.headerStyle = {
                    height: r + "px",
                    "padding-top": s + "px"
                }, n.isPartyExisted || n.partyInGame ? (f = r + 40, t.inAppStyle.bannerStyle = {
                    top: r + "px"
                }) : f = r, o = f + u.inputHeight, e = "calc(" + u.dialogHeight + " - " + o + "px)", t.inAppStyle.dialogStyle = {
                    top: f + "px",
                    height: e
                }, t.inAppStyle.friendsListStyle = {
                    top: r + u.searchHeight + "px",
                    height: "100%"
                }
            }
        },
        setInAppLayout: function(n) {
            var t = n.inAppLayout,
                r = t.topBarHeight / 2 - 9,
                i;
            t.headerStyle = {
                height: t.topBarHeight + "px",
                "padding-top": r + "px"
            }, i = "calc(100% - " + t.topBarHeight + "px)", t.chatBodyHeight = {
                height: i
            }
        },
        setResizeInputLayout: function(n, t, i, r) {
            var o, f, u = n.inApp ? n.inAppLayout : n.layout,
                e = u.topBarHeight,
                h = c(t, r, n),
                s;
            i.isPartyExisted || i.partyInGame ? (o = e + u.bannerHeight, f = e + h + u.bannerHeight) : (o = e, f = e + h), s = n.inApp ? "calc(" + u.dialogHeight + " - " + f + "px)" : u.dialogHeight - f + "px", n.inApp ? (r.inAppStyle.dialogStyle = {
                top: o + "px",
                height: s
            }, r.inAppStyle.inputStyle = {
                height: t
            }) : (r.defaultStyle.dialogStyle = {
                height: s
            }, r.defaultStyle.inputStyle = {
                height: t
            })
        },
        setInApp: function(n) {
            e = n
        },
        isInApp: function() {
            return e
        }
    }
}]);