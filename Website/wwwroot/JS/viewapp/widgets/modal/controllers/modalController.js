// ~/viewapp/widgets/modal/controllers/modalController.js
"use strict";
modal.controller("modalController", ["$log", "$scope", "$sce", "$uibModalInstance", "modalData", "modalService", function(n, t, i, r, u, f) {
    t.modalData = u, t.closeActions = f.closeActions, t.close = function(n) {
        r.close(n)
    }, t.dismiss = function() {
        r.dismiss("dismissed")
    }, t.renderHtml = i.trustAsHtml
}]);