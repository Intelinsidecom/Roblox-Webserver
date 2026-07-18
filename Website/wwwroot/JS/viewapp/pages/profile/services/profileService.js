// ~/viewapp/pages/profile/services/profileService.js
"use strict";
profile.factory("profileService", ["httpService", "$rootScope", "$timeout", "$log", function(n, t, i) {
    var u = {};
    return {
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
        sendFriendRequest: function(t, i) {
            var r = {
                    url: t
                },
                u = {
                    targetUserID: i
                };
            return n.httpPost(r, u)
        },
        removeFriend: function(t, i) {
            var r = {
                    url: t
                },
                u = {
                    targetUserID: i
                };
            return n.httpPost(r, u)
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
        blockUser: function(t, i) {
            var r = {
                    url: t
                },
                u = {
                    blockeeId: i
                };
            return n.httpPost(r, u)
        },
        impersonateUser: function(t, i) {
            var r = {
                    url: t
                },
                u = {
                    displayedUserId: i
                };
            return n.httpPost(r, u)
        },
        updateStatus: function(t, i) {
            var u = {
                    url: t,
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8"
                    }
                },
                r = {
                    status: i,
                    sendToFacebook: !1
                };
            return r = $.param(r), n.httpPost(u, r)
        },
        getFriendShipCount: function(t, i) {
            var r = {
                    url: t,
                    withCredentials: !0,
                    retryable: !0
                },
                u = {
                    userId: i
                };
            return n.httpGet(r, u)
        },
        getCollections: function(t, i) {
            var r = {
                    url: t
                },
                u = {
                    userId: i
                };
            return n.httpGet(r, u)
        },
        getGroups: function(t, i) {
            var r = {
                    url: t
                },
                u = {
                    userId: i
                };
            return n.httpGet(r, u)
        },
        getPlayerAssets: function(t, i, r) {
            var u = {
                    url: t
                },
                f = {
                    assetTypeId: r,
                    userId: i
                };
            return n.httpGet(u, f)
        },
        setProfileData: function(n) {
            u = n
        },
        getProfileData: function() {
            return u
        },
        refreshLazyLoadImage: function() {
            i(function() {
                t.$emit("lazyImg:refresh")
            })
        }
    }
}]);