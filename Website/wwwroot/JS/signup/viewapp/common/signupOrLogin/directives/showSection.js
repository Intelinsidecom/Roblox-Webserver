// ~/viewapp/common/signupOrLogin/directives/showSection.js
"use strict";
signupOrLogin.directive("rbxShowSection", ["$rootScope", "displayService", function(n, t) {
    return {
        restrict: "A",
        scope: {
            sectionType: "="
        },
        link: function(i, r) {
            r.bind("click", function() {
                var u = t.getDisplayState(),
                    r;
                t.setDisplayState(i.sectionType), n.$apply(), Freebloxia.EventStream && (r = {
                    nodeName: this.nodeName,
                    toSectionType: i.sectionType,
                    fromSectionType: u
                }, Freebloxia.EventStream.SendEvent("formSectionToggle", "authForm", r))
            })
        }
    }
}]);