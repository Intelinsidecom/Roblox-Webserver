"use strict";

notificationStream.factory("notificationStreamUtility", ["signalR", "layoutLibrary", "$log", function(n, t) {
        var r=t.notificationSourceType, u=t.links, f=t.stringTemplates; return {
            templates:t.directiveTemplatesName, links:u, textTemplate:t.textTemplate, stringTemplates:f, layout: {
                pageDataInitialized: !1, firstTimeNotificationStream: !1, getRecentDataInitialized: !1, isNotificationContentOpen: !1, isLazyLoadingRequested: !1, isGetRecentDataLoadedRequested: !0, notificationsScrollbarSelector:"#notification-stream-scrollbar", settingLink:Roblox&&Roblox.Endpoints?Roblox.Endpoints.getAbsoluteUrl(u.settingLink):u.settingLink, friendRequestLink:Roblox&&Roblox.Endpoints?Roblox.Endpoints.getAbsoluteUrl(u.friendRequestLink):u.friendRequestLink, bannerEnabled: !1, emptyNotificationEnabled: !1, notificationsLazyLoadingEnabled: !1, isNotificationsLoading: !1, isStreamBodyInteracted: !1, bannerText:"", errorText:"", dataBindSelector:"#notification-stream", dataContainerSelector:"#notification-stream-container"
            }

            , notificationApiParams: {
                startIndexOfNotifications:0, pageSizeOfNotifications:10, loadMoreNotifications: !1
            }

            , library: {
                unreadNotifications:0, userIdList:[], userLibrary: {}

                , prefixLocalStoragekey:"user_", inApp: !1, isPhone: !1, isTouch:Roblox&&Roblox.DeviceFeatureDetection?Roblox.DeviceFeatureDetection.isTouch: !1, eventStreamMetaData: {}
            }

            , notificationsName:n.notifications, notificationSourceType:r, signalRType:n.types, friendRequestReceivedLayout:t.friendRequestReceivedLayout, friendRequestAcceptedLayout:t.friendRequestAcceptedLayout, friendRequestActionType:t.friendRequestActionType, getAbsoluteUrl:function(n, t) {
                return Roblox&&Roblox.Endpoints?Roblox.Endpoints.generateAbsoluteUrl(n,t,!0):getFormatString(n, t)
            }

            , getFormatString:function(n, t) {
                var i, r, u; for(i in t)r=t[i], u=new RegExp("{" +i.toLowerCase()+"(:.*?)?\\??}"), n=n.replace(u, r); return n
            }

            , isNotificationTypeValid:function(n) {
                var t= !1, i; for(i in r)if(r[i]===n) {
                    t= !0; break
                }

                return t
            }

            , isCardClickable:function(n) {
                var t=n.notificationSourceType; switch(t) {
                    case r.friendRequestAccepted:return n.eventCount>1||n.eventCount===1&&n.metadataCollection.length===0; case r.privateMessageReceived:return !0
                }

                return !1
            }

            , normalizeUser:function(n, t) {
                var i= {
                    userId:null, userName:null
                }

                ; switch(n) {
                    case r.friendRequestReceived:i.userId=t.SenderUserId, i.userName=t.SenderUserName; break; case r.friendRequestAccepted:i.userId=t.AccepterUserId, i.userName=t.AccepterUserName; break; case r.privateMessageReceived:i.userId=t.AuthorUserId, i.userName=t.AuthorUserName
                }

                return i
            }

            , getUserHtmlTemplate:function(n, t) {
                var i=""; switch(n) {
                    case r.friendRequestAccepted:i=t>1?f.boldLink:f.userLink; break; case r.friendRequestReceived:default:i=f.userLink
                }

                return i
            }

            , normalizeDisplayText:function(n, t) {
                var i= {}

                ; switch(n) {
                    case r.friendRequestReceived:i= {
                        defaultPrefixText:this.friendRequestReceivedLayout.defaultPrefixText, defaultPostfixText:t===1?this.friendRequestReceivedLayout.defaultPostfixTextBySingle:this.friendRequestReceivedLayout.defaultPostfixTextByMulti, displayText:t===1?this.friendRequestReceivedLayout.friendRequestTextBySingle:this.friendRequestReceivedLayout.friendRequestTextByMulti, requestConfirmedText:t===1?this.friendRequestReceivedLayout.requestConfirmedTextBySingle:this.friendRequestReceivedLayout.requestConfirmedTextByMulti
                    }

                    ; break; case r.friendRequestAccepted:i= {
                        defaultPrefixText:this.friendRequestAcceptedLayout.defaultPrefixText, defaultPostfixText:t===1?this.friendRequestAcceptedLayout.defaultPostfixTextBySingle:this.friendRequestAcceptedLayout.defaultPostfixTextByMulti, displayText:this.friendRequestAcceptedLayout.friendRequestAcceptedText, requestConfirmedText:this.friendRequestAcceptedLayout.requestConfirmedText
                    }
                }

                return i
            }

            , buildScrollbar:function(n) {
                var t=angular.element(document.querySelector(n)); t.mCustomScrollbar({
                    autoHideScrollbar: !1, autoExpandScrollbar: !1, contentTouchScroll:1e4, documentTouchScroll: !1, mouseWheel: {
                        preventDefault: !0
                    }

                    , advanced: {
                        autoScrollOnFocus: !1
                    }
                })
        }
    }
}

]);