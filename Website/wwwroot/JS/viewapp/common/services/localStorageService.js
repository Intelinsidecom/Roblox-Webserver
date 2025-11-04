// ~/viewapp/common/services/localStorageService.js
robloxAppService.factory("localStorageService", ["$log", function(n) {
    return {
        getUserKey: function(n) {
            return "user_" + n
        },
        storage: function() {
            return localStorage
        },
        setLocalStorage: function(n, t) {
            this.storage() && localStorage.setItem(n, JSON.stringify(t))
        },
        getLocalStorage: function(n) {
            if (this.storage()) return JSON.parse(localStorage.getItem(n))
        },
        listenLocalStorage: function(n) {
            this.storage() && angular.isDefined(n) && (window.addEventListener ? window.addEventListener("storage", n, !1) : window.attachEvent("onstorage", n))
        },
        removeLocalStorage: function(n) {
            this.storage() && localStorage.removeItem(n)
        }
    }
}]);