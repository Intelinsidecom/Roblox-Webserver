// utilities/robloxTabs.js
$(function() {
    $.History.bind(function(n) {
        var t = "#" + n.substring(2);
        $(".rbx-tab a").each(function() {
            $(this).attr("href") == t && $(this).click()
        })
    });
    var n = ".rbx-tab";
    $(n).click(function(n) {
        if (n.hasOwnProperty("originalEvent")) {
            var t = "!/" + $(this).find("a").attr("href").substring(1);
            $.History.go(t)
        }
    })
});