// Places/Create.js
var Roblox = Roblox || {};
Roblox.IDE = Roblox.IDE || {}, Roblox.IDE.CreatePlace = function() {
    function i(n, t) {
        return n.replace(/{(\d+)}/g, function(n, i) {
            return typeof t[i] != "undefined" ? t[i] : n
        })
    }

    function r() {
        function e(n) {
            var t = $(".tab-container div.tab.active");
            t.removeClass("active"), $(".tab-content").removeClass("tab-active"), n.addClass("active"), $("#" + n.data("id")).addClass("tab-active"), n.data("id") == "access_tab" && Roblox.PlayerAccess.initializeChosen()
        }

        function s(n) {
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
        var u = $("#finishButton"),
            f = $("input#Name"),
            o = Roblox.IDE.validator({
                button: u,
                enabledClass: t,
                disabledClass: n
            }, [{
                input: f,
                errorSpan: $("#nameRow span")
            }], !0),
            r;
        u.click(function() {
            return u.hasClass(n) ? !1 : ($("#TemplateID").val($(".template.template-selected").attr("placeid")), $("form").submit(), $(this).addClass(n), $(this).prop("disabled", !0), s(), !1)
        }), $("#cancelButton").click(function() {
            document.location = $(this).data("return-url")
        }), $("div.tab").bind("click", function() {
            e($(this))
        }), $("#TemplateID").val() != "" && $(".template").each(function() {
            $(this).attr("placeid") == $("#TemplateID").val() && $(this).addClass("template-selected")
        }), $("div.validation-summary-errors").attr("data-valmsg-summary") == "true" && e($('.tab-container div.tab[data-id="basicsettings_tab"]')), r = $("#userData"), f.val(i(r.text(), [r.data("name"), r.data("placeNumber")])), o.init()
    }
    var t = "btn-primary",
        n = "btn-disabled-primary";
    return $(r), {}
}();