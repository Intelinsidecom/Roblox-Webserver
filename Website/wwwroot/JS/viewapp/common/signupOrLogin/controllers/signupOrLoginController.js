// ~/viewapp/common/signupOrLogin/controllers/signupOrLoginController.js
"use strict";
signupOrLogin.controller("SignupOrLoginController", ["$document", "$rootScope", "displayService", function(n, t, i) {
    n.on("display:updated", function(n, r) {
        i.setDisplayState(r), t.$apply()
    })
}]);