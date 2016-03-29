define(['durandal/app', 'knockout', 'plugins/dialog'], function(app, ko, modal) {
    var service = new function() {
        var self = this;

        self.activeDialog = ko.observable();
        self.width = ko.observable('200px');

        self.visible = ko.observable(false);
        self.overlayVisible = ko.observable(false);

        self.openDialog = function (properties, data) {
            self.overlayVisible(!!properties.overlay);
            self.width(properties.width);
            self.activeDialog({
                model: 'viewmodels/dialogs/' + properties.name,
                view: 'views/dialogs/' + properties.name,
                activationData: data
            });
            self.visible(true);
        };

        self.openModal = function(module, options) {
            return modal.show(new module(), options);
        }

        self.closeDialog = function () {
            self.visible(false);
            self.overlayVisible(false);
            self.activeDialog(null);
        };

        self.activate = function () {

        };
    };

    return service;
});