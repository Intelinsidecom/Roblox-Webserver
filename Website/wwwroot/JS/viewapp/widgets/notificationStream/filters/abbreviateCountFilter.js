"use strict";

notificationStreamIcon.filter("abbreivateCount", function() {
        var n=100, t= {
            100:"99+", 1e3:"1K+"
        }

        ; return function(i, r, u) {
            return(r||(r=n), u||(u=t[r]), i>=r)?u:i
        }
    });