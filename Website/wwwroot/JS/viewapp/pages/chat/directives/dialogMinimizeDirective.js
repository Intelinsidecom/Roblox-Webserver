// ~/viewapp/pages/chat/directives/dialogMinimizeDirective.js
"use strict";
chat.directive("dialogMinimize", ["$window", "$compile", "$templateCache", "chatService", "chatUtility", "$log", function(n, t, i, r, u, f) {
    return {
        restrict: "A",
        scope: {
            chatLibrary: "="
        },
        templateUrl: Roblox.ChatTemplates.DialogMinimizeTemplate,
        link: function(n, t) {
            var r = function() {
                var i = n.chatLibrary.chatLayout,
                    r = n.chatLibrary.dialogIdList.length,
                    u = i.widthOfChat,
                    f = i.widthOfDialog + i.spaceOfDialog,
                    e = +u + r * f + i.spaceOfDialog;
                t.css("right", e)
            };
            n.dialogType = u.dialogType, n.hasMinimizedDialogs = !1, n.layoutIdHasClicked = !1, n.openDialog = function(t) {
                var i, r;
                f.debug(" -------------------openDialog------------------ " + t), i = n.chatLibrary.dialogIdList.pop(), n.chatLibrary.dialogDict[i].isUpdated = !0, n.chatLibrary.dialogDict[i].updateStatus = u.dialogStatus.MINIMIZE, n.chatLibrary.dialogIdList.push(t), n.chatLibrary.dialogDict[t].isUpdated = !0, n.chatLibrary.dialogDict[t].updateStatus = u.dialogStatus.REPLACE, r = n.chatLibrary.minimizedDialogIdList.indexOf(t), r > -1 && (n.chatLibrary.minimizedDialogIdList.splice(r, 1), delete n.chatLibrary.minimizedDialogData[t])
            }, n.remove = function(t) {
                var i = n.chatLibrary.minimizedDialogIdList.indexOf(t);
                i > -1 && (n.chatLibrary.minimizedDialogIdList.splice(i, 1), delete n.chatLibrary.minimizedDialogData[t], delete n.chatLibrary.dialogDict[t])
            }, Roblox.BootstrapWidgets.SetupPopover("top", {
                selector: "#dialogs-minimize"
            }), n.$watch(function() {
                return n.chatLibrary.minimizedDialogIdList
            }, function(t, i) {
                angular.isUndefined(t) || t == i || (f.debug("------ watch minimizedDialogIdList ----- "), t.length > 0 ? (n.hasMinimizedDialogs || (n.hasMinimizedDialogs = !0), r()) : t.length === 0 && (n.hasMinimizedDialogs = !1))
            }, !0)
        }
    }
}]);