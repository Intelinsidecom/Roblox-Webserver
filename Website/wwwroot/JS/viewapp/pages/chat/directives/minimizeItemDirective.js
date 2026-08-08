// ~/viewapp/pages/chat/directives/minimizeItemDirective.js
"use strict";
chat.directive("minimizeItem", ["$log", function() {
    return {
        restrict: "A",
        scope: !0,
        link: function(n) {
            var u = function() {
                    n.$apply(n.openDialog(n.dialogLayoutId))
                },
                r;
            angular.element("#dialogs-minimize").on("click touchstart", ".popover-content #" + n.dialogLayoutId + " .minimize-title", u);
            r = function() {
                n.$apply(n.remove(n.dialogLayoutId))
            };
            angular.element("#dialogs-minimize").on("click touchstart", ".popover-content #" + n.dialogLayoutId + " .minimize-close", r)
        }
    }
}]);