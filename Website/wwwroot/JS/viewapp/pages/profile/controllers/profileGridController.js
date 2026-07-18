// ~/viewapp/pages/profile/controllers/profileGridController.js
"use strict";
profile.controller("profileGridController", ["$scope", function(n) {
    n.NumberOfVisibleRows = 0, n.isGridOn = !1, n.visibleItems = 0, n.containerHeight = n.NumberOfVisibleRows * 232 + 8 * (n.NumberOfVisibleRows - 1), n.elementClassName = "", n.containerClassName = "", n.elementWidthWithPadding = 160, n.init = function(t, i) {
        n.elementClassName = t, n.containerClassName = i, n.loadMore(), n.loadMore()
    }, n.loadMore = function() {
        var i = n.visibleItems,
            t = 6 * (n.NumberOfVisibleRows + 1),
            r = $("." + n.elementClassName);
        n.showImages(i, t, r), n.NumberOfVisibleRows++, n.visibleItems = t
    }, n.showImages = function(t, i, r) {
        for (var f, u = t; u < i; u++) f = $(r).find("." + n.containerClassName + "[data-index=" + u + "] img"), $(f).attr("src") || $(f).attr("src", $(f).attr("data-src"))
    }, n.updateDisplay = function(t) {
        n.isGridOn = t
    }
}]);