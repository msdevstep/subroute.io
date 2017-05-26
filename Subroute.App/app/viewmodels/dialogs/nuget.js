define(['knockout', 'config', 'services/uri', 'plugins/dialog', 'services/ajax'], function (ko, config, uri, dialog, ajax) {
    return function () {
        var self = this;
        
        self.loading = ko.observable(false);
        self.keywords = ko.observable().extend({ rateLimit: { timeout: 500, method: "notifyWhenChangesStop" } });
        self.skip = ko.observable(0);
        self.take = ko.observable(0);
        self.totalCount = ko.observable(0);
        self.packages = ko.observableArray([]);

        self.formatVersion = function (version) {
            if (!version) {
                return '0.0.0.0';
            }

            return [version.major, version.minor, version.build, version.revision].join('.');
        };

        self.formatAuthorLine = function (package) {
            if (!package.authors)
                return 'By Unknown Author - Version: ' + self.formatVersion(package.version);

            package.authorsDisplay = 'By ' + package.authors.join(', ') + ' - Version: ' + self.formatVersion(package.version);
        };

        self.searchPackages = function (keywords) {
            self.loading(true);
            self.packages([]);

            var requestUri = uri.getNugetUri(keywords, 0, 20);

            ajax.request({
                url: requestUri
            }).then(function (data) {
                var packages = data.results;

                for (var i = 0; i < packages.length; i++) {
                    if (!packages[i].authors)
                        break;

                    self.formatAuthorLine(packages[i]);
                };

                self.skip(data.skip);
                self.take(data.take);
                self.totalCount(data.totalCount);
                self.packages(packages);
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