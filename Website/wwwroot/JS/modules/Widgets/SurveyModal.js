typeof Roblox=="undefined" &&(Roblox= {}),
typeof Roblox.SurveyModal=="undefined" &&(Roblox.SurveyModal=function() {
        function t() {
            $('[data-modal-handle="survey"]').find("iframe").show(), $('[data-modal-handle="survey"]').modal(i)
        }

        function n() {
            $.modal.close(), $('[data-modal-handle="survey"]').find("iframe").hide()
        }

        var i= {
            overlayClose: !0, escClose: !0, opacity:80, overlayCss: {
                backgroundColor:"#000"
            }

            , onClose:n
        }

        ; return {
            open:t
        }
    }

    ());