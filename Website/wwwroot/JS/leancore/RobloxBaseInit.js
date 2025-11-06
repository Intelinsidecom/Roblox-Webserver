// leancore/RobloxBaseInit.js
"modal" in $.fn && "noConflict" in $.fn.modal && ($.fn.bootstrapModal = $.fn.modal.noConflict()), $(function() {
    $(".ie8 input[type=password]").attr("placeholder", "Password");
    if ($.fn && $.fn.placeholder) {
        $("input, textarea").placeholder();
    }
})
;