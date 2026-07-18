// ~/viewapp/pages/profile/controllers/profileCollectionsController.js
"use strict";
profile.controller("profileCollectionsController", ["$scope", "$window", "$log", "profileService", "profileJsonEndPoints", function(n, t, i, r, u) {
    n.showTradeWindow = function(n) {
        t.open(n.target.href, "_blank", "scrollbars=0, resizable=no, height=624, width=898"), n.preventDefault()
    }, n.collections = [], n.pageData = r.getProfileData(), n.getCollectionsData = function() {
        var t = u.collections;
        return r.getCollections(t, n.pageData.profileUserId).then(function(t) {
            i.debug("my collections: ", t), n.collections = t.CollectionsItems, r.refreshLazyLoadImage()
        })
    }
}]);