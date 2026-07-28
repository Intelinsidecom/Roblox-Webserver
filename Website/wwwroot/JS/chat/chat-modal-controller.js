// Chat-specific modal controller.
// Identical to widgets/modal/controllers/modalController.js but uses
// $modalInstance (0.11.2 API) instead of $uibModalInstance (2.5.0 API).
// Registers as "modalController" on the "modal" module so it matches
// modalOptions.commonController without modifying any shared files.
"use strict";
angular.module("modal").controller("modalController", ["$log", "$scope", "$sce", "$modalInstance", "modalData", "modalService", function(n, t, i, r, u, f) {
    t.modalData = u, t.closeActions = f.closeActions, t.close = function(n) {
        r.close(n)
    }, t.dismiss = function() {
        r.dismiss("dismissed")
    }, t.renderHtml = i.trustAsHtml
}]);
