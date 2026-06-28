// IDE/BuildTemplates.js
function getSelectedTemplateType() {
    return $('div.templates[data-templatetype="' + $("ul.templatetypes li.selectedType").attr("data-templatetype") + '"]')
}
$(function() {
    var n = $("ul.templatetypes li"),
        t;
    n.click(function() {
        var n = getSelectedTemplateType();
        n.hide(), $("ul.templatetypes li.selectedType").removeClass("selectedType"), $(this).addClass("selectedType"), n = getSelectedTemplateType(), n.show()
    }), t = n.first(), t.addClass("selectedType"), getSelectedTemplateType().show(), Roblox.require("Widgets.PlaceImage", function() {
        Roblox.Widgets.PlaceImage.populate()
    }), $(".template").click(function() {
        Roblox.Client.isIDE() ? (GoogleAnalyticsEvents && GoogleAnalyticsEvents.FireEvent(["Studio", "PlaceTemplateOpened", $(this).attr("placeid")]), window.editTemplateInStudio($(this).attr("placeid"))) : Roblox.GenericModal.open("New Project", "/images/Icons/img-alert.png", "To build using this template, open to this page in <a target='_blank' href='https://web.archive.org/web/20190716171201/http://wiki.roblox.com/index.php/Studio'>ROBLOX Studio</a>.")
    }), $(".template a").removeAttr("href")
});