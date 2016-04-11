define(['config'], function (config) {
    return new function () {
        var self = this;

        self.apiUrl = config.apiUrl.replace(/\/+$/, "");

        self.getRoutesUri = function () {
            return self.apiUrl + '/routes/v1/';
        };

        self.getPublicRoutesUri = function () {
            return self.apiUrl + '/gallery/v1/';
        };

        self.getDefaultRouteUri = function() {
            return self.apiUrl + '/routes/v1/default/random';
        };

        self.getRouteUri = function (uri) {
            if (!uri) {
                return self.getRoutesUri();
            };

            return self.getRoutesUri() + uri;
        };

        self.getRouteSettingsUri = function(uri) {
            return self.getRouteUri(uri) + '/settings';
        };

        self.getRequestUri = function (uri, id) {
            return self.getRouteUri(uri) + '/requests/' + id;
        };

        self.getRequestExecutionCountUri = function (uri) {
            return self.getRouteUri(uri) + '/requests/count';
        };

        self.getRoutePublishUri = function (uri) {
            return self.getRoutesUri() + uri + '/publish';
        };

        self.getCompileUri = function () {
            return self.apiUrl + '/compile/v1';
        };

        self.trimSlash = function (value) {
            return value.replace(/\/+$/, "");
        };
    };
});