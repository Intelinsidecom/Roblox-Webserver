// Places/Templates.js
$(function() {
    function n() {
        return $('div.templates[data-templatetype="' + $("ul.templatetypes li.active").attr("data-templatetype") + '"]')
    }
    var t = $("ul.templatetypes li"),
        i;
    t.click(function() {
        var t = n();
        return t.hide(), $("ul.templatetypes li.active").removeClass("active"), $(this).addClass("active"), t = n(), t.show(), !1
    }), i = t.first(), i.addClass("active"), n().show(), Roblox.require("Widgets.PlaceImage", function() {
        Roblox.Widgets.PlaceImage.populate()
    }), $(".template").click(function() {
        $(".template.template-selected").removeClass("template-selected"), $(this).addClass("template-selected")
    }), $(".template a").removeAttr("href")
});