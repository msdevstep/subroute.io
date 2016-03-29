define(['knockout', 'services/uri', 'moment', 'services/ajax'], function (ko, uriBuilder, moment, ajax) {
    return function () {
        var self = this;
        self.routes = ko.observableArray([]);
        self.routesLoading = ko.observable(false);

        self.loadRoutes = function() {
            self.routesLoading(true);
            self.routes.removeAll();

            ajax.request({
                url: uriBuilder.getPublicRoutesUri(),
                type: 'GET',
                data: {
                    skip: 0,
                    take: 100,
                    search: ''
                }
            }).then(function (data) {
                ko.utils.arrayForEach(data.results, function (item) {
                    self.routes.push(item);
                    console.log(self.routes());
                });
            }).always(function () {
                self.routesLoading(false);
            });
        };

        self.formatTimestamp = function (updatedOn) {
            if (!updatedOn)
                return '';

            var dateInt = Date.parse(updatedOn);
            return moment(dateInt).format('MM/DD/YYYY h:mm A');
        };

        self.activate = function (token) {
            self.loadRoutes();
        };
    };
});