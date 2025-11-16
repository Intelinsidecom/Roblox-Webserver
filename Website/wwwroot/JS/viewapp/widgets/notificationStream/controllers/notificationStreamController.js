"use strict";

notificationStream.controller("notificationStreamController", ["$scope", "$document", "$timeout", "notificationStreamService", "notificationStreamUtility", "userService", "eventStreamService", "$log", function(n, t, i, r, u, f, e, o) {
        function h() {
            n.layout=angular.copy(u.layout), n.notificationApiParams=angular.copy(u.notificationApiParams), n.getUnreadNotificationCount(), n.library.inApp&&(n.openNotificationStream(), n.layout.isNotificationContentOpen= !0)
        }

        function c() {
            n.library=u.library, n.resetNotificationStreamData(), r.initialize().then(function(t) {
                    t&&(r.setApiEndpoints(t.NotificationDomain), u.layout.pageDataInitialized= !0, n.updateSettingsInLibrary(t))
                }

                , function() {
                    o.debug("----- initialize data request failed ----")
                })
        }

        n.updatePopoverLayout=function() {
            Roblox&&Roblox.Popover&&i(function() {
                    return Roblox.Popover.setUpTrianglePosition(n.layout.dataBindSelector, n.layout.dataContainerSelector)
                })
        }

        , n.buildNotificationsList=function(t) {
            var i=[], r; t.forEach(function(t) {
                    var f=t.metadataCollection; r=t.notificationSourceType, u.isNotificationTypeValid(r)&&(f.forEach(function(t) {
                                var e=u.normalizeUser(r, t), f, o; if(e&&e.userId !=null&&e.userName !=null) {
                                    if(f=e.userId, o=e.userName, n.library.userIdList.indexOf(f)>-1)return !1; i.push(f), n.library.userIdList.push(f), n.library.userLibrary[f]= {
                                        id:f, name:o, profileLink:u.getAbsoluteUrl(u.links.profileLink,{id:f})
                                    }
                                }
                            }), t.isClickable=u.isCardClickable(t), n.notificationIds.indexOf(t.id)<0&&n.notificationIds.push(t.id), n.notifications[t.id]=t)

                }), n.layout.emptyNotificationEnabled=n.notificationIds.length===0, i.length>0&&f.getUserAvatar(i).then(function(t) {
                    t&&t.length>0&&t.forEach(function(t, r) {
                            n.library.userLibrary[i[r]].thumbnail=t
                        })
                }

                , function() {
                    o.debug("----- getUserAvatar request failed ----")
                })
        }

        , n.getRecentNotifications=function() {
            n.layout.getRecentDataInitialized= !1, r.getRecentNotifications(n.notificationApiParams.startIndexOfNotifications, n.notificationApiParams.pageSizeOfNotifications).then(function(t) {
                    n.layout.getRecentDataInitialized= !0, n.layout.isGetRecentDataLoadedRequested= !1, t&&t.length>0&&(n.buildNotificationsList(t), n.layout.isLazyLoadingRequested= !0, t.length===n.notificationApiParams.pageSizeOfNotifications&&(n.notificationApiParams.startIndexOfNotifications=n.notificationApiParams.startIndexOfNotifications+n.notificationApiParams.pageSizeOfNotifications, n.notificationApiParams.loadMoreNotifications= !0))
                }

                , function() {
                    o.debug("--- getRecentNotifications call failed ----- "), n.layout.getRecentDataInitialized= !0
                })
        }

        , n.clearUnreadNotifications=function() {
            n.library.unreadNotifications>0&&(n.library.unreadNotifications=0, t.triggerHandler("Roblox.NotificationStream.UnreadNotifications", {
                    count:0, isNotificationContentOpen:n.layout.isNotificationContentOpen
                }))
    }

    , n.openNotificationStream=function() {
        n.layout.isGetRecentDataLoadedRequested&&(n.resetNotificationStreamData(), n.getRecentNotifications()), n.library.unreadNotifications>0&&r.clearUnread().then(function() {
                n.clearUnreadNotifications()
            }

            , function() {
                o.debug("--- clearUnread call failed ----- ")
            })
    }

    , n.toggleNotificationContent=function(t) {
        t?n.layout.isNotificationContentOpen= !1:(n.layout.isNotificationContentOpen= !n.layout.isNotificationContentOpen, n.layout.isNotificationContentOpen&&n.layout.isGetRecentDataLoadedRequested&&(n.openNotificationStream(), n.updatePopoverLayout())), n.layout.isNotificationContentOpen&&n.layout.bannerEnabled&&(n.layout.bannerEnabled= !1), n.layout.isLazyLoadingRequested&&(n.layout.isLazyLoadingRequested= !1)
    }

    , n.getUnreadNotificationCount=function() {
        r.unreadCount().then(function(i) {
                i&&(n.library.unreadNotifications=i.unreadNotifications, n.layout.isNotificationContentOpen?(n.layout.bannerText=n.library.unreadNotifications+u.textTemplate.newNotificationPostfix, n.layout.bannerText+=n.library.unreadNotifications>1?"s":""):t.triggerHandler("Roblox.NotificationStream.UnreadNotifications", {
                        count:i.unreadNotifications, isNotificationContentOpen:n.layout.isNotificationContentOpen
                    }))
        }

        , function() {
            o.debug("--- unreadCount call failed ----- ")
        })
}

, n.resetNotificationStreamData=function() {
    n.notificationIds=[], n.notifications= {}

    , n.notificationApiParams&&(n.notificationApiParams.startIndexOfNotifications=0)
}

, n.reloadNotificationStreamData=function() {
    n.resetNotificationStreamData(), n.library.unreadNotifications>0&&r.clearUnread(), n.getRecentNotifications(), n.layout.bannerEnabled= !1
}

, n.updateNewNotificationInfo=function() {
    n.layout.isGetRecentDataLoadedRequested= !0, n.getUnreadNotificationCount(), n.layout.isNotificationContentOpen&&n.$evalAsync(function() {
            n.layout.bannerEnabled= !0
        })
}

, n.updateSettingsInLibrary=function(t) {
    n.library.currentUserId=t.CurrentUserId, n.library.inApp=t.InApp, n.library.isPhone=t.IsUserOnPhone, n.library.inAndroidApp=t.InAndroidApp, n.library.iniOSApp=t.IniOSApp, n.library.inUWPApp=t.InUWPApp, n.library.bannerDismissTimeSpan=t.BannerDismissTimeSpan, n.library.signalRDisconnectionResponseInMilliseconds=t.SignalRDisconnectionResponseInMilliseconds, n.library.isChatDisabledByPrivacySetting=t.IsChatDisabledByPrivacySetting, n.library.eventStreamMetaData= {
        userId:t.CurrentUserId, inApp:t.InApp
    }
}

, n.handleSignalRSuccess=function() {
    n.$evalAsync(function() {
            n.layout.errorBannerEnabled= !1
        })
}

, n.handleSignalRError=function() {
    i(function() {
            n.layout.errorBannerEnabled= !0, n.layout.errorText=u.textTemplate.noNetworkConnectionText
        }

        , n.library.signalRDisconnectionResponseInMilliseconds)
}

, n.handleNotificationStreamNotification=function(t) {
    o.debug("--------- this is NotificationStream subscription -----------" +t.Type); switch(t.Type) {
        case u.signalRType.NewNotification:n.updateNewNotificationInfo(); break; case u.signalRType.NotificationsRead:n.clearUnreadNotifications(); break; case u.signalRType.NotificationRevoked:n.getUnreadNotificationCount(), n.layout.isStreamBodyInteracted||n.reloadNotificationStreamData()
    }
}

, n.handleChatPrivacySettingNotification=function(t) {
    o.debug("--------- this is ChatPrivacySettingNotifications subscription -----------" +t.Type); try {
        switch(t.Type) {
            case u.signalRType.chatEnabled:n.library.isChatDisabledByPrivacySetting= !1; break; case u.signalRType.chatDisabled:n.library.isChatDisabledByPrivacySetting= !0
        }
    }

    catch(r) {
        var i="ChatPrivacySettingNotifications:" +t.Type+": "; r&&r.message&&(i+=r.message), o.debug(i)
    }
}

, n.initializeRealTimeSubscriptions=function() {
    if(angular.isDefined(Roblox.RealTime)) {
        var t=Roblox.RealTime.Factory.GetClient(); t.SubscribeToConnectionEvents(n.handleSignalRSuccess, n.handleSignalRSuccess, n.handleSignalRError), t.Subscribe(u.notificationsName.NotificationStream, n.handleNotificationStreamNotification), t.Subscribe(u.notificationsName.ChatPrivacySettingNotifications, n.handleChatPrivacySettingNotification)
    }
}

, c(), t.bind("Roblox.Popover.Status", function(t, i) {
        o.debug("notificationStreamController"), n.layout&&n.$evalAsync(function() {
                if( !n.layout.isNotificationContentOpen&& !i.isHidden) {
                    var t=angular.copy(n.library.eventStreamMetaData); t.countOfUnreadNotification=n.library.unreadNotifications, e.sendEventWithTarget(e.eventNames.notificationStream.openContent, i.eventType, t)
                }

                n.toggleNotificationContent(i.isHidden)
            })

    }); var s=n.$watch(function() {
        return u.layout.pageDataInitialized
    }

    , function(t, i) {
        angular.isDefined(t)&&t&&t !==i&&(o.debug("----- initializeLayout ----"), h(), n.initializeRealTimeSubscriptions())

    }); n.$on("$destroy", function() {
        s&&s()
    })
}

]);