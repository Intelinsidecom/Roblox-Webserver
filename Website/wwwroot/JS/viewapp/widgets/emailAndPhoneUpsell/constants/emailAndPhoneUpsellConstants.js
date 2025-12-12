// ~/viewapp/widgets/emailAndPhoneUpsell/constants/emailAndPhoneUpsellConstants.js
"use strict";
emailAndPhoneUpsell.constant("emailAndPhoneUpsellConstants", {
    templates: {
        emailAndPhoneUpsell: "email-and-phone-upsell"
    },
    urls: {
        getScreen: "/v1/user/screens/contact-upsell",
        submitEmail: "/account/changeemail",
        verifyEmail: "/my/account/sendverifyemail"
    },
    screenTypes: {
        none: "None",
        addEmail: "AddEmail",
        emailForm: "EmailForm",
        verifyEmail: "VerifyEmail"
    },
    events: {
        addEmail: "addEmail",
        verifyEmail: "verifyEmail",
        modalAction: "modalAction"
    },
    modalActions: {
        shown: "shown",
        dismissed: "dismissed"
    },
    eventContext: "contactUpsell",
    modalEventContextPrefix: "contactUpsell_",
    header: "Don't get locked out!",
    close: "Close",
    addEmail: "Add Email",
    submit: "Submit",
    verifyEmail: "Verify Email",
    addEmailUnder13Prompt: "Please add your parent's email address to your account to ensure that you can always access your Roblox account.",
    addEmail13AndOverPrompt: "Please add an email address to your account to ensure that you can always access your Roblox account.",
    verifyEmailUnder13Prompt: "Please verify your parent's email address to ensure that you can always access your Roblox account.",
    verifyEmail13AndOverPrompt: "Please verify your email address to ensure that you can always access your Roblox account.",
    parentsEmailPlaceholder: "Parent's Email",
    emailPlaceholder: "Email",
    verifyPasswordPlaceholder: "Verify Account Password",
    success: "Success",
    invalidEmail: "Invalid email",
    errorMessage: "An error occurred",
    verifyEmailUnder13SuccessMessage: "Verification link has been sent to your email - please verify your email to secure your account.",
    verifyEmail13AndOverSuccessMessage: "Verification link has been sent to your parent's email - please verify your parent's email to secure your account."
});