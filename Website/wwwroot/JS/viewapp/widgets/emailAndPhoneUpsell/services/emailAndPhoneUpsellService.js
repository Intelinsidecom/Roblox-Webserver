// ~/viewapp/widgets/emailAndPhoneUpsell/services/emailAndPhoneUpsellService.js
"use strict";
emailAndPhoneUpsell.factory("emailAndPhoneUpsellService", ["httpService", "emailAndPhoneUpsellConstants", function(n, t) {
    function r(n) {
        i = n
    }

    function u() {
        var r = i + t.urls.getScreen,
            u = {
                url: r
            };
        return n.httpGet(u)
    }

    function f(i, r) {
        var u = t.urls.submitEmail,
            f = {
                url: u
            },
            e = {
                emailAddress: i,
                password: r
            };
        return n.httpPost(f, e)
    }

    function e() {
        var i = t.urls.verifyEmail,
            r = {
                url: i
            };
        return n.httpPost(r)
    }
    var i;
    return {
        setAccountSettingsDomain: r,
        getScreen: u,
        submitEmail: f,
        verifyEmail: e
    }
}]);