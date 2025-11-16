"use strict";

robloxHelpers.directive("enterEscapeShift", ["keyCode", function(n) {
        return {
            restrict:"A", link:function(t, i, r) {
                i.bind("keydown keypress", function(i) {
                        var u=i.keyCode||i.which; u !==n.enter||i.shiftKey||(i.preventDefault(), t.$apply(function() {
                                    t.$eval(r.enterEscapeShift)
                                }))
                    })
            }
        }
    }

    ]);