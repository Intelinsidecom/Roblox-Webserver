// ~/viewapp/widgets/modal/services/modalService.js
"use strict";
modal.factory("modalService", ["$uibModal", "modalOptions", "modalStringService", function(n, t, i) {
    function u(i) {
        var u = angular.extend({}, r, i),
            f = n.open({
                templateUrl: t.commonTemplateUrl,
                controller: t.commonController,
                windowClass: u.cssClass || "",
                animation: u.animation || t.defaults.animation,
                keyboard: u.keyboard || t.defaults.keyboard,
                backdrop: u.closeButtonShow ? !0 : "static",
                openedClass: u.openedClass || "modal-open-noscroll",
                resolve: {
                    modalData: u
                }
            });
        return f.result.then(angular.noop, angular.noop), f
    }
    var r = angular.extend({}, i.params, t.params);
    return {
        open: u
    }
}]);