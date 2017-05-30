define(['knockout', 'config', 'services/uri', 'plugins/dialog', 'services/ajax', 'durandal/system'], function (ko, config, uri, dialog, ajax, system) {
    return function () {
        var self = this;
        
        self.loading = ko.observable(false);
        self.keywords = ko.observable('').extend({ rateLimit: { timeout: 500, method: "notifyWhenChangesStop" } });
        self.skip = ko.observable(0);
        self.take = ko.observable(20);
        self.totalCount = ko.observable(0);
        self.packages = ko.observableArray([]);
        self.pageCount = ko.computed(function () {
            return self.packages().length;
        });

        self.formatAuthorLine = function (package) {
            if (!package.authors)
                return 'By Unknown Author - Version: ' + package.version;

            package.authorsDisplay = 'By ' + package.authors.join(', ') + ' - Version: ' + package.version;
        };

        self.nextPageAvailable = ko.computed(function () {
            return self.skip() + self.pageCount() < self.totalCount();
        });

        self.nextPage = function () {
            if (!self.nextPageAvailable()) {
                return;
            }
            
            self.skip(self.skip() + self.take());
        };

        self.previousPageAvailable = ko.computed(function () {
            return self.skip() > 0;
        });

        self.previousPage = function () {
            var skip = self.skip() - self.take();

            if (skip < 0)
                skip = 0;

            self.skip(skip);
        };

        self.pageDetails = ko.computed(function () {
            var index = self.skip();
            var limit = Math.min(self.skip() + self.take(), self.totalCount());
            var total = self.totalCount();

            return index + ' - ' + limit + ' of ' + total;
        });

        self.selectPackage = function (package) {
            system.log(package);
            dialog.close(self, package);
        };

        self.searchPackages = function () {
            self.loading(true);
            self.packages([]);

            var requestUri = uri.getNugetUri(self.keywords(), self.skip(), self.take());

            ajax.request({
                url: requestUri
            }).then(function (data) {
                var packages = data.results;

                for (var i = 0; i < packages.length; i++) {
                    if (!packages[i].authors)
                        break;

                    self.formatAuthorLine(packages[i]);
                };
                
                self.totalCount(data.totalCount);
                self.packages(packages);
            }).fail(function () {
            }).always(function () {
                self.loading(false);

                system.log('Keywords: ' + self.keywords());
                system.log('Skip: ' + self.skip());
                system.log('Take: ' + self.take());
                system.log('Page Count: ' + self.pageCount());
                system.log('Total Count: ' + self.totalCount());
            });
        };

        self.close = function () {
            dialog.close(self);
        };

        self.activate = function (options) {
            self.skip.subscribe(self.searchPackages);
            self.keywords.subscribe(function () {
                self.skip(0);
                self.searchPackages();
            });

            self.searchPackages('');
        };
    };
});