// DeveloperConsoleWarning.js
typeof Roblox == "undefined" && (Roblox = {}), typeof Roblox.DeveloperConsoleWarning == "undefined" && (Roblox.DeveloperConsoleWarning = function() {
    var n = "\n      _______      _________      _____       ______     _\n     / _____ \\    |____ ____|    / ___ \\     | ____ \\   | |\n    / /     \\_\\       | |       / /   \\ \\    | |   \\ \\  | |\n    | |               | |      / /     \\ \\   | |   | |  | |\n    \\ \\______         | |      | |     | |   | |___/ /  | |\n     \\______ \\        | |      | |     | |   |  ____/   | |\n            \\ \\       | |      | |     | |   | |        | |\n     _      | |       | |      \\ \\     / /   | |        |_|\n    \\ \\_____/ /       | |       \\ \\___/ /    | |         _\n     \\_______/        |_|        \\_____/     |_|        |_|\n\n     Keep your account safe! Do not paste any text here.\n\n     If someone is asking you to paste text here then you're \n     giving someone access to your account, your gear, and\n     your ROBUX.\n\n     To learn more about keeping your account safe you can go to\n\n     https://en.help.roblox.com/hc/en-us/articles/203313380-Account-Security-Theft-Keeping-your-Account-Safe-",
        t = function() {
            typeof console != "undefined" && typeof console.log != "undefined" && console.log(n)
        };
    return {
        showWarning: t
    }
}());