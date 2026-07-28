// ~/viewapp/pages/messages/controllers/messagesContentController.js
"use strict";
messages.controller("messagesContentController", ["$scope", "messagesService", "$document", "$location", "$log", function(n, t, i, r) {
    n.toggleMessagesBox = function(u) {
        if (n.resetCurrentStatus(), u === n.moduleState.detail) {
            n.currentStatus.loadMessages = !1, this.message.IsRead || (this.message.IsRead = !0, t.beginMarkMessagesRead([this.message.Id], !0, !1).then(function() {
                i.triggerHandler("Roblox.Messages.CountChanged")
            })), n.toggleSelection(this.$index), n.messageContent.selectedMessage = this.message, Roblox === undefined || Roblox.Linkify === undefined || this.message.IsSystemMessage || (this.message.Body = Roblox.Linkify.String(this.message.Body));
            var f = r.search();
            f.messageIdx = this.$index, r.search(f)
        } else n.messageContent.selectedMessageIndexs = [], n.messageContent.selectedMessage = null;
        n.currentStatus.moduleState = u
    }, n.messageContent.selectedAll = !1, n.messageContent.selectedMessageIndexs = [], n.resetCheckboxStatusByMessageIndexes = function() {
        angular.forEach(n.messageContent.selectedMessageIndexs, function(t) {
            var i = n.messageContent.messages.data.Collection[t];
            i.checked = !1
        }), n.messageContent.selectedMessageIndexs = [], n.messageContent.selectedAll && (n.messageContent.selectedAll = !1)
    }, n.toggleSelection = function(t) {
        var i = n.messageContent.selectedMessageIndexs.indexOf(t);
        i > -1 ? n.messageContent.selectedMessageIndexs.splice(i, 1) : n.messageContent.selectedMessageIndexs.push(t)
    }, n.checkAll = function() {
        n.messageContent.selectedAll = !n.messageContent.selectedAll, angular.forEach(n.messageContent.messages.data.Collection, function(t, i) {
            t.checked = n.messageContent.selectedAll, n.messageContent.selectedAll && n.messageContent.selectedMessageIndexs.push(i)
        }), n.messageContent.selectedAll || (n.messageContent.selectedMessageIndexs = [])
    }, n.sendMessage = {
        disableReplyBtn: !1,
        disableSendBtn: !1,
        replyContent: "",
        sendComplete: !1,
        sendResult: {},
        includePreviousMessage: !0
    }
}]);