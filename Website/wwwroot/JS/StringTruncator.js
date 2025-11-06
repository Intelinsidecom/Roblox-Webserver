// StringTruncator.js
function InitStringTruncator() {
    isInitialized || (fitStringSpan = document.createElement("span"), fitStringSpan.style.display = "inline-block", fitStringSpan.style.visibility = "hidden", fitStringSpan.style.height = "0px", fitStringSpan.style.padding = "0px", document.body.appendChild(fitStringSpan), isInitialized = !0)
}

function fitStringToWidth(n, t, i) {
    function o(n) {
        return n.replace("<", "&lt;").replace(">", "&gt;")
    }
    var u, r, f, e, s;
    if (isInitialized || InitStringTruncator(), i && (fitStringSpan.className = i), u = o(n), fitStringSpan.innerHTML = u, fitStringSpan.offsetWidth > t) {
        for (r = 0, e = n.length; s = e - r >> 1;) f = r + s, fitStringSpan.innerHTML = o(n.substring(0, f)) + "&hellip;", fitStringSpan.offsetWidth > t ? e = f : r = f;
        u = n.substring(0, r) + "&hellip;"
    }
    return u
}

function fitStringToWidthSafe(n, t, i) {
    var r = fitStringToWidth(n, t, i),
        u;
    return r.indexOf("&hellip;") != -1 && (u = r.lastIndexOf(" "), u != -1 && u + 10 <= r.length && (r = r.substring(0, u + 2) + "&hellip;")), r
}

function fitStringToWidthSafeText(n, t, i) {
    return fitStringToWidthSafe(n, t, i).replace("&hellip;", "...")
}
var isInitialized = !1,
    fitStringSpan = null;