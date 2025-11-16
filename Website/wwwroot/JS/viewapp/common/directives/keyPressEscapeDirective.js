"use strict";

robloxHelpers.directive("keyPressEscape", function() {
        return function(n, t, i) {
            t.bind("keydown keypress", function(t) {
                    t.which===27&&(n.$apply(function() {
                                n.$eval(i.keyPressEscape)
                            }), t.preventDefault())
                })
        }
    });