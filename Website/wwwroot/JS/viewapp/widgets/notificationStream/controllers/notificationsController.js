"use strict";

notificationStream.controller("notificationsController", ["$scope", "notificationStreamService", "notificationStreamUtility", "hybridService", "eventStreamService", "urlService", "chatDispatchService", "$document", "$log", function(n, t, i, r, u, f, e, o, s) {
        function h(t, i, r) {
            var e=n.notifications[i], f=angular.copy(n.library.eventStreamMetaData); f.notificationType=e.notificationSourceType, u.sendEventWithTarget(t, r, f)
        }

        n.acceptFriend=function(r, f, e) {
            s.debug("---------------- acceptFriend --------- "); var c=n.notifications[f]; n.interactNotification(c), t.acceptFriend(n.library.currentUserId, r).then(function() {
                    var t=n.notifications[f]; t.friendRequestActionType=i.friendRequestActionType.chatBtn, t.metadataCollection[0].IsAccepted= !0, t.isFlipped= !0, o.triggerHandler("Roblox.Friends.CountChanged"), h(u.eventNames.notificationStream.acceptFriendRequest, f, e.type)
                })
        }

        , n.ignoreFriend=function(i, r, f) {
            s.debug("---------------- ignoreFriend --------- "); var e=n.notifications[r]; n.interactNotification(e), t.ignoreFriend(n.library.currentUserId, i).then(function() {
                    var t=n.notifications[r]; t.isSlideOut= !0, h(u.eventNames.notificationStream.ignoreFriendRequest, r, f.type)
                })
        }

        , n.removeNotification=function(t) {
            s.debug("---------------- removeNotification --------- notificationId:  " +t); var i=n.notificationIds.indexOf(t); n.notificationIds.splice(i, 1), delete n.notifications[t]
        }

        , n.chat=function(t, i, r) {
            var o, f; n.library.isChatDisabledByPrivacySetting||(o=n.notifications[i], n.interactNotification(o), f=e.buildPermissionVerifier(n.library), f.uwpApp.hybridRequired= !0, e.startChat(t, f), h(u.eventNames.notificationStream.chat, i, r.type))
        }

        , n.interactNotification=function(n) {
            n.isInteracted||t.markInteracted(n.id).then(function() {
                    n.isInteracted= !0
                })
        }

        , n.clickCard=function(t) {
            var u="", e= {}

            , h, o; switch(t.notificationSourceType) {
                case i.notificationSourceType.friendRequestReceived:n.interactNotification(t), t.eventCount>1||t.metadataCollection.length===0?(u=i.links.friendRequestLink, n.library.inApp?(e= {
                            feature:i.links.friendRequestTabName, urlPath:u
                        }

                        , r.navigateToFeature(e, function(n) {
                                s.debug("openUserFriendsPage ---- status:" +n)

                            })):window.location.href=f.getAbsoluteUrl(u)):t.metadataCollection&&t.metadataCollection.length>0&&(o=t.metadataCollection[0], u=i.links.profileLink, h=o.SenderUserId, n.library.inApp?r.openUserProfile(parseInt(h), function(n) {
                            s.debug("openUserProfile ---- status:" +n)

                        }):window.location.href=i.getAbsoluteUrl(u,{id:h})); break; case i.notificationSourceType.friendRequestAccepted:n.interactNotification(t), u=i.links.friendsLink, n.library.inApp?(e= {
                        feature:i.links.friendsTabName, urlPath:u
                    }

                    , r.navigateToFeature(e, function(n) {
                            s.debug("openUserFriendsPage ---- status:" +n)

                        })):window.location.href=f.getAbsoluteUrl(u); break; case i.notificationSourceType.privateMessageReceived:n.interactNotification(t), t.eventCount>1||t.metadataCollection.length===0?u=i.links.inboxLink:t.metadataCollection&&t.metadataCollection.length>0&&(o=t.metadataCollection[0], u=i.links.inboxLink+i.links.inboxMessageDetailQuery+o.MessageId), n.library.inApp?(e= {
                        feature:i.links.inboxTabName, urlPath:u
                    }

                    , r.navigateToFeature(e, function(n) {
                            s.debug("openUserProfile ---- status:" +n)
                        })):window.location.href=f.getAbsoluteUrl(u)
            }
        }

        , n.notificationSourceType=i.notificationSourceType
    }

    ]);