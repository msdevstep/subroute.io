define(['knockout', 'services/uri', 'durandal/system', 'services/dialog', 'services/ajax', 'durandal/app'], function (ko, uriBuilder, system, dialog, ajax, app) {
    return new function () {
        var self = this;

        self.routes = ko.observableArray();
        self.routesLoading = ko.observable(false).extend({ throttle: 200 });

        self.hasRoutes = ko.computed(function () {
            return self.routesLoading() || (self.routes() && self.routes().length >= 0);
        });

        self.hideDialog = function() {
            dialog.closeDialog();
            return true;
        };

        self.activate = function () {
            var uri = uriBuilder.getRoutesUri();

            self.routesLoading(true);
            self.routes([]);

            ajax.request({
                url: uri
            }).then(function(routes) {
                self.routes(routes);
            }).fail(function(error) {
                system.error(error);
            }).always(function() {
                app.trigger('routes:loading', false);
                self.routesLoading(false);
            });
        };
    };
});