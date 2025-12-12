// ~/viewapp/common/directives/toggleLoadingDirective.js
"use strict";
robloxApp.directive("toggleLoading", [function() {
    return {
        restrict: "A",
        link: function(n, t, i) {
            var f = i.isInline,
                e = '<div class="spinner spinner-sm spinner-no-margin' + (f ? "" : " spinner-block") + '"></div>',
                r = angular.element(e),
                u;
            t.after(r), r.hide(), u = n.$watch(i.isLoading, function(n, i) {
                if (n !== i)
                    if (n) {
                        var u = t[0].offsetHeight,
                            f = t[0].offsetWidth;
                        r.css("height", u + "px"), r.css("width", f + "px"), t.hide(), r.show()
                    } else t.show(), r.hide()
            }, !0), n.$on("$destroy", function() {
                u && u()
            })
        }
    }
}]);