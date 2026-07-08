// ~/viewapp/widgets/captcha/services/captchaInterface.js
captcha.factory("captchaInterface", ["$q", function() {
    var t = Roblox.Captcha || {};
    return {
        types: t.types,
        setEndpoint: t.setEndpoint,
        setInvisibleMode: t.setInvisibleMode,
        setSiteKey: t.setSiteKey,
        reset: t.reset,
        render: t.render,
        execute: t.execute
    }
}]);