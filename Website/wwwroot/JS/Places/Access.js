// places/Access.js
typeof Roblox == 'undefined' &&
    (Roblox = {}),
    Roblox.PlayerAccess = function() {
        function u() {
            var n = $('#GamePlaceAccess').data('invite-list-user-limit');
            $('#GamePlaceAccess').data('permissions-service-enabled') === 'True' &&
                $('.chzn-container').length <= 0 &&
                (
                    $('.chosen').chosen({
                        no_results_text: Roblox.PlayerAccess.strings.SearchingFor,
                        max_selected_options: n,
                        disable_search: !0,
                        disable_search_threshold: 1
                    }),
                    $('.chosen').bind(
                        'liszt:maxselected',
                        function() {
                            $('#maxUserNameLimit').show()
                        }
                    ),
                    $('.chzn-choices').before(
                        '<label class="form-label">' + Roblox.PlayerAccess.strings.InviteList + ':</label>'
                    ),
                    $('#Access_Whitelist').is(':checked') ||
                    $('input[id*=visit]').filter(function() {
                        return /\d+ visit/.test(this.id)
                    }).is(':checked') ||
                    $('#customlist_chzn').hide(),
                    $('.chzn-container input').keyup(function(n) {
                        n.which !== 13 &&
                            $('#customlist-error').remove()
                    }),
                    $('#customlist').chosen().change(
                        function() {
                            var n = $('#customlist').val();
                            $('#customlist-usernames').val(n),
                                $.browser.msie &&
                                parseInt($.browser.version, 10) === 7 &&
                                (
                                    n == null ? $('.chzn-container-multi .chzn-choices').css('padding-bottom', '0') : $('.chzn-container-multi .chzn-choices').css('padding-bottom', '3px')
                                )
                        }
                    ),
                    $('.search-choice-close').click(function(n) {
                        n.stopPropagation()
                    })
                )
        }

        function t() {
            $('#PlayerLimit').show(),
                $('#NumPlayers').show(),
                $('#GamePlaceAccess').show(),
                $('#PrivateServersAccess').show();
            var n = $('#playerAccess');
            n.is(':visible') &&
                $('#GamePlaceAccess').is(':visible') &&
                (
                    Roblox.PlayerAccess.initializeChosen(),
                    $('#navbar').height(n.height())
                )
        }

        function f() {
            $('#SellGameAccessCheckbox').attr('checked') ? (
                $('#PricingPanel').show(),
                i('Robux'),
                $('#PlayerLimit').show(),
                $('#NumPlayers').show(),
                $('#PlaceTypePanel').hide(),
                $('#copyLock').hide(),
                $('#comments').hide()
            ) : (
                $('#PricingPanel').hide(),
                t(),
                $('#PlaceTypePanel').show(),
                $('#copyLock').show(),
                $('#comments').show()
            )
        }

        function e() {
            var i = $('#SellGameAccess'),
                r = i.data('minprice'),
                t = i.data('maxprice'),
                n = $('#PriceInput').val();
            $('#PricingError').hide(),
                $('#PricingErrorMax').hide(),
                isNaN(n) ? (
                    $('#PriceInput').val(n.replace(/\D/g, '')),
                    $('#PricingError').show()
                ) : n < r ? ($('#PriceInput').val(r), $('#PricingError').show()) : t !== null &&
                n > t &&
                ($('#PriceInput').val(t), $('#PricingErrorMax').show())
        }

        function o(n, t) {
            var i = $('#PrivateServerPricingError').hide();
            return !isNaN(t) &&
                !isNaN(n) &&
                t < n ? ($('#PrivateServerPriceInput').val(n), i.show(), n) : t
        }

        function i() {
            var n = $('#SellGameAccess'),
                t = n.data('minprice'),
                i = n.data('maxprice');
            t !== i &&
                e(),
                $('#MarketPlaceFee').html(h()),
                $('#Profit').html(l())
        }

        function s() {
            var i = $('#PrivateServerPriceContainer'),
                r = i.data('taxrate'),
                t = i.data('minprice'),
                n = $('#PrivateServerPriceInput').val();
            n = o(t, n),
                $('#PrivateServerMarketplaceFeeText').html(c(r, t, n)),
                $('#PrivateServerUserProfitText').html(a(r, t, n))
        }

        function h() {
            var t = $('#SellGameAccess'),
                i = t.data('minprice'),
                r = t.data('maxprice'),
                u = t.data('commisionrate'),
                f = $('#PriceInput').val();
            return n(u, i, r, f)
        }

        function c(t, i, r) {
            return n(t, i, -1, r)
        }

        function n(n, t, i, r) {
            var s = 1,
                u,
                f,
                o = s,
                e;
            return f = t !== i ? r : t,
                e = Math.round(f * n),
                u = e > o ? e : o
        }

        function l() {
            var n = $('#SellGameAccess'),
                t = n.data('minprice'),
                i = n.data('maxprice'),
                u = n.data('commisionrate'),
                f = $('#PriceInput').val();
            return r(u, t, i, f)
        }

        function a(n, t, i) {
            return r(n, t, -1, i)
        }

        function r(t, i, r, u) {
            var f;
            return f = i !== r ? u : i,
                f - n(t, i, r, u)
        }
        return {
            initializeChosen: u,
            displayGamePlace: t,
            checkSaleOptions: f,
            calculateFeeAndProfit: i,
            calculatePrivateServerFeeAndProfit: s
        }
    }(),
    $(
        function() {
            'use strict';

            function at(n) {
                a.attr('disabled', n ? !0 : null),
                    ht.toggleClass('disabled', n),
                    n ? e.show() : e.hide()
            }

            function vt(n) {
                l.attr('disabled', n ? !0 : null),
                    l.toggleClass('disabled', n),
                    n ? s.show() : s.hide()
            }

            function yt(n) {
                c.attr('disabled', n ? !0 : null),
                    pt.toggleClass('disabled', n),
                    n ? st.show() : st.hide(),
                    c.prop('checked') ? (v.hide(), ut.show(), it.is(':checked') ? r.hide() : r.show()) : (
                        Roblox.AccessData.privateServersAllowed ? v.show() : v.hide(),
                        ut.hide(),
                        r.hide()
                    )
            }

            function o() {
                var n = a.prop('checked'),
                    t = l.val() === 'Everyone',
                    i = c.prop('checked'),
                    f = !t ||
                    i,
                    r,
                    u;
                at(f),
                    r = n ||
                    i,
                    vt(r),
                    u = n ||
                    !t,
                    yt(u)
            }
            var b = $('#MaxPlayersInput'),
                t = $('#PreferredPlayersInput'),
                i = $('#FriendSlotsInput'),
                tt = $('#FriendSlotRadioButtons'),
                u = $('#CustomFriendSlotDropdown'),
                f = $('#EmptyFriendSlotError'),
                k = $('label[data-device=\'Console\'] input[type=\'checkbox\']'),
                lt = $('div[data-console-agreement-enabled]').attr('data-console-agreement-enabled') === 'True',
                ut = $('#PrivateServerDetails'),
                g = $('#PrivateServerPriceInput'),
                et = $('#PrivateServerPricingError'),
                d = $('#PrivateServerPriceChangeWarning'),
                ht = $('#SellGameAccessLabel'),
                pt = $('#PrivateServerAccessLabel'),
                e = $('.sell-access-tooltip'),
                s = $('.place-access-tooltip'),
                nt = $('.private-server-tooltip'),
                rt = $('.number-of-players-preferred-tooltip'),
                ft = $('.number-of-players-max-tooltip'),
                ot = $('.regional-availability-tooltip'),
                a = $('#SellGameAccessCheckbox'),
                l = $('#Access'),
                c = $('#AllowPrivateServersCheckbox'),
                p = $('#EmptyFriendSlots'),
                h = $('#CustomFriendSlots'),
                st = $('#PrivateServersError'),
                v = $('#PrivateServerDisableWarning'),
                wt = $('#PrivateServerRadioButtons'),
                it = $('#FreePrivateServers'),
                ct = $('#PaidPrivateServers'),
                r = $('#PrivateServerPriceContainer'),
                y,
                w,
                n;
            k.change(
                    function() {
                        lt &&
                            this.checked &&
                            (
                                k.prop('checked', !1),
                                Roblox.GenericConfirmation.open({
                                    titleText: Roblox.PlayerAccess.strings.ConsoleAccessContentAgreementTitleText,
                                    bodyContent: Roblox.PlayerAccess.strings.ConsoleAccessContentAgreementBodyContent,
                                    acceptText: Roblox.PlayerAccess.strings.ConsoleAccessContentAgreementAcceptText,
                                    declineText: Roblox.PlayerAccess.strings.ConsoleAccessContentAgreementDeclineText,
                                    acceptColor: Roblox.GenericConfirmation.gray,
                                    declineColor: Roblox.GenericConfirmation.gray,
                                    onAccept: function() {
                                        k.prop('checked', !0)
                                    },
                                    allowHtmlContentInBody: !0
                                })
                            )
                    }
                ),
                $('#Genre').change(
                    function() {
                        $('#advancedsettings_genre').text($(this).find(':selected').text())
                    }
                ),
                h.is(':checked') ? ($(u).attr('hidden', null), $(f).prop('hidden', 'hidden')) : p.is(':checked') &&
                ($(f).prop('hidden', null), $(u).prop('hidden', 'hidden'));
            $('#NumPlayers').on(
                'change',
                '#MaxPlayersInput',
                function() {
                    var e = 1,
                        r = Number(b.val()),
                        u = Number($(i).val()),
                        f = Number($(t).val());
                    for ($(i).empty(), $(t).empty(), n = e; n < r; n++) $(i).append($('<option></option>').attr('value', n).text(n));
                    for (n = e; n <= r; n++) $(t).append($('<option></option>').attr('value', n).text(n));
                    u < r &&
                        u > 0 ? i.val(u) : i.val(1),
                        f < r &&
                        f > 0 ? t.val(f) : t.val(1)
                }
            );
            if (
                $(tt).change(
                    function() {
                        h.is(':checked') ? ($(u).attr('hidden', null), $(f).prop('hidden', 'hidden')) : p.is(':checked') ? ($(f).prop('hidden', null), $(u).prop('hidden', 'hidden')) : ($(f).prop('hidden', 'hidden'), $(u).prop('hidden', 'hidden'))
                    }
                ),
                $(tt).length > 0
            ) $('#NumPlayers').on(
                'change',
                '#MaxPlayersInput, #FriendSlotsInput',
                function() {
                    var n = Number(b.val()),
                        t;
                    n == 1 ? ($(p).prop('checked', !0).change(), $(h).prop('disabled', !0)) : ($(h).prop('disabled', !1), t = Number(i.val()), t > n - 1 && i.val(n - 1))
                }
            );
            else $('#NumPlayers').on(
                'change',
                '#MaxPlayersInput, #PreferredPlayersInput',
                function() {
                    var n = Number(b.val()),
                        i = Number(t.val());
                    i > n &&
                        t.val(n)
                }
            );
            if (
                nt.length > 0 &&
                nt.tipsy(),
                rt.length > 0 &&
                rt.tipsy(),
                ft.length > 0 &&
                ft.tipsy(),
                e.length > 0 &&
                e.tipsy(),
                s.length > 0 &&
                s.tipsy(),
                ot.length > 0 &&
                ot.tipsy(),
                et.hide(),
                d.hide(),
                ct.is(':checked') &&
                Roblox.PlayerAccess.calculatePrivateServerFeeAndProfit(),
                Roblox.PlayerAccess.checkSaleOptions(),
                a.click(function() {
                    o(),
                        Roblox.PlayerAccess.checkSaleOptions()
                }),
                $(wt).change(
                    function() {
                        if (it.is(':checked')) $(r).hide(),
                            $(g).val(0),
                            $(et).hide();
                        else {
                            $(r).show();
                            var n = r.data('defaultprice');
                            $(g).val(n),
                                Roblox.PlayerAccess.calculatePrivateServerFeeAndProfit()
                        }
                        d.show()
                    }
                ),
                $('#PriceInput').change(function() {
                    Roblox.PlayerAccess.calculateFeeAndProfit()
                }),
                g.change(
                    function() {
                        d.show(),
                            Roblox.PlayerAccess.calculatePrivateServerFeeAndProfit()
                    }
                ),
                Roblox.PlayerAccess.displayGamePlace(),
                $('#GamePlaceAccess').data('permissions-service-enabled') === 'True' &&
                (
                    $('#GamePlaceAccess input').change(
                        function() {
                            $(this).is(':checked') &&
                                (
                                    $(this).is('#Access_Whitelist') ||
                                    $(this).attr('id').indexOf('visit') > -1
                                ) ? $('#customlist_chzn').show() : $('#customlist_chzn').hide()
                        }
                    ),
                    y = $('#customlist-usernames').val(),
                    y.length > 0
                )
            )
                for (w = y.split(','), n = 0; n < w.length; n++) $('#customlist').append('<option selected="selected">' + w[n] + '</option>');
            $('#playerAccessTab').one(
                'click',
                function() {
                    if (!Roblox.AccessData.isDevelopSiteForVipServersEnabled) return !1;
                    var t = $('#ActivePrivateServersSubscriptions'),
                        r = $('#ActivePrivateServersCount'),
                        n = $('#PrivateServerCountsLoading'),
                        i = $('#PrivateServerDetailsError');
                    return i.hide(),
                        $.ajax({
                            cache: !1,
                            type: 'GET',
                            url: Roblox.AccessData.vipServerConfigurationLink
                        }).done(
                            function(i) {
                                var e = i.activeSubscriptionsCount < 0 ? 'over ' + -i.activeSubscriptionsCount : i.activeSubscriptionsCount,
                                    o = 'There are currently ' + e + ' private server subscriptions active for this place.',
                                    u,
                                    f;
                                t.text(o),
                                    u = i.activeServersCount < 0 ? 'over ' + -i.activeServersCount : i.activeServersCount,
                                    f = 'There are currently ' + u + ' private servers active for this place.',
                                    r.text(f),
                                    n &&
                                    n.hide()
                            }
                        ).fail(function() {
                            t.hide(),
                                i.show(),
                                n &&
                                n.hide()
                        }),
                        !1
                }
            );
            $('#configureUniverseForm div.tab[data-id = \'accessTab\']').one(
                'click',
                function() {
                    if (!Roblox.AccessData.isDevelopSiteForVipServersEnabled) return !1;
                    $('#ActivePrivateServersCount').hide(),
                        $('#ActivePrivateServersSubscriptions').hide(),
                        $('#PrivateServerDetailsError').hide()
                }
            );
            l.change(o),
                c.click(o),
                o()
        }
    );