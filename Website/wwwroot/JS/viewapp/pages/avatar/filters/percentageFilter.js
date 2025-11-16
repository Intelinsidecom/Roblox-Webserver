"use strict";

avatar.filter("percentage", ["$filter", function(n) {
        return function(t, i) {
            return n("number")(t*100, i)
        }
    }

    ]);