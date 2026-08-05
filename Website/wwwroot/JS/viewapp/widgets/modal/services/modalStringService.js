// ~/viewapp/widgets/modal/services/modalStringService.js
"use strict";
modal.factory("modalStringService", ["languageResource", function(n) {
    // languageResource.get() returns "" and warns when a key is missing,
    // which left every default modal button blank. Fall back to plain English
    // labels so the buttons render even when no language map is loaded.
    function safe(key, fallback) {
        var v = n.get(key);
        return v && v.length > 0 ? v : fallback;
    }
    return {
        params: {
            actionButtonText: safe("Action.Yes", "Yes"),
            neutralButtonText: safe("Action.OK", "OK")
        }
    }
}]);