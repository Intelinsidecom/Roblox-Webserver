"use strict";

notificationStream.factory("notificationStreamService", ["httpService", "$q", "$log", "urlService", function(n, t, i, r) {
        var u="/notification-stream/notification-stream-data", f="/api/friends/acceptfriendrequest", e="/api/friends/declinefriendrequest"; return {
            endpoints: {
                initializeData: {
                    url:r.getAbsoluteUrl(u), retryable: !0
                }

                , acceptFriendRequest: {
                    url:r.getAbsoluteUrl(f), retryable: !1
                }

                , ignoreFriendRequest: {
                    url:r.getAbsoluteUrl(e), retryable: !1
                }
            }

            , setApiEndpoints:function(n) {
                var t=n+"/v2/"; this.endpoints.unreadCount= {
                    url:t+"stream-notifications/unread-count", retryable: !0, withCredentials: !0
                }

                , this.endpoints.getRecent= {
                    url:t+"stream-notifications/get-recent", retryable: !0, withCredentials: !0
                }

                , this.endpoints.clearUnread= {
                    url:t+"stream-notifications/clear-unread", retryable: !1, withCredentials: !0
                }

                , this.endpoints.markInteracted= {
                    url:t+"stream-notifications/mark-interacted", retryable: !1, withCredentials: !0
                }

                , this.endpoints.updateNotificationSettings= {
                    url:t+"notifications/update-notification-settings", retryable: !1, withCredentials: !0
                }
            }

            , initialize:function() {
                var t= {}

                ; return n.httpGet(this.endpoints.initializeData, t)
            }

            , unreadCount:function() {
                var t= {}

                ; return n.httpGet(this.endpoints.unreadCount, t)
            }

            , clearUnread:function() {
                var t= {}

                ; return n.httpPost(this.endpoints.clearUnread, t)
            }

            , getRecentNotifications:function(t, i) {
                var r= {
                    startIndex:t, maxRows:i
                }

                ; return n.httpGet(this.endpoints.getRecent, r)
            }

            , markInteracted:function(t) {
                var i= {
                    eventId:t
                }

                ; return n.httpPost(this.endpoints.markInteracted, i)
            }

            , acceptFriend:function(t, i) {
                var r= {
                    targetUserID:i, invitationID:t
                }

                ; return n.httpPost(this.endpoints.acceptFriendRequest, r)
            }

            , ignoreFriend:function(t, i) {
                var r= {
                    targetUserID:i, invitationID:t
                }

                ; return n.httpPost(this.endpoints.ignoreFriendRequest, r)
            }

            , updateNotificationSettings:function(t, i) {
                var u= {
                    notificationSourceType:t, receiverDestinationType:"NotificationStream", isEnabled:i
                }

                , r=[]; return r.push(u), n.httpPost(this.endpoints.updateNotificationSettings, r)
            }
        }
    }

    ]);