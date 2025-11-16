"use strict";

robloxHelpers.directive("inputMaxLength", function() {
        return {
            require:"ngModel", link:function(n, t, i, r) {
                function f(n) {
                    if(n.length>u) {
                        var t=n.substring(0, u); return r.$setViewValue(t), r.$render(), t
                    }

                    return n
                }

                var u=Number(i.inputMaxLength); r.$parsers.push(f)
            }
        }
    });