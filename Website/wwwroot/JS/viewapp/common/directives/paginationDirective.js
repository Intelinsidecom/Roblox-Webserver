// ~/viewapp/common/directives/paginationDirective.js
"use strict";
robloxApp.directive("pagination", ["$log", "$parse", "$document", "_", function(n, t, i, r) {
    function f(i, r, u) {
        var f = t(u.currentPage)(i),
            o = t(u.numPages)(i),
            s = t(u.itemsPerPage)(i),
            h = u.scrollToTop ? t(u.scrollToTop) : !1,
            c = t(u.pageChanged)(i);
        n.debug("page directive data :", f, o, s), i.pagination.curPage = f, i.pagination.totalPages = o, i.pagination.pageUpdated = function(n) {
            var t = i.pagination.curPage,
                r = i.pagination.totalPages;
            n === "next" ? t = Math.min(t + 1, r) : n === "prev" ? t = Math.max(t - 1, 1) : n === "first" ? t = 1 : n === "last" && (t = r), i.pagination.curPage !== t && (i.pagination.curPage = t, c(t), e(h))
        }
    }

    function e(n) {
        n && u.scrollTop(0)
    }
    var u = i.find("body");
    return {
        restrict: "AC",
        templateUrl: "rbx-pagination",
        link: function(i, u, e) {
            var o = i.$watch(function() {
                var n = t(e.modelChanged)(i);
                return r.isFunction(n) ? n() : n
            }, function(t, r) {
                n.debug("watch triggered"), t !== r && (i.pagination = {}, f(i, u, e))
            }, !0);
            i.$on("$destroy", function() {
                o && o()
            })
        }
    }
}]);