// ~/viewapp/common/formEvents/directives/formContext.js
"use strict";
formEvents.directive("rbxFormContext", function() {
    return {
        require: "form",
        restrict: "A",
        link: function(n, t, i, r) {
            var u = r.$name;
            r.context = i.context + u.charAt(0).toUpperCase() + u.substr(1)
        }
    }
});