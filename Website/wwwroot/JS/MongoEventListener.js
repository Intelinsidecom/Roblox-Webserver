// MongoEventListener.js
MongoListener = new RBXBaseEventListener, MongoListener.handleEvent = function(n, t) {
    var i;
    return i = typeof t.shard == "undefined" ? this.defaultShard : t.shard, typeof t.eventName == "undefined" && (t.eventName = n.type), t.eventName === "JavascriptExceptionLoggingEvent" ? dataMap = {
        category: "category",
        url: "url",
        msg: "msg",
        line: "line",
        ua: "UA"
    } : (typeof t.category == "undefined" && (t.category = MongoListener.getCategoryFromEventName(t.eventName)), t.userId = MongoListener.userId, t.ip = MongoListener.clientIpAddress, dataMap = {
        guid: "guid",
        category: "category",
        userId: "userid",
        ip: "ip"
    }), this.fireEvent(i, this.distillData(t, dataMap)), !0
}, MongoListener.getCategoryFromEventName = function(n) {
    switch (n) {
        case "rbx_evt_sitetouch":
            return "SiteTouch";
        case "rbx_evt_fmp":
            return "FiveMinutePlay";
        case "rbx_evt_play_user":
        case "rbx_evt_play_guest":
            return "Play";
        default:
            return n
    }
}, MongoListener.distillData = function(n, t) {
    var i = {};
    for (dataKey in t) typeof n[dataKey] != typeof undefined && (i[t[dataKey]] = encodeURIComponent(n[dataKey]));
    return i
}, MongoListener.fireEvent = function(n, t) {
    $.ajax({
        url: MongoListener.loggingURI + n,
        dataType: "jsonp",
        jsonpCallback: "MongoListener.callback",
        data: t
    })
}, MongoListener.callback = function() {}, MongoListener.events = ["JavascriptExceptionLoggingEvent"];