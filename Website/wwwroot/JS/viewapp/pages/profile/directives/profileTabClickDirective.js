// ~/viewapp/pages/profile/directives/profileTabClickDirective.js
"use strict";
profile.directive("profileTabClick", ["profileService", function(n) {
    return {
        restrict: "A",
        link: function(t, i) {
            var u = i.find(".rbx-tab");
            u.click(function() {
                n.refreshLazyLoadImage()
            })
        }
    }
}]);