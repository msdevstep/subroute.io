define(['knockout', 'config', 'services/uri', 'plugins/dialog', 'services/ajax'], function (ko, config, uri, dialog, ajax) {
    return function () {
        var self = this;
        var Base64 = { _keyStr: "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=", encode: function (e) { var t = ""; var n, r, i, s, o, u, a; var f = 0; e = Base64._utf8_encode(e); while (f < e.length) { n = e.charCodeAt(f++); r = e.charCodeAt(f++); i = e.charCodeAt(f++); s = n >> 2; o = (n & 3) << 4 | r >> 4; u = (r & 15) << 2 | i >> 6; a = i & 63; if (isNaN(r)) { u = a = 64 } else if (isNaN(i)) { a = 64 } t = t + this._keyStr.charAt(s) + this._keyStr.charAt(o) + this._keyStr.charAt(u) + this._keyStr.charAt(a) } return t }, decode: function (e) { var t = ""; var n, r, i; var s, o, u, a; var f = 0; e = e.replace(/[^A-Za-z0-9\+\/\=]/g, ""); while (f < e.length) { s = this._keyStr.indexOf(e.charAt(f++)); o = this._keyStr.indexOf(e.charAt(f++)); u = this._keyStr.indexOf(e.charAt(f++)); a = this._keyStr.indexOf(e.charAt(f++)); n = s << 2 | o >> 4; r = (o & 15) << 4 | u >> 2; i = (u & 3) << 6 | a; t = t + String.fromCharCode(n); if (u != 64) { t = t + String.fromCharCode(r) } if (a != 64) { t = t + String.fromCharCode(i) } } t = Base64._utf8_decode(t); return t }, _utf8_encode: function (e) { e = e.replace(/\r\n/g, "\n"); var t = ""; for (var n = 0; n < e.length; n++) { var r = e.charCodeAt(n); if (r < 128) { t += String.fromCharCode(r) } else if (r > 127 && r < 2048) { t += String.fromCharCode(r >> 6 | 192); t += String.fromCharCode(r & 63 | 128) } else { t += String.fromCharCode(r >> 12 | 224); t += String.fromCharCode(r >> 6 & 63 | 128); t += String.fromCharCode(r & 63 | 128) } } return t }, _utf8_decode: function (e) { var t = ""; var n = 0; var r = c1 = c2 = 0; while (n < e.length) { r = e.charCodeAt(n); if (r < 128) { t += String.fromCharCode(r); n++ } else if (r > 191 && r < 224) { c2 = e.charCodeAt(n + 1); t += String.fromCharCode((r & 31) << 6 | c2 & 63); n += 2 } else { c2 = e.charCodeAt(n + 1); c3 = e.charCodeAt(n + 2); t += String.fromCharCode((r & 15) << 12 | (c2 & 63) << 6 | c3 & 63); n += 3 } } return t } }

        self.loading = ko.observable(true);
        self.activePanel = ko.observable({
            view: 'dialogs/request/details.html',
            compositionComplete: function () {
                dialog.getContext().reposition('.messageBox');
            }
        });
        self.request = ko.observable();
        self.requestPayloadSyntax = ko.observable('json');
        self.responsePayloadSyntax = ko.observable('json');

        self.statusCodeAndMessage = ko.computed(function() {
            if (!self.request()) {
                return '';
            };

            if (!self.request().statusMessage) {
                return self.request().method + ' ' + self.request().statusCode;
            };

            return self.request().method + ' ' + self.request().statusCode + ' - ' + self.request().statusMessage;
        });

        self.requestHeaders = ko.computed({
            read: function() {
                if (!self.request()) {
                    return '';
                };

                return ko.toJSON(self.request().requestHeaders);
            },
            write: function(value) {

            }
        });

        self.responseHeaders = ko.computed({
            read: function () {
                if (!self.request()) {
                    return '';
                };

                return ko.toJSON(self.request().responseHeaders);
            },
            write: function (value) {

            }
        });

        self.requestPayload = ko.computed({
                read: function () {
                    if (!self.request()) {
                        return '';
                    };

                    if (!self.request().requestPayload) {
                        return '';
                    };

                    return Base64.decode(self.request().requestPayload);
                },
                write: function (value) {

                }
            });

        self.responsePayload = ko.computed({
                read: function () {
                    if (!self.request()) {
                        return '';
                    };

                    if (!self.request().responsePayload) {
                        return '';
                    };

                    return Base64.decode(self.request().responsePayload);
                },
                write: function (value) {

                }
            });

        self.showDetails = function () {
            self.activePanel({
                view: 'dialogs/request/details.html',
                compositionComplete: function () {
                    dialog.getContext().reposition('.messageBox');
                }
            });
        };

        self.showHeaders = function () {
            self.activePanel({
                view: 'dialogs/request/headers.html',
                compositionComplete: function () {
                    dialog.getContext().reposition('.messageBox');
                }
            });
        };

        self.showRequestPayload = function () {
            self.activePanel({
                view: 'dialogs/request/request-payload.html',
                compositionComplete: function () {
                    dialog.getContext().reposition('.messageBox');
                }
            });
        };

        self.showResponsePayload = function () {
            self.activePanel({
                view: 'dialogs/request/response-payload.html',
                compositionComplete: function () {
                    dialog.getContext().reposition('.messageBox');
                }
            });
        };

        self.close = function () {
            dialog.close(self);
        };

        self.activate = function (options) {
            var requestUri = uri.getRequestUri(options.uri, options.id);

            ajax.request({
                url: requestUri
            }).then(function (request) {
                self.request(request);
            }).fail(function () {
                dialog.showMessage('Unable to load selected request, please try again.', 'Request Unavailable', ['Ok']);
            })
            .always(function () {
                self.loading(false);
            });
        };
    };
});