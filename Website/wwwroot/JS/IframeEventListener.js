// IframeEventListener.js
$(function() {
    typeof $.receiveMessage == "function" && $.receiveMessage(function(t) {
        n(t.data)
    });
    var n = function(n) {
        var i, r, t;
        n.indexOf("resizeFrame") != -1 ? (t = n.split(","), i = t[1] ? parseInt(t[1]) : null, Roblox.SignupOrLoginParent.resizeFrame(i)) : n.indexOf("resizeModal") != -1 ? (t = n.split(","), i = t[1] ? parseInt(t[1]) : null, Roblox.SignupOrLoginModal.resizeModal(i)) : n.indexOf("resize") != -1 ? (t = n.split(","), $("#iframe-login").css({
            height: t[1]
        })) : n.indexOf("fbRegister") != -1 ? (t = n.split("^"), r = "&fbname=" + encodeURIComponent(t[1]) + "&fbem=" + encodeURIComponent(t[2]) + "&fbdt=" + encodeURIComponent(t[3]), window.location.href = "../Login/Default.aspx?iFrameFacebookSync=true" + r) : n.indexOf("reload") != -1 && (n.indexOf(",") != -1 ? (t = n.split(","), window.location.href = t[1]) : window.location.reload())
    }
});