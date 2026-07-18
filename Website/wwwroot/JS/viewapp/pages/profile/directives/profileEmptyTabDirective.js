// ~/viewapp/pages/profile/directives/profileEmptyTabDirective.js
"use strict";
profile.directive("profileEmptyTab", function() {
    return {
        restrict: "AC",
        link: function(n, t) {
            n.profileLayout = n.profileLayout || {}, t.children().length === 0 && (n.profileLayout.userHasNoCreations = !0)
        }
    }
});