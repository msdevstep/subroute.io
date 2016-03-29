define(['config', 'durandal/app', 'knockout'], function (config, app, ko) {
    var viewModel = new function () {
        var self = this;

        self.redirectHash = ko.observable();

        self.message = ko.observable();

        self.gitHubAuthorizeUrl = ko.computed(function() {
            return config.gitHubAuthorizeUrl + '?client_id=' + config.gitHubClientId + '&redirect_uri=' + config.gitHubCallbackUrl + '?hash=' + self.redirectHash() + '&scope=' + config.gitHubScope; 
        });

        self.facebookAuthorizeUrl = ko.computed(function() {
            return config.facebookAuthorizeUrl + '?client_id=' + config.facebookClientId + '&redirect_uri=' + config.facebookCallbackUrl + '?hash=' + self.redirectHash() + '&scope=' + config.facebookScope;
        });

        self.microsoftAuthorizeUrl = ko.computed(function() {
            return config.microsoftAuthorizeUrl + '?client_id=' + config.microsoftClientId + '&redirect_uri=' + config.microsoftCallbackUrl + '?hash=' + self.redirectHash() + '&scope=' + config.microsoftScope + '&response_type=code';
        });

        self.hasMessage = ko.computed(function() {
            return !!self.message;
        });

        self.activate = function(data) {
            if (!data) {
                self.message('');
                self.redirectHash('');
                return;
            };

            self.message(data.message);
            self.redirectHash(data.redirectHash);
        };
    };

    return viewModel;
});