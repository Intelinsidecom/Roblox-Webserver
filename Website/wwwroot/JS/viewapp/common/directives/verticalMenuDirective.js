// ~/viewapp/common/directives/verticalMenuDirective.js
"use strict";
robloxApp.directive("verticalMenu", [function() {
    function n() {
        Roblox.BootstrapWidgets.SetupVerticalMenu()
    }
    return {
        restrict: "A",
        link: function(t, i, r) {
            var u = t.$watch(r.resetVerticalMenu, function() {
                n()
            });
            t.$on("$destroy", function() {
                u && u()
            })
        }
    }
}]);