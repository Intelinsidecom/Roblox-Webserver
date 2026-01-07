// Universes/Configure.js
$(function() {
    function n() {
        return $(".PlaceSelectorScript").length > 0
    }

    function t(t) {
        o(t);
        var i = t.data("maindiv");
        $("#" + i).show(), i !== "places" || n() || Roblox.PlaceSelector.Init(), $.address.hash(i)
    }

    function u(t) {
        $("#places").html(t), $(function() {
            n() || Roblox.PlaceSelector.Init()
        })
    }

    function f() {
        var n = $("#Name").val().trim();
        if ($(".name-error").hide(), n == "") {
            $(".name-error").show();
            return
        }
        
        // Show processing modal
        e();
        
        // Gather form data
        var idValue = $("#Id").val();
        console.log("DEBUG JS: Id field value:", idValue);
        var formData = {
            Id: idValue,
            Name: $("#Name").val(),
            Description: $("#Description").val(),
            Genre: $("#Genre").val(),
            CharacterForce: $("input[name='CharacterForce']:checked").val(),
            ScaleChoice: $("input[name='ScaleChoice']:checked").val()
        };
        console.log("DEBUG JS: Form data:", formData);
        
        // Submit via AJAX
        $.ajax({
            url: "/places/doconfigure2",
            method: "POST",
            headers: { "X-Requested-With": "XMLHttpRequest" },
            data: formData,
            traditional: true,
            success: function(response) {
                // Hide processing modal
                $.modal.close();
                
                if (response.success) {
                    // Success - reload page to show updated data
                    location.reload();
                } else {
                    // Error returned from server
                    alert(response.message || "An error occurred while saving.");
                }
            },
            error: function(xhr, status, error) {
                // Hide processing modal
                $.modal.close();
                
                // Show error message
                var errorMessage = "An error occurred while saving. Please try again.";
                
                if (xhr.responseJSON && xhr.responseJSON.errors) {
                    // Handle validation errors
                    var errors = xhr.responseJSON.errors;
                    if (errors.Name) {
                        errorMessage = errors.Name[0];
                    } else if (errors.Description) {
                        errorMessage = errors.Description[0];
                    } else if (errors.Genre) {
                        errorMessage = errors.Genre[0];
                    }
                } else if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                } else if (xhr.responseJSON && xhr.responseJSON.title) {
                    errorMessage = xhr.responseJSON.title;
                }
                
                // Show error message (you can customize this part)
                alert(errorMessage);
            }
        });
    }

    function e() {
        var n = {
            overlayClose: !1,
            opacity: 80,
            overlayCss: {
                backgroundColor: "#000"
            },
            escClose: !1
        };
        typeof closeClass != "undefined" && closeClass !== "" && $.modal.close("." + closeClass), $("#ProcessingView").modal(n)
    }

    function o(n) {
        $(".configure-tab").hide(), $("#navbar div.selected").removeClass("selected"), n.addClass("selected")
    }

    function i(n, t, i) {
        $("#universe-error").hide(), $(n).toggle(), $(n).next(".loading-button").toggle();
        var f = $("#universe-configure").data("universeid"),
            r = {
                placeId: t,
                universeId: f
            },
            e = $("#universe-configure").data("configureplaceurl");
        $.post(i, r, function(t) {
            t.success ? $.ajax({
                url: e,
                data: r,
                success: function(n) {
                    u(n)
                },
                cache: !1
            }) : ($(n).toggle(), $(n).next(".loading-button").toggle(), $("#universe-error").text(t.message), $("#universe-error").show())
        }), $("[data-retry-url]").loadRobloxThumbnails(), $(".tooltip-top").tipsy({
            gravity: "s"
        })
    }

    function r() {
        var n = $.address.hash(),
            i;
        n = n.replace("/", "").escapeHTML(), n.length == 0 && (n = "basicSettings"), i = $('[data-maindiv="' + n + '"]'), i.length > 0 && t(i)
    }
    $(".verticaltab").click(function() {
        return t($(this)), !1
    });
    var s = $("#UniverseAvatarType:checked").attr("value");
    $(document).ready(function() {
        $(".gameavatartype:checked").click()
    }), $("#okButton").click(function() {
        f()
    });
    $("#universe-configure").on("click", ".add-place-button", function() {
        var n = this;
        Roblox.PlaceSelector.Open(function(t) {
            var r = $("#universe-configure").data("addplaceurl");
            i(n, t, r)
        })
    });
    $("#universe-configure").on("click", ".remove-place-button", function() {
        var n = $(this).data("placeid"),
            t = $("#universe-configure").data("removeplaceurl");
        i(this, n, t)
    });
    $("#universe-configure").on("click", ".load-more-places-button", function() {
        var t = $(this).parent(),
            n = t.parent(),
            i = n.find(".universe-place-container").length,
            r = $("#current-places").data("universeid"),
            u = n.data("isuniversecreation"),
            f = {
                startRow: i,
                universeId: r,
                isUniverseCreation: u
            },
            e = $("#universe-configure").data("loadmoreplacesurl");
        return $.ajax({
            url: e,
            cache: !1,
            data: f,
            dataType: "html",
            success: function(i) {
                t.remove();
                var r = $(i);
                r.hide().appendTo(n).fadeIn(), $("[data-retry-url]").loadRobloxThumbnails()
            }
        }), !1
    });
    $("[data-retry-url]").loadRobloxThumbnails(), r(), $.address.externalChange(r)
});