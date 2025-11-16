"use strict";

notificationStreamIcon.directive("notificationIndicator", ["notificationStreamUtility", "$document", "$log", function(n, t, i) {
        return {
            restrict:"A", replace: !0, scope: !0, templateUrl:n.templates.notificationIndicatorTemplate, link:function(n) {
                function f(t) {
                    n.layout=n.layout|| {}

                    , n.layout.unreadNotifications=t.count, n.layout.isNotificationContentOpen=t.isNotificationContentOpen
                }

                t.bind("Roblox.NotificationStream.UnreadNotifications", function(t, r) {
                        i.debug(" ----- notificationStreamIconController --- args.count --------" +r.count), n.$evalAsync(f(r))
                    })
            }
        }
    }

    ]);