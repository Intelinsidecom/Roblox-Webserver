// ~/viewapp/common/formEvents/directives/formInteraction.js
"use strict";
formEvents.directive("rbxFormInteraction", function() {
    return {
        require: "^form",
        restrict: "A",
        link: function(n, t, i, r) {
            t.bind("blur", function() {
                Freebloxia.FormEvents && Freebloxia.FormEvents.SendInteractionOffFocus(r.context, angular.element(this).attr("name"))
            }).bind("focus", function() {
                Freebloxia.FormEvents && Freebloxia.FormEvents.SendInteractionFocus(r.context, angular.element(this).attr("name"))
            })
        }
    }
});