"use strict";

robloxApp.directive("stickToTop", ["$window", function(n) {
        return {
            restrict:"A", scope: {
                isElementAtTop:"="
            }

            , link:function(t, i) {
                var u=i[0].getBoundingClientRect(), r=u.top; angular.element(n).bind("scroll", function() {
                        var i=n.pageYOffset; !t.isElementAtTop&&i>=r?(t.isElementAtTop= !0, t.$apply()):t.isElementAtTop&&i<=r&&(t.isElementAtTop= !1, t.$apply())
                    })
            }
        }
    }

    ]);