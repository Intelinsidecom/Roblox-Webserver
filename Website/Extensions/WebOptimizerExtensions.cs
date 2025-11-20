using Microsoft.Extensions.DependencyInjection;
using WebOptimizer;

namespace Website.Extensions
{
    public static class WebOptimizerExtensions
    {
        public static IServiceCollection AddWebOptimizerPipeline(this IServiceCollection services)
        {
            services.AddWebOptimizer(pipeline =>
            {
                pipeline.AddJavaScriptBundle(
                    "/js/site.bundle.js",
                    new[]
                    {
                        "JS/lib/athena/athena.js",
                        "JS/lib/jquery/jquery-1.11.1.min.js",
                        "JS/lib/jquery/jquery-migrate-1.2.1.min.js",
                        "JS/roblox.js",
                        "JS/jquery.cookie.js",
                        "JS/RobloxCookies.js",
                        "JS/RobloxEventStream.js",
                        "JS/RobloxEventManager.js",
                        // Angular and deps
                        "JS/angular/angular.js",
                        "JS/angular/angular.ng-modules.js",
                        "JS/angular/angular-elastic.js",
                        "JS/angular/angular-sanitize.min.js",
                        "JS/angular/angular-ui-router.min.js",
                        "JS/angular/ui-bootstrap-0.11.2.js",
                        // third-party libs
                        "JS/lib/gigya/gigya.js",
                        // Bootstrap folder
                        "JS/Bootstrap/CaptchaModal.js",
                        "JS/Bootstrap/GenericConfirmation.js",
                        // common
                        "JS/common/forms.js",
                        // events
                        "JS/Events/PageHeartbeatEvent.js",
                        "JS/Events/UserInteractionsEvent.js",
                        // extensions
                        "JS/extensions/string.js",
                        "JS/extensions/ThreeDeeThumbnails.js",
                        "JS/extensions/Thumbnails.js",
                        // game
                        "JS/Game/GamePlayEvents.js",
                        // GA
                        "JS/GoogleAnalytics/GoogleAnalyticsEvents.js",
                        // jPlayer
                        "JS/jPlayer/2.9.2/jquery.jplayer.min.js",
                        // Landing
                        "JS/Landing/RollerCoaster/RollerCoaster.js",
                        // leancore
                        "JS/leancore/iesupport/jquery.placeholder.js",
                        "JS/leancore/libs/bootstrap.min.js",
                        "JS/leancore/libs/json3.min.js",
                        "JS/leancore/libs/underscore-min.js",
                        "JS/leancore/BootstrapNamespacing.js",
                        "JS/leancore/Navigation.js",
                        "JS/leancore/RobloxBaseInit.js",
                        "JS/leancore/RobloxHeaderInit.js",
                        // Login
                        "JS/Login/Login.js",
                        "JS/Login/SignupOrLogIn.js",
                        "JS/Login/SignupOrLoginContainer.js",
                        // polyfill
                        "JS/polyfill/ie7localStorage.js",
                        // widgets
                        "JS/widgets/jquery.mCustomScrollbar.concat.min.js",
                        "JS/Reference/widget.js",
                        // thumbnails
                        "JS/Thumbnails/ThumbnailView.js",
                        // Tracking
                        "JS/Tracking/AsyncGoogleOnScript.js",
                        "JS/Tracking/FormEvents.js",
                        "JS/Tracking/SignupTrackingScript.js",
                        // utilities
                        "JS/utilities/deviceFeatureDetection.js",
                        "JS/utilities/lazyLoad.js",
                        "JS/utilities/performance.js",
                        // viewapp common
                        "JS/viewapp/common/formEvents/formEvents.js",
                        "JS/viewapp/common/signupOrLogin/signupOrLogin.js",
                        "JS/viewapp/common/formEvents/directives/formContext.js",
                        "JS/viewapp/common/formEvents/directives/formInteraction.js",
                        "JS/viewapp/common/formEvents/directives/formValidation.js",
                        "JS/viewapp/common/formEvents/directives/formValidationRedactInput.js",
                        "JS/viewapp/common/services/robloxService.js",
                        "JS/viewapp/common/services/eventStreamService.js",
                        "JS/viewapp/common/services/googleAnalyticsEventsService.js",
                        "JS/viewapp/common/services/httpService.js",
                        "JS/viewapp/common/services/hybridService.js",
                        "JS/viewapp/common/services/imagesService.js",
                        "JS/viewapp/common/services/localStorageService.js",
                        "JS/viewapp/common/services/numericCodeService.js",
                        "JS/viewapp/common/services/performanceService.js",
                        "JS/viewapp/common/services/realtimeService.js",
                        "JS/viewapp/common/services/urlService.js",
                        "JS/viewapp/common/services/userService.js",
                        "JS/viewapp/common/filters.js",
                        "JS/viewapp/common/helpers.js",
                        "JS/viewapp/app.js",
                        "JS/viewapp/common/signupOrLogin/controllers/captchaController.js",
                        "JS/viewapp/common/signupOrLogin/controllers/loginController.js",
                        "JS/viewapp/common/signupOrLogin/controllers/signupController.js",
                        "JS/viewapp/common/signupOrLogin/controllers/signupOrLoginController.js",
                        "JS/viewapp/common/signupOrLogin/directives/showSection.js",
                        "JS/viewapp/common/signupOrLogin/directives/validBirthday.js",
                        "JS/viewapp/common/signupOrLogin/directives/validPassword.js",
                        "JS/viewapp/common/signupOrLogin/directives/validPasswordConfirm.js",
                        "JS/viewapp/common/signupOrLogin/directives/validUsername.js",
                        "JS/viewapp/common/signupOrLogin/services/captchaService.js",
                        "JS/viewapp/common/signupOrLogin/services/displayService.js",
                        "JS/viewapp/common/signupOrLogin/services/loginService.js",
                        "JS/viewapp/common/signupOrLogin/services/signupService.js",
			"JS/viewapp/common/helpers.js",
			"JS/viewapp/common/services/robloxService.js",
			"JS/viewapp/common/services/httpService.js",
			"JS/viewapp/common/services/cacheService.js",
			"JS/viewapp/common/services/googleAnalyticsEventsService.js",
			"JS/angular/ui-bootstrap-0.11.2.js",
                        // remaining root scripts
                        "JS/ABPlaceLauncher.js",
                        "JS/AjaxAvatarThumbnail.js",
                        "JS/CharacterSelect.js",
                        "JS/ClientInstaller.js",
                        "JS/CookieUpgrader.js",
                        "JS/DeveloperConsoleWarning.js",
                        "JS/DropDownNav.js",
                        "JS/EventTracker.js",
                        "JS/FormValidator.js",
                        "JS/GenericConfirmation.js",
                        "JS/GenericModal.js",
                        "JS/GoogleEventListener.js",
                        "JS/GPTAdScript.js",
                        "JS/IEMetroInstructions.js",
                        "JS/IframeEventListener.js",
                        "JS/iFrameLogin.js",
                        "JS/InstallationInstructions.js",
                        "JS/JavaScriptEndpoints.js",
                        "JS/jquery.ba-postmessage.min.js",
                        "JS/jquery.simplemodal-1.3.5.js",
                        "JS/jquery.tipsy.js",
                        "JS/jquery.validate.js",
                        "JS/jquery.validate.unobtrusive.js",
                        "JS/JSErrorTracker.js",
                        "JS/json2.min.js",
                        "JS/Linkify.js",
                        "JS/MadStatus.js",
                        "JS/MasterPageUI.js",
                        "JS/PlaceLauncher.js",
                        "JS/ShopInterstitialModal.js",
                        "JS/SignupFormValidatorGeneric.js",
                        "JS/SiteTouchEvent.js",
                        "JS/SocialLogin.js",
                        "JS/StringTruncator.js",
                        "JS/TwoStepVerificationModal.js",
                        "JS/UpsellAdModal.js",
                        "JS/VideoPreRoll.js",
                        "JS/lib/ajax/MicrosoftAjax.js",
                        "JS/lib/ajax/MicrosoftAjaxWebForms.js",
                        "JS/webkit.js",
                        "JS/XsrfToken.js"
                    }
                );
            });

            return services;
        }
    }
}
