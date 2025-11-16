"use strict";

robloxHelpers.directive("imageRetry", ["robloxImagesService", "$log", function(n, t) {
        return {
            restrict:"A", scope: {
                thumbnail:"="
            }

            , link:function(i, r, u) {
                var e=function(i) {
                    i&& !i.Final&&n.getImageUrl(i,function(n){
                    u.resetSrc?n&&(i.Url=n):u.src&&u.src.length>0||u.disablePlaceholder?(u.$set("src", n), t.debug("got final, setting src")):u.lazyImg&&(u.$set("lazyImg", n), t.debug("got final, setting lazyimg"))
                }

                , 0)
        }

        ,
        f;

        e(i.thumbnail),
        f=i.$watch(function() {
                return i.thumbnail
            }

            , function(n) {
                n&&e(n)

            }),
        i.$on("$destroy", function() {
                f&&f()
            })
    }
}
}

]);