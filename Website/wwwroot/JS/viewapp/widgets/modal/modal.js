// ~/viewapp/widgets/modal/modal.js
"use strict";
var modal = angular.module("modal", ["ui.bootstrap"]);
modal.config(["$injector", function($injector) {
    if (Roblox.Lang && Roblox.Lang.ControlsResources) {
        try {
            var lp = $injector.get("languageResourceProvider");
            lp.setLanguageKeysFromFile(Roblox.Lang.ControlsResources);
        } catch(e) {}
    }
}]);