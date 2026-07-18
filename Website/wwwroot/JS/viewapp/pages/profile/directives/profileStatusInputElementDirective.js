// ~/viewapp/pages/profile/directives/profileStatusInputElementDirective.js
"use strict";
profile.directive("statusInputElement", ["$timeout", function(n) {
    return {
        link: function(t, i) {
            t.$watch(function() {
                return t.profileHeaderLayout.statusInputFocused
            }, function(t) {
                t === !0 && n(function() {
                    i[0].focus()
                })
            });
            i.on("focus", function() {
                var i = this;
                t.profileHeaderLayout.focusedElement != i && (t.profileHeaderLayout.focusedElement = i, n(function() {
                    i.select()
                }, 10))
            });
            i.on("blur", function() {
                t.profileHeaderLayout.focusedElement = null, t.profileHeaderLayout.statusInputFocused = !1
            })
        }
    }
}]);