// ~/viewapp/common/directives/tooltipDirective.js
"use strict";
robloxApp.directive("tooltip", [function() {
    return {
        restrict: "A",
        link: function() {
            Roblox.BootstrapWidgets.SetupTooltip()
        }
    }
}]);