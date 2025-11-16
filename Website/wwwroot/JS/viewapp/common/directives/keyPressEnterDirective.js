"use strict";

robloxHelpers.directive("keyPressEnter", function() {
        return function(n, t, i) {
            t.bind("keydown keypress", function(t) {
                    t.which===13&&(n.$apply(function() {
                                n.$eval(i.keyPressEnter)
                            }), t.preventDefault())
                })
        }
    });