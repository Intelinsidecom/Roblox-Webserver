// ~/viewapp/pages/messages/directives/messagesDetailDirective.js
"use strict";
messages.directive("rbxMessagesDetail", ["messagesService", "$sce", "$log", function(n, t, i) {
    return {
        restrict: "A",
        scope: {
            toggleMessagesBox: "&",
            selectedMessage: "=",
            sendMessage: "=",
            currentStatus: "=",
            messageDefaults: "="
        },
        replace: !0,
        templateUrl: Roblox.websiteTemplates.messagesBodyTemplate,
        link: function(r) {
            i.debug("============== message detail =============="), r.sendMessage.disableSendBtn = !1, r.sendMessage.disableReplyBtn = !1, r.sendMessage.sendComplete = !1, r.sendMessage.sendResult = {}, r.sendMessage.replyContent = "", r.sendReply = function() {
                i.debug("inside send reply function"), r.sendMessage.disableSendBtn = !0;
                var u = t.trustAsHtml(r.sendMessage.replyContent).$$unwrapTrustedValue();
                n.beginSendMessage(r.selectedMessage.Subject, u, r.selectedMessage.Sender.UserId, r.selectedMessage.Id, r.sendMessage.includePreviousMessage)
            }, r.presetMessage = function(n) {
                r.sendMessage.replyContent = n.Message
            }, r.$watch(function() {
                return n.sentMessageComplete
            }, function(t, i) {
                t != i && t && (r.sendMessage.sendComplete = !0, r.sendMessage.disableSendBtn = !1, r.sendMessage.sendResult = n.sendMessageError, r.sendMessage.sendResult.hasError ? Roblox.BootstrapWidgets.ToggleSystemMessage($(".alert-warning"), 100, 2e3) : (Roblox.BootstrapWidgets.ToggleSystemMessage($(".alert-success"), 100, 2e3), $(document).triggerHandler("Roblox.Messages.MessageSent")), n.sendMessageError.hasError || (r.sendMessage.disableReplyBtn = !1, r.sendMessage.replyContent = ""))
            }, !0)
        }
    }
}]);