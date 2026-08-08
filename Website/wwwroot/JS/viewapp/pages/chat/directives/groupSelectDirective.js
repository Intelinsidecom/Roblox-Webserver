// ~/viewapp/pages/chat/directives/groupSelectDirective.js
"use strict";
chat.directive("groupSelect", ["$log", function() {
    return {
        restrict: "A",
        link: function(n, t) {
            var r = n.chatLibrary.inApp ? n.chatLibrary.inAppLayout.topBarHeight : n.chatLibrary.layout.topBarHeight;
            n.$watch(function() {
                return t.innerHeight()
            }, function(t, i) {
                if (t && t !== i) {
                    var s = "#" + n.dialogData.layoutId + " .dialog-container",
                        h = "#" + n.dialogData.layoutId + " " + n.friendsScrollbarElm,
                        c = angular.element(s),
                        e = angular.element(h),
                        o, f, u;
                    n.chatLibrary.inApp ? (u = t, f = "calc(100% - " + u + "px)") : (u = r + t, o = c.height(), f = o - u), e.css("height", f), e.mCustomScrollbar("update")
                }
            }, !0)
        }
    }
}]);