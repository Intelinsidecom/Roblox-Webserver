// ~/viewapp/common/formEvents/directives/formValidation.js
"use strict";
formEvents.directive("rbxFormValidation", function() {
    return {
        require: ["^form", "ngModel"],
        restrict: "A",
        link: function(n, t, i, r) {
            n.$watch(function() {
                var n = r[1];
                return n.$modelValue
            }, function(t) {
                var i = r[1],
                    f = r[0],
                    u;
                (n.badSubmit || i.$dirty) && i.$invalid && (u = i.redactedInput ? "[Redacted]" : t, Roblox.FormEvents && Roblox.FormEvents.SendValidationFailed(f.context, i.$name, u, i.$validationMessage))
            })
        }
    }
});