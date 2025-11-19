// ~/viewapp/common/directives/alphanumericInputDirective.js
"use strict";
robloxApp.directive("alphanumericInput", function() {
    return {
        require: "ngModel",
        restrict: "A",
        link: function(n, t, i, r) {
            function u(n) {
                if (n == undefined) return "";
                var t = n.replace(/[^0-9a-zA-Z]/g, "");
                return t !== n && (r.$setViewValue(t), r.$render()), t
            }
            var f = null;
            r.$parsers.push(u)
        }
    }
});