// GoogleEventListener.js
GoogleListener = new RBXBaseEventListener, GoogleListener.handleEvent = function(n, t) {
    function r(n) {
        return n = n.toLowerCase(), n == "win32" ? n = "Windows" : n == "osx" && (n = "Mac"), n
    }
    var f, u, i;
    switch (n.type) {
        case "rbx_evt_initial_install_begin":
            t.os = r(t.os), t.category = "Bootstrapper Install Begin", i = {
                os: "action"
            };
            break;
        case "rbx_evt_ftp":
            t.os = r(t.os), t.category = "First Time Played", i = {
                os: "action"
            };
            break;
        case "rbx_evt_initial_install_success":
            t.os = r(t.os), t.category = "Bootstrapper Install Success", i = {
                os: "action"
            };
            break;
        case "rbx_evt_fmp":
            t.os = r(t.os), t.category = "Five Minute Play", i = {
                os: "action"
            };
            break;
        case "rbx_evt_abtest":
            i = {
                experiment: "category",
                variation: "action",
                version: "opt_label"
            };
            break;
        case "rbx_evt_card_redemption":
            t.category = "CardRedemption", i = {
                merchant: "action",
                cardValue: "opt_label"
            };
            break;
        default:
            return console.log("GoogleListener - Event registered without handling instructions: " + n.type), !1
    }
    return i.category = "category", u = this.distillData(t, i), this.fireEvent(u), !0
}, GoogleListener.distillData = function(n, t) {
    var i = {},
        r;
    for (dataKey in t) typeof n[dataKey] != typeof undefined && (i[t[dataKey]] = n[dataKey]);
    return r = [i.category, i.action], i.opt_label != null && (r = r.concat(i.opt_label)), i.opt_value != null && (r = r.concat(i.opt_value)), r
}, GoogleListener.fireEvent = function(n) {
    if (typeof _gaq != typeof undefined) {
        var t = ["_trackEvent"],
            i = ["b._trackEvent"];
        _gaq.push(t.concat(n)), _gaq.push(i.concat(n))
    }
}, GoogleListener.events = ["rbx_evt_initial_install_begin", "rbx_evt_ftp", "rbx_evt_initial_install_success", "rbx_evt_fmp", "rbx_evt_abtest", "rbx_evt_card_redemption"];