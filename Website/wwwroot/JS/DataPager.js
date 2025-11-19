// DataPager.js
function DataPager(n, t, i, r, u, f, e, o) {
    function b() {
        s < 1 && (s = 1), s > c && (s = c), h.find(".disabled").removeClass("disabled"), a.Paging_PageNumbers_AreLinks ? h.find(":contains('" + s + "')").addClass("disabled") : h.find(".CurrentPage").text(s), s == 1 && (h.find('[pagingaction="First"]').addClass("disabled"), h.find('[pagingaction="Previous"]').addClass("disabled"), h.find('[pagingaction="Next"]').removeClass("disabled"), h.find('[pagingaction="Last"]').removeClass("disabled")), s == c && (h.find('[pagingaction="First"]').removeClass("disabled"), h.find('[pagingaction="Previous"]').removeClass("disabled"), h.find('[pagingaction="Next"]').addClass("disabled"), h.find('[pagingaction="Last"]').addClass("disabled"))
    }

    function g() {
        if (elem = $(this), !elem.hasClass("disabled") && elem.attr("pagingaction") !== undefined) {
            s = parseInt(s, 10);
            switch (elem.attr("pagingaction")) {
                case "First":
                    s = 1;
                    break;
                case "Previous":
                    s -= 1;
                    break;
                case "Next":
                    s += 1;
                    break;
                case "Last":
                    s = c;
                    break;
                default:
                    s = elem.html()
            }
            s < 1 && (s = 1), s > c && (s = c), a.pagingClickFunction ? a.pagingClickFunction(s) : (l.html('<div class="loading"></div>'), v((s - 1) * t + 1, t))
        }
    }

    function p() {
        var i, r;
        if (n <= t) {
            h.html("");
            return
        }
        if (i = $("<span></span>").append('<a class="disabled pager first" pagingaction="First">First</a><a class="disabled pager previous" pagingaction="Previous">Previous</a>'), a.Paging_PageNumbers_AreLinks) {
            for (i.append('<a class="disabled pager page text" pagingaction="PageNum">1</a>'), r = t; r < n; r += t) i.append('<a pagingaction="PageNum">' + (r / t + 1) + "</a>");
            i.append('<a class="pager next" pagingaction="Next">Next</a><a class="pager last" pagingaction="Last">Last</a>')
        } else i = $("<span></span>").append('<a class="disabled pager first" pagingaction="First"></a><a class="disabled pager previous" pagingaction="Previous"></a>'), i.append('<span class="page text">Page <span class="CurrentPage">1</span> of <span class="TotalPages">' + c + "</span></span>"), i.append('<a class="pager next" pagingaction="Next"></a><a class="pager last" pagingaction="Last"></a>');
        i.children().each(function() {
            $(this).bind("click", g)
        }), h.html($('<div class="JSPager_Container"></div>').append(i))
    }
    var y;
    if (!(this instanceof DataPager)) return new DataPager(n, containerDivID, u, f);
    var n = n,
        t = t,
        s = 1,
        c = Math.ceil(n / t),
        l = $("#" + i),
        h = $("#" + r),
        v = u,
        w = f,
        d = e,
        a = {
            Paging_PageNumbers_AreLinks: !0,
            FetchItemsOnLoad: !0,
            pagingClickFunction: !1
        };
    for (y in o) a[y] = o[y];
    var nt = function(n) {
            l.html("");
            var t = n;
            l.append(t)
        },
        tt = function(i, r) {
            r != undefined && (s = r), n = i, (c != Math.ceil(n / t) || n == t || n == 0) && (c = Math.ceil(n / t), p()), b()
        },
        k = function(i) {
            var r, u;
            if (l.html(""), r = i.data, r == null || r.length == 0) {
                l.append(w(r));
                return
            }
            for (u = 0; u < r.length; u++) l.append(w(r[u])), d(r[u]);
            l.append('<div style="clear:both;"></div>'), n = i.totalItems, c != Math.ceil(n / t) && (c = Math.ceil(n / t), p()), b()
        };
    return function() {
        a.FetchItemsOnLoad && v(1, t), n > t && p()
    }(), {
        getItemsPaged: v,
        update: k,
        updateHtml: nt,
        callUpdatePager: tt,
        totalItems: n,
        itemContainerDiv: l,
        getStartIndex: function() {
            return (s - 1) * t + 1
        },
        getCurrentPage: function() {
            return s
        },
        isLastPage: function() {
            return c == s
        }
    }
}