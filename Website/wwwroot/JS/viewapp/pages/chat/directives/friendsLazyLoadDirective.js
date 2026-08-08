// ~/viewapp/pages/chat/directives/friendsLazyLoadDirective.js
"use strict";
chat.directive("friendsLazyLoad", ["chatService", "$log", function(n) {
    return {
        restrict: "A",
        scope: !0,
        link: function(t, i) {
            var u = function() {
                if (!t.dialogParams.loadMoreFriends) return !1;
                t.dialogLayout.isChatLoading = !0, n.getFriends(t.chatLibrary.userId, t.dialogParams.startIndexOfFriendList, t.dialogParams.pageSizeOfFriendList).then(function(n) {
                    t.dialogLayout.isChatLoading = !1, t.updateFriends(n), n.length < t.dialogParams.pageSizeOfFriendList ? t.dialogParams.loadMoreFriends = !1 : t.dialogParams.startIndexOfFriendList = +t.dialogParams.startIndexOfFriendList + t.dialogParams.pageSizeOfFriendList, i.mCustomScrollbar("update")
                }, function() {
                    t.dialogLayout.isChatLoading = !1
                })
            };
            t.chatLibrary.inApp ? Roblox.Scrollbar.listenToScroll(i, u) : i.mCustomScrollbar({
                autoExpandScrollbar: !1,
                scrollInertia: 5,
                contentTouchScroll: 1,
                mouseWheel: {
                    preventDefault: !0
                },
                callbacks: {
                    onTotalScroll: u,
                    onOverflowYNone: u
                }
            })
        }
    }
}]);