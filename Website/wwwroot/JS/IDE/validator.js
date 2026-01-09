// IDE/validator.js
var Roblox = Roblox || {};
Roblox.IDE = Roblox.IDE || {},
    Roblox.IDE.validator = function() {
        function r(n, t, i) {
            n.toggleClass('field-validation-valid', !t),
                n.toggleClass('field-validation-error', !!t),
                t ? (
                    n.text(t),
                    i &&
                    Roblox.StyleguideConversions.convertMvcErrorToStyleGuide()
                ) : n.html('').removeClass('tool-tip')
        }

        function u(r) {
            var f,
                e,
                u,
                o;
            if (r.attr('type') === 'file') {
                if (r[0].files.length <= 0) return 'You must select a file.';
                if (r.attr('accept'))
                    for (
                        f = r.attr('accept').split('/')[0],
                        e = new RegExp(r.attr('accept').replace('*', '.*')),
                        u = 0; u < r[0].files.length; u++
                    )
                        if (!e.test(r[0].files[u].type)) return 'File type must be ' + f
            } else {
                if (r.data(n) && !r.val().trim()) return r.data(n);
                if (r.data(t) && (o = new RegExp(r.data(t)), !o.test(r.val()))) return r.data(i)
            }
            return ''
        }
        var n = 'val-required',
            t = 'val-regex-pattern',
            i = 'val-regex';
        return function(n, t, i) {
            function f(f) {
                var e = !0;
                return t.forEach(
                        function(n) {
                            var t = u(n.input);
                            t &&
                                (e = !1),
                                (f || !t) &&
                                r(n.errorSpan, t, i)
                        }
                    ),
                    e ? n.button.removeAttr('disabled') : n.button.attr('disabled', 'disabled'),
                    n.button.toggleClass(n.enabledClass, e).toggleClass(n.disabledClass, !e).prop('disabled', !e),
                    e
            }

            function e() {
                t.forEach(
                        function(n) {
                            if (n.input.attr('type') === 'file') n.input.change(function() {
                                f(!0)
                            });
                            else n.input.on('blur keyup', function() {
                                f(!0)
                            })
                        }
                    ),
                    f(!1)
            }
            return {
                init: e,
                validateInputs: f
            }
        }
    }();