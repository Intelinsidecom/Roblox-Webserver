// ~/viewapp/studio/toolbox/GAEService.js
"use strict";
clientToolbox.factory("GAEService", function() {
    return {
        fireEvent: function(n) {
            if (window._gaq) {
                var t = ["_trackEvent"].concat(n);
                _gaq.push(t)
            }
        }
    }
});