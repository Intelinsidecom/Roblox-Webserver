"use strict";

avatar.directive("pageFocus", ["$document", "$log", function(n, t) {
        return {
            restrict:"A", scope: {
                focusGained:"&focusGained", focusLost:"&focusLost"
            }

            , link:function(n) {
                angular.element(document).ready(function() {
                        var i=null; $(window).focus(function() {
                                var u, r; t.debug("Window focus"), u=+new Date, i !==null&&(r=(u-i)/1e3, n.secondsIdle=r, n.focusGained({
                                        secondsIdle:r
                                    }))

                        }), $(window).blur(function() {
                            t.debug("Window blur"), i=+new Date, n.focusLost()
                        })
                })
        }
    }
}

]);