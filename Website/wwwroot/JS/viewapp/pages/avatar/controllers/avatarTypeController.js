// ~/viewapp/pages/avatar/controllers/avatarTypeController.js
"use strict";
avatar.controller("avatarTypeController", ["$scope", "$log", "$timeout", "$q", "avatarService", "avatarConstants", function(n, t, i, r, u, f) {
    function e() {
        n.$parent.scaleEnabled = n.avatarType === "R15"
    }
    n.avatarType = f.avatarType.defaultOnPageLoad, n.avatarTypes = f.avatarType.avatarTypes, n.updateAvatarType = function() {
        var t = n.avatarType;
        e(), u.setAvatarType(t).then(function() {
            n.refreshThumbnail()
        }, function() {
            n.systemFeedback.error(f.avatarType.failedToUpdate)
        })
    }, n.$on(f.events.avatarDetailsLoaded, function(t, i) {
        n.avatarType = i.playerAvatarType, e()
    })
}]);