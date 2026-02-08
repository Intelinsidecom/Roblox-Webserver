// jquery/owlCarousel/owlCarousel.js
"use strict";
var Roblox = Roblox || {};
Roblox.OwlCarousel = Roblox.OwlCarousel || {}, Roblox.OwlCarousel.init = function() {
    $(".owl-carousel").owlCarousel({
        items: 1,
        margin: 50,
        loop: !0,
        nav: !0,
        navText: ['<span class="icon-games-carousel-left"></span>', '<span class="icon-games-carousel-right"></span>'],
        navSpeed: 1e3,
        dotsSpeed: 1e3,
        mouseDrag: !1
    })
}, $(function() {
    Roblox.OwlCarousel.init()
});