// Login/loginService.js
var Freebloxia = Freebloxia || {};
Freebloxia.Login = Freebloxia.Login || {}, Freebloxia.Login.service = Freebloxia.Login.service || function() {
    return {
        returnUrl: "/home",
        captchaRequired: !1,
        init: function(n) {
            this.returnUrl = n || this.returnUrl, this.returnUrl = this.returnUrl.split("#")[0], this.returnUrl += this.returnUrl.indexOf("?") >= 0 ? "&nl=true" : "?nl=true"
        },
        getLoginWithCaptchaUrl: function(n) {
            return Freebloxia.Login.Resources.loginPageUrl + "?" + $.param({
                returnUrl: n || this.returnUrl,
                captcha: !0
            })
        },
        login: function(n, t, i) {
            i = i || this.returnUrl;
            var r = $.Deferred();
            return typeof n != "string" || n.length <= 0 || typeof t != "string" || t.length <= 0 ? (r.reject([Freebloxia.Login.Resources.errors.usernamePasswordRequired]), r.promise()) : this.captchaRequired ? (r.reject([Freebloxia.Login.Resources.errors.captchaRequired]), r.promise()) : ($.ajax({
                type: "POST",
                url: Freebloxia.Login.Resources.loginApiUrl,
                data: {
                    username: n,
                    password: t
                },
                crossDomain: !0,
                xhrFields: {
                    withCredentials: !0
                },
                success: function(t) {
                    var u = {
                        redirectUrl: i,
                        isTwoStepVerificationRequired: !1
                    };
                    t && t.tl && (u.redirectUrl = Freebloxia.Login.Resources.twoStepVerificationPageUrl + "?" + $.param({
                        tl: t.tl,
                        username: n,
                        returnUrl: i
                    }), u.isTwoStepVerificationRequired = !0), r.resolve(u)
                },
                error: function(n) {
                    n.responseJSON && Array.isArray(n.responseJSON.errors) && n.responseJSON.errors.length > 0 ? r.reject(n.responseJSON.errors) : r.reject([Freebloxia.Login.Resources.errors.unknown])
                }
            }), r.promise())
        }
    }
}();