// ~/viewapp/pages/chat/directives/removeFocusDirective.js
"use strict";
chat.directive("removeFocus", ["$log", function() {
    return {
        restrict: "A",
        scope: !0,
        link: function(n, t) {
            t.bind("click touchstart", function(t) {
                t.preventDefault(), n.sendMessage()
            })
        }
    }
}]);