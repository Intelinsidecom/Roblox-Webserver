// Tracking/FormEvents.js
typeof Freebloxia == "undefined" && (Freebloxia = {}), typeof Freebloxia.FormEvents == "undefined" && (Freebloxia.FormEvents = function() {
    function n(n, t, i) {
        Freebloxia.EventStream && Freebloxia.EventStream.SendEvent(n, t, i)
    }

    function t(t, i, r, u) {
        var f = {
            msg: u,
            input: r,
            field: i,
            vis: !0
        };
        n("formValidation", t, f)
    }

    function i(t, i) {
        var r = {
            aType: "focus",
            field: i
        };
        n("formInteraction", t, r)
    }

    function r(t, i) {
        var r = {
            aType: "offFocus",
            field: i
        };
        n("formInteraction", t, r)
    }

    function u(t, i) {
        var r = {
            aType: "click",
            field: i
        };
        n("formInteraction", t, r)
    }
    return {
        SendValidationFailed: t,
        SendInteractionFocus: i,
        SendInteractionOffFocus: r,
        SendInteractionClick: u
    }
}());