// ~/viewapp/pages/profile/filters/profileHeaderStatusTextFilter.js
"use strict";
profile.filter("statusfilter", function() {
    return function(n) {
        return n ? '"' + n + '"' : ""
    }
});