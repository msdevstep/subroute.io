define(['knockout', 'config', 'services/uri', 'plugins/dialog', 'services/ajax'], function (ko, config, uri, dialog, ajax) {
    return function () {
        var self = this;
        
        self.loading = ko.observable(true);

        self.close = function () {
            dialog.close(self);
        };

        self.activate = function (options) {
            self.loading(false);
        };
    };
});