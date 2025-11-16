"use strict";

robloxHelpers.directive("focusMe", ["$timeout", function(n) {
        return {
            scope: {
                trigger:"@focusMe"
            }

            , link:function(t, i, r) {
                t.$watch(function() {
                        return r.focusMe
                    }

                    , function(t) {
                        t&&n(function() {
                                t==="true" &&i[0].focus()
                            }

                            , 0)
                    })
            }
        }
    }

    ]);