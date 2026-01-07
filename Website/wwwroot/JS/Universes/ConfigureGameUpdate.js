// Universes/ConfigureGameUpdate.js
$(function() {
    var t = $(".configure-game-update"),
        n = {
            inputSection: null,
            historySection: null,
            previewSection: null,
            sendConfirmSection: null
        },
        u = t.data("publish-endpoint-url"),
        f = t.data("universe-id"),
        r = {
            year: "numeric",
            month: "short",
            day: "numeric"
        },
        i = Roblox && Roblox.Intl && new Roblox.Intl;
    n.inputSection = function() {
        function l() {
            var t;
            b.text(""), v ? (n.removeAttr("disabled"), w.removeAttr("disabled"), p.removeAttr("disabled")) : (n.attr("disabled", "true"), w.attr("disabled", "true"), p.attr("disabled", "true"), o && (t = i ? i.d(o, r) : o, b.text(" Your last game update for " + tt + " was sent on " + t + ".")))
        }

        function a() {
            var i = n.val().length,
                t;
            t = i === 0 ? c + " character limit." : s ? c + " character maximum." : i + "/" + c, k.text(t), n.toggleClass("limit-reached", s), k.toggleClass("limit-reached", s)
        }

        function d() {
            var t = n.val(),
                i = e.data("text-filter-url");
            return $.ajax({
                url: i,
                method: "POST",
                data: JSON.stringify(t),
                contentType: "application/json; charset=utf-8",
                dataType: "json"
            })
        }
        var u = {
                onPreview: null,
                onSend: null,
                sendComplete: null,
                sendError: null
            },
            e = t.find(".game-update-input-container"),
            n = e.find(".game-update-input-text"),
            w = e.find(".game-update-button.preview-button"),
            p = e.find(".game-update-button.send-button"),
            c = n.attr("maxlength"),
            k = e.find(".game-update-content-length"),
            it = !1,
            s = !1,
            tt = t.data("universe-name"),
            v = e.data("enabled"),
            o = e.data("last-sent-date"),
            b = e.find(".game-update-messages .last-sent"),
            f = t.find(".game-update-sent-message"),
            y = f.find(".message"),
            g = f.find(".close-icon"),
            h = 200,
            nt = 3e3;
        o && (o = new Date(o)), u.sendInProgress = function() {
            v = !1, l(), y.text("Sending..."), f.hide().removeClass("error success").addClass("sending").slideDown(h)
        }, u.sendComplete = function(t) {
            o = t, v = !1, l(), n.val(""), a(), y.text("Sent"), f.hide().removeClass("error sending").addClass("success").slideDown(h), setTimeout(function() {
                f.slideUp(h)
            }, nt)
        }, u.sendError = function(n) {
            v = !0, l(), y.text(n), f.hide().removeClass("sending success").addClass("error").slideDown(h)
        };
        n.on("keyup", function() {
            var t = n.val();
            t.length < c && (s = !1), a()
        });
        n.on("keypress paste", function(t) {
            if (t.charCode) {
                var i = n.val();
                !s && i.length >= c && (s = !0, a())
            }
        });
        n.on("blur", function() {
            s = !1, a()
        });
        return w.click(function() {
            var t = n.val().trim();
            f.hide(), t.length > 0 && u.onPreview && d().done(function(n) {
                u.onPreview(n.filteredGameUpdateText, n.isFiltered)
            }).fail(function() {
                u.sendError("Text filter is not available. Please try again later.")
            })
        }), p.click(function() {
            var t = n.val().trim(),
                i;
            f.hide(), t.length > 0 && u.onSend && d().done(function(n) {
                u.onSend(n.filteredGameUpdateText, n.isFiltered)
            }).fail(function() {
                u.sendError("Text filter is not available. Please try again later.")
            })
        }), g.click(function() {
            f.slideUp(h)
        }), l(), u
    }(), n.historySection = function() {
        function nt() {
            return $.ajax({
                url: d,
                method: "GET",
                contentType: "application/json; charset=utf-8",
                dataType: "json"
            })
        }

        function tt(n) {
            for (var i = [], t = n - 1; t < n - 1 + l && t < f.length; t++) i.push(f[t]);
            return i
        }

        function a(n) {
            var r = n.impressions,
                t, i;
            return (isNaN(n.impressions) || n.impressions < 0) && (r = "-"), t = isNaN(n.impressions) || isNaN(n.plays) || n.plays <= 0 || n.impressions <= 0 ? "-" : (n.plays / n.impressions * 100).toFixed(1) + "%", i = isNaN(n.impressions) || isNaN(n.unfollows) || n.unfollows <= 0 || n.impressions <= 0 ? "-" : (n.unfollows / n.impressions * 100).toFixed(1) + "%", {
                sentDate: new Date(n.createdOn),
                sender: n.creatorName,
                senderId: n.creatorId,
                content: n.content,
                views: r,
                playRate: t,
                unfollowRate: i
            }
        }

        function e() {
            var rt = v.find("tbody"),
                nt, e, a, ut, p, t, ft, d, et, ot;
            if (g.hide(), f && f.length > 0) {
                for (nt = f.length, u.show(), y.hide(), e = Math.ceil(nt / l), n < 1 && (n = e), a = (n - 1) * l + 1, rt.empty(), p = tt(a), ut = a + p.length - 1, d = 0; d < p.length; d++) t = p[d], et = i ? i.d(t.sentDate, r) : t.sentDate, ot = i ? i.d(t.sentDate, "time") : "", ft = b.replace("{date}", et).replace("{time}", ot).replace("{sender}", t.sender).replace("{profile-url}", "href='" + Roblox.Endpoints.getAbsoluteUrl(k.replace("{user-id}", t.senderId) + "'")).replace("{vws}", t.views).replace("{pr}", t.playRate).replace("{ur}", t.unfollowRate).replace("{content}", t.content.escapeHTML()), rt.append(ft);
                it.text(a + " - " + ut + " of " + nt + " Results"), w.text(n + " of " + e), h.removeAttr("disabled"), c.removeAttr("disabled"), s.removeAttr("disabled"), o.removeAttr("disabled"), n <= 1 && (h.attr("disabled", !0), s.attr("disabled", !0)), n >= e && (c.attr("disabled", !0), o.attr("disabled", !0))
            } else u.hide(), y.show()
        }
        var p = {
                sendComplete: null
            },
            g = t.find(".history-spinner"),
            u = t.find(".game-update-history-container"),
            y = t.find(".no-history-data-message"),
            v = u.find(".game-update-history-table"),
            n = 1,
            l = u.data("page-size"),
            it = u.find(".game-update-history-section-header .result-index"),
            w = u.find(".game-update-history-pager .pager-index"),
            h = u.find(".game-update-history-pager .pager-first"),
            c = u.find(".game-update-history-pager .pager-last"),
            s = u.find(".game-update-history-pager .pager-prev"),
            o = u.find(".game-update-history-pager .pager-next"),
            b = "<tr>" + v.find("tr.history-row-template").html() + "</tr>",
            k = "/users/{user-id}/profile",
            d = u.data("history-data-endpoint-url"),
            f;
        return h.click(function() {
            n = 1, e()
        }), s.click(function() {
            n = n - 1, e()
        }), o.click(function() {
            n = n + 1, e()
        }), c.click(function() {
            n = -1, e()
        }), p.sendComplete = function(t) {
            f.unshift(a(t)), n = 1, e()
        }, nt().done(function(n) {
            var t;
            for (f = [], t = 0; t < n.length; t++) f.push(a(n[t]));
            e()
        }).fail(function() {
            f = [], e()
        }), p
    }(), n.previewSection = function() {
        var n = {
                setContentAndDetectOverflow: null,
                preview: null,
                onSend: null
            },
            i = t.find(".game-update-preview-container"),
            r = i.find(".game-update-preview-panel"),
            u = r.find(".device"),
            f = r.find(".filtered-error"),
            e = r.find(".truncated-warning"),
            o = r.find(".game-icon");
        return n.setContentAndDetectOverflow = function(n, t) {
            var r = !1;
            return (f.hide(), e.hide(), u.each(function() {
                var t = $(this),
                    i = t.find(".preview-text .update-message");
                i.text(n)
            }), t) ? (f.show(), !1) : (i.css.visibility = "hidden", i.show(), u.each(function() {
                var i = $(this),
                    n = i.find(".preview-text"),
                    t;
                t = n.prop("scrollWidth") > n.width() || n.prop("scrollHeight") > n.height(), i.toggleClass("overflow", t), r = r || t
            }), r && e.show(), i.hide(), r)
        }, n.preview = function(t, r) {
            if (t && t.length !== 0) {
                var u = n.setContentAndDetectOverflow(t, r);
                Roblox.Dialog.open({
                    titleText: "Preview",
                    bodyContent: i.html(),
                    showAccept: !r,
                    acceptText: "Send",
                    showDecline: !r,
                    declineText: "Cancel",
                    allowHtmlContentInBody: !0,
                    cssClass: "game-update-preview-modal",
                    xToCancel: !0,
                    onAccept: function() {
                        if (n.onSend) n.onSend(t, r, u)
                    }
                })
            }
        }, o.loadRobloxThumbnails(), n
    }(), n.sendConfirmSection = function() {
        var n = {
                showDialog: null,
                onPreview: null,
                onConfirm: null
            },
            t = $(".game-update-send-confirm-container");
        return n.showDialog = function(i, r, u) {
            var f = t.find(".filtered-confirm"),
                e = t.find(".send-confirm");
            Roblox.Dialog.open({
                bodyContent: r ? f.html() : e.html(),
                acceptText: r ? "OK" : "Send",
                showDecline: !r,
                declineText: "Cancel",
                allowHtmlContentInBody: !0,
                cssClass: "game-update-send-confirm-modal",
                xToCancel: !0,
                onOpenCallback: function() {
                    var t = $(".game-update-send-confirm-modal"),
                        f = t.find(".truncated-warning");
                    !r && u && f.show(), (r || u) && t.find(".preview-link").click(function() {
                        if (Roblox.Dialog.close(), n.onPreview) n.onPreview(i, r)
                    })
                },
                onAccept: function() {
                    if (!r && n.onConfirm) n.onConfirm(i, r)
                }
            })
        }, n
    }(), n.inputSection.onPreview = function(t, i) {
        n.previewSection.preview(t, i)
    }, n.inputSection.onSend = function(t, i) {
        var r = !1;
        i || (r = n.previewSection.setContentAndDetectOverflow(t, i)), n.sendConfirmSection.showDialog(t, i, r)
    }, n.previewSection.onSend = function(t, i, r) {
        n.sendConfirmSection.showDialog(t, i, r)
    }, n.sendConfirmSection.onPreview = function(t, i) {
        n.previewSection.preview(t, i)
    }, n.sendConfirmSection.onConfirm = function(t, i) {
        t && t.length !== 0 && !i && $.ajax({
            url: u,
            method: "POST",
            data: JSON.stringify(t),
            contentType: "application/json; charset=utf-8",
            dataType: "json"
        }).done(function(t) {
            n.historySection.sendComplete(t), n.inputSection.sendComplete(new Date(t.createdOn))
        }).fail(function(t) {
            var i = t && t.length > 0 ? t[0].message : "Error sending game update. Please try again later.";
            n.inputSection.sendError(i)
        })
    }
});