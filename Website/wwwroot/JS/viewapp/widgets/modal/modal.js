// ~/viewapp/widgets/modal/modal.js
"use strict";
var modal = angular.module("modal", ["ui.bootstrap"]).config(["$uibModalProvider", "$injector", function(n, t) {
    if (n.options.openedClass = "modal-open-noscroll", n.options.animation = !1, Roblox.Lang && Roblox.Lang.ControlsResources) {
        var i = t.get("languageResourceProvider");
        i.setLanguageKeysFromFile(Roblox.Lang.ControlsResources)
    }
}]);