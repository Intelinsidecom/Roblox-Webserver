// ~/viewapp/common/formEvents/directives/formValidationRedactInput.js
"use strict";
formEvents.directive("rbxFormValidationRedactInput", function() {
    return {
        require: "ngModel",
        restrict: "A",
        link: function(n, t, i, r) {
            r.redactedInput = !0
        }
    }
});