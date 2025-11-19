// jquery.filter_input.js
(function(n) {
    n.fn.extend({
        filter_input: function(t) {
            function i(t) {
                var i = t.charCode ? t.charCode : t.keyCode ? t.keyCode : 0,
                    r;
                return (i == 8 || i == 9 || i == 13 || i == 35 || i == 36 || i == 37 || i == 39 || i == 46) && n.browser.mozilla && t.charCode == 0 && t.keyCode == i ? !0 : (r = String.fromCharCode(i), u.test(r)) ? !0 : !1
            }
            var r = {
                    regex: ".*",
                    live: !1
                },
                t = n.extend(r, t),
                u = new RegExp(t.regex);
            if (t.live) n(this).live("keypress", i);
            else return this.each(function() {
                var t = n(this);
                t.unbind("keypress").keypress(i)
            })
        }
    })
})(jQuery);