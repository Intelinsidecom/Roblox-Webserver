"use strict";

robloxHelpers.directive("onFinishRender", ["$timeout", function(n) {
        return {
            restrict:"A", link:function(t, i, r) {
                t.$last=== !0&&n(function() {
                        t.$emit(r.onFinishRender)
                    })
            }
        }
    }

    ]);