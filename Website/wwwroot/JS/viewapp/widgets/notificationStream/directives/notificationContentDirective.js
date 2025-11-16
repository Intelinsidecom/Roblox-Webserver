"use strict";

notificationStream.directive("notificationContent", ["notificationStreamService", "notificationStreamUtility", "$log", function(n, t) {
        return {
            restrict:"A", replace: !0, scope: !0, templateUrl:t.templates.notificationContentTemplate
        }
    }

    ]);