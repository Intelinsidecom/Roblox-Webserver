"use strict";

avatar.filter("round", [function() {
        return function(n, t) {
            return t*Math.round(n/t)
        }
    }

    ]);