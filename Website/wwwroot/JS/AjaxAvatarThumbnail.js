// AjaxAvatarThumbnail.js
var RobloxThumbs = function() {
    function n(t, i, r) {
        $.get("/thumbs/rawavatar.ashx", {
            UserID: i,
            ThumbnailFormatID: r
        }, function(u) {
            u == "PENDING" ? window.setTimeout(function() {
                n(t, i, r)
            }, 3e3) : u.substring(5, 0) == "ERROR" || $("#" + t).attr("src", u)
        })
    }
    return {
        GenerateAvatarThumb: function(t, i, r) {
            $("#" + t).attr("src", "/images/spinners/waiting.gif"), n(t, i, r)
        }
    }
}();