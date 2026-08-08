// ~/viewapp/pages/chat/controllers/friendsController.js
"use strict";
chat.controller("chatFriendsController", ["$scope", "chatService", "chatUtility", "$log", function(n, t, i) {
    n.dialogLayout.scrollToBottom = !1, n.dialogLayout.IsdialogContainerVisible = !1, n.dialogParams = angular.copy(i.dialogParams), n.dialogType = angular.copy(i.dialogType), n.userPresenceTypes = angular.copy(i.userPresenceTypes), n.friendsScrollbarElm = i.getScrollBarSelector(n.dialogData, i.scrollBarType.FRIENDSELECTION), n.dialogData.scrollBarType = i.scrollBarType.FRIENDSELECTION, n.dialogData.isCreated = !0;
    var u = function() {
        t.getFriends(n.chatLibrary.userId, n.dialogParams.startIndexOfFriendList, n.dialogParams.pageSizeOfFriendList).then(function(t) {
            t ? (n.updateFriends(t), t.length === n.dialogParams.pageSizeOfFriendList && (n.dialogParams.loadMoreFriends = !0, n.dialogParams.startIndexOfFriendList = +n.dialogParams.startIndexOfFriendList + n.dialogParams.pageSizeOfFriendList, u())) : (n.dialogParams.loadMoreFriends = !1, n.dialogParams.startIndexOfFriendList = 0)
        })
    };
    n.chatLibrary.friendIds.length > 0 && n.updateFriends(), u(), n.isOverLoaded()
}]);