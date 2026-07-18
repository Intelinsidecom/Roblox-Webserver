// ~/viewapp/pages/profile/directives/profileAccoutrementsDataDirective.js
"use strict";
profile.directive("profileAccoutrementsData", function() {
    return {
        restrict: "A",
        scope: {
            profileAccoutrementsLayout: "="
        },
        link: function(n, t, i) {
            n.profileAccoutrementsLayout = {
                numberOfAccoutrements: i.numberofaccoutrements,
                numberOfPages: Math.ceil(i.numberofaccoutrements / i.accoutrementsperpage),
                currentPageNumber: 0,
                inTouchScreen: i.intouchscreen === "true"
            }
        }
    }
});