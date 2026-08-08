// ~/viewapp/pages/chat/directives/chatPlaceholderDirective.js
"use strict";
chat.directive("chatPlaceholder", ["$log", function() {
    return {
        restrict: "A",
        scope: !0,
        templateUrl: Roblox.ChatTemplates.ChatPlaceholderTemplate,
        link: function() {}
    }
}]);