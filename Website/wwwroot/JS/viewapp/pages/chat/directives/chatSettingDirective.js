// ~/viewapp/pages/chat/directives/chatSettingDirective.js
"use strict";
chat.directive("chatSetting", ["chatService", "partyService", "$log", function() {
    return {
        restrict: "A",
        scope: !0,
        link: function(n, t) {
            var o = function(t) {
                    n.chatLibrary.inApp && t.preventDefault(), n.$digest(n.addFriends())
                },
                r, u, f, e;
            t.on("click touchstart", "#add-friends", o);
            r = function(t) {
                n.chatLibrary.inApp && t.preventDefault(), n.$digest(n.viewParticipants())
            };
            t.on("click touchstart", "#view-participants", r);
            u = function(t) {
                n.chatLibrary.inApp && t.preventDefault(), n.$digest(n.leaveGroup())
            };
            t.on("click touchstart", "#leave-group", u);
            if (f = function(t) {
                    var i = t.data;
                    n.$digest(n.abuseReport(i.userId, i.isConfirmed))
                }, n.dialogData && !n.dialogData.IsGroupChat) {
                angular.forEach(n.dialogData.userIds, function(t) {
                    t !== n.chatLibrary.userId && (e = t)
                });
                t.on("click touchstart", "#abuse-report", {
                    userId: e,
                    isConfirmed: !1
                }, f)
            }
        }
    }
}]);