"use strict";

notificationStream.directive("lazyLoading", ["notificationStreamService", "notificationStreamUtility", "$document", "$log", function(n, t, i, r) {
        return {
            restrict:"A", scope: !0, link:function(t, u) {
                t.callbackLazyLoad=function() {
                    if( !t.notificationApiParams)return !1; t.notificationApiParams.loadMoreNotifications&&(t.layout.notiticationsLazyLoadingEnabled= !0, n.getRecentNotifications(t.notificationApiParams.startIndexOfNotifications, t.notificationApiParams.pageSizeOfNotifications).then(function(n) {
                                t.layout.notiticationsLazyLoadingEnabled= !1, n&&n.length>0?(t.buildNotificationsList(n), t.notificationApiParams.startIndexOfNotifications=t.notificationApiParams.startIndexOfNotifications+t.notificationApiParams.pageSizeOfNotifications, n.length<t.notificationApiParams.pageSizeOfNotifications&&(t.notificationApiParams.loadMoreNotifications= !1, t.notificationApiParams.startIndexOfNotifications=0)):(t.notificationApiParams.loadMoreNotifications= !1, t.notificationApiParams.startIndexOfNotifications=0)
                            }

                            , function() {
                                t.layout.notiticationsLazyLoadingEnabled= !1, r.debug("---error from get Notificaitons in lazyLoadingDirective.js---")
                            }))
                }

                , t.setupScrollbar=function() {
                    u.mCustomScrollbar({
                        autoExpandScrollbar: !1, scrollInertia:5, contentTouchScroll:1, mouseWheel: {
                            preventDefault: !0
                        }

                        , callbacks: {
                            onTotalScrollOffset:100, onTotalScroll:t.callbackLazyLoad, onOverflowYNone:t.callbackLazyLoad
                        }
                    })
            }

            , t.destroyScrollbar=function() {
                r.debug("----- destroyScrollbar ----"), u.mCustomScrollbar("destroy")
            }

            ; var o=function() {
                t.library.inApp?(Roblox.Scrollbar.setUpOverflowY(u, t.callbackLazyLoad), Roblox.Scrollbar.listenToScroll(i, null, t.callbackLazyLoad)):t.setupScrollbar()
            }

            , e=t.$watch(function() {
                    return t.layout&&t.layout.isLazyLoadingRequested
                }

                , function(n, i) {
                    angular.isDefined(n)&&n !==i&&(r.debug("----- initializeLayout ----"), n?o():t.library.inApp||t.destroyScrollbar())
                }

                , !0); t.$on("$destroy", function() {
                    e&&e()
                })
        }
    }
}

]);