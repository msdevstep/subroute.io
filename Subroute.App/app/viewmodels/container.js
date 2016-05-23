define(['plugins/router', 'knockout', 'services/dialog', 'config', 'services/authentication', 'durandal/app', 'moment'], function (router, ko, dialog, config, authentication, app, moment) {
    var rootRouter = router
    .makeRelative({ moduleId: 'viewmodels' })
    .map([
        { route: ['', 'routes/:action(/:uri)'], moduleId: 'routes', title: 'Routes', nav: true },
        { route: 'documentation', moduleId: 'documentation', title: 'Documentation', nav: true },
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
        self.apiUriVisible = ko.observable(false);
        self.config = config;
        self.auth = authentication;

        self.selectApiUri = function () {
            $('#script-title').select();
        };

        self.showFullUri = function () {
            self.apiUriVisible(true);
        };

        self.activeScriptName = ko.computed(function() {
            var title = 'subroute.io';

            if (self.scriptName()) {
                title += ' / <span class="script-name">' + self.scriptName() + '</span>';
            };

            return title;
        });

        self.activeApiScriptName = ko.computed(function() {
            var title = config.apiUrl;

            if (self.scriptName()) {
                title += 'v1/' + self.scriptName();
            };

            return '<input id="script-title" type="text" class="script-name-api" readonly="readonly" value="' + title + '" />';
        });

        self.toggleScriptUri = function() {

        };

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

        self.showNextSuggestion = function() {
            app.trigger('suggestion:new', {});
            return true;
        };

        self.hideActiveElements = function() {
            dialog.closeDialog();
            self.apiUriVisible(false);

            // When we don't return true here, it prevent various knockout bindings from functioning properly.
            // Since this is attached to bubbled click events. Returning false or nothing prevents that bubbling.
            return true;
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

        self.compositionComplete = function () {
            UserVoice.push(['addTrigger', '#uservoice']);
        };
    };
});