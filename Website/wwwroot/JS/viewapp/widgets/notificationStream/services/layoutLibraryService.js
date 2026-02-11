// ~/viewapp/widgets/notificationStream/services/layoutLibraryService.js
notificationStream.factory("layoutLibrary", function() {
    return {
        inApp: false,
        isPhone: false,
        isTablet: false,
        isDesktop: true,
        
        getDeviceType: function() {
            if (this.isPhone) return 'phone';
            if (this.isTablet) return 'tablet';
            if (this.inApp) return 'app';
            return 'desktop';
        }
    };
});