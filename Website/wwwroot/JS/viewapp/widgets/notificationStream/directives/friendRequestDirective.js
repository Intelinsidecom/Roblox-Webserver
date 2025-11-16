"use strict";

notificationStream.directive("friendRequest", ["notificationStreamService", "notificationStreamUtility", "$log", function(n, t) {
        return {
            restrict:"A", replace: !0, scope: {
                notification:"=", library:"=", acceptFriend:"&", ignoreFriend:"&", chat:"&", interactNotification:"&"
            }

            , templateUrl:t.templates.friendRequestTemplate, link:function(n) {
                n.notificationSourceType=t.notificationSourceType, n.friendRequestLink=t.layout.friendRequestLink; var o=n.notification.metadataCollection, s=n.notification.notificationSourceType, f=o.length, u=n.notification.eventCount?n.notification.eventCount:f, e="", h=function() {
                    var i, r; n.userIds=[], n.notificationDisplayText="", o.forEach(function(i, r) {
                            var h=t.normalizeUser(s, i), f=h.userId, l=h.userName, a=n.library.userLibrary[f]&&n.library.userLibrary[f].profileLink?n.library.userLibrary[f].profileLink:t.getAbsoluteUrl(t.links.profileLink,{id:f}), c, o; n.userIds.indexOf(f)<0&&n.userIds.push(f), c=t.getUserHtmlTemplate(s, u), o=t.getFormatString(c, {
                                userId:f, userName:l, profileLink:a
                            }), r<1?e+=o:r<2&&(u>2?e+=", " +o+", ":u===2&&(e+=" and " +o))
                    }), i=t.normalizeDisplayText(s, u), f===0?(e=i.defaultPrefixText+u+i.defaultPostfixText, n.notificationDisplayText=e):((u>2||u>f)&&(r=f>2?u-2:u-f, e+=" and " +r+(r===1?" other":" others")), n.notificationDisplayText=e+i.displayText, n.requestConfirmedText=e+i.requestConfirmedText)
            }

            , c=function() {
                n.friendRequestActionType=t.friendRequestActionType, n.notification.notificationSourceType===t.notificationSourceType.friendRequestReceived?n.notification.friendRequestActionType=f !==1||u !==1||o[0].IsAccepted?f===1&&u===1&&o[0].IsAccepted?t.friendRequestActionType.chatBtn:t.friendRequestActionType.viewAllBtn:o[0].IsAccepted?t.friendRequestActionType.chatBtn:t.friendRequestActionType.acceptIgnoreBtns:n.notification.notificationSourceType===t.notificationSourceType.friendRequestAccepted&&f===1&&u===1&&(n.notification.friendRequestActionType=t.friendRequestActionType.chatBtn)
            }

            , l=function() {
                h(), c()
            }

            ; l()
        }
    }
}

]);