"use strict";

robloxHelpers.directive("numbersOnly", function() {
        return {
            require:"ngModel", link:function(n, t, i, r) {
                function u(n) {
                    if(n==undefined)return""; var t=n.replace(/[^0-9]/g, ""); return t !==n&&(r.$setViewValue(t), r.$render()), t
                }

                r.$parsers.push(u)
            }
        }
    });