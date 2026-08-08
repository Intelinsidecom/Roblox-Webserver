// ~/viewapp/pages/chat/controllers/dialogsController.js
"use strict";
chat.controller("dialogsController", ["$scope", "chatService", "chatUtility", "partyService", "messageService", "chatHybridService", "$window", "$log", function(n, t, i, r, u, f, e, o) {
    var s = function(t, r, u) {
            var e = u,
                o = n.getLayoutId(t.Id, r),
                s;
            t.dialogType = r, e !== o ? (n.chatLibrary.inApp && !n.chatLibrary.tabletInApp && f.openDialog(), n.getUserInfoForConversation(t), t.layoutId = o, t.isConversation = !0, n.updateChatViewModel(t, !0), n.chatUserDict[e].selectedUserIds = [], n.chatUserDict[e].selectedUsersDict = {}, delete n.chatUserDict[e], s = n.chatLibrary.dialogIdList.indexOf(e), s > -1 ? (n.destroyDialogLayout(u), delete n.chatLibrary.dialogDict[e], n.chatLibrary.dialogIdList[s] = o) : n.chatLibrary.dialogIdList.push(o), n.chatLibrary.dialogDict[o] = angular.copy(i.dialogInitValue)) : (n.getUserInfoForConversation(t), n.updateChatViewModel(t, !0), n.chatLibrary.dialogDict[e].isUpdated = !0, n.chatLibrary.dialogDict[e].updateStatus = i.dialogStatus.REFRESH)
        },
        h = function(t) {
            var r = i.newParty.layoutId;
            n.chatUserDict[r] = i.newParty, n.chatUserDict[r].selectedUserIds = [], n.chatUserDict[r].selectedUserIds.push(t), n.chatLibrary.party.isPartyExisted = !0, n.sendInvite(r)
        };
    n.createParty = function(t, u) {
        i.cleanPartyList(n.chatLibrary, n.chatUserDict, null);
        var f = n.chatUserDict[u].selectedUserIds,
            e = t.Id;
        n.chatUserDict[u].selectedUserIds = [], r.partyCreate(e, f).then(function(r) {
            n.chatLibrary.party.isPartyLeader = !0, t.party = r, s(t, i.dialogType.PARTY, u)
        })
    }, n.sendInvite = function(u) {
        if (o.debug("------------- sendInvite ------------"), n.chatUserDict[u].dialogType !== i.dialogType.CHAT && n.chatUserDict[u].dialogType !== i.dialogType.GROUPCHAT || n.chatUserDict[u].addMoreFriends) n.chatUserDict[u].addMoreFriends ? t.addToConversation(n.chatUserDict[u].selectedUserIds, n.chatUserDict[u].Id).then(function(t) {
            var f, e;
            t.ConversationId && (n.chatUserDict[u].addMoreFriends = !1, f = n.chatUserDict[u], (n.chatUserDict[u].dialogType === i.dialogType.PARTY || n.chatUserDict[u].party) && (e = f.selectedUserIds, e.forEach(function(n) {
                r.partyInvite(f.party.Id, n)
            })), f.userIds = f.userIds.concat(f.selectedUserIds), f.selectedUserIds = [], f.selectedUsersDict = {})
        }, function() {
            o.debug(" ---- addToConversation ---- failed --- from sendInvite request------")
        }) : n.chatUserDict[u].dialogType === i.dialogType.NEWGROUPCHAT && t.startGroupConversation(n.chatUserDict[u].selectedUserIds).then(function(n) {
            if (n.Success) {
                var t = n.Conversation;
                s(t, i.dialogType.GROUPCHAT, u)
            }
        }, function() {
            o.debug(" ---- startGroupConversation ---- failed --- on sending startGroupConveration request------")
        });
        else {
            var f = n.chatUserDict[u],
                e = angular.copy(f.userIds),
                h = e.indexOf(n.chatLibrary.userId);
            h > -1 && e.splice(h, 1), f.selectedUserIds = e, n.createParty(f, u)
        }
    }
}]);