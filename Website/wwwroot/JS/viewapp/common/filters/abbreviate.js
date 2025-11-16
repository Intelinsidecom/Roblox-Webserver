"use strict";

robloxFilters.filter("abbreviate", ["$filter", function(n) {
        var i=null, u=["thousand", "million", "billion"], t= {
            thousand:1e3, million:1e6, billion:1e9
        }

        , f= {
            thousand:"K+", million:"M+", billion:"B+"
        }

        , r=function(r, u) {
            return i&&u===i?n("number")(r):n("number")((r/t[u]).toFixed(0), 0)+f[u]
        }

        ; return function(f, e) {
            return(typeof e !="undefined" &&(i=u[e]), f<t.thousand*10)?n("number")(f):f<t.million?r(f, "thousand"):f<t.billion?r(f, "million"):r(f, "billion")
        }
    }

    ]);