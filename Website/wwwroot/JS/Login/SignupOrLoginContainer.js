// Login/SignupOrLoginContainer.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.SignupOrLoginContainer == "undefined" && (Roblox.SignupOrLoginContainer = function() {
    function n() {
        $(".login-header").hide(), $(".signup-header").hide(), $(".two-step-header").hide()
    }

    function t() {
        $(document).on("authFormToggle", function(t, i) {
            switch (i.toSectionType) {
                case Roblox.SignupOrLogin.SectionType.login:
                    n(), $(".login-header").show();
                    break;
                case Roblox.SignupOrLogin.SectionType.signup:
                    n(), $(".signup-header").show()
            }
        })
    }
    return {
        Init: t
    }
}()), $(function() {
    Roblox.SignupOrLoginContainer.Init()
});