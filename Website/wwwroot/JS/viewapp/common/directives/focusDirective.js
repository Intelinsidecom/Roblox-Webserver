"use strict";

robloxHelpers.directive("focusModel", function() {
        return function(n, t, i) {
            var r=function() {
                n.$evalAsync(function() {
                        n.$eval(i.focusModel)
                    })
            }

            ; t[0].addEventListener("focus", r, !0)
        }
    });