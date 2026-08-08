// ~/viewapp/pages/chat/services/cookieService.js
"use strict";
chat.factory("cookieService", ["chatUtility", "$log", function() {
    return {
        isCookieDefined: function(n) {
            return angular.isDefined($.cookie(n)) && $.cookie(n)
        },
        updateCookie: function(n, t, i) {
            $.cookie(n, JSON.stringify(t), i)
        },
        retrieveCookie: function(n) {
            return this.isCookieDefined(n) ? JSON.parse($.cookie(n)) : []
        },
        destroyCookie: function(n, t) {
            $.cookie(n, null, t)
        }
    }
}]);