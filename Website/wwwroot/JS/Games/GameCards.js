// Games/GameCards.js
Roblox = Roblox || {}, Roblox.GameCards = Roblox.GameCards || function() {
    function f() {
        e(s, ct, ht, st, ot)
    }

    function u() {
        e(et, ft, ut, rt, y)
    }

    function c() {
        f(), u()
    }

    function h() {
        c(), lt(), at()
    }

    function r() {
        return t = t || Roblox.RealTime.Factory.GetClient()
    }

    function lt() {
        if (Roblox && Roblox.RealTime) {
            var n = r();
            n.Subscribe("GameFavoriteNotifications", function(n) {
                n && (n.Type === "Favorite" || n.Type === "Unfavorite") && u()
            })
        }
    }

    function at() {
        if (Roblox && Roblox.RealTime) {
            var n = r();
            n.Subscribe("GameCloseNotifications", function(n) {
                n && n.Type === "Close" && f()
            })
        }
    }
    var s = "#recently-visited-places",
        ct = "#recently-visited-places-content",
        ht = "#recently-visited-places-list",
        st = "#recently-visited-places-content-spinner",
        ot = Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl("/home/recently-visited-places") : "",
        et = "#my-favorites-games",
        ft = "#my-favorites-games-content",
        ut = "#my-favorites-games-list",
        rt = "#my-favorites-games-content-spinner",
        it = "#game-card-link",
        tt = "#game-card-thumb-container",
        o = "#game-card-title",
        nt = "#game-card-name-secondary",
        g = "#game-card-creator-by",
        d = ".vote-container",
        k = ".vote-background",
        b = ".vote-bar",
        w = ".vote-down-count",
        p = ".vote-up-count",
        y = Roblox.Endpoints ? Roblox.Endpoints.getAbsoluteUrl("/user/favorites/places") : "",
        v = "#game-item-card-template",
        t, n = Roblox.I18nData && Roblox.I18nData.isI18nEnabledOnGames && Roblox.Intl && new Roblox.Intl,
        a = function(t, i) {
            var e = "",
                f;
            t.HasErrorOcurred || (e = n ? n.f(i.labelPlaying, {
                playerCount: t.PlayerCount
            }) : Number(t.PlayerCount).toLocaleString() + " Playing");
            var l = Number(t.TotalUpVotes).toLocaleString(),
                a = Number(t.TotalDownVotes).toLocaleString(),
                s = "";
            t.TotalUpVotes === 0 && t.TotalDownVotes === 0 && (s = "no-votes");
            var u = t.Name.escapeHTML(),
                y = u.substring(0, Math.min(u.length, 40)),
                rt = '<img class="game-card-thumb"' + (t.UseDataSrc ? "data-" : "") + 'src="' + t.Thumbnail.Url + '"' + (t.Thumbnail.Final ? "" : "data-retry-url = ") + t.Thumbnail.RetryUrl + ' alt="' + u + "\" thumbnail='" + JSON.stringify(t.Thumbnail) + "' image-retry/>",
                ut = t.CreatorName.escapeHTML(),
                h = "<a id='creator-link-bottom' class='text-link' href='" + t.CreatorAbsoluteUrl + "'>" + ut + "</a>",
                ft = n ? n.f(i.labelCreator, {
                    creatorLink: h
                }) : "By " + h,
                r = $(v).clone(),
                c = r.find(d);
            return r.find(it).attr("href", t.GameDetailReferralUrl), r.find(tt).html(rt), r.find(o).attr("title", u), r.find(o).html(y), r.find(nt).html(e), c.attr("data-upvotes", t.TotalUpVotes), c.attr("data-downvotes", t.TotalDownVotes), r.find(k).addClass(s), r.find(w).html(a), r.find(p).html(l), r.find(g).html(ft), Roblox.Voting && (f = r.find(b), f.attr("data-voting-processed", !0), Roblox.Voting.UpdateVoteBar(t.TotalUpVotes, t.TotalDownVotes, f)), r.html()
        },
        l = function(n) {
            var t = '<ul class="hlist game-cards ' + (n.ShowSmallGameIcon ? "game-cards-sm" : "") + '">',
                r = {
                    labelCreator: n.LabelCreatorByJs,
                    labelPlaying: n.LabelPlayingPhraseJs
                };
            for (i in n.GameDisplayModels) t += a(n.GameDisplayModels[i], r);
            return t += "</ul>"
        },
        e = function(n, t, i, r, u) {
            $(t).addClass("hidden"), $(r).removeClass("hidden"), $(i).addClass("game-card-list");
            var f = function(u) {
                if (u.data && (u.Data = u.data), !u || !u.Data || !u.Data.GameDisplayModels || u.Data.GameDisplayModels.length <= 0) {
                    $(r).addClass("hidden"), $(n).addClass("hidden"), $(t).html("");
                    return
                }
                var o = l(u.Data);
                $(r).hide(), $(i).removeClass("game-card-list"), $(t).removeClass("hidden").html(o)
            };
            $.getJSON(u, f)
        };
    $(document).ready(h)
}();