// ~/viewapp/common/signupOrLogin/directives/validPasswordConfirm.js
"use strict";
signupOrLogin.directive("rbxValidPasswordConfirm", function() {
    return {
        require: "ngModel",
        restrict: "A",
        scope: {
            match: "=",
            name: "="
        },
        link: function(n, t, i, r) {
            n.$watch(function() {
                return n.match === r.$modelValue
            }, function(t) {
                r.$validationMessage = t ? angular.isString(r.$modelValue) ? "" : "Please enter a password confirmation." : Roblox.Resources.AnimatedSignupFormValidator.doesntMatch, n.$unitTestValidationMessage = r.$validationMessage, r.$setValidity("match", angular.isString(r.$modelValue) && t)
            })
        }
    }
});