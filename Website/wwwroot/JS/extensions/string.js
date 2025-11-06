// extensions/string.js
$.extend(String.prototype, function() {
    function n() {
        return this.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#39;")
    }
    return {
        escapeHTML: n
    }
}());