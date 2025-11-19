// Events/AjaxPageLoadEvent.js
var Roblox = Roblox || {};
Roblox.AjaxPageLoadEvent = function() {
    var n = function(n, t) {
        Roblox.EventStream && Roblox.EventStream.SendEventWithTarget("ajaxPageLoad", n, {
            Url: t
        }, Roblox.EventStream.TargetTypes.WWW)
    };
    return {
        SendEvent: n
    }
}();