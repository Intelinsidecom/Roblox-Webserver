"use strict";

avatar.directive("onInputFinished", ["$timeout", "$log", function(n, t) {
        return {
            scope: {
                fn:"&onInputFinished"
            }

            , restrict:"A", link:function(n, i) {
                var r=/Edge/.test(navigator.userAgent); if(r&&i.is("input[type=range]")) {
                    t.debug("Is Edge, using input event"); i.on("input", function() {
                            t.debug("onInputFinished input"), n.$apply(n.fn())
                        })
                }

                else {
                    t.debug("Is not Edge, using change event"); i.on("change", function() {
                            t.debug("onInputFinished change"), n.$apply(n.fn())
                        })
                }
            }
        }
    }

    ]);