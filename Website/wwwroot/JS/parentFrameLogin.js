// parentFrameLogin.js
$(function() {
    var n = !1,
        t, i;
    $("#header-login").click(function(i) {
        return n = !n, t(n), $("#iFrameLogin").toggle(), $("#header-login").toggleClass("active"), i.stopPropagation(), !1
    }), $("#headerLogin").click(function(i) {
        return n = !n, t(n), $("#iFrameLogin").toggle(), $("#headerLogin").toggleClass("active"), i.stopPropagation(), !1
    }), $(document).click(function() {
        n && ($("#header-login").removeClass("active"), $("#headerLogin").removeClass("active"), $("#iFrameLogin").hide(), n = !1)
    }), t = function(n) {
        $(".IframeAdHide").each(function() {
            $(this).height() == 90 && $(this).width() == 728 && (n ? $(this).css("visibility", "hidden") : $(this).css("visibility", "visible"))
        })
    }, $("#header-login-wrapper").data("display-opened") == "True" && ($("#header-login").addClass("active"), $("#iFrameLogin").css("display", "block")), i = function(n) {
        var t, i;
        n.indexOf("resize") != -1 && (t = n.split(","), $("#iframe-login").css({
            height: t[1]
        })), n.indexOf("fbRegister") != -1 && (t = n.split("^"), i = "&fbname=" + encodeURIComponent(t[1]) + "&fbem=" + encodeURIComponent(t[2]) + "&fbdt=" + encodeURIComponent(t[3]), window.location.href = "../Login/Default.aspx?iFrameFacebookSync=true" + i)
    }, $.receiveMessage(function(n) {
        i(n.data)
    })
});