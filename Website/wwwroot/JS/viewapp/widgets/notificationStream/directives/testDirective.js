"use strict";

notificationStream.directive("test", ["notificationStreamUtility", "$log", function(n) {
        return {
            restrict:"A", scope: !0, templateUrl:n.templates.testTemplate, link:function(n) {
                var r=n.notification.metadataCollection; n.notificationDisplayText="", r.forEach(function(t) {
                        n.notificationDisplayText+=t.Detail
                    })
            }
        }
    }

    ]);