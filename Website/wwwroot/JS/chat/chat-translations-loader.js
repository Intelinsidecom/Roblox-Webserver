"use strict";
angular.module("robloxApp").run(["languageResource", function (n) {
    if (window.Roblox && Roblox.LangDynamicDefault && Roblox.LangDynamicDefault["Feature.Chat"]) {
        n.setLanguageKeysFromFile(Roblox.LangDynamicDefault["Feature.Chat"]);
    }
}]);
