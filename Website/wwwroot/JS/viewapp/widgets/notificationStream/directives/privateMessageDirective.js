"use strict";

notificationStream.directive("privateMessage", ["notificationStreamService", "notificationStreamUtility", "$log", function(n, t) {
        return {
            restrict:"A", replace: !0, scope: {
                notification:"=", library:"="
            }

            , templateUrl:t.templates.privateMessageTemplate, link:function(n) {
                var u=function() {
                    var i, u, r; n.notificationSourceType=t.notificationSourceType, n.friendRequestLink=t.layout.friendRequestLink, n.privateMessageLayout= {
                        displayUserId:null, displayUserName:"", messagePreview:"", isStacked: !1
                    }

                    , i=n.notification.metadataCollection, u=n.notification.notificationSourceType, (n.notification.eventCount>1||i&&i.length===0)&&(n.privateMessageLayout.isStacked= !0), i&&i.length>0&&(r=t.normalizeUser(u, i[0]), n.privateMessageLayout.displayUserId=r.userId, n.privateMessageLayout.displayUserName=r.userName, n.privateMessageLayout.messagePreview=i[0].BodyPreview)
                }

                ; u()
            }
        }
    }

    ]);