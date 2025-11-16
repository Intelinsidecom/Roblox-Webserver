"use strict";

robloxApp.directive("autoResize", ["$window", function(n) {
        return {
            restrict:"A", link:function(t, i, r) {
                t.attrs= {
                    rows:1, maxLines:999
                }

                ; for(var u in t.attrs)r[u]&&(t.attrs[u]=parseInt(r[u])); t.getOffset=function() {
                    for(var f=n.getComputedStyle(i[0], null), r=["paddingTop", "paddingBottom"], u=0, t=0; t<r.length; t++)u+=parseInt(f[r[t]]); return u
                }

                , t.autoResize=function() {
                    var n=0, r= !1, u; return i[0].scrollHeight-t.offset>t.maxAllowedHeight?(i[0].style.overflowY="scroll", n=t.maxAllowedHeight):(i[0].style.overflowY="hidden", i[0].style.height="auto", n=i[0].scrollHeight, r= !0), i[0].style.height=n+"px", r
                }

                , t.offset=t.getOffset(), t.lineHeight=parseInt(i.css("line-height").replace("px", "")), t.maxAllowedHeight=t.lineHeight*t.attrs.maxLines-t.offset, t.$watch(r.ngModel, function() {
                        t.autoResize()
                    }

                    , !0)
            }
        }
    }

    ]);