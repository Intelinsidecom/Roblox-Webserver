// ~/viewapp/pages/profile/directives/profileAccoutrementsSliderDirective.js
"use strict";
profile.directive("profileAccoutrementsSlider", function() {
    return {
        restrict: "A",
        scope: {
            profileAccoutrementsLayout: "="
        },
        link: function(n, t) {
            var r = 95;
            n.$watch("profileAccoutrementsLayout", function(n) {
                if (n && (n.inTouchScreen || Roblox.DeviceFeatureDetection.isTouch)) {
                    var u = n.numberOfAccoutrements;
                    t.css("width", u * r)
                }
            }, !0)
        }
    }
});