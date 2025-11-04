// ~/viewapp/common/signupOrLogin/services/displayService.js
"use strict";
signupOrLogin.factory("displayService", ["$rootScope", "$document", function(n, t) {
    var i = Roblox.SignupOrLogin.SectionType.signup;
    return {
        getDisplayState: function() {
            return i
        },
        setDisplayState: function(r) {
            i = r, n.$broadcast("display:updated", r), t.triggerHandler("authFormToggle", {
                toSectionType: r
            })
        }
    }
}]);