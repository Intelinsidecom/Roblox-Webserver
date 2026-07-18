// flot/FlotGraphing.js
var Roblox = Roblox || {};
Roblox.FlotGraphing = Roblox.FlotGraphing || function() {
    function t(n, t, i, r, u) {
        var f = new Roblox.FlotChart(t, u);
        f.setUrl(n), f.drawChartFromEndpoint(i, r)
    }

    function i(n, t, i) {
        var r = new Roblox.FlotChart(t, i);
        r.drawChartFromData(n)
    }

    function r(t, i) {
        var u = new Date(t),
            s = u.getMonth() + 1,
            h = u.getDate(),
            e = u.getFullYear(),
            r, f, o;
        return i === "daily" ? u.getUTCMonth() + 1 + "/" + u.getUTCDate() + "/" + e : i === "monthly" ? n[u.getUTCMonth()] + " " + e : (r = u.getHours(), f = u.getMinutes(), r > 11 ? (o = "PM", r = r === 12 ? r : r - 12) : (o = "AM", r = r === 0 ? 12 : r), f = f < 10 ? "0" + f.toString() : f.toString(), s + "/" + h + "/" + e + " " + r + ":" + f + " " + o)
    }
    var n = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    return {
        DrawChartFromEndpoint: t,
        DrawChartFromData: i,
        ConvertTimeToReadableString: r
    }
}();