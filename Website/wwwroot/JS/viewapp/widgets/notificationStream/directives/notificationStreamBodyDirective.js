"use strict";

notificationStream.directive("notificationStreamBody", ["$document", "$log", function(n, t) {
        return {
            restrict:"A", replace: !0, scope: !0, link:function(i, r) {
                n.on("click touchstart", function(n) {
                        i.layout&&( !i.layout.isStreamBodyInteracted&&r.has(n.target).length>0?i.layout.isStreamBodyInteracted= !0:i.layout.isStreamBodyInteracted&& !r.has(n.target).length>0&&(i.layout.isStreamBodyInteracted= !1), t.debug(" ------------------scope.layout.isStreamBodyInteracted----------------- " +i.layout.isStreamBodyInteracted))
                    })
            }
        }
    }

    ]);