"use strict";

var cursorPagination=angular.module("cursorPagination", []).config(["$qProvider", function(n) {
        angular.isFunction(n.errorOnUnhandledRejections)&&n.errorOnUnhandledRejections( !1)
    }

    ]);