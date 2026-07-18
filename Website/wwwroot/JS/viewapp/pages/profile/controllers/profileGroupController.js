// ~/viewapp/pages/profile/controllers/profileGroupController.js
"use strict";
profile.controller("profileGroupController", ["$log", "$scope", "profileService", "profileJsonEndPoints", function(n, t, i, r) {
    t.layout = {
        groupsToShowOnLoad: 12,
        groupsPerRow: 6,
        numberOfVisibleRows: 0,
        isGridOn: !1,
        visibleItems: 0
    }, t.pageData = i.getProfileData(), t.groups = [], t.getGroupsData = function() {
        var u = r.groups;
        return i.getGroups(u, t.pageData.profileUserId).then(function(i) {
            n.debug("my groups: ", i), t.groups = i.Groups, t.layout.numberOfVisibleRows = Math.min(Math.ceil(t.layout.groupsToShowOnLoad / t.layout.groupsPerRow), Math.ceil(t.groups.length / t.layout.groupsPerRow)), t.layout.visibleItems = Math.min(t.groups.length, t.layout.groupsToShowOnLoad)
        })
    }, t.loadMoreGroups = function() {
        var n = t.layout.groupsPerRow * (t.layout.numberOfVisibleRows + 1);
        t.layout.numberOfVisibleRows++, t.layout.visibleItems = n, i.refreshLazyLoadImage()
    }, t.updateDisplay = function(n) {
        t.layout.isGridOn = n, i.refreshLazyLoadImage()
    }
}]);