// ~/viewapp/pages/chat/controllers/dialogController.js
"use strict";
chat.controller("dialogController", ["$scope", "chatService", "chatUtility", "partyService", "messageService", "$window", "chatHybridService", "$log", function(n, t, i, r, u, f, e, o) {
    var h = angular.element(document.querySelector("#chat-data-model")),
        c = function(i, r) {
            t.removeFromConversation(n.chatLibrary.userId, i).then(function() {
                n.closeDialog(r)
            }, function() {
                o.debug("---------- removeFromConversation request failed------- ")
            })
        },
        l = function() {
            n.chatLibrary.party.isPartyExisted = !1, r.partyLeave(n.dialogData.party.Id).then(function() {
                i.removeParty(n.chatLibrary, n.dialogData)
            }, function() {
                o.debug("---------- partyLeave request failed------- ")
            })
        },
        a = function(i) {
            t.sendMessage(n.dialogData.Id, i.RawContent).then(function(t) {
                i.sendingMessage = !1, t.ResultType !== 0 ? (i.sendMessageHasError = !0, i.error = t.StatusMessage) : (angular.isUndefined(n.dialogData.messagesDict) && (n.dialogData.messagesDict = {}), i.Id = t.MessageId, n.dialogData.messagesDict[t.MessageId] = i)
            }, function() {
                o.debug(" ------ sendMessage error -------"), i.sendingMessage = !1, i.sendMessageHasError = !0
            })
        },
        s = function() {
            t.getFriends(n.chatLibrary.userId, n.dialogParams.startIndexOfFriendList, n.dialogParams.pageSizeOfFriendList).then(function(t) {
                t && t.length <= n.dialogParams.pageSizeOfFriendList ? (n.updateFriends(t), n.dialogData.scrollBarType = i.scrollBarType.FRIENDSELECTION, t.length === n.dialogParams.pageSizeOfFriendList && (n.dialogParams.loadMoreFriends = !0, n.dialogParams.startIndexOfFriendList = +n.dialogParams.startIndexOfFriendList + n.dialogParams.pageSizeOfFriendList, s())) : n.dialogParams.loadMoreFriends = !1
            })
        };
    n.dialogParams = angular.copy(i.dialogParams), n.dialogLayout.numberOfPartyMembers = h.data("numberofmembersforpartychrome"), n.userPresenceTypes = i.userPresenceTypes, n.dialogData.messageForSend = "", n.dialogLayout.scrollbarElm = i.getScrollBarSelector(n.dialogData, i.scrollBarType.MESSAGE), n.friendsScrollbarElm = i.getScrollBarSelector(n.dialogData, i.scrollBarType.FRIENDSELECTION), n.focusDialog = function() {
        n.dialogLayout.active = !1
    }, n.updateDialog = function() {
        if (o.debug("---- updateDialog callback ---- Scrollbars updated"), !n.dialogLayout.IsdialogContainerVisible) {
            var t = angular.element(document.querySelector(n.dialogLayout.scrollbarElm));
            t.find(".mCustomScrollBox").addClass("dialog-visible"), n.dialogLayout.IsdialogContainerVisible = !0
        }
        return !1
    }, n.sendMessage = function() {
        if (n.dialogData.messageForSend.length > 0) {
            var r = new Date,
                t = {
                    Read: !0,
                    Content: n.dialogData.messageForSend,
                    RawContent: n.dialogData.messageForSend,
                    SenderUserId: n.chatLibrary.userId,
                    sendingMessage: !0,
                    sendMessageHasError: !1,
                    Sent: r.getTime().toString()
                };
            i.sanitizeMessage(t), u.buildTimeStamp(t, n.dialogData), n.dialogData.messageForSend = "", angular.isUndefined(n.dialogData.ChatMessages) && (n.dialogData.ChatMessages = []), u.setClusterMaster(n.dialogData, t), n.dialogData.DisplayMessage = t, a(t)
        }
    }, n.keyPressEnter = function() {
        n.sendMessage()
    }, n.abuseReport = function(t, i) {
        if (n.dialogLayout.isConfirmationOn = !0, t && (n.dialogData.userIdForAbuseReport = t), i && n.dialogData.userIdForAbuseReport) {
            e.closeDialog();
            var r = Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl("/abusereport/chat?id=" + n.dialogData.userIdForAbuseReport + "&redirectUrl=" + escape(window.location) + "&conversationId=" + n.dialogData.Id) : "/abusereport/chat?id=" + n.dialogData.userIdForAbuseReport + "&redirectUrl=" + escape(window.location) + "&conversationId=" + n.dialogData.Id;
            window.location.href = r, n.dialogData.userIdForAbuseReport = null, n.dialogLayout.isConfirmationOn = !1
        }
    }, n.leaveGroup = function() {
        if (n.dialogData.dialogType === i.dialogType.PARTY) l();
        else if (n.dialogData.dialogType === i.dialogType.PENDINGPARTY) {
            var t = n.dialogData.party.Id;
            r.removeFromParty(t, n.chatLibrary.userId)
        }(n.dialogData.dialogType === i.dialogType.GROUPCHAT || n.dialogData.dialogType === i.dialogType.PENDINGPARTY) && (n.chatLibrary.conversationsDict[n.dialogData.Id].remove = !0, c(n.dialogData.Id, n.dialogData.layoutId))
    }, n.addFriends = function() {
        n.dialogData.addMoreFriends = !0, n.chatLibrary.friendIds.length > 0 && n.updateFriends(), s()
    }, n.viewParticipants = function() {
        n.dialogLayout.lookUpMembers = !n.dialogLayout.lookUpMembers
    }, n.removeMember = function(u) {
        if (n.dialogData.dialogType === i.dialogType.PARTY) {
            var f = n.dialogData.party.Id;
            r.removeFromParty(f, u)
        }
        t.removeFromConversation(u, n.dialogData.Id).then(function() {
            n.isOverLoaded()
        }, function() {
            o.debug("---------- removeMember request failed------- ")
        })
    }, n.joinParty = function() {
        r.partyJoin(n.dialogData.party.Id).then(function(t) {
            t.Success && (n.chatLibrary.partyIds.push(n.dialogData.party.Id), n.chatLibrary.partiesDict[n.dialogData.party.Id] = {
                conversationId: n.dialogData.Id,
                layoutId: n.dialogData.layoutId
            }, n.dialogData.dialogType = i.dialogType.PARTY)
        }, function() {
            o.debug("-------------partyAcceptInvite request failed -------------")
        })
    }, n.joinGame = function() {
        r.joinGame(n.dialogData, n.chatLibrary.inApp)
    }, n.linkToProfile = function(n) {
        return n.stopPropagation(), n.target.classList.contains("disabled") ? (n.preventDefault(), !1) : void 0
    }, n.goToProfile = function() {
        n.chatLibrary.inApp && e.closeDialog()
    }, t.getMessages(n.dialogData.Id, null, n.dialogParams.pageSizeOfGetMessages).then(function(t) {
        t.length > 0 ? (n.dialogData.ChatMessages = [], n.dialogData.messagesDict = {}, u.manipulateMessages(n.dialogData, t, n.chatLibrary.friendsDict), n.dialogData.scrollBarType = i.scrollBarType.MESSAGE) : (n.dialogData.scrollBarType = i.scrollBarType.MESSAGE, n.updateDialog())
    }, function() {
        o.debug("---------- getMessages request failed------- ")
    }), n.$on("elastic:resize", function(t, r, u, f) {
        o.debug("---- oldHeight -----" + u + "---- newHeight -----" + f), u !== f && (i.setResizeInputLayout(n.chatLibrary, f, n.dialogData, n.dialogLayout), i.updateScrollbar(n.dialogLayout.scrollbarElm))
    })
}]);