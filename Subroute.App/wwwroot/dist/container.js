define(['plugins/router', 'knockout', 'services/dialog', 'config', 'services/authentication', 'durandal/app', 'moment'], function (router, ko, dialog, config, authentication, app, moment) {
    var rootRouter = router
    .makeRelative({ moduleId: 'viewmodels' })
    .map([
        { route: ['', 'routes/:action(/:uri)'], moduleId: 'routes', title: 'Routes', nav: true },
        { route: 'gallery', moduleId: 'gallery', title: 'Gallery', nav: true }
    ]).buildNavigationModel();

    rootRouter.activate();

    return new function () {
        var self = this;

        self.dialog = dialog;
        self.router = rootRouter;
        self.expanded = ko.observable(false);
        self.heroVisible = ko.observable(false);
        self.scriptName = ko.observable();
        self.routesLoading = ko.observable(false);
        self.config = config;
        self.auth = authentication;

        self.activeHash = ko.computed(function () {
            if (!self.router.activeInstruction()) {
                return '#';
            };

            return self.router.activeInstruction();
        });

        self.showRoutes = function () {
            if (!self.auth.isAuthenticated()) {
                self.auth.login();

                return;
            };

            app.trigger('routes:loading', true);
            dialog.openDialog({ name: 'list', width: '300px', overlay: false });
        };

        self.hideHero = function() {
            localStorage.setItem('subroute-hide-hero', moment().format());
            self.heroVisible(false);
        };

        self.activate = function() {
            var visited = localStorage.getItem('subroute-hide-hero');

            if (!visited) {
                self.heroVisible(true);
            };

            app.on('uri:changed').then(function(uri) {
                self.scriptName(uri);
            });

            app.on('routes:loading', function(value) {
                self.routesLoading(value);
            });
        };
    };
});