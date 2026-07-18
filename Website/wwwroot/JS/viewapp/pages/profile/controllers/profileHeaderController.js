// ~/viewapp/pages/profile/controllers/profileHeaderController.js
"use strict";
profile.controller("profileHeaderController", ["$scope", "profileService", "robloxModalService", "realtimeService", "chatDispatchService", "$document", "$log", "captchaInterface", function(n, t, i, r, u, f, e, o) {
    function d() {
        var t = [],
            i = r.notificationTypes.friendshipNotifications;
        t[i.friendshipCreated] = n.updateLayoutByAcceptingFriendRequest, t[i.friendshipDestroyed] = n.updateLayoutByRemovingFriendRequest, t[i.friendshipDeclined] = n.updateLayoutByDecliningFriendRequest, t[i.friendshipRequested] = n.updateLayoutBySendingFriendRequest, r.listenToNotification(r.realTimeTypes.friendshipNotifications, t)
    }
    var w = "#profile-trade-items",
        l = "#profile-block-user",
        h = "#profile-header-more",
        k = "#profile-header-impersonate",
        c = "#profile-header-update-status",
        v = "#profile-follow-user",
        y = "#userStatusText",
        p = "#profile-message";
    n.captchaSetting = {
        isActivated: !1,
        captchaType: null,
        successCB: null,
        errorCB: null
    };
    var s = Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl("/Login/Signup.aspx") : "/Login/Signup.aspx",
        b = Roblox && Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl("/Home") : "/Home",
        a = function(n) {
            return n && n.EventArgs
        };
    n.setMessageBtnDisplay = function(n) {
        if (n) {
            var t = n.canMessage && n.userId > 0;
            n.enableMessageBtn = t, n.isChatDisabledByPrivacySetting ? (n.showMessageBtn = !0, n.showMessageLink = !1, n.showChatBtn = !1) : (n.showMessageBtn = !n.areFriends, n.showMessageLink = t && n.areFriends, n.showChatBtn = n.userId > 0 && n.areFriends)
        }
    }, n.profileHeaderLayout = {}, n.acceptFriendRequest = function() {
        n.profileHeaderLayout.userId ? t.acceptFriendRequest(n.profileHeaderLayout.acceptFriendRequestUrl, n.profileHeaderLayout.incomingFriendRequestId, n.profileHeaderLayout.profileUserId).then(function(t) {
            t && t.success ? n.updateLayoutByAcceptingFriendRequest() : (n.profileHeaderLayout.hasError = !0, n.profileHeaderLayout.errorMsg = t.message ? t.message : "")
        }, function() {
            n.profileHeaderLayout.hasError = !0, n.profileHeaderLayout.errorMsg = "Sending acceptFriendRequest is failed!"
        }) : window.location = s
    }, n.sendFriendRequest = function() {
        n.profileHeaderLayout.userId ? t.sendFriendRequest(n.profileHeaderLayout.sendFriendRequestUrl, n.profileHeaderLayout.profileUserId).then(function(t) {
            t && t.success ? (n.profileHeaderLayout.maySendFriendInvitation = !1, n.profileHeaderLayout.friendRequestPending = !0, f.triggerHandler("Roblox.Friendship.FriendRequestSent")) : (n.profileHeaderLayout.hasError = !0, n.profileHeaderLayout.errorMsg = t.message ? t.message : "")
        }, function(t) {
            t && t.message === "Captcha" ? (n.captchaSetting.successCB = n.sendFriendRequest, n.captchaSetting.captchaType = o.types.addFriend, n.captchaSetting.isActivated = !0) : (n.profileHeaderLayout.hasError = !0, n.profileHeaderLayout.errorMsg = "Sending acceptFriendRequest is failed!")
        }) : window.location = s
    }, n.updateFriendshipCount = function() {
        t.getFriendShipCount(n.profileHeaderLayout.getFriendshipCountUrl, n.profileHeaderLayout.profileUserId).then(function(t) {
            t && t.success && (n.profileHeaderLayout.friendsCount = t.count)
        })
    }, n.updateLayoutByAcceptingFriendRequest = function() {
        n.$evalAsync(function() {
            n.profileHeaderLayout.areFriends || (n.profileHeaderLayout.incomingFriendRequestPending = !1, n.profileHeaderLayout.friendRequestPending = !1, n.profileHeaderLayout.areFriends = !0, n.updateFriendshipCount(), n.setMessageBtnDisplay(n.profileHeaderLayout))
        })
    }, n.updateLayoutByRemovingFriendRequest = function(t) {
        n.$evalAsync(function() {
            if (a(t) && parseInt(t.EventArgs.UserId2) !== parseInt(n.profileHeaderLayout.profileUserId)) return !1;
            n.profileHeaderLayout.areFriends && (n.profileHeaderLayout.maySendFriendInvitation = !0, n.profileHeaderLayout.originalMaySendFriendInvitation = n.profileHeaderLayout.maySendFriendInvitation, n.profileHeaderLayout.areFriends = !1, n.updateFriendshipCount(), n.setMessageBtnDisplay(n.profileHeaderLayout))
        })
    }, n.updateLayoutByDecliningFriendRequest = function() {
        n.$evalAsync(function() {
            n.profileHeaderLayout.maySendFriendInvitation = !0, n.profileHeaderLayout.originalMaySendFriendInvitation = n.profileHeaderLayout.maySendFriendInvitation, n.profileHeaderLayout.areFriends = !1
        })
    }, n.updateLayoutBySendingFriendRequest = function(t) {
        a(t) && parseInt(t.EventArgs.UserId1) === parseInt(n.profileHeaderLayout.profileUserId) && n.$evalAsync(function() {
            n.profileHeaderLayout.incomingFriendRequestPending = !0, n.profileHeaderLayout.friendRequestPending = !1, n.profileHeaderLayout.areFriends = !1
        })
    }, n.removeFriend = function() {
        n.profileHeaderLayout.userId ? t.removeFriend(n.profileHeaderLayout.removeFriendRequestUrl, n.profileHeaderLayout.profileUserId).then(function(t) {
            t && t.success ? n.updateLayoutByRemovingFriendRequest() : (n.profileHeaderLayout.hasError = !0, n.profileHeaderLayout.errorMsg = t.message ? t.message : "")
        }, function() {
            n.profileHeaderLayout.hasError = !0, n.profileHeaderLayout.errorMsg = "Sending acceptFriendRequest is failed!"
        }) : window.location = s
    };
    angular.element(h).on("click", v, function() {
        n.profileHeaderLayout.isFollowing ? n.unFollow() : n.follow()
    });
    n.follow = function() {
        n.profileHeaderLayout.userId ? t.follow(n.profileHeaderLayout.followUrl, n.profileHeaderLayout.profileUserId).then(function(t) {
            t && t.success ? (n.profileHeaderLayout.isFollowing = !0, n.profileHeaderLayout.followersCount = parseInt(n.profileHeaderLayout.followersCount) + 1) : (n.profileHeaderLayout.hasError = !0, n.profileHeaderLayout.errorMsg = t.message ? t.message : "")
        }, function(t) {
            t && t.message === "Captcha" ? (n.captchaSetting.successCB = n.follow, n.captchaSetting.captchaType = o.types.follow, n.captchaSetting.isActivated = !0) : (n.profileHeaderLayout.hasError = !0, n.profileHeaderLayout.errorMsg = "Sending acceptFriendRequest is failed!")
        }) : window.location = s
    }, n.unFollow = function() {
        n.profileHeaderLayout.userId ? t.follow(n.profileHeaderLayout.unFollowUrl, n.profileHeaderLayout.profileUserId).then(function(t) {
            t && t.success ? (n.profileHeaderLayout.isFollowing = !1, n.profileHeaderLayout.followersCount = parseInt(n.profileHeaderLayout.followersCount) - 1) : (n.profileHeaderLayout.hasError = !0, n.profileHeaderLayout.errorMsg = t.message ? t.message : "")
        }, function() {
            n.profileHeaderLayout.hasError = !0, n.profileHeaderLayout.errorMsg = "Sending acceptFriendRequest is failed!"
        }) : window.location = s
    }, n.sendMessage = function() {
        window.location = n.profileHeaderLayout.userId ? n.profileHeaderLayout.messageUrl : s
    };
    angular.element(h).on("click", p, function() {
        n.sendMessage()
    });
    n.chat = function() {
        var t = n.profileHeaderLayout.profileUserId,
            i = u.buildPermissionVerifier(n.profileHeaderLayout);
        u.startChat(t, i)
    };
    angular.element(h).on("click touchstart", w, function() {
        n.tradeItems()
    });
    n.tradeItems = function() {
        window.open("/Trade/TradeWindow.aspx?TradePartnerID=" + n.profileHeaderLayout.profileUserId, "_blank", "scrollbars=0, resizeable=1, height=658, width=898")
    };
    angular.element(h).on("click", l, function() {
        n.blockUser()
    });
    n.blockUser = function() {
        var r = n.profileHeaderLayout.isVieweeBlocked ? "profile-block-user-modal.html" : "profile-unblock-user-modal.html";
        i.open(r, "").then(function() {
            var i = n.profileHeaderLayout.isVieweeBlocked ? "/userblock/unblockuser" : "/userblock/blockuser";
            t.blockUser(i, n.profileHeaderLayout.profileUserId).then(function(t) {
                var i, r;
                t && t.success ? (i = n.profileHeaderLayout.isVieweeBlocked, i ? (n.profileHeaderLayout.maySendFriendInvitation = n.profileHeaderLayout.originalMaySendFriendInvitation, n.profileHeaderLayout.mayFollow = n.profileHeaderLayout.originalMayFollow, n.setMessageBtnDisplay(n.profileHeaderLayout), n.profileHeaderLayout.canTrade = n.profileHeaderLayout.originalCanTrade) : (n.profileHeaderLayout.maySendFriendInvitation = !1, n.profileHeaderLayout.incomingFriendRequestPending = !1, n.profileHeaderLayout.friendRequestPending = !1, n.profileHeaderLayout.mayFollow = !1, n.profileHeaderLayout.enableMessageBtn = !1, n.profileHeaderLayout.canTrade = !1), n.profileHeaderLayout.isVieweeBlocked = !i, r = n.profileHeaderLayout.isVieweeBlocked ? "Unblock User" : "Block User", angular.element(l).text(r)) : (n.profileHeaderLayout.hasError = !0, n.profileHeaderLayout.errorMsg = "Operation failed! You may have blocked too many people.")
            }, function() {
                n.profileHeaderLayout.hasError = !0, n.profileHeaderLayout.errorMsg = "Sending acceptFriendRequest is failed!"
            })
        }, function() {
            e.debug("Modal dismissed at: " + new Date)
        })
    };
    angular.element(h).on("click touchstart", k, function() {
        n.impersonateUser()
    });
    n.impersonateUser = function() {
        t.impersonateUser(n.profileHeaderLayout.impersonateUrl, n.profileHeaderLayout.profileUserId).then(function(t) {
            t && t.success ? window.location = b : (n.profileHeaderLayout.hasError = !0, n.profileHeaderLayout.errorMsg = t.message ? t.message : "")
        }, function() {
            n.profileHeaderLayout.hasError = !0, n.profileHeaderLayout.errorMsg = "Sending impersonateUser has failed!"
        })
    }, n.revealStatusForm = function() {
        n.profileHeaderLayout.mayUpdateStatus && n.$evalAsync(function() {
            n.profileHeaderLayout.statusFormShown = !0, n.profileHeaderLayout.hasError = !1, n.profileHeaderLayout.statusInputFocused = !0, n.profileHeaderLayout.statusFormSending = !1, n.profileHeaderLayout.focusedElement = null, n.profileHeaderLayout.statusTextInput = n.profileHeaderLayout.statusText
        })
    }, n.updateStatus = function(i) {
        i ? (n.$evalAsync(function() {
            n.profileHeaderLayout.statusFormSending = !0
        }), t.updateStatus(n.profileHeaderLayout.updateStatusUrl, n.sanitizeStatus(n.profileHeaderLayout.statusTextInput)).then(function(t) {
            t && t.success ? (n.profileHeaderLayout.statusFormShown = !1, n.profileHeaderLayout.hasError = !1, n.profileHeaderLayout.errorMsg = "", n.profileHeaderLayout.statusInputFocused = !1, n.profileHeaderLayout.statusText = t.message) : (n.profileHeaderLayout.hasError = !0, n.profileHeaderLayout.errorMsg = t.message ? t.message : "There was an error updating your status."), n.profileHeaderLayout.statusFormSending = !1
        }, function() {
            n.profileHeaderLayout.hasError = !0, n.profileHeaderLayout.errorMsg = "There was an error updating your status.", n.profileHeaderLayout.statusFormSending = !1
        })) : n.$evalAsync(function() {
            n.profileHeaderLayout.statusFormShown = !1, n.profileHeaderLayout.statusFormSending = !1, n.profileHeaderLayout.statusInputFocused = !1, n.profileHeaderLayout.hasError = !1, n.profileHeaderLayout.errorMsg = ""
        })
    }, n.blurStatusForm = function(t) {
        if (t && typeof t.target != "undefined") {
            var i = t.target,
                r = "#" + i.getAttribute("id");
            if (i.getAttribute("id") != null && r == c || r == y) return
        }
        n.updateStatus(!1)
    }, n.sanitizeStatus = function(n) {
        return n.replace(/}|{/gi, "")
    }, d();
    angular.element(h).on("click touchstart", c, function() {
        n.revealStatusForm()
    });
    angular.element(f).on("click touchstart", function(t) {
        var i = angular.element("#statusForm");
        if (i[0]) {
            if (i[0].contains(t.target)) return;
            n.blurStatusForm(t)
        }
    })
}]);