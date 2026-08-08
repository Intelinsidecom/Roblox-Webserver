// ~/viewapp/pages/chat/directives/lazyLoadDirective.js
"use strict";
chat.directive("lazyLoad", ["chatService", "chatUtility", "$log", function(n, t, i) {
    return {
        restrict: "A",
        scope: !0,
        link: function(r, u) {
            var s = angular.element(document.querySelector(".chat-friend-list")),
                e = function() {
                    r.buildChatUserListByFriends(r.chatApiParams.startIndexOfFriendList, r.chatApiParams.pageSizeOfFriendList).then(function(n) {
                        r.chatLibrary.chatLayout.isChatLoading = !1, r.filterFriends(n), n.length > 0 ? (r.chatApiParams.startIndexOfFriendList = +r.chatApiParams.startIndexOfFriendList + r.chatApiParams.pageSizeOfFriendList, t.updateScrollbar(t.chatLayout.scrollbarClassName), n.length < r.chatApiParams.pageSizeOfFriendList && (r.chatApiParams.loadMoreFriends = !1, r.chatApiParams.startIndexOfFriendList = 0)) : (r.chatApiParams.loadMoreFriends = !1, r.chatApiParams.startIndexOfFriendList = 0)
                    }, function() {
                        r.chatLibrary.chatLayout.isChatLoading = !1, i.debug("---error from get Friends in lazyLoadDirective.js---")
                    })
                },
                o = function() {
                    if (!r.chatApiParams || r.chatLibrary.chatLayout.errorMaskEnable || !r.chatApiParams.loadMoreConversations && !r.chatApiParams.loadMoreFriends) return !1;
                    r.chatLibrary.chatLayout.isChatLoading = !0, r.chatApiParams.loadMoreConversations && n.getUserConversations(r.chatApiParams.pageNumberOfConversations, r.chatApiParams.pageSizeOfConversations).then(function(n) {
                        r.chatLibrary.chatLayout.isChatLoading = !1, n && n.length > 0 ? (r.refreshFriendsDict(), r.buildChatUserListByConversations(n), r.chatApiParams.pageNumberOfConversations++, t.updateScrollbar(t.chatLayout.scrollbarClassName), n.length < r.chatApiParams.pageSizeOfConversations && (r.chatApiParams.loadMoreConversations = !1, r.chatApiParams.loadMoreFriends = !0, r.chatApiParams.pageNumberOfConversations = 1, r.chatApiParams.pageNumberOfPartyInvites = 1, e())) : (r.chatApiParams.loadMoreConversations = !1, r.chatApiParams.loadMoreFriends = !0, r.chatApiParams.pageNumberOfConversations = 1, e())
                    }, function() {
                        r.chatLibrary.chatLayout.isChatLoading = !1, i.debug("---error from get Conversations in lazyLoadDirective.js---")
                    }), r.chatApiParams.loadMoreFriends && e()
                };
            r.chatLibrary.inApp ? Roblox.Scrollbar.listenToScroll(u, o) : u.mCustomScrollbar({
                autoExpandScrollbar: !1,
                scrollInertia: 5,
                contentTouchScroll: 1,
                mouseWheel: {
                    preventDefault: !0
                },
                callbacks: {
                    onTotalScrollOffset: 100,
                    onTotalScroll: o,
                    onOverflowYNone: o
                }
            })
        }
    }
}]);