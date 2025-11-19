// ~/viewapp/widgets/modal/services/modalStringService.js
"use strict";
modal.factory("modalStringService", ["languageResource", function(n) {
    return {
        params: {
            actionButtonText: n.get("Action.Yes"),
            neutralButtonText: n.get("Action.OK")
        }
    }
}]);