// Studio/Plugins/PluginInfo.js
var Roblox = Roblox || {};
typeof Roblox.Plugins == "undefined" && (Roblox.Plugins = {}), Roblox.Plugins.PluginInfo = function() {
    var n = function(n) {
        var u = "75",
            r = Roblox.Plugins.PluginInfo.Resources.moreText,
            f = Roblox.Plugins.PluginInfo.Resources.lessText,
            t = n.find(".more-block"),
            i = n.find(".adjust");
        t.height(u).css("overflow", "hidden"), t[0].scrollHeight > t.innerHeight() && (i.text(r), i.show(), i.toggle(function() {
            t.css("height", "auto").css("overflow", "visible"), $(this).text(f)
        }, function() {
            t.css("height", u).css("overflow", "hidden"), $(this).text(r)
        }))
    };
    return {
        init: n
    }
}();