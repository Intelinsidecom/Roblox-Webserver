// extensions/Thumbnails.js
$(function() {
    var n = $("#image-retry-data"),
        t = n ? n.data("image-retry-timer") : 1500,
        i = n ? n.data("image-retry-max-times") : 10;
    var ongoingRequests = {};
    var thumbnailCache = {};
    var CACHE_DURATION = 300000;
    
    setInterval(function() {
        var now = Date.now();
        for (var url in thumbnailCache) {
            if (!thumbnailCache[url].data.Final && (now - thumbnailCache[url].timestamp > CACHE_DURATION)) {
                delete thumbnailCache[url];
            }
        }
    }, 60000); // Clean every minute
    
    $.fn.loadRobloxThumbnails = function() {
        function n(r) {
            var u = r.data("retry-url");
            if (!u) return;
            
            var now = Date.now();
            if (thumbnailCache[u]) {
                var cachedData = thumbnailCache[u].data;
                if (cachedData.Final || (now - thumbnailCache[u].timestamp < CACHE_DURATION)) {
                    if (cachedData.Final) {
                        var f = r.find("img");
                        if (f.length === 1) {
                            f.attr("src", cachedData.Url);
                        } else {
                            var originalImg = r.find("img.original-image");
                            if (originalImg.length > 0) {
                                originalImg.attr("src", cachedData.Url);
                            }
                        }
                        r.removeAttr("data-retry-url");
                    }
                    return;
                }
            }
            
            if (ongoingRequests[u]) {
                return;
            }
            
            ongoingRequests[u] = true;
            
            $.ajax({
                url: u,
                dataType: "json",
                cache: true,
                crossDomain: true,
                xhrFields: {
                    withCredentials: true
                },
                success: function(u) {
                    delete ongoingRequests[r.data("retry-url")];
                    thumbnailCache[r.data("retry-url")] = {
                        data: u,
                        timestamp: Date.now()
                    };
                    
                    if (u.Final) {
                        var f = r.find("img");
                        
                        if (f.length === 1) {
                            f.attr("src", u.Url);
                        } else {
                            var originalImg = r.find("img.original-image");
                            if (originalImg.length > 0) {
                                originalImg.attr("src", u.Url);
                            }
                        }
                        r.removeAttr("data-retry-url")
                    } else {
                        r.retryCount = r.retryCount ? r.retryCount + 1 : 1;
                        if (r.retryCount < i) {
                            setTimeout(function() {
                                n(r)
                            }, t)
                        }
                    }
                },
                error: function() {
                    delete ongoingRequests[r.data("retry-url")];
                }
            })
        }
        return this.each(function() {
            var i = $(this);
            setTimeout(function() {
                n(i)
            }, 100)
        })
    }
});