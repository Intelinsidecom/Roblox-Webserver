// ~/viewapp/pages/chat/directives/backBtnDirective.js
"use strict";
chat.directive("backBtn", ["$log", function() {
    return {
        restrict: "A",
        scope: !0,
        link: function(n, t) {
            t.bind("click touchstart", function(t) {
                t.preventDefault(), n.closeDialog(n.dialogData.layoutId)
            })
        }
    }
}]);