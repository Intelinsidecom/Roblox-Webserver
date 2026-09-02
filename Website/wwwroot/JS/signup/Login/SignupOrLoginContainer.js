// Login/SignupOrLoginContainer.js
typeof Freebloxia == "undefined" && (Freebloxia = {}), typeof Freebloxia.SignupOrLoginContainer == "undefined" && (Freebloxia.SignupOrLoginContainer = function() {
    function n() {
        $(".login-header").hide(), $(".signup-header").hide(), $(".two-step-header").hide()
    }

    function t() {
        $(document).on("authFormToggle", function(t, i) {
            switch (i.toSectionType) {
                case Freebloxia.SignupOrLogin.SectionType.login:
                    n(), $(".login-header").show();
                    break;
                case Freebloxia.SignupOrLogin.SectionType.signup:
                    n(), $(".signup-header").show()
            }
        })
    }
    return {
        Init: t
    }
}()), $(function() {
    Freebloxia.SignupOrLoginContainer.Init()
});