// ~/viewapp/common/services/errorMessageService.js
"use strict";
robloxApp.factory("errorMessageService", function() {
    return this.createErrorMapper = function(n, t) {
        var i = {
            defaultError: t
        };
        return angular.extend(i, n), i.getErrorMessage = function(n) {
            return this[n] || this.defaultError
        }, i.getErrorMessageFromResponse = function(n) {
            var t = "defaultError";
            return n && n.errors && n.errors[0] && n.errors[0].hasOwnProperty("code") && (t = n.errors[0].code), i.getErrorMessage(t)
        }, i
    }, this
});