// utilities/jquery.history.override.js
$.History.extractHash = function() {
    return function(n) {
        return n.replace(/^[^#!\/]*#/, "").replace(/^#+|#+$/, "")
    }
}();