// IDE/create.js
var Roblox = Roblox || {};
Roblox.IDE = Roblox.IDE || {}, Roblox.IDE.Create = Roblox.IDE.Create || function() {
    function i(n) {
        var t = {
            overlayClose: !1,
            opacity: 80,
            overlayCss: {
                backgroundColor: "#000"
            },
            escClose: !1
        };
        typeof n != "undefined" && n !== "" && $.modal.close("." + n), $("#ProcessingView").modal(t)
    }

    function r() {
        window.location.href = $(".boxed-body").data("return-url")
    }

    function u() {
        window.close()
    }

    function f() {
        var e = $("input#Name"),
            f = $("#finishButton"),
            o = Roblox.IDE.validator({
                button: f,
                enabledClass: t,
                disabledClass: n
            }, [{
                input: e,
                errorSpan: $("#nameRow span")
            }], !1);
        $("#cancelButton").click(r), $("#closeButton").click(u), f.click(function() {
            return $(this).hasClass(n) ? !1 : ($(this).removeClass(t).addClass(n), i(), $("form").submit(), !1)
        }), o.init()
    }
    var t = "btn-primary",
        n = "btn-disabled-primary";
    return $(f), {}
}();