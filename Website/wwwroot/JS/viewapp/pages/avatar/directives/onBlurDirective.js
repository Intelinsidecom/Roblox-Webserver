"use strict";

avatar.directive("onBlur", ["$document", "$log", "$timeout", function(n, t, i) {
        return {
            restrict:"A", scope: {
                blurTarget:"=", onBlur:"&"
            }

            , link:function(r, u) {
                var o=r.blurTarget, e=function(i) {
                    u.has(i.target).length>0?t.debug("clicked inside element"):(t.debug("clicked outside element"), n.off("click touchstart", e), o.active= !1, r.$apply(), r.$apply(r.onBlur()))
                }

                , s=r.$watch("blurTarget.active", function() {
                        typeof o.active !="undefined" &&(o.active?(t.debug("binding click handler"), i(function() {
                                        n.on("click touchstart", e)
                                    }

                                    , 0)):(t.debug("unbinding click handler"), n.off("click touchstart", e)))

                    }); r.$on("$destroy", function() {
                        s&&s(), n.off("click touchstart", e)
                    })
            }
        }
    }

    ]);