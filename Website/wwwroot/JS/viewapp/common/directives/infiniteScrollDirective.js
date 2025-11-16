"use strict";

robloxApp.directive("infiniteScroll", ["$rootScope", "$window", "$timeout", "$parse", function(n, t, i, r) {
        return {
            link:function(u, f, e) {
                var s, o, c, h, l, a; t=angular.element(t), c=0, e.infiniteScrollDistance !=null&&u.$watch(e.infiniteScrollDistance, function(n) {
                        return c=parseInt(n, 10)

                    }), l= !0, h= !0, s= !1, e.infiniteScrollDisabled !=null&&u.$watch(e.infiniteScrollDisabled, function(n) {
                        return h= !n, h&&s?(s= !1, o()):void 0

                    }), o=function() {
                    if( !l) return !1;
                    var r, o, i, a, off;
                    a = t.height() + t.scrollTop();
                    // Guard: element may not have an offset (detached/hidden)
                    try {
                        off = (f && typeof f.offset === "function") ? f.offset() : null;
                    } catch(_) { off = null; }
                    if (!off || typeof off.top === "undefined") {
                        return !1;
                    }
                    r = off.top + f.height();
                    o = r - a;
                    i = o <= t.height() * c;
                    return (i && h) ? (n.$$phase ? u.$eval(e.infiniteScroll) : u.$apply(e.infiniteScroll)) : (i ? s = !0 : void 0)
                }

                , e.infiniteScrollAlwaysDisabled !==null&&(a=u.$watch(function() {
                            return r(e.infiniteScrollAlwaysDisabled)(u)
                        }

                        , function(n) {
                            n !==null&&typeof n !="undefined" &&(l= !n)

                        })); t.on("scroll", o); return u.$on("manualInfiniteScrollCheck", o), u.$on("$destroy", function() {
                        return a&&a(), t.off("scroll", o)

                    }), i(function() {
                        if(e.infiniteScrollImmediateCheck) {
                            if(u.$eval(e.infiniteScrollImmediateCheck))return o()
                        }

                        else return o()
                    }

                    , 0)
            }
        }
    }

    ]);