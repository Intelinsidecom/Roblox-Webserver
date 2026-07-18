// ~/viewapp/pages/profile/directives/profileAccoutrementsPageDirective.js
"use strict";
profile.directive("profileAccoutrementsPage", function() {
    return {
        restrict: "A",
        scope: {
            profileAccoutrementsLayout: "="
        },
        link: function(n, t) {
            n.$watch("profileAccoutrementsLayout", function(n) {
                var u, r, f;
                if (n && (u = n.numberOfPages, u > 1))
                    for (r = 0; r < u; r++) f = angular.element(t.children()[r]), f.removeClass("hidden")
            }, !0)
        }
    }
});