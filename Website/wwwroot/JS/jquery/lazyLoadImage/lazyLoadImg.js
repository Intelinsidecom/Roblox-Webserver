// jquery/lazyLoadImage/lazyLoadImg.js
"use strict";
var Roblox = Roblox || {};
Roblox.LazyLoadImg = Roblox.LazyLoadImg || {}, Roblox.LazyLoadImg.init = function(n, t) {
    var i = (t ? t.selector + " " : "") + "img.lazy";
    $(i).lazyload({
        skip_invisible: !0,
        placeholder: "",
        loading_class: "thumbnail-placeholder",
        threshold: n,
        container: t
    })
}, Roblox.LazyLoadImg.scrollWhenTabsAreClicked = function() {
    $(".rbx-tab").on("shown.bs.tab", function() {
        $(document).trigger("scroll")
    })
}, Roblox.LazyLoadImg.updateImageScroll = function(n) {
    n ? n.trigger("scroll") : $(document).trigger("scroll")
}, $(function() {
    Roblox.LazyLoadImg.init(), Roblox.LazyLoadImg.scrollWhenTabsAreClicked()
});