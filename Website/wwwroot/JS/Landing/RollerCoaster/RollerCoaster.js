// Landing/RollerCoaster/RollerCoaster.js
function MoveMagicLine(n) {
    "use strict";
    var t = $("#magic-line");
    if (t.length != 0 && (!t.is(":animated") || n)) {
        var i = 0,
            r = 0,
            u = $(document).scrollTop();
        var whatsRobloxOffset = $("#WhatsRobloxContainer").offset();
        var navbarHeight = $(".navbar-landing").height() || 0;
        if (whatsRobloxOffset && u < whatsRobloxOffset.top - navbarHeight) {
            if (i = $("#PlayLink").position().left, r = $("#PlayLink").width(), t.data("curtab") == 1 && !n) return;
            t.data("curtab", 1)
        } else {
            var robloxDeviceOffset = $("#RobloxDeviceText").offset();
            if (robloxDeviceOffset && u < robloxDeviceOffset.top - navbarHeight) {
                if (i = $("#AboutLink").position().left, r = $("#AboutLink").width(), t.data("curtab") == 2 && !n) return;
                t.data("curtab", 2)
            } else {
                if (i = $("#PlatformLink").position().left, r = $("#PlatformLink").width(), t.data("curtab") == 3 && !n) return;
                t.data("curtab", 3)
            }
        }
        t.animate({
            queue: "link-magic-line",
            left: i,
            width: r
        })
    }
}

function scrollTo(n, t) {
    "use strict";
    var f = $("#magic-line"),
        i = 0,
        r = 0,
        u;
    switch (n) {
        case 1:
            i = $("#PlayLink").position().left, r = $("#PlayLink").width();
            break;
        case 2:
            i = $("#AboutLink").position().left, r = $("#AboutLink").width();
            break;
        case 3:
            i = $("#PlatformLink").position().left, r = $("#PlatformLink").width()
    }
    f.animate({
        left: i,
        width: r
    });
    var targetOffset = $(t).offset();
    if (!targetOffset) {
        return !1; // Element not found or detached, cannot scroll
    }
    u = targetOffset.top;
    $("html, body").animate({
        scrollTop: u
    }, 600);
    return !1
}

function validateLogin() {
    var n = $("#LoginUsername").val(),
        t = $("#LoginPassword").val();
    return n ? $("#LoginUsernameParent").removeClass("has-error") : $("#LoginUsernameParent").addClass("has-error"), t ? $("#LoginPasswordParent").removeClass("has-error") : $("#LoginPasswordParent").addClass("has-error"), t && n
}
$(function() {
    $("[autofocus]:not(:focus)").eq(0).focus();
    var n = !1,
        t = $(".navbar-landing");
    $(window).on("scroll touchmove", function() {
        var i = $(window).scrollTop(),
            r = i > 75 || n ? .85 : .35 + i / 150;
        t.css("background-color", "rgba(0,0,0," + r + ")")
    });
    $(window).on("resize", function() {
        MoveMagicLine(!0)
    });
    setInterval(MoveMagicLine, 400);
    $("#LandingNavbar").on("show.bs.collapse", function() {
        t.css("background-color", "rgba(0,0,0,0.85)"), n = !0
    });
    $("#LandingNavbar").on("hide.bs.collapse", function() {
        n = !1
    });
    $("#LogInForm").submit(function(n) {
        validateLogin() || n.preventDefault()
    });
    $("#LogInForm").on("keyup propertychange paste", "input[name]", validateLogin);
    Roblox.SignupOrLogin.init({
        onSignupSuccess: function() {
            var n = $("#SignUpFormContainer").data("return-url");
            typeof n == "string" && n.length > 0 ? (n += n.indexOf("?") === -1 ? "?" : "&", n += "nu=true", window.location.href = n) : window.location.href = "/games?nu=true"
        },
        onLoginSuccess: function() {
            var n = $("#SignUpFormContainer").data("return-url");
            typeof n == "string" && n.length > 0 ? window.location.href = n : window.location.reload()
        }
    })
});