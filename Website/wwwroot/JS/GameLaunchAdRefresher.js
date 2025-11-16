$(function() {
        $(Roblox.GameLauncher).on(Roblox.GameLauncher.startClientSucceededEvent, function() {
                typeof googletag !="undefined" &&googletag.cmd.push(function() {
                        googletag.pubads().refresh()
                    })
            })
    });