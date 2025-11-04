// ~/viewapp/common/signupOrLogin/directives/validBirthday.js
"use strict";
signupOrLogin.directive("rbxValidBirthday", function() {
    return {
        require: "ngModel",
        link: function(n, t, i, r) {
            n.$watch(function() {
                return !angular.isUndefined(r.$modelValue) && r.$modelValue !== ""
            }, function(n) {
                r.$setValidity("birthday", n), r.$validationMessage = "Invalid birthday"
            })
        }
    }
});