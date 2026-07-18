// ~/viewapp/pages/profile/controllers/profileUtilitiesController.js
"use strict";
profile.controller("profileUtilitiesController", ["$log", "$scope", "robloxModalService", "profileService", "profileJsonEndPoints", function(n, t, i, r, u) {
    t.layoutContent = {
        showMore: !0,
        linkName: "More",
        nameOfOpen: "More",
        nameOfClose: "Less"
    }, t.toggleContent = function(n) {
        t.layoutContent.showMore = !n, t.layoutContent.linkName = n ? t.layoutContent.nameOfClose : t.layoutContent.nameOfOpen
    }, t.loadMore = function() {
        t.layoutContent.showMore = !1
    }, t.showPastUsernames = function() {
        i.open("profile-past-usernames-modal.html", "")
    }, t.layout = {
        title: "",
        assetUrl: "",
        showSeeAllButton: !1
    }, t.pageData = r.getProfileData(), t.assets = [], t.getPlayerAssets = function(i) {
        var f = u.playerAssets;
        return r.getPlayerAssets(f, t.pageData.profileUserId, i).then(function(i) {
            n.debug("my player assets: ", i), t.assets = i.Assets, t.layout.title = i.Title, t.layout.assetUrl = i.AssetTypeInventoryUrl, t.layout.showSeeAllButton = i.IsSeeAllHeaderButtonVisible, r.refreshLazyLoadImage()
        })
    }
}]);