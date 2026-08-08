// ~/viewapp/pages/chat/directives/chatBarDirective.js
"use strict";
chat.directive("chatBar", ["$log", function() {
    return {
        restrict: "A",
        scope: !0,
        replace: !0,
        templateUrl: Roblox.ChatTemplates.ChatBarTemplate,
        link: function() {
            Roblox.BootstrapWidgets.SetupTooltip()
        }
    }
}]);