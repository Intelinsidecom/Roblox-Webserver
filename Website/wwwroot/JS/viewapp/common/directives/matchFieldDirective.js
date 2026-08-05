// ~/viewapp/common/directives/matchFieldDirective.js
"use strict";
robloxApp.directive("matchField", function() {
    return {
        require: "ngModel",
        link: function(n, t, i, r) {
            n.$watch(i.matchField, function(n) {
                r.$viewValue !== undefined && r.$viewValue !== "" && r.$setValidity("matchField", n === r.$viewValue)
            }), r.$validators.matchField = function(t) {
                return t === n.$eval(i.matchField)
            }
        }
    }
});