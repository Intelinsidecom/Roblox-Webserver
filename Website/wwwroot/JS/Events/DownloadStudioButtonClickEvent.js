// Events/DownloadStudioButtonClickEvent.js
var Roblox = Roblox || {};
Roblox.DownloadStudioButtonClickEvent = function() {
    var n = function(n, t) {
            typeof GoogleAnalyticsEvents != "undefined" && GoogleAnalyticsEvents && GoogleAnalyticsEvents.FireEvent([n + t, "DownloadStudioButtonClick"]), Roblox.EventStream.SendEvent("downloadStudioButtonClick", n + t, {})
        },
        t = function() {
            if (Roblox.EventStream) {
                var t = "unknown",
                    i = $("#rbx-body");
                i.length > 0 && i.data("internal-page-name") && (t = i.data("internal-page-name"));
                $(".studio-launch").on("click", function() {
                    n(t, "")
                });
                $(".studio-launch-header").on("click", function() {
                    n(t, "-header")
                });
                $(".studio-launch-image").on("click", function() {
                    n(t, "-image")
                });
                $(".studio-launch-footer").on("click", function() {
                    n(t, "-footer")
                })
            }
        };
    return {
        Init: t
    }
}(), $(function() {
    Roblox.DownloadStudioButtonClickEvent.Init()
});