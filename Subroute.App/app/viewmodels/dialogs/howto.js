define(['knockout', 'config'], function (ko, config) {
    return new function () {
        var self = this;

        self.apiEndpoint = ko.computed(function() {
            return config.apiUrl + 'v1/{your-route-uri}';
        });

        self.activate = function () {

        };
    };
});