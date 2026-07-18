// ~/viewapp/pages/profile/directives/profileUtilitiesDirective.js
profile.directive("truncate", function() {
    return {
        scope: {
            layoutContent: "="
        },
        link: function(n, t) {
            if (n.layoutContent || (n.layoutContent = {}), n.layoutContent.hasMoreContent = !1, t.find("li") && t.find("li").length > 0) {
                var r = t.find("li").length,
                    u = t.find("li").width(),
                    f = Math.floor(t.width() / u);
                f + 1 < r && (n.layoutContent.hasMoreContent = !0)
            }
        }
    }
});