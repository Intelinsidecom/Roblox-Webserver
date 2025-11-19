// ~/viewapp/common/directives/imageRetryDirective.js
"use strict";
robloxApp.directive("imageRetry", ["robloxImagesService", "$log", function(n, t) {
    return {
        restrict: "A",
        scope: {
            thumbnail: "="
        },
        link: function(i, r, u) {
            var e = function(i) {
                    if (i && !i.Final) {
                        var r = +new Date;
                        n.getImageUrl(i, function(n, f) {
                            if (f) {
                                var e = +new Date;
                                Roblox.ThumbnailMetrics && Roblox.ThumbnailMetrics.logFinalThumbnailTime(e - r)
                            } else Roblox.ThumbnailMetrics && Roblox.ThumbnailMetrics.logThumbnailTimeout();
                            u.resetSrc ? n && (i.Url = n) : u.src && u.src.length > 0 || u.disablePlaceholder ? (u.$set("src", n), t.debug("got final, setting src")) : u.lazyImg && (u.$set("lazyImg", n), t.debug("got final, setting lazyimg"))
                        }, 0)
                    }
                },
                f;
            e(i.thumbnail), f = i.$watch(function() {
                return i.thumbnail
            }, function(n) {
                n && e(n)
            }), i.$on("$destroy", function() {
                f && f()
            })
        }
    }
}]);