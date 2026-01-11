// IDE/Thumbnail.js
var Roblox = Roblox || {};
Roblox.IDE = Roblox.IDE || {},
    Roblox.IDE.Resources = Roblox.IDE.Resources || {},
    $.extend(
        Roblox.IDE.Resources, {
            DeletetitleText: 'Delete Thumbnail',
            DeletebodyContent: 'Are you sure you want to permanently delete this thumbnail?',
            DeleteText: 'Delete',
            AddtitleText: 'Add Thumbnail',
            AddbodyContent: 'Are you sure you want to buy this thumbnail for ',
            AddacceptText: 'Buy Now',
            CancelText: 'Cancel',
            AddGeneratedThumbTitleText: 'Auto Generated Thumbnail',
            AddGeneratedThumbBodyText: 'This thumbnail will be moderated. Are you sure you want to submit this thumbnail?',
            AddGeneratedThumbAcceptText: 'Submit',
            AddGeneratedThumbCancelText: 'Cancel'
        }
    ),
    Roblox.IDE.ThumbnailGallery = Roblox.IDE.ThumbnailGallery ||
    function() {
        function n() {
            function s() {
                if (o) {
                    var n = $('.SelectedYouTubeGalleryIcon').next();
                    n.length > 0 &&
                        u(n)
                }
            }

            function u(t) {
                var i = $(t).find('.smallGalleryThumbItem'),
                    e = i.find('.divYouTubeVideoPlayer'),
                    r,
                    o,
                    h,
                    u,
                    f;
                e.length != 0 ? (
                        i = $(e.children()[0].cloneNode(!0)),
                        (i.is('object') || i.is('iframe')) &&
                        (
                            i.height('230'),
                            i.attr('Height', '230px'),
                            i.width('420'),
                            i.attr('Width', '420px'),
                            r = RobloxYouTubeVideoManager.GetVideo(i.attr('id')),
                            r.Muted = n,
                            r.Autoplay = !0
                        )
                    ) : (
                        o = i.find('img')[0],
                        i = $(o).clone(),
                        i.height('230'),
                        i.width('420'),
                        setTimeout(function() {
                            s()
                        }, 5000)
                    ),
                    $('#divTransitionLargeGalleryItem').remove(),
                    $('#ItemThumbnail').append(
                        '<div id=\'divTransitionLargeGalleryItem\' style=\'display: none; position: absolute;\'>'
                    ),
                    $('#divTransitionLargeGalleryItem').append(i),
                    $('#divTransitionLargeGalleryItem').fadeIn('slow', function() {}),
                    $('#bigGalleryThumbItem').fadeOut(
                        'slow',
                        function() {
                            $('#divTransitionLargeGalleryItem').attr('id', 'bigGalleryThumbItem'),
                                $(this).remove()
                        }
                    ),
                    h = $('.SelectedYouTubeGalleryIcon').attr('data-place-media-item-alt-text'),
                    u = $('#AltText').val(),
                    h != u &&
                    $('.SelectedYouTubeGalleryIcon').attr('data-place-media-item-alt-text', u),
                    $('.divSmallGalleryItem').removeClass('SelectedYouTubeGalleryIcon'),
                    $(t).addClass('SelectedYouTubeGalleryIcon'),
                    f = $(t).attr('data-place-media-item-alt-text'),
                    $('#AltText').val(f),
                    $('#updateAltTextCharCount').text(f.length),
                    $('#updateAltTextError').text(''),
                    $('#updateAltTextFormGroup').removeClass('form-has-error'),
                    $('#updateAltTextCharacters').show(),
                    $('#updateAltTextSuccess').hide()
            }

            function h(t, i, r, u) {
                function f(t, i, r) {
                    t == RobloxYouTube.Events.States.Ended &&
                        (n = r.Player().isMuted())
                }
                var e = t,
                    o = RobloxYouTubeVideoManager.AddVideo(new RobloxYouTubeVideo(e, f)),
                    s = i,
                    h = t,
                    c = !1,
                    l = !1;
                o.Init(s, c, h, r, u, l)
            }

            function c() {
                var t = [],
                    n;
                $('.divSmallGalleryItem').each(function(n) {
                        var i = $(this).data('place-media-item-id');
                        t[n] = i
                    }),
                    n = '';
                for (key in t) n += t[key] + ',';
                f ? $.getJSON('/Thumbs/AssetMedia/PlaceMediaItemSortHandler.ashx', {
                    sort: n
                }) : $.post('/thumbnail/set-asset-media-sort-order', {
                    sort: n
                })
            }
            if (document.getElementById('ThumbnailMediaScript') === null) {
                var t = $('#ThumbnailDisplayContainer'),
                    f = t.data('is-place'),
                    e = t.data('media-items-count'),
                    o = !1,
                    n = !0,
                    i = $('#AltText'),
                    r = $('#saveAltTextButton');
                $('#thumbnails').attr('data-universe-id') == '0' &&
                    i &&
                    r &&
                    (i.prop('disabled', !0), r.addClass('btn-disabled-neutral'));
                $('#AltText').on(
                    'input',
                    function() {
                        $('#updateAltTextError').text(''),
                            $('#updateAltTextCharCount').text($(this).val().length),
                            $('#updateAltTextFormGroup').removeClass('form-has-error'),
                            $('#updateAltTextCharacters').show(),
                            $('#updateAltTextSuccess').hide()
                    }
                );
                $('#saveAltTextButton').click(
                        function() {
                            var n = $(this),
                                t = $('#thumbnails').attr('data-universe-id');
                            if (t != '0') {
                                var u = $('.SelectedYouTubeGalleryIcon').attr('data-place-media-item-id'),
                                    f = $('#AltText').val(),
                                    i = $('#updateAltTextError'),
                                    r = $('#updateAltTextSuccess');
                                n.addClass('btn-disabled-neutral'),
                                    n.prop('disabled', !0),
                                    r.hide(),
                                    $.ajax({
                                        type: 'POST',
                                        url: Roblox.EnvironmentUrls.developApi + '/v1/universes/' + t + '/thumbnails/alt-text',
                                        data: JSON.stringify({
                                            MediaAssetId: u,
                                            MediaAssetAltText: f
                                        }),
                                        contentType: 'application/json; charset=utf-8',
                                        dataType: 'json',
                                        success: function(n) {
                                            $('.SelectedYouTubeGalleryIcon').attr('data-place-media-item-id') == n.MediaAssetId &&
                                                (
                                                    $('.SelectedYouTubeGalleryIcon').attr('data-place-media-item-alt-text', n.MediaAssetAltText),
                                                    $('#AltText').val(n.MediaAssetAltText)
                                                ),
                                                i.text(''),
                                                $('#updateAltTextFormGroup').removeClass('form-has-error'),
                                                $('#updateAltTextCharacters').show(),
                                                r.show()
                                        },
                                        error: function(n) {
                                            var t = n.responseJSON.errors[0].message;
                                            i.text(t),
                                                $('#updateAltTextFormGroup').addClass('form-has-error'),
                                                $('#updateAltTextCharacters').hide()
                                        },
                                        complete: function() {
                                            n.removeClass('btn-disabled-neutral'),
                                                n.prop('disabled', !1)
                                        }
                                    })
                            }
                        }
                    ),
                    $('.youTubeVideoOverlay').click(function() {
                        n = !1,
                            u($(this).parent())
                    }),
                    $('#divSmallGalleryScrollContainer').width(e * 130),
                    $('#divSmallGalleryScrollContainer').sortable({
                        update: function() {
                            c()
                        }
                    }),
                    $('.divYouTubeVideoPlayer').each(
                        function() {
                            var childDiv = $(this).children('div')[0];
                            if (!childDiv || !childDiv.id) {
                                console.warn('YouTube player element ID not found, skipping initialization');
                                return;
                            }
                            
                            var t = childDiv.id,
                                n = $(this).data('video-size'),
                                i = $(this).data('video-hash'),
                                r = n === 'large' ? 420 : 120,
                                u = n === 'large' ? 230 : 70;
                            h(t, i, r, u)
                        }
                    )
            }
        }
        return {
            init: n
        }
    }(),
    Roblox.IDE.Thumbnails = Roblox.IDE.Thumbnails ||
    function() {
        function i(n) {
            var t = $('input[name=thumbnailType]:checked'),
                i = '<span class=\'icon-robux-16x16\'></span><span>',
                r = i + t.attr('data-cost-in-robux') + '</span>';
            Roblox.GenericConfirmation.open({
                titleText: Roblox.IDE.Resources.AddtitleText,
                bodyContent: Roblox.IDE.Resources.AddbodyContent + r + '?',
                acceptText: Roblox.IDE.Resources.AddacceptText,
                declineText: Roblox.IDE.Resources.CancelText,
                allowHtmlContentInBody: !0,
                acceptColor: Roblox.GenericConfirmation.green,
                onAccept: n
            })
        }

        function u() {
            var t = $('#addGeneratedImageButton'),
                i;
            t.addClass(n),
                i = setInterval(
                    function() {
                        var r = $('#autogenerate span').attr('data-retry-url');
                        r ||
                            (t.removeClass(n), clearInterval(i))
                    },
                    300
                )
        }

        function f(t, i) {
            t.toggleClass('btn-neutral', i).toggleClass(n, !i),
                i ? t.removeAttr('disabled') : t.attr('disabled', '')
        }

        function t(n) {
            if (n && n.success === false) {
                $('#thumbnailResponse').show().find('.status-confirm').text(n.message);
                return;
            }
            
            var hasAutoGeneratedThumbnail = $('#ThumbnailDisplayContainer').data('has-auto-generated-thumbnail') === true;
            var autoGeneratedThumbnailUrl = $('#ThumbnailDisplayContainer').data('auto-generated-thumbnail-url') || '';

            var $galleryContainer = $('#divSmallGalleryScrollContainer');
            
            if (n && n.thumbnails && Array.isArray(n.thumbnails)) {
                $galleryContainer.empty();
                
                n.thumbnails.forEach(function(thumbnail, index) {
                    if (thumbnail.type === 'image') {
                        var cacheBuster = Date.now();
                        var thumbnailUrlWithCache = thumbnail.url.includes('?') 
                            ? thumbnail.url + '&_t=' + cacheBuster 
                            : thumbnail.url + '?_t=' + cacheBuster;
                        
                        var $galleryItem = $('<div class="divSmallGalleryItem smallGalleryThumbItem' + (index === 0 ? ' SelectedYouTubeGalleryIcon' : '') + '" ' +
                            'data-thumbnailid="' + thumbnail.id + '" ' +
                            'data-place-media-item-id="' + thumbnail.id + '" ' +
                            'data-place-media-item-alt-text="' + (thumbnail.altText || '') + '" ' +
                            'data-place-media-item-url="' + thumbnail.url + '">' +
                            '<a data-remove-url="" data-thumbnailid="' + thumbnail.id + '" class="ImageButton closeBtnCircle_35h RemoveYouTubeGalleryImage"></a>' +
                            '<img src="' + thumbnailUrlWithCache + '" alt="' + (thumbnail.altText || '') + '" class="smallGalleryThumbImage" />' +
                            '</div>');
                        
                        $galleryContainer.append($galleryItem);
                    }
                });
                
                Roblox.IDE.ThumbnailGallery.init();
                
                $('#ThumbnailDisplayContainer').attr('data-media-items-count', n.thumbnails.length);

                var selectedType = $("input[name='thumbnailType']:checked").val();
                if (selectedType !== 'image' && selectedType !== 'video') {
                    $('#divSmallGalleryScrollContainer .divSmallGalleryItem').hide();
                }
            }
            
            if (hasAutoGeneratedThumbnail && autoGeneratedThumbnailUrl) {
                $('.thumbnail-preview img').attr('src', autoGeneratedThumbnailUrl);
            }
        }

        function e() {
            var h = $('#remove-thumbnail-error'),
                o = $('#addImageButton'),
                s = $('#thumbnailImageFile'),
                c = Roblox.IDE.validator({
                    button: o,
                    enabledClass: r,
                    disabledClass: n
                }, [{
                    input: s,
                    errorSpan: $('#ImageUpload .field-validation-valid')
                }], !1),
                e;
            u(),
                e = $('#ThumbnailDisplayContainer').data('place-id'),
                $('a[data-remove-url]').click(
                    function() {
                        var n = $(this),
                            t = $('#ThumbnailDisplayContainer').data('remove-url'),
                            i = {
                                thumbnailid: n.data('thumbnailid'),
                                id: e
                            };
                        return Roblox.GenericConfirmation.open({
                                titleText: Roblox.IDE.Resources.DeletetitleText,
                                bodyContent: Roblox.IDE.Resources.DeletebodyContent,
                                acceptText: Roblox.IDE.Resources.DeleteacceptText,
                                declineText: Roblox.IDE.Resources.CancelText,
                                onAccept: function() {
                                    $.post(
                                        t,
                                        i,
                                        function(t) {
                                            t.success ? n.parent().remove() : h.addClass('field-validation-error').text(t.message)
                                        }
                                    )
                                }
                            }),
                            !1
                    }
                );
            $('#thumbnails').on(
                'click',
                'input[name=\'thumbnailType\']',
                function() {
                    var selectedType = $(this).val();
                    
                    // Show/hide upload sections
                    if (selectedType === 'image') {
                        $('#thumbnails #VideoUpload').hide();
                        $('#thumbnails #autogenerate').hide();
                        $('#thumbnails #ImageUpload').show();
                        $('#divSmallGalleryScrollContainer .divSmallGalleryItem').show();
                    } else if (selectedType === 'video') {
                        $('#thumbnails #ImageUpload').hide();
                        $('#thumbnails #autogenerate').hide();
                        $('#thumbnails #VideoUpload').show();
                        $('#divSmallGalleryScrollContainer .divSmallGalleryItem').show();
                    } else {
                        $('#thumbnails #ImageUpload').hide();
                        $('#thumbnails #VideoUpload').hide();
                        $('#thumbnails #autogenerate').show();
                        $('#divSmallGalleryScrollContainer .divSmallGalleryItem').hide();
                    }
                }
            );
            o.click(
                    function() {
                        function o() {
                            var n = new FormData;
                            n.append('thumbnailImageFile', s[0].files[0]),
                                n.append('id', e),
                                n.append(
                                    '__RequestVerificationToken',
                                    $('[name=__RequestVerificationToken]').val()
                                ),
                                u.show(),
                                r.hide(),
                                $.ajax({
                                    url: r.data('form-post-url'),
                                    type: 'POST',
                                    success: function(n) {
                                        t(n),
                                            u.hide(),
                                            r.show()
                                    },
                                    error: function(xhr, status, error) {
                                        var errorMessage = 'Error uploading thumbnail image';
                                        if (xhr.responseJSON && xhr.responseJSON.message) {
                                            errorMessage = xhr.responseJSON.message;
                                        } else if (xhr.responseText) {
                                            try {
                                                var errorData = JSON.parse(xhr.responseText);
                                                if (errorData.message) {
                                                    errorMessage = errorData.message;
                                                }
                                            } catch (e) {
                                                errorMessage = xhr.statusText || 'Unknown error occurred';
                                            }
                                        }
                                        
                                        if (typeof window.showMessage === 'function') {
                                            window.showMessage(errorMessage, true);
                                        }
                                        
                                        u.hide(),
                                            r.show()
                                    },
                                    data: n,
                                    cache: !1,
                                    contentType: !1,
                                    processData: !1
                                })
                        }
                        var r = $(this),
                            u,
                            f,
                            h;
                        r.hasClass(n) ||
                            (
                                u = $('#LoadingImage'),
                                f = $('input[name=thumbnailType]:checked'),
                                f &&
                                (
                                    h = f.attr('data-cost-in-robux'),
                                    h ? i(o) : Roblox.GenericConfirmation.open({
                                        titleText: Roblox.IDE.Resources.AddtitleText,
                                        bodyContent: Roblox.IDE.Resources.AddGeneratedThumbBodyText,
                                        acceptText: Roblox.IDE.Resources.AddGeneratedThumbAcceptText,
                                        declineText: Roblox.IDE.Resources.AddGeneratedThumbCancelText,
                                        onAccept: o
                                    })
                                )
                            )
                    }
                ),
                $('a#addGeneratedImageButton').click(
                    function() {
                        function u() {
                            var n = new FormData;
                            n.append('id', e),
                                r.show(),
                                i.hide(),
                                $.ajax({
                                    url: i.data('form-post-url'),
                                    type: 'POST',
                                    success: function(n) {
                                        t(n),
                                            r.hide(),
                                            i.show()
                                    },
                                    error: function() {
                                        r.hide(),
                                            i.show()
                                    },
                                    data: n,
                                    cache: !1,
                                    contentType: !1,
                                    processData: !1
                                })
                        }
                        var i = $(this),
                            r = $('#LoadingImage');
                        if (i.hasClass(n)) {
                            event.preventDefault();
                            return
                        }
                        Roblox.GenericConfirmation.open({
                            titleText: Roblox.IDE.Resources.AddGeneratedThumbTitleText,
                            bodyContent: Roblox.IDE.Resources.AddGeneratedThumbBodyText,
                            acceptText: Roblox.IDE.Resources.AddGeneratedThumbAcceptText,
                            declineText: Roblox.IDE.Resources.AddGeneratedThumbCancelText,
                            onAccept: o
                        })
                    }
                ),
                $('a#addVideoButton').click(
                    function() {
                        function o() {
                            r.show(),
                                n.hide(),
                                $.post(
                                    n.data('form-post-url'), {
                                        id: e,
                                        thumbnailYoutubeUrl: u,
                                        __RequestVerificationToken: f
                                    },
                                    function(i) {
                                        if (i && i.success === false) {
                                            if (typeof window.showMessage === 'function') {
                                                window.showMessage(i.message, true);
                                            } else {
                                                $('#thumbnailResponse').show().find('.status-confirm').text(i.message);
                                            }
                                        } else {
                                            // Success case - show success message and refresh UI
                                            if (typeof window.showMessage === 'function') {
                                                window.showMessage(i.message || 'Video thumbnail added successfully', false);
                                            } else {
                                                $('#thumbnailResponse').show().find('.status-confirm').text(i.message || 'Video thumbnail added successfully');
                                            }
                                        }
                                        r.hide(),
                                            n.show()
                                    }
                                ).fail(function(xhr, status, error) {
                                    var errorMessage = 'Error uploading thumbnail video';
                                    if (xhr.responseJSON && xhr.responseJSON.message) {
                                        errorMessage = xhr.responseJSON.message;
                                    } else if (xhr.responseText) {
                                        try {
                                            var errorData = JSON.parse(xhr.responseText);
                                            if (errorData.message) {
                                                errorMessage = errorData.message;
                                            }
                                        } catch (e) {
                                            errorMessage = xhr.statusText || 'Unknown error occurred';
                                        }
                                    }
                                    
                                    if (typeof window.showMessage === 'function') {
                                        window.showMessage(errorMessage, true);
                                    } else {
                                        $('#thumbnailResponse').show().find('.status-confirm').text(errorMessage);
                                    }
                                    
                                    r.hide(),
                                        n.show();
                                });
                        }
                        var n = $(this);
                        if (!n.hasClass('btn-disabled-neutral')) {
                            var r = $('#LoadingImage'),
                                u = $('#txtYouTubeVideoUrl').val(),
                                f = $('[name=__RequestVerificationToken]').val();
                            i(o)
                        }
                    }
                );
            c.init();
            
            // Add YouTube URL validation for video upload button
            var videoButton = $('#addVideoButton');
            var videoUrlInput = $('#txtYouTubeVideoUrl');
            
            // Function to validate YouTube URL and enable/disable button
            function validateYouTubeUrl() {
                var url = videoUrlInput.val().trim();
                var isValid = url.length > 0 && (
                    url.includes('youtube.com/watch?v=') || 
                    url.includes('youtu.be/') ||
                    url.includes('youtube.com/embed/')
                );
                
                // More robust validation for YouTube URL format
                if (isValid) {
                    // Extract video ID and validate it
                    var videoId = '';
                    if (url.includes('youtube.com/watch?v=')) {
                        videoId = url.split('v=')[1]?.split('&')[0];
                    } else if (url.includes('youtu.be/')) {
                        videoId = url.split('youtu.be/')[1]?.split('?')[0];
                    } else if (url.includes('youtube.com/embed/')) {
                        videoId = url.split('embed/')[1]?.split('?')[0];
                    }
                    
                    // Video ID should be 11 characters and contain valid chars
                    isValid = videoId && videoId.length >= 11 && /^[a-zA-Z0-9_-]+$/.test(videoId);
                }
                
                videoButton.toggleClass('btn-neutral', isValid).toggleClass('btn-disabled-neutral', !isValid);
                videoButton.prop('disabled', !isValid);
            }
            
            // Add event listener for YouTube URL input
            videoUrlInput.on('input keyup blur', validateYouTubeUrl);
            
            // Initial validation
            validateYouTubeUrl();
        }
        var r = 'btn-neutral',
            n = 'btn-disabled-neutral';
        return {
            init: e
        }
    }(),
    $(
        function() {
            Roblox.IDE.Thumbnails.init(),
                Roblox.IDE.ThumbnailGallery.init()
        }
    );