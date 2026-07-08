// ~/viewapp/pages/friends/controllers/friendsController.js
"use strict";
friends.controller("friendsController", ["$scope", "friendsService", "robloxModalService", "$log", "$timeout", "$document", "$location", "realtimeService", "captchaInterface", function(n, t, i, r, u, f, e, o, s) {
    function v() {
        n.$evalAsync(function() {
            (n.currentData.activeTab === t.friendTabs.friends || n.currentData.activeTab === t.friendTabs.friendRequests) && n.refreshTab()
        })
    }

    function y() {
        n.$evalAsync(function() {
            n.currentData.activeTab === t.friendTabs.friends && n.refreshTab()
        })
    }

    function a() {
        n.$evalAsync(function() {
            n.currentData.activeTab === t.friendTabs.friendRequests && n.refreshTab()
        })
    }

    function p() {
        var n = [],
            t = o.notificationTypes.friendshipNotifications;
        n[t.friendshipCreated] = v, n[t.friendshipDestroyed] = y, n[t.friendshipDeclined] = a, n[t.friendshipRequested] = a, o.listenToNotification(o.realTimeTypes.friendshipNotifications, n)
    }
    var c = Roblox && Roblox.Endpoints ? Roblox.Endpoints.generateAbsoluteUrl("/Login/Signup.aspx") : "/Login/Signup.aspx",
        l = {
            FriendsLimitExceeded: 1,
            AlreadyExists: 2,
            InvalidParameters: 3,
            UserWasNeverAFriend: 4,
            UserWasNeverAFollower: 5,
            SelfFriendingAttempt: 6,
            SelfFollowingAttempt: 7,
            NotRecipient: 8,
            FloodLimitExceeded: 9,
            DoesNotExist: 10,
            OperationDisabled: 11,
            CurrentUserFriendsLimitExceeded: 12,
            OtherUserFriendsLimitExceeded: 13
        },
        h;
    n.captchaSetting = {
        isActivated: !1,
        captchaType: s.types.follow,
        successCB: null,
        errorCB: null
    }, n.friendsTabs = [{
        name: t.friendTabs.friends,
        label: "Friends",
        tooltip: "Friends are established when two ROBLOX users mutually agree to friendship."
    }, {
        name: t.friendTabs.following,
        label: "Following",
        tooltip: "People whose activity you have chosen to follow."
    }, {
        name: t.friendTabs.followers,
        label: "Followers",
        tooltip: "People who have chosen to follow your activity."
    }, {
        name: t.friendTabs.friendRequests,
        label: "Friend Requests",
        tooltip: "Friends are established when two ROBLOX users mutually agree to friendship."
    }], n.currentData = n.currentData ? n.currentData : {
        currentPage: 0,
        totalPages: 0,
        activeTab: n.friendsTabs[0].tab,
        stateLabel: "",
        hasError: !1,
        errorMsg: "",
        ignoreAll: !1,
        templateVisible: !1,
        totalPerPage: 18,
        isAUser: !1,
        tooltipLabel: ""
    }, n.friendsContent = {
        friends: {}
    }, n.isDeclineAllFriendRequestsButtonDisabled = !1, h = angular.element(document.getElementById("state-properties")), n.currentData.userId = h.attr("data-loggedinuserid"), n.currentData.profileUserId = h.attr("data-userid"), n.currentData.removeFriendUrl = h.attr("data-removefriendurl"), n.currentData.acceptFriendUrl = h.attr("data-acceptfriendurl"), n.currentData.declineFriendUrl = h.attr("data-declinefriendurl"), n.currentData.declineAllFriendsUrl = h.attr("data-declineallfriendsurl"), n.currentData.followUrl = h.attr("data-followurl"), n.currentData.unFollowUrl = h.attr("data-unfollowurl"), n.currentData.unFriendUrl = h.attr("data-unfriendurl"), n.currentData.userName = h.attr("data-username"), n.currentData.isMyProfile = n.currentData.userId == n.currentData.profileUserId ? !0 : !1, n.currentData.isAUser = !(n.currentData.userId == 0), n.populateFriendMetaData = function(t) {
        for (var i, r = 0; r < t.Friends.length; r++) i = t.Friends[r], i.isPresenceOnline = i.IsOnline && !i.InGame && !i.InStudio, i.isPresenceGame = i.InGame && i.IsOnline, i.isPresenceStudio = i.InStudio && i.IsOnline, i.lastLocation = i.LastLocation, i.isAvailable = !0, n.currentData.activeTab === "friends" || n.currentData.activeTab === "following" && i.IsFollowed ? i.IsOnline || (i.lastLocation = "Offline") : (i.lastLocation = i.IsOnline ? "Online" : "Offline", i.AbsolutePlaceURL = "", i.isAvailable = !i.IsDeleted)
    }, n.getFriends = function(i, r) {
        t.setUserId(), t.beginUpdateFriends(r, i).then(function(t) {
            var i = t.CurrentPage ? t.CurrentPage : 0;
            n.currentData.currentPage = i, n.currentData.totalPages = Math.ceil(t.TotalFriends / n.currentData.totalPerPage), t.Friends && t.Friends.length > 0 && n.populateFriendMetaData(t), n.friendsContent.friends = {
                data: t,
                hasError: !1
            }, n.currentData.templateVisible = !0
        }, function(t) {
            n.currentData.currentPage = 1, n.currentData.totalPages = 1, n.friendsContent.friends = {
                data: t,
                hasError: !0
            }
        })
    }, n.acceptFriendRequest = function(r, u, f) {
        n.currentData.isAUser ? t.acceptFriendRequest(n.currentData.acceptFriendUrl, n.currentData.userId, r).then(function(t) {
            t && t.success ? (u.UserId = null, n.friendsContent.friends.data.TotalFriends = n.friendsContent.friends.data.TotalFriends - 1, n.getFriends("friend-requests", f)) : (n.currentData.hasError = !0, n.currentData.errorMsg = t.message ? t.message : "", t.errorId === l.CurrentUserFriendsLimitExceeded ? i.open("current-user-reached-friends-max.html", "") : t.errorId === l.OtherUserFriendsLimitExceeded ? i.open("requester-reached-friends-max.html", "") : i.open("something-went-wrong.html", ""))
        }, function() {
            n.currentData.hasError = !0, n.currentData.errorMsg = "Sending acceptFriendRequest has failed!", i.open("something-went-wrong.html", "")
        }) : window.location = c
    }, n.declineFriendRequest = function(i, r, u) {
        n.currentData.isAUser ? t.declineFriendRequest(n.currentData.declineFriendUrl, n.currentData.userId, i).then(function(t) {
            r.UserId = null, n.friendsContent.friends.data.TotalFriends = n.friendsContent.friends.data.TotalFriends - 1, t ? n.getFriends("friend-requests", u) : (n.currentData.hasError = !0, n.currentData.errorMsg = t.message ? t.message : ""), f.triggerHandler("Roblox.Friends.CountChanged")
        }, function() {
            n.currentData.hasError = !0, n.currentData.errorMsg = "Sending declineFriendRequest has failed!"
        }) : window.location = c
    }, n.declineAllFriendRequests = function() {
        n.isDeclineAllFriendRequestsButtonDisabled = !0, n.currentData.isAUser ? t.declineAllFriendRequests(n.currentData.declineAllFriendsUrl).then(function() {
            n.isDeclineAllFriendRequestsButtonDisabled = !1, n.currentData.ignoreAll = !0, n.friendsContent.friends.data.TotalFriends = 0, n.currentData.totalPages = 0, f.triggerHandler("Roblox.Friends.CountChanged")
        }, function() {
            n.isDeclineAllFriendRequestsButtonDisabled = !1, n.currentData.hasError = !0, n.currentData.errorMsg = "Sending declineFriendRequests has failed!"
        }) : window.location = c
    }, n.follow = function(i) {
        var r = i.UserId;
        n.currentData.isAUser ? t.follow(n.currentData.followUrl, r).then(function(t) {
            t && t.success ? i.IsFollowed = !0 : (n.currentData.hasError = !0, n.currentData.errorMsg = t.message ? t.message : "")
        }, function(t) {
            t && t.message === "Captcha" ? (n.captchaSetting.successCB = function() {
                n.follow(i)
            }, n.captchaSetting.isActivated = !0) : (n.currentData.hasError = !0, n.currentData.errorMsg = "Sending follow has failed!")
        }) : window.location = c
    }, n.unFollow = function(i) {
        var r = i.UserId;
        n.currentData.isAUser ? t.follow(n.currentData.unFollowUrl, r).then(function(t) {
            t && t.success ? (i.IsDeleted && (i.isCardFrozen = !0), i.IsFollowed = !1) : (n.currentData.hasError = !0, n.currentData.errorMsg = t.message ? t.message : "")
        }, function() {
            n.currentData.hasError = !0, n.currentData.errorMsg = "unfollowing has failed!"
        }) : window.location = c
    }, n.unFriend = function(i) {
        var r = i.UserId;
        n.currentData.isAUser ? t.unfriend(n.currentData.unFriendUrl, r).then(function(t) {
            t && t.success ? i.isCardFrozen = !0 : (n.currentData.hasError = !0, n.currentData.errorMsg = t.message ? t.message : "")
        }, function() {
            n.currentData.hasError = !0, n.currentData.errorMsg = "unfriending has failed!"
        }) : window.location = c
    }, n.newPage = function(t) {
        var i = t === "prev" ? n.currentData.currentPage / n.currentData.totalPerPage - 1 : n.currentData.currentPage / n.currentData.totalPerPage + 1;
        return i > -1 && i < n.currentData.totalPages && n.getFriends(n.currentData.activeTab, i * n.currentData.totalPerPage), i * n.currentData.totalPerPage
    }, n.disablePrevious = function() {
        return n.currentData.currentPage / n.currentData.totalPerPage == 0
    }, n.disableNext = function() {
        return n.currentData.currentPage / n.currentData.totalPerPage == n.currentData.totalPages - 1
    }, n.updateLayout = function() {
        n.currentData && (n.currentData.hasBtns = n.currentData.isMyProfile && n.currentData.activeTab === "friend-requests")
    }, n.hasMenu = function(n, t) {
        var i = !1;
        return n && n.isMyProfile && (n.activeTab === "following" && t && !t.isCardFrozen ? i = !0 : t && t.IsDeleted && !t.isCardFrozen && (i = !0)), i
    }, n.refreshTab = function(t, i) {
        angular.isUndefined(t) && (t = n.currentData.activeTab), angular.isUndefined(i) && (i = n.currentData.currentPage), n.getFriends(t, i), n.updateLayout()
    }, p(), n.$on("$stateChangeSuccess", function(t, i, r) {
        n.currentData.templateVisible = !1, i.authenticate && n.currentData.userId !== n.currentData.profileUserId && (e.path("/friends"), t.preventDefault()), u(function() {
            var u = r.page ? r.page : 0,
                t;
            n.currentData.activeTab = i.name, t = i.label, n.currentData.stateLabel = t.replace("-", " "), n.currentData.ignoreAll = !1, n.friendsTabs.forEach(function(t) {
                t.name === n.currentData.activeTab && (n.currentData.tooltipLabel = t.tooltip)
            }), n.refreshTab(i.name, u)
        }, 200)
    })
}]);