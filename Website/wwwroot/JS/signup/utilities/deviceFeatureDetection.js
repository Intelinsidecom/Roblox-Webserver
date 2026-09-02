// utilities/deviceFeatureDetection.js
Freebloxia = Freebloxia || {}, Freebloxia.DeviceFeatureDetection = function() {
    function i() {
        !t.hasClass("in-studio") && ("ontouchstart" in window || navigator.maxTouchPoints > 0 || navigator.msMaxTouchPoints > 0) && (n = !0, t.addClass("touch"))
    }
    var n = !1,
        t = $(".container-main");
    return i(), {
        isTouch: n
    }
}();