// StyleguideConversions.js
typeof Roblox == 'undefined' &&
    (Roblox = {}),
    typeof Roblox.StyleguideConversions == 'undefined' &&
    (
        Roblox.StyleguideConversions = function() {
            function n() {
                $('.field-validation-error').length > 0 &&
                    $('.field-validation-error').each(
                        function(n, t) {
                            var i = $(t),
                                r;
                            i.addClass('tool-tip'),
                                r = i.text(),
                                i.html('<img src="/images/UI/img-tail-left.png" class="right">' + r)
                        }
                    )
            }
            return {
                convertMvcErrorToStyleGuide: n
            }
        }()
    );