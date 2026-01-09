// Places/DeviceSelection.js
$(
    function() {
        'use strict';
        var n = $('#device-type-error').hide(),
            t = $('div.deviceTypeSection input:checkbox');
        t.on(
            'change',
            function() {
                var r,
                    i;
                if (this.checked) n.hide();
                else {
                    for (r = 0, i = 0; i < t.length; i++) t[i].checked &&
                        (r += 1);
                    r < 1 ? (n.show(), this.checked = !0) : n.hide()
                }
            }
        )
    }
);