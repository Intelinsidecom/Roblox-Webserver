// extensions/Thumbnails.js
$(function() {
    var n = $("#image-retry-data"),
        t = n ? n.data("image-retry-timer") : 1500,
        i = n ? n.data("image-retry-max-times") : 10;
    $.fn.loadRobloxThumbnails = function() {
        function n(r) {
            var u = r.data("retry-url");
            u && $.ajax({
                url: u,
                dataType: "json",
                cache: !1,
                crossDomain: !0,
                xhrFields: {
                    withCredentials: !0
                },
                success: function(u) {
                    if (u.Final) {
                        var f = r.find("img");
                        f.length === 1 ? f.attr("src", u.Url) : r.find("img.original-image").attr("src", u.Url), r.removeAttr("data-retry-url")
                    } else r.retryCount = r.retryCount ? r.retryCount + 1 : 1, r.retryCount < i && setTimeout(function() {
                        n(r)
                    }, t)
                }
            })
        }
        return this.each(function() {
            var i = $(this);
            setTimeout(function() {
                n(i)
            }, t)
        })
    }
});