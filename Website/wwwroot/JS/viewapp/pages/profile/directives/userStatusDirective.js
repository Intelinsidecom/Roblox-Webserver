// ~/viewapp/pages/profile/directives/userStatusDirective.js
"use strict";
profile.directive("userStatus", ["layoutLibrary", function(n) {
    return {
        scope: !0,
        templateUrl: n.templateLinks.userStatus
    }
}]);