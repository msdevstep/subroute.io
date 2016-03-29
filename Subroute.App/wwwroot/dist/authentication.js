define(['jquery', 'durandal/system', 'config', 'knockout', 'cookie', 'base64', 'moment'], function ($, system, config, ko, cookie, base64, moment) {
    var vm = function () {
        var self = this;

        self.isAuthenticated = ko.observable(false);
        self.provider = ko.observable();
        self.token = ko.observable();
        self.id = ko.observable();
        self.username = ko.observable();
        self.avatarUri = ko.observable();
        self.name = ko.observable();
        self.email = ko.observable();
        self.initials = ko.observable();
        self.isAdmin = ko.observable(false);

        self.useInitials = ko.computed(function () {
            return !self.avatarUri();
        });

        self.lock = new window.Auth0Lock('NuIcxXuxDMf2xZkP0kTX0aa28RyoFvAJ', 'subroute.auth0.com');
        self.login = function (callbackHash) {
            var options = {
                authParams: {
                    scope: 'openid profile offline_access',
                    state: callbackHash
                }
            };

            lock.show(options);
        };

        self.logout = function () {
            self.isAuthenticated(false);
            self.provider(null);
            self.token(null);
            self.id(null);
            self.username(null);
            self.avatarUri(null);
            self.name(null);
            self.email(null);
            self.initials(null);
            self.isAdmin(false);

            localStorage.removeItem('id_token');
            localStorage.removeItem('refresh_token');
            window.location.href = '/';
        };

        self.isExpired = ko.computed(function () {
            //if (!self.expires()) {
            //    return true;
            //};

            //var now = moment.utc();
            //var expireDate = moment.unix(self.expires());

            //// Current UTC data is after expiration data, so token is expired.
            //if (now.isAfter(expireDate)) {
            //    return true;
            //}

            return false;
        });

        self.initialize = function () {
            var idToken = localStorage.getItem('id_token');
            system.log(idToken);
            if (idToken) {
                lock.getProfile(idToken, function (err, profile) {
                    if (err) {
                        return alert('There was an error geting the profile: ' + err.message);
                    }
                    system.log(profile);
                    self.isAuthenticated(true);
                    self.id(profile.user_id);
                    self.token(idToken);
                    self.username(profile.nickname);
                    self.avatarUri(profile.picture);
                    self.name(profile.name);
                    self.email(profile.email);

                    if (profile.user_metadata && profile.user_metadata.isAdmin) {
                        self.isAdmin(profile.user_metadata.isAdmin);
                    };

                    var cookieMatches = profile.name.match(/\b(\w)/g);

                    if (cookieMatches) {
                        self.initials(cookieMatches.join(''));
                    };

                    if (profile.identities && profile.identities.length > 0) {
                        self.provider(profile.identities[0].provider);
                    };
                });
            };
        };
    };

    return new vm();
});