// ~/viewapp/common/formEvents/directives/formInteraction.js
"use strict";
formEvents.directive("rbxFormInteraction", function() {
    return {
        require: "^form",
        restrict: "A",
        link: function(n, t, i, r) {
            t.bind("blur", function() {
                Roblox.FormEvents && Roblox.FormEvents.SendInteractionOffFocus(r.context, angular.element(this).attr("name"))
            }).bind("focus", function() {
                Roblox.FormEvents && Roblox.FormEvents.SendInteractionFocus(r.context, angular.element(this).attr("name"))
            })
        }
    }
});