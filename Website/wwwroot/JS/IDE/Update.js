// IDE/Update.js
var Roblox = Roblox || {};
Roblox.IDE = Roblox.IDE || {},
    Roblox.IDE.Update = function() {
        function i() {
            return $('#navbar').data('is-update')
        }

        function f(n) {
            Roblox.Dialog.open({
                titleText: Roblox.IDE.Resources.AllowCopyingTitleText,
                bodyContent: Roblox.IDE.Resources.AllowCopyingTitleContent,
                acceptText: Roblox.IDE.Resources.AllowCopyingAcceptText,
                declineText: Roblox.IDE.Resources.AllowCopyingCancelText,
                imageUrl: '/img/Icons/img-alert.png',
                onAccept: n
            })
        }

        function e(n) {
            Roblox.Dialog.open({
                titleText: Roblox.IDE.Resources.DisableVIPServersWarningTitleText,
                bodyContent: Roblox.IDE.Resources.DisableVIPServersWarningBodyContent,
                acceptText: Roblox.IDE.Resources.DisableVIPServersWarningAcceptText,
                declineText: Roblox.IDE.Resources.DisableVIPServersWarningCancelText,
                imageUrl: '/img/Icons/img-alert.png',
                onAccept: n
            })
        }

        function r() {
            var n = $('#okButton');
            $('form#configureUniverseForm').submit(),
                n.addClass(t),
                n.prop('disabled', !0),
                o()
        }

        function n(n) {
            $('.creator-dashboard-redirection-banner').hide(),
                $('#playerAccess').hide(),
                $('#permissions').hide(),
                $('#basicSettings').hide(),
                $('#icons').hide(),
                $('#thumbnails').hide(),
                $('#versionHistory').hide(),
                $('#developerProducts').hide(),
                $('#universe').hide(),
                $('#navbar div.selected').removeClass('selected'),
                $('div.actionButtons').show();
            var t = n.attr('id');
            $('#' + t + '-redirection-banner').show(),
                n.addClass('selected')
        }

        function o(n) {
            var t = {
                overlayClose: !1,
                opacity: 80,
                overlayCss: {
                    backgroundColor: '#000'
                },
                escClose: !1
            };
            typeof n != 'undefined' &&
                n !== '' &&
                $.modal.close('.' + n),
                $('#ProcessingView').modal(t)
        }

        function s() {
            function w() {
                var n = h.prop('checked');
                return !a &&
                    n
            }

            function b() {
                var n = c.prop('checked');
                return v &&
                    !n
            }

            function l() {
                return i &&
                    b() ? e(function() {
                        r()
                    }) : r(),
                    !1
            }

            function k() {
                return o.hasClass(t) ? !1 : i &&
                    w() ? (f(function() {
                        l()
                    }), !1) : l()
            }

            function d() {
                var t = typeof localStorage != 'undefined' &&
                    (typeof Roblox.LocalStorage === 'undefined' || Roblox.LocalStorage.isAvailable()),
                    n = 'Roblox.IDE.Update.versionHistoryMessageShown';
                t &&
                    !localStorage.getItem(n) &&
                    (
                        localStorage.setItem(n, '1'),
                        Roblox.Dialog.open({
                            titleText: Roblox.IDE.Resources.VersionHistoryBehaviorTitleText,
                            bodyContent: Roblox.IDE.Resources.VersionHistoryBehaviorBodyText,
                            acceptText: Roblox.IDE.Resources.OkText,
                            showAccept: !0,
                            showDecline: !1
                        })
                    )
            }

            function s(n) {
                var t = $('#versionHistoryItems').data('asset-id');
                $('#versionHistoryItems .versionHistoryTable').html(''),
                    $('#versionHistoryLoading').show(),
                    $.ajax({
                        url: $('#versionHistoryItems').data('version-history-items-url'),
                        cache: !1,
                        data: {
                            assetID: t,
                            page: n
                        },
                        success: function(n) {
                            $('#versionHistoryLoading').hide(),
                                // Replace the entire table content to prevent header duplication
                                $('#versionHistoryItems .versionHistoryTable').html(n)
                        },
                        error: function() {
                            $('#versionHistoryLoading').hide(),
                                $('#versionHistoryError').show()
                        }
                    })
            }
            var h = $('#copyLock input'),
                a = h.prop('checked'),
                c = $('#AllowPrivateServersCheckbox'),
                v = c.prop('checked'),
                y = $('input#Name'),
                o = $('#okButton'),
                p = Roblox.IDE.validator({
                    button: o,
                    enabledClass: u,
                    disabledClass: t
                }, [{
                    input: y,
                    errorSpan: $('.description-field-container .name-error')
                }], !0);
            
            
            o.click(k),
                $(document).ready(function() {
                    $('#basicSettingsTab').click()
                }),
                $('#cancelButton').click(function() {
                    document.location = $(this).attr('href')
                }),
                $('#permissionsTab').click(function() {
                    n($(this)),
                        $('#permissions').show()
                }),
                $('#playerAccessTab').click(
                    function() {
                        var t = $(this).data('cd-redirection-link');
                        t &&
                            (window.location.href = t),
                            n($(this)),
                            $('#playerAccess').show(),
                            $('#GamePlaceAccess').is(':visible') &&
                            (
                                Roblox.PlayerAccess.initializeChosen(),
                                Roblox.PlayerAccess.checkSaleOptions()
                            )
                    }
                ),
                $('#basicSettingsTab').click(function() {
                    n($(this)),
                        $('#basicSettings').show()
                }),
                $('#iconsTab').click(
                    function() {
                        var t = $(this).data('cd-redirection-link');
                        t &&
                            (window.location.href = t),
                            n($(this)),
                            $('#icons').show()
                    }
                ),
                $('#thumbnailTab').click(
                    function() {
                        var t = $(this).data('cd-redirection-link');
                        t &&
                            (window.location.href = t),
                            n($(this)),
                            $('#thumbnails').show()
                    }
                ),
                $('#versionHistoryTab').click(
                    function() {
                        n($(this)),
                            $('#versionHistory').show(),
                            $('#versionHistoryItems').data('show-popup') === !0 &&
                            d()
                    }
                ),
                $('#developerProductsTab').click(
                    function() {
                        n($(this));
                        var t = $('#developerProducts');
                        t.show(),
                            t.attr('loaded') ? t.trigger('onRefreshed', []) : t.attr('loaded', !0),
                            t.show(),
                            $('div.actionButtons').show();
                        t.off('onViewChange').on(
                            'onViewChange',
                            function(n, t) {
                                t === 'listing' ? $('div.actionButtons').show() : $('div.actionButtons').hide()
                            }
                        )
                    }
                ),
                $('#universeTab').click(function() {
                    n($(this)),
                        $('#universe').show()
                }),
                p.init(),
                $('div.validation-summary-errors').attr('data-valmsg-summary') === 'true' &&
                (
                    $('#basicSettings').show(),
                    $('#playerAccess').hide(),
                    $('#permissions').hide(),
                    $('#navbar div.selected').removeClass('selected'),
                    $('#basicSettingsTab').addClass('selected')
                );
            $('#versionHistoryItems').on(
                'click',
                '.previous',
                function() {
                    if (this.className.indexOf('disabled') >= 0) return !1;
                    s(parseInt($('.robloxVersionHistoryPageNum').text()) - 1)
                }
            );
            $('#versionHistoryItems').on(
                'click',
                '.next',
                function() {
                    if (this.className.indexOf('disabled') >= 0) return !1;
                    s(parseInt($('.robloxVersionHistoryPageNum').text()) + 1)
                }
            );
            $('#versionHistoryItems').on(
                    'click',
                    '.revertLink',
                    function() {
                        var t = $(this).data('asset-version-id');
                        Roblox.Dialog.open({
                            titleText: Roblox.IDE.Resources.RevertTitleText,
                            bodyContent: Roblox.IDE.Resources.RevertBodyContent,
                            acceptText: Roblox.IDE.Resources.RevertAcceptText,
                            declineText: Roblox.IDE.Resources.CancelText,
                            onAccept: function() {
                                $('#versionHistoryItems').html(''),
                                    $('#versionHistoryLoading').show(),
                                    $.ajax({
                                        url: $('#versionHistoryItems').data('revert-url'),
                                        type: 'POST',
                                        cache: !1,
                                        data: {
                                            assetVersionID: t
                                        },
                                        success: function() {
                                            s(1)
                                        },
                                        error: function() {
                                            $('#versionHistoryLoading').hide(),
                                                $('#versionHistoryRevertError').show()
                                        }
                                    })
                            }
                        })
                    }
                ),
                $('#versionHistoryItems').on(
                    'click',
                    '.downloadLink',
                    function(e) {
                        e.preventDefault();
                        var $ln = $(this);
                        var url = $ln.data('download-url') ||
                            $ln.attr('href');
                        if (!url) {
                            $('#versionHistoryDownloadError').show();
                            return false;
                        }
                        fetch(url, {
                            credentials: 'same-origin'
                        }).then(
                            function(response) {
                                if (response.status === 200) {
                                    return response.blob().then(
                                        function(blob) {
                                            var disposition = response.headers.get('Content-Disposition') ||
                                                '';
                                            var filename = 'download';
                                            var m = /filename\\*=UTF-8''([^;]+)|filename="?([^";]+)"?/.exec(disposition);
                                            if (m) {
                                                filename = decodeURIComponent(m[1] || m[2] || filename);
                                            }
                                            var blobUrl = window.URL.createObjectURL(blob);
                                            var a = document.createElement('a');
                                            a.href = blobUrl;
                                            a.download = filename;
                                            document.body.appendChild(a);
                                            a.click();
                                            a.remove();
                                            window.URL.revokeObjectURL(blobUrl);
                                        }
                                    );
                                } else {
                                    $('#versionHistoryDownloadError').show();
                                }
                            }
                        ).catch(function() {
                            $('#versionHistoryDownloadError').show();
                        });
                        return false;
                    }
                );
        }
        var u = 'btn-neutral',
            t = 'btn-disabled-neutral';
        return $(s), {}
    }();