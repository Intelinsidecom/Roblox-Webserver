// SocialLogin.js
var Roblox = Roblox || {};
typeof Roblox.SocialLogin == "undefined" && (Roblox.SocialLogin = new function() {
    function i() {
        gigya.socialize.getUserInfo({
            callback: r
        })
    }

    function r(t) {
        var i = $("#SocialIdentitiesInformation").data("rbx-login");
        $.ajax({
            url: i,
            method: "POST",
            data: {
                currentUid: t.UID
            },
            success: n,
            error: u
        })
    }

    function u() {
        $(".connect-button[data-rbx-provider]").each(function() {
            var n = $(this).data("rbx-provider"),
                t = $("#connect-" + n);
            t.text("Sorry, an error occurred. Please try again later.")
        })
    }

    function n() {
        gigya.socialize.getUserInfo({
            callback: f
        })
    }

    function f(n) {
        var t = [];
        $(".connect-button[data-rbx-provider]").each(function() {
            t.push($(this.valueOf()).data("rbx-provider"))
        }), $(t).each(function() {
            var i = this.valueOf(),
                t = "connect-" + i;
            n.user.identities.hasOwnProperty(i) ? ($("#" + t + " .connect-button").hide(), $("#" + t + " .nickname").text(n.user.identities[i].nickname), $("#" + t + " .disconnect-link").html("Disconnect"), i === "facebook" && $("#FacebookConnectCard").length && ($("#FacebookConnectCard p").html("You've successfully connected as: <b>" + n.user.nickname + "</b>"), $("#FacebookConnectCard #connect-facebook").hide(), $("#FacebookConnectCard").fadeOut(5e3))) : ($("#" + t + " .nickname").html(""), $("#" + t + " .disconnect-link").html(""), $("#" + t + " .connect-button").show())
        })
    }

    function e(t) {
        gigya.socialize.addConnection({
            provider: t,
            callback: function() {
                var i = $("#SocialIdentitiesInformation").data("rbx-update"),
                    t = "socialStatusBar";
                $.ajax({
                    url: i,
                    method: "POST",
                    success: function() {
                        n()
                    },
                    error: function(n) {
                        $("#" + t).html(JSON.parse(n.responseText).message), $("#" + t).show().delay(5e3).fadeOut(1e3)
                    }
                })
            }
        })
    }

    function o(t) {
        var r = $("#SocialIdentitiesInformation").data("rbx-disconnect"),
            i = "socialStatusBar";
        $.ajax({
            url: r,
            method: "POST",
            data: {
                provider: t
            },
            success: function() {
                n()
            },
            error: function(n) {
                $("#" + i).html(JSON.parse(n.responseText).message), $("#" + i).show().delay(5e3).fadeOut(1e3)
            }
        })
    }

    function s(n, i) {
        var f = $("#SocialIdentitiesInformation").data("rbx-login-redirect-url"),
            u, r;
        typeof f != "undefined" && (u = "https://" + window.location.hostname + (window.location.port ? ":" + window.location.port : "") + f, r = {}, r = t ? {
            provider: n,
            authFlow: i,
            callback: function(t) {
                t && (t.user.provider = n, $.ajax({
                    url: u,
                    method: "POST",
                    data: t.user,
                    success: function(n) {
                        n.userId ? n.isFromStudio ? Roblox.SignupOrLoginIframe.reloadParent("/ide/welcome") : Roblox.SignupOrLoginIframe.reloadParent() : n.url ? (Roblox.SignupOrLoginIframe.resizeFrame(), window.location.href = n.url) : n.parentUrl && Roblox.SignupOrLoginIframe.reloadParent(n.parentUrl)
                    },
                    error: function() {}
                }))
            }
        } : {
            provider: n,
            authFlow: i,
            redirectMethod: "post",
            redirectURL: u
        }, gigya.socialize.login(r))
    }
    var t = typeof $("#SocialIdentitiesInformation").data("is-rendered-in-iframe") != "undefined" && Roblox.SignupOrLoginIframe;
    return {
        updateLoginStatus: i,
        showConnectionsUi: n,
        addConnection: e,
        removeConnection: o,
        login: s,
        isRenderedInIframe: t
    }
}), $(function() {
    typeof $("#SocialIdentitiesInformation").data("user-is-authenticated") != "undefined" && (Roblox.SocialLogin.isRenderedInIframe ? Roblox.SignupOrLoginIframe.reloadParent() : Roblox.SocialLogin.updateLoginStatus()), $(".connect-button").click(function() {
        Roblox.SocialLogin.addConnection($(this).data("rbx-provider"))
    }), $(".disconnect-link").click(function() {
        Roblox.SocialLogin.removeConnection($(this).data("rbx-provider"))
    });
    $(".social-login").on("click", function(n) {
        typeof $("#SocialIdentitiesInformation").data("force-use-redirect") != "undefined" ? Roblox.SocialLogin.isRenderedInIframe ? Roblox.SignupOrLoginIframe.reloadParent("/social/redirect-to-facebook") : Roblox.SocialLogin.login($(this).data("rbx-provider"), "redirect") : Roblox.SocialLogin.login($(this).data("rbx-provider"), "popup"), n.preventDefault()
    })
});