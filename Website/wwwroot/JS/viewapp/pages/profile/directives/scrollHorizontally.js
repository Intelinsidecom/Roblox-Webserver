// ~/viewapp/pages/profile/directives/scrollHorizontally.js
"use strict";
profile.directive("horizontalScrollBar", function() {
    return {
        restrict: "A",
        link: function(n, t, i) {
            var r = t[0];
            t.bind("scroll", function() {
                r.scrollLeft + r.offsetWidth > r.scrollWidth - 100 && n.$apply(i.horizontalScrollBar)
            })
        }
    }
});