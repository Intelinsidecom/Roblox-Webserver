// ~/viewapp/common/signupOrLogin/directives/validPassword.js
"use strict";
signupOrLogin.directive("rbxValidPassword", function() {
    return {
        require: "ngModel",
        link: function(n, t, i, r) {
            n.$watch(function() {
                return !angular.isUndefined(r.$modelValue) && Roblox.Animated2014SignupFormValidator.verifyPassword(r.$modelValue, n.signup.username)
            }, function(t) {
                r.$setValidity("password", angular.isString(r.$modelValue) && t === ""), r.$validationMessage = angular.isString(r.$modelValue) ? t : "Please enter a password.", n.$unitTestValidationMessage = r.$validationMessage
            })
        }
    }
});