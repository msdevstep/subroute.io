define(['knockout', 'config', 'services/uri', 'plugins/dialog', 'services/ajax'], function (ko, config, uri, dialog, ajax) {
    return function () {
        var self = this;
        
        self.loading = ko.observable(false);
        self.keywords = ko.observable().extend({ rateLimit: { timeout: 500, method: "notifyWhenChangesStop" } });
        self.packages = ko.observableArray([]);

        self.formatAuthorLine = function (package) {
            if (!package.authors)
                return 'By Unknown Author - Version: ' + package.version;

            package.authorsDisplay = 'By ' + package.authors.join(', ') + ' - Version: ' + package.version;
        };

        self.searchPackages = function (keywords) {
            self.loading(true);
            self.packages([]);

            var requestUri = uri.getNugetUri(keywords, 0, 20);

            ajax.request({
                url: requestUri
            }).then(function (data) {
                for (var i = 0; i < data.length; i++) {
                    if (!data[i].authors)
                        break;

                    self.formatAuthorLine(data[i]);
                };

                self.packages(data);
            }).fail(function () {
            }).always(function () {
                self.loading(false);
            });
        };

        self.close = function () {
            dialog.close(self);
        };

        self.activate = function (options) {
            self.keywords.subscribe(function (value) {
                self.searchPackages(value);
            });

            self.searchPackages('');
        };
    };
});