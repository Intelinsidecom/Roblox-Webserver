// ~/viewapp/pages/profile/directives/profileHeaderDataDirective.js
"use strict";
profile.directive("profileHeaderData", ["profileService", function(n) {
    return {
        restrict: "A",
        scope: {
            profileHeaderLayout: "=",
            setMessageBtnDisplay: "&"
        },
        link: function(t, i, r) {
            var u = Roblox && Roblox.ProfileHeaderData ? Roblox.ProfileHeaderData : {
                userId: r.userid,
                profileUserId: r.profileuserid,
                profileUserName: r.profileusername,
                friendsCount: r.friendscount,
                followersCount: r.followerscount,
                followingsCount: r.followingscount,
                acceptFriendRequestUrl: r.acceptfriendrequesturl,
                areFriends: r.arefriends === "true",
                friendRequestPending: r.friendrequestpending === "true",
                friendUrl: r.friendurl,
                incomingFriendRequestId: r.incomingfriendrequestid,
                incomingFriendRequestPending: r.incomingfriendrequestpending === "true",
                originalMaySendFriendInvitation: r.maysendfriendinvitation === "true",
                maySendFriendInvitation: r.maysendfriendinvitation === "true",
                removeFriendRequestUrl: r.removefriendrequesturl,
                sendFriendRequestUrl: r.sendfriendrequesturl,
                originalMayFollow: r.mayfollow === "true",
                mayFollow: r.mayfollow === "true",
                isFollowing: r.isfollowing === "true",
                followUrl: r.followurl,
                unFollowUrl: r.unfollowurl,
                canMessage: r.canmessage === "true",
                messageUrl: r.messageurl,
                canBeFollowed: r.canbefollowed === "true",
                originalCanTrade: r.cantrade === "true",
                canTrade: r.cantrade === "true",
                isBlockButtonVisible: r.isblockbuttonvisible === "true",
                isVieweeBlocked: r.isvieweeblocked === "true",
                getFollowScript: r.getfollowscript,
                isMoreBtnVisible: r.ismorebtnvisible === "true",
                mayImpersonate: r.mayimpersonate === "true",
                impersonateUrl: r.impersonateurl,
                mayUpdateStatus: r.mayupdatestatus === "true",
                updateStatusUrl: r.updatestatusurl,
                statusText: r.statustext,
                editStatusMaxLength: r.editstatusmaxlength,
                getFriendshipCountUrl: r.getfriendshipcounturl,
                inApp: r.inapp === "true",
                inAndroidApp: r.inandroidapp === "true",
                iniOSApp: r.iniosapp === "true",
                isChatDisabledByPrivacySetting: r.ischatdisabledbyprivacysetting === "true"
            };
            angular.extend(t.profileHeaderLayout, u);
            n.setProfileData(t.profileHeaderLayout), t.setMessageBtnDisplay({
                layout: t.profileHeaderLayout
            }), Roblox && Roblox.Performance && Roblox.Performance.setPerformanceMark("header_data")
        }
    }
}]);
