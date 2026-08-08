// ~/viewapp/pages/chat/directives/friendsSelectionDirective.js
"use strict";
chat.directive("friendsSelection", ["chatUtility", function(n) {
    return {
        restrict: "A",
        templateUrl: Roblox.ChatTemplates.FriendsSelectionTemplate,
        link: function(t) {
            var u = function() {
                if (angular.isUndefined(t.dialogData) || angular.isUndefined(t.dialogData.selectedUserIds)) return !1;
                t.dialogLayout.inviteBtnDisabled = t.dialogData.dialogType === n.dialogType.NEWGROUPCHAT ? t.dialogData.selectedUserIds.length < 2 : t.dialogData.selectedUserIds.length === 0
            };
            u(), t.$watch(function() {
                return t.dialogData.selectedUserIds
            }, function() {
                u()
            }, !0)
        }
    }
}]);