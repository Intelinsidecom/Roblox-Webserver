// ~/viewapp/pages/friends/services/friendsService.js
"use strict";
friends.factory("friendsService", ["httpService", "$log", function(n) {
    function u(n) {
        switch (n) {
            case i.friends:
                return "AllFriends";
            case i.friendRequests:
                return "FriendRequests";
            case i.following:
                return "Following";
            case i.followers:
                return "Followers";
            default:
                return "AllFriends"
        }
    }
    var f = 18,
        r = 100,
        i = {
            friends: "friends",
            following: "following",
            followers: "followers",
            friendRequests: "friend-requests"
        };
    return {
        userId: 0,
        friendTabs: i,
        getFriendsTypeName: u,
        setUserId: function() {
            var n = angular.element(document.getElementById("state-properties"));
            return this.userId = n.attr("data-userid"), !0
        },
        beginUpdateFriends: function(t, i) {
            var e = {
                    url: "/users/friends/list-json",
                    noCache: !0
                },
                o = {
                    userId: this.userId,
                    currentPage: t,
                    imgWidth: r,
                    imgHeight: r,
                    pageSize: f,
                    friendsType: u(i)
                };
            return n.httpGet(e, o)
        },
        acceptFriendRequest: function(t, i, r) {
            var u = {
                    url: t
                },
                f = {
                    targetUserID: r,
                    invitationID: i
                };
            return n.httpPost(u, f)
        },
        declineFriendRequest: function(t, i, r) {
            var u = {
                    url: t
                },
                f = {
                    targetUserID: r,
                    invitationID: i
                };
            return n.httpPost(u, f)
        },
        declineAllFriendRequests: function(t) {
            var i = {
                url: t
            };
            return n.httpPost(i, null)
        },
        follow: function(t, i) {
            var r = {
                    url: t
                },
                u = {
                    targetUserId: i
                };
            return n.httpPost(r, u)
        },
        unfriend: function(t, i) {
            var r = {
                    url: t
                },
                u = {
                    targetUserId: i
                };
            return n.httpPost(r, u)
        }
    }
}]);