// Chat-specific shim: provides $uibModal by wrapping ui-bootstrap 0.11.2's $modal.
// On pages where ui.bootstrap is redefined as ["ui.bootstrap.v2"], $uibModal already
// exists from ui-bootstrap-custom-tpls-2.5.0.min.js, so we skip the shim entirely.
(function() {
    try {
        var uiMod = angular.module("ui.bootstrap");
        var hasV2 = uiMod.requires && uiMod.requires.indexOf("ui.bootstrap.v2") !== -1;
        if (!hasV2) {
            uiMod.config(["$provide", function($provide) {
                $provide.provider("$uibModal", ["$modalProvider", function($modalProvider) {
                    this.options = $modalProvider.options;
                    this.$get = ["$injector", function($injector) {
                        return $injector.get("$modal");
                    }];
                }]);
            }]);
        }
    } catch(e) {
        console.warn("chat-modal-shim: skipped", e);
    }
})();
