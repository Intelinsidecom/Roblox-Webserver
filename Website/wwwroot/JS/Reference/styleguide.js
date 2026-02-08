// Reference/styleguide.js
function listUnappliedRules() {
    for (var i = document.styleSheets[0].rules || document.styleSheets[0].cssRules, r = [], n, t = 0; t < i.length; t++) n = i[t], typeof n.selectorText != "undefined" && $(n.selectorText).length == 0 && n.selectorText.indexOf(":hover") == -1 && n.selectorText.indexOf(":active") == -1 && n.selectorText.indexOf(":focus") == -1 && r.push(n.selectorText);
    return r
}

function buildTableOfContents() {
    var n;
    $(".doc-heading").each(function(t, i) {
        var r, u, f;
        $(i).parent().hasClass("sg-section") || (r = $(this).text().toLowerCase().replace(/ /g, "-"), $(this).attr("id", r), i.tagName === "H3" ? (n = $("#side-nav"), u = $("<li class='menu-item'><a id='sg-link-" + r + "' href='#" + r + "'>" + $(this).text() + "</a></li>"), f = $("<ul class='sub-menu'></ul>"), u.append(f), n.append(u), n = f) : n.append("<li class='sub-menu-item'><a class='small' href='#" + r + "'>" + $(this).text() + "</a></li>"))
    })
}

function setupScrollSpy() {
    if (typeof $.fn.scrollspy !== 'undefined') {
        $("body").scrollspy({
            target: "#side-nav"
        })
    }
}

function init() {
    var n = $("#menu-btn"),
        t = $("#side-nav-container");
    n.bind("click touchstart", function() {
        return t.toggleClass("expanded"), !1
    })
}
buildDocumentationHTML = function() {
    function n(n, t, i, r) {
        var u;
        return n === "" ? u = r.append(document.createTextNode(i)) : (u = document.createElement(n), u.className = t, u.textContent = i, r.append(u)), $(u)
    }

    function i(t, i, r) {
        n("span", "attr-key", " " + t + "=", r), n("span", "attr-value", '"' + i + '"', r)
    }

    function r(t, i, r) {
        n("span", "scss-key", " " + t + ":", r), n("span", "scss-value", "" + i + ";", r)
    }

    function u(t) {
        var r = t.siblings(".angular-widget"),
            i, u;
        return r.length > 0 && (i = r.detach(), t.prepend(i), i = null), n("hr", "", "", t), n("div", "hint-text", "Code", t), u = n("pre", "code-piece", "", t), n("code", "html", "", u)
    }

    function t(u, f, e) {
        var o = $(f),
            c = $(u),
            s = c[0].tagName.toLowerCase(),
            v = c[0].attributes,
            h = $(u).clone().children().remove().end().text().replace(/[ \t\r\n]+/g, " "),
            l, a;
        for (h === " " ? h = "" : h.length > 80 && (h = h.slice(0, 80) + "..."), l = "", a = 1; a < e; a++) l += "  ";
        if (n("", "", l, o), s == "style") n("span", "scss-comment", "/* Usage:" + h + " */", o), n("br", "", "", o), n("span", "scss-class", ".my-class", o), n("span", "", "{", o), n("br", "", "", o), $.each(v, function(t, i) {
            r(i.name, i.value, o), n("br", "", "", o)
        }), n("span", "", "}", o), n("br", "", "", o);
        else {
            if (n("span", "tag", "<" + s, o), $.each(v, function(n, t) {
                    i(t.name, t.value, o)
                }), s == "img" || s == "br" || s == "input" || s == "hr") {
                n("span", "tag", " />", o), n("br", "", "", o);
                return
            }
            n("span", "tag", ">", o), n("", "", h, o), c.children().length > 0 && n("br", "", "", o), c.children().each(function(n, i) {
                t(i, o, e + 1)
            }), c.children().length > 0 && n("", "", l, o), n("span", "tag", "</" + s + ">", o), n("br", "", "", o)
        }
    }

    function f(n) {
        var i = $(n),
            r, f;
        i.length != 0 && i.attr("id") != "hidden" && (r = i.children(), f = u(i), r.each(function(n, i) {
            t(i, f, 1)
        }))
    }
    return function() {
        $(".sg-section").each(function() {
            f(this)
        })
    }
}(), $(function() {
    buildDocumentationHTML(), buildTableOfContents(), setupScrollSpy(), init()
});