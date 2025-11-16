"use strict";

robloxApp.directive("clickOutside", ["$document", "$parse", function(n, t) {
        return {
            restrict:"A", link:function(i, r, u) {
                var e=t(u.clickOutside), f=function(n) {
                    var t=r[0] !==n.target&&r.find(n.target).length===0; t&&i.$apply(function() {
                            e(i, {})
                    })
            }

            ; n.on("click", f); i.$on("$destroy", function() {
                    n.off("click", f)
                })
        }
    }
}

]);