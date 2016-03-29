define(['services/dialog', 'config', 'services/authentication', 'jquery', 'qtip'], function (dialog, config, auth, $, qtip) {
    return new function () {
        var self = this;

        self.auth = auth;
        self.config = config;

        self.showLogin = function () {
            auth.login();
        };

        self.logout = function () {
            // Hide all open tooltips, then perform logout, otherwise
            // the tooltip doesn't have a chance to hide before reload.
            $('div.qtip:visible').hide(0, auth.logout);
        }
    };
});