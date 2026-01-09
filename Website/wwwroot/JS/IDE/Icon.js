// IDE/Icon.js
Roblox = Roblox || {},
    Roblox.Icons = Roblox.Icons || {},
    Roblox.Resources = Roblox.Resources || {},
    Roblox.Resources.Icons = {
        AddtitleText: 'Add Icon',
        AddbodyContent: 'Are you sure you want to add this icon? This will delete your existing icon.',
        AddacceptText: 'Upload Now',
        CancelText: 'Cancel',
        DeletetitleText: 'Delete Icon',
        DeletebodyContent: 'Are you sure you want to permanently delete this icon?',
        DeleteText: 'Delete',
        ErrorText: 'An unknown error occurred.',
        EmptyText: 'Please choose a file.',
        AddGeneratedThumbTitleText: 'Auto Generated Icon',
        AddGeneratedThumbBodyText: 'This icon will be moderated. Are you sure you want to submit this icon?',
        AddGeneratedThumbAcceptText: 'Submit',
        AddGeneratedThumbCancelText: 'Cancel'
    },
    Roblox.Icons.Init = function() {
        function e(n) {
            t.html(n),
                document.getElementById('IconScript') === null &&
                $(function() {
                    Roblox.Icons.Init()
                })
        }

        function h() {
            var n = $('#addGeneratedIconImageButton'),
                t;
            n.addClass(f),
                t = setInterval(
                    function() {
                        var i = $('#autogenerate span').attr('data-retry-url');
                        i ||
                            (n.removeClass(f), clearInterval(t))
                    },
                    s
                )
        }
        h();
        var r = $('#IconDisplayContainer'),
            u = r.data('place-id'),
            o = r.data('place-icon-id'),
            z = r.data('random-icon'),
            i = $('#UploadStatus'),
            n = $('#LoadingImage'),
            f = 'btn-disabled-neutral',
            t = $('#icons'),
            s = 300;
        i.html() ||
            i.hide(),
            $('a[data-remove-icon-url]').click(
                function() {
                    var a = $(this),
                        n = r.data('remove-icon-url'),
                        t = {
                            placeId: u,
                            placeIconId: o
                        };
                    return Roblox.GenericConfirmation.open({
                            titleText: Roblox.Resources.Icons.DeletetitleText,
                            bodyContent: Roblox.Resources.Icons.DeletebodyContent,
                            acceptText: Roblox.Resources.Icons.DeleteacceptText,
                            declineText: Roblox.Resources.Icons.CancelText,
                            onAccept: function() {
                                $.ajax({
                                    url: n,
                                    type: 'POST',
                                    data: t,
                                    dataType: 'json',
                                    success: function(resp) {
                                        if (resp && resp.success) {
                                            window.location.reload();
                                        } else {
                                            var msg = resp && resp.message ? resp.message : Roblox.Resources.Icons.ErrorText;
                                            i.html(msg).removeClass('status-confirm').addClass('error-message').show();
                                        }
                                    },
                                    error: function() {
                                        i.html(Roblox.Resources.Icons.ErrorText).removeClass('status-confirm').addClass('error-message').show();
                                    }
                                });
                            }
                        }),
                        !1
                }
            );
        t.on(
            'click',
            'input[name=\'iconType\']',
            function() {
                $(this).attr('id') === 'imageIcon' ? (t.find('#autogenerate').hide(), t.find('#ImageUpload').show()) : (t.find('#ImageUpload').hide(), t.find('#autogenerate').show())
            }
        );
        $('a#addIconButton').click(
                function() {
                    var t = $(this);
                    $('#iconImageFile').val() ? Roblox.GenericConfirmation.open({
                        titleText: Roblox.Resources.Icons.AddtitleText,
                        bodyContent: Roblox.Resources.Icons.AddbodyContent,
                        acceptText: Roblox.Resources.Icons.AddacceptText,
                        declineText: Roblox.Resources.Icons.CancelText,
                        allowHtmlContentInBody: !0,
                        acceptColor: Roblox.GenericConfirmation.green,
                        onAccept: function() {
                            var r = new FormData;
                            r.append('iconImageFile', $('#iconImageFile')[0].files[0]),
                                r.append('placeId', u),
                                n.show(),
                                t.hide(),
                                $.ajax({
                                    url: t.data('form-post-url'),
                                    type: 'POST',
                                    success: function(i) {
                                        e(i),
                                            n.hide(),
                                            t.show(),
                                            window.location.reload()
                                    },
                                    error: function(jqXHR, textStatus, errorThrown) {
                                        n.hide();
                                        t.show();
                                        var errorMessage = Roblox.Resources.Icons.ErrorText;
                                        if (jqXHR.responseJSON && jqXHR.responseJSON.error) {
                                            errorMessage = jqXHR.responseJSON.error;
                                        }
                                        i.html(errorMessage).removeClass('status-confirm').addClass('error-message').show()
                                    },
                                    data: r,
                                    cache: !1,
                                    contentType: !1,
                                    processData: !1
                                })
                        }
                    }) : i.html(Roblox.Resources.Icons.EmptyText).removeClass('status-confirm').addClass('error-message').show()
                }
            ),
            $('a#addGeneratedIconImageButton').click(
                function() {
                    function r() {
                        var r = new FormData;
                        r.append('placeId', u),
                            r.append('random-icon', z),
                            n.show(),
                            t.hide(),
                            $.ajax({
                                url: t.data('form-post-url'),
                                type: 'POST',
                                success: function(i) {
                                    e(i),
                                        n.hide(),
                                        t.show(),
                                        setTimeout(function() {
                                            window.location.reload()
                                        }, 2000)
                                },
                                error: function() {
                                    n.hide(),
                                        t.show(),
                                        i.html(Roblox.Resources.Icons.ErrorText).removeClass('status-confirm').addClass('error-message').show()
                                },
                                data: r,
                                cache: !1,
                                contentType: !1,
                                processData: !1
                            })
                    }
                    var t = $(this);
                    if (t.hasClass(f)) {
                        event.preventDefault();
                        return
                    }
                    Roblox.GenericConfirmation.open({
                        titleText: Roblox.Resources.Icons.AddGeneratedThumbTitleText,
                        bodyContent: Roblox.Resources.Icons.AddGeneratedThumbBodyText,
                        acceptText: Roblox.Resources.Icons.AddGeneratedThumbAcceptText,
                        declineText: Roblox.Resources.Icons.AddGeneratedThumbCancelText,
                        onAccept: r
                    })
                }
            )
    },
    $(function() {
        Roblox.Icons.Init()
    });