"use strict";

robloxHelpers.directive("formValidation", function() {
        return {
            require:["^form", "ngModel"], restrict:"A", link:function(n, t, i, r) {
                n.$watch(function() {
                        var n=r[1]; return n.$viewValue
                    }

                    , function(n) {
                        var u, t, i, f; typeof Roblox.FormEvents !="undefined" &&(u=r[0], t=r[1], t.$dirty&&t.$invalid&&(i=[], angular.forEach(t.$error, function(n, t) {
                                        n=== !0&&i.push(t)
                                    }), f=t.redactedInput?"[Redacted]":n, Roblox.FormEvents.SendValidationFailed(u.$name, t.$name, f, i.join(","))))
                    })
            }
        }
    });