define(['durandal/system', 'knockout', 'services/dialog', 'config', 'services/authentication', 'services/uri', 'plugins/router', 'moment', 'durandal/app', 'services/ajax', 'viewmodels/dialogs/request', 'viewmodels/dialogs/nuget', 'ace/ace', 'ace/range', 'highlight'], function (system, ko, dialog, config, authentication, uriBuilder, router, moment, app, ajax, requestModal, nugetModal, ace, range, hljs) {
    return function () {
        var self = this;

        self.defaultSuggestion = ko.observable();
        self.isNew = ko.observable(false);
        self.savedUri = ko.observable();
        self.uri = ko.observable();
        self.title = ko.observable();
        self.action = ko.observable('new');
        self.script = ko.observable('');
        self.isOnline = ko.observable(false);
        self.isCurrent = ko.observable(false);
        self.isDefault = ko.observable(false);
        self.executionCount = ko.observable(0);
        self.settings = ko.observableArray();
        self.packages = ko.observableArray();
        self.updatedOn = ko.observable();
        self.publishedOn = ko.observable();
        self.activePanel = ko.observable('routes/properties.html');
        self.savingRouteSettings = ko.observable(false);
        self.savingRoutePackages = ko.observable(false);
        self.suggesting = ko.observable(false);
        self.publishing = ko.observable(false);
        self.compiling = ko.observable(false);
        self.saving = ko.observable(false);
        self.auth = authentication;
        self.config = config;
        self.toolboxVisible = ko.observable(true);

        self.emptyPackageText = ko.computed(function () {
            if (self.isNew()) {
                return 'Save your route before you can add packages.';
            };

            return 'No packages have been added.';
        });

        self.canAddPackage = ko.computed(function () {
            return !self.isNew() && !self.savingRoutePackages();
        });

        self.hasPackages = ko.computed(function () {
            if (!self.packages()) {
                return false;
            };
            return self.packages().length > 0;
        });

        self.hasSettings = ko.computed(function() {
            if (!self.settings()) {
                return false;
            };
            return self.settings().length > 0;
        });

        self.contentTypes = {
            "application/json": '{\n    "Foo": "Bar"\n}',
            "application/xml": '<request>\n    <foo>Bar</foo>\n</request>',
            "text/html": '<div id="foo">\n    Bar\n</div>',
            "text/plain": 'Foobar',
            "multipart/form-data": '',
            "application/x-www-form-urlencoded": 'foo=bar'
        };

        self.diagnostics = ko.observableArray([]);
        self.diagnosticsVisible = ko.observable(false);
        self.diagnosticsCount = ko.computed(function () {
            if (!self.diagnostics())
                return 0;

            return self.diagnostics().length;
        });

        self.statusMessage = ko.observable("Ready");
        self.statusClass = ko.observable('neutral');

        self.isPublishing = ko.computed(function () {
            return self.publishing() && !self.isNew();
        });

        self.showSuggestionInfo = ko.computed(function () {
            return !!self.defaultSuggestion() && config.enableSuggestions;
        });

        self.showSuccessMessage = function (value) {
            self.statusMessage(value);
            self.statusClass('success');
        }

        self.showNeutralMessage = function (value) {
            self.statusMessage(value);
            self.statusClass('neutral');
        }

        self.showErrorMessage = function (value) {
            self.statusMessage(value);
            self.statusClass('error');
        }

        self.showProperties = function () {
            self.activePanel('routes/properties.html');
        };

        self.showRequests = function () {
            self.activePanel('routes/requests.html');
        };

        self.showClient = function () {
            self.activePanel('routes/client.html');
        };

        self.showSettings = function() {
            self.activePanel('routes/settings.html');
        };

        self.showRequest = function (request) {
            var options = {
                uri: self.uri(),
                id: request.id
            };
            dialog.openModal(requestModal, options);
            return false;
        };

        self.showNuget = function (request) {
            var options = {
                uri: self.uri(),
                id: request.id
            };
            dialog.openModal(nugetModal, options).then(function (package) {
                var exists = ko.utils.arrayFirst(self.packages(), function (item) {
                    return item.id == package.id;
                });

                var packageModel = {
                    id: package.id,
                    version: package.version
                };

                if (!exists) {
                    self.packages.push(packageModel);
                } else {
                    exists.version = package.version;
                };

                self.savePackages().fail(function () {
                    // Remove added package to restore state.
                    self.packages.remove(packageModel);
                });
            });

            return false;
        };

        self.formatPackageText = function (package) {
            var text = package.id + ' ' + package.version;

            if (text && text.length > 40) {
                text = text.substr(0, 40).trim() + '&hellip;';
            };

            return text;
        };

        self.savePackages = function () {
            var requestUri = uriBuilder.getRoutePackagesUri(self.uri());

            self.savingRoutePackages(true);

            return ajax.request({
                url: requestUri,
                type: 'PUT',
                data: ko.toJSON(self.packages()),
                contentType: 'application/json'
            }).then(function (data) {
                // Replace existing packages with packages returned from the server.
                self.packages.removeAll();
                for (var i = 0; i < data.length; i++) {
                    self.packages.push(data[i]);
                };
            }).fail(function (error) {
                alert('Unable to save route packages.');
            }).always(function () {
                self.savingRoutePackages(false);
            });
        };

        self.removePackage = function (package) {
            if (self.savingRoutePackages()) {
                return;
            };

            self.packages.remove(package);

            self.savePackages().fail(function () {
                // Readd the package to the array to restore proper state.
                self.packages.push(package);
            });
        };

        self.showHowTo = function () {
            dialog.openDialog({ name: 'howto', width: '400px', overlay: false });
        };

        self.saveDataForRecall = function () {
            var data = {
                uri: self.uri(),
                title: self.title(),
                code: self.script(),
                isOnline: self.isOnline(),
                isCurrent: self.isCurrent(),
                isDefault: self.isDefault(),
                updatedOn: self.updatedOn(),
                publishedOn: self.publishedOn()
            };

            localStorage.setItem('subroute-unsaved-route', ko.toJSON(data));
        };

        self.hideTooltipStorageValue = ko.computed({
            read: function() {
                return localStorage.getItem('subroute-hide-save-tooltip');
            },
            write: function(value) {
                localStorage.setItem('subroute-hide-save-tooltip', value);
            }
        });

        self.showSaveTooltip = ko.computed(function () {
            return self.showSuggestionInfo() && !self.hideTooltipStorageValue();
        });

        self.hideSaveTooltip = function() {
            self.hideTooltipStorageValue(true);
            $('.save-button-qtip').qtip('destroy');
        };

        self.addSetting = function() {
            if (!self.settings()) {
                self.settings([]);
            };

            self.settings.push({
                name: '',
                value: ''
            });
        };

        self.removeSetting = function(setting) {
            self.settings.remove(setting);
        };

        self.saveRouteSettings = function() {
            var requestUri = uriBuilder.getRouteSettingsUri(self.uri());

            self.savingRouteSettings(true);

            ajax.request({
                url: requestUri,
                type: 'PUT',
                data: ko.toJSON(self.settings()),
                contentType: 'application/json'
            }).then(function (data) {
                // Replace existing settings with settings returned from the server.
                self.settings.removeAll();
                for (var i = 0; i < data.length; i++) {
                    self.settings.push(data[i]);
                };
            }).fail(function (error) {
                alert('Unable to Save Route Settings.');
            }).always(function() {
                self.savingRouteSettings(false);
            });
        };

        self.clearErrors = function () {
            var editor = ace.edit('editor');
            var session = editor.getSession();
            var markers = session.getMarkers();

            for (marker in markers) {
                session.removeMarker(marker);
            }
        };

        self.showErrors = function (diagnostics) {
            var editor = ace.edit('editor');
            var session = editor.getSession();
            var aceRange = range.Range;

            self.clearErrors();

            for (var d = 0; d < diagnostics.length; d++) {
                var diagnostic = diagnostics[d];
                var start = diagnostic.start;
                var end = diagnostic.end;

                // When it's a single character error, nothing will show if we don't add an offset.
                if (start.line == end.line && start.character == end.character) {
                    var line = session.getLine(start.line);

                    if (end.character >= line.length) {
                        start.character--;
                    } else {
                        end.character++;
                    };
                }

                var css = diagnostic.severity == 'Error' ? 'ace_error' : 'ace_warning';

                var markerId = session.addMarker(new aceRange(start.line, start.character, end.line, end.character), css, 'text');
            }
        };

        self.compile = function () {
            var requestUri = uriBuilder.getCompileUri();

            self.compiling(true);
            self.showNeutralMessage('Compiling...');

            return ajax.request({
                url: requestUri,
                type: 'POST',
                data: self.script(),
                contentType: 'text/plain'
            }).then(function (data) {
                self.clearErrors();

                system.log(data);
                self.diagnostics([]);
                self.diagnosticsVisible(false);
                system.log(data);
                self.showSuccessMessage('Compiled Successfully');
            }).fail(function (error) {
                var data = JSON.parse(error.responseText);

                self.showErrors(data.diagnostics);

                self.diagnostics(data.diagnostics);
                self.diagnosticsVisible(true);
                self.showErrorMessage('Compile Failed - ' + self.diagnosticsCount() + ' Message(s)');
            }).always(function () {
                self.compiling(false);
            });
        };

        self.publish = function () {
            // Save data if we aren't authenticated and present auth dialog.
            if (!self.auth.isAuthenticated()) {
                self.saveDataForRecall();
                self.auth.login('routes/recall');

                return true;
            };

            var requestUri = uriBuilder.getRoutePublishUri(self.uri());
            var publish = function () {
                self.publishing(true);
                self.compiling(true);
                self.showNeutralMessage('Publishing...');
                return ajax.request({
                    url: requestUri,
                    type: 'POST',
                    dataType: 'json'
                }).then(function (data) {
                    system.log(data);
                    self.updatePageData(data.route);
                    self.diagnostics([]);
                    self.diagnosticsVisible(false);
                    system.log(data);
                    self.showSuccessMessage('Published Successfully');
                }).fail(function (error) {
                    var data = JSON.parse(error.responseText);
                    self.updatePageData(data.route);
                    self.diagnostics(data.compilation.diagnostics);
                    self.diagnosticsVisible(true);
                    self.showErrorMessage('Publish Failed - ' + self.diagnosticsCount() + ' Message(s)');
                }).always(function () {
                    self.publishing(false);
                    self.compiling(false);
                });
            };

            self.publishing(true);
            return self.saveChanges()
                .then(publish)
                .fail(function () { self.publishing(false); });
        };

        self.saveChanges = function () {
            // Save data if we aren't authenticated and present auth dialog.
            if (!self.auth.isAuthenticated()) {
                self.saveDataForRecall();
                self.auth.login('routes/recall');
                return false;
            };

            self.saving(true);
            self.showNeutralMessage('Saving...');

            var payload = {
                userId: authentication.id(),
                uri: self.uri(),
                title: self.title(),
                code: self.script(),
                isDefault: self.isDefault()
            };

            var requestUri = uriBuilder.getRouteUri();
            var type = 'POST';

            // Switch to edit mode if we have a uri and are editing.
            if (!self.isNew()) {
                requestUri = uriBuilder.getRouteUri(self.savedUri());
                type = 'PATCH';

                // Discard uri, type, and userid as we are only editing the code.
                delete payload.userId;
            };

            return ajax.request({
                url: requestUri,
                type: type,
                data: ko.toJSON(payload),
                contentType: 'application/json'
            }).then(function (data) {
                system.log(data);

                // For new routes, or if the route uri has changed
                // then navigate to the new edit page.
                if (self.isNew() || self.uri() !== self.savedUri()) {
                    router.navigate('#routes/edit/' + self.uri());
                }

                self.updatePageData(data);
                self.showSuccessMessage('Saved Successfully');
            }).fail(function (error) {
                self.showErrorMessage('Save Failed');
            }).always(function () {
                self.saving(false);
            });
        };

        self.publishAndRun = function () {
            // Save data if we aren't authenticated and present auth dialog.
            if (!self.auth.isAuthenticated()) {
                self.saveDataForRecall();
                self.auth.login('routes/recall');
                return;
            };

            self.publish().then(self.compile).then(self.showClient).then(function () {
                self.showSuccessMessage('Published Successfully');
                self.execute();
            });
        };

        self.loadRandomSuggestion = function () {
            self.suggesting(true);
            var requestUri = uriBuilder.getDefaultRouteUri();

            ajax.request({
                url: requestUri
            }).then(function (data) {
                self.defaultSuggestion(data);
                self.script(data.code);

                $('.qtip').show();

                // Force editor to resize after suggestion bar shows.
                self.resizeEditor();
            }).fail(function () {
                self.script(config.blankRoute);
            }).always(function () {
                self.suggesting(false);
            });
        };

        self.loadRoute = function (uri) {
            var requestUri = uriBuilder.getRouteUri(uri);

            ajax.request({
                url: requestUri
            }).then(function (data) {
                self.updatePageData(data);
                self.updateExecutionCount(uri);
                system.log(data);
            }).fail(function (error) {
                system.error(error);
            });
        };

        self.useBlankDocument = function () {
            self.script(config.blankRoute);
            self.defaultSuggestion(null);
        };

        self.updateExecutionCount = function () {
            if (!self.uri()) {
                return;
            };

            var requestUri = uriBuilder.getRequestExecutionCountUri(self.uri());

            ajax.request({
                url: requestUri
            }).then(function (data) {
                self.executionCount(data);
            }).fail(function (error) {
                self.executionCount('Unknown');
                system.error(error);
            });
        };

        self.updatePageData = function (route) {
            self.title(route.title);
            self.script(route.code);
            self.isOnline(route.isOnline);
            self.isCurrent(route.isCurrent);
            self.isDefault(route.isDefault);
            self.settings(route.settings);
            self.packages(route.packages);
            self.updatedOn(route.updatedOn);
            self.publishedOn(route.publishedOn);
        };

        self.generateUri = function () {
            // Use epoch (milliseconds) to generate a semi-unique route name as a suggestion.
            // Epoch time has too much precision (13 digits), so we'll trim all but the last 
            // 7 characters of the total number to get a small enough integer to display.
            var epoch = (new Date).getTime();
            var epochPartial = epoch.toString().substring(7);
            return 'route-' + epochPartial;
        };

        self.formatPosition = function (diagnostic) {
            return 'Line ' + (diagnostic.start.line + 1) + ', Character ' + diagnostic.start.character;
        };

        self.canActivate = function (action, uri) {
            if (!action) {
                if (config.enableSuggestions) {
                    return { redirect: '#routes/suggest' }
                };
                return { redirect: '#routes/new' }
            };

            self.uri.subscribe(function (value) {
                app.trigger('uri:changed', value);
            });

            app.on('suggestion:new', function() {
                self.loadRandomSuggestion();
            });

            return true;
        };

        self.activate = function (action, uri) {
            // Change action to new if suggestions aren't allowed.
            if (!config.enableSuggestions && action === 'suggest') {
                action = 'new';
            };

            self.action(action);

            // Setup default selected payload.
            self.switchDefaultPayload();

            // Build random uri for new routes.
            var generatedUri = self.generateUri();

            // Load random default code for suggest requests.
            if (action === 'suggest') {
                self.isNew(true);

                self.loadRandomSuggestion();
                self.uri(generatedUri);

                return;
            };

            // Load local code (if any) for create requests.
            if (action === 'new') {
                self.isNew(true);

                self.uri(generatedUri);
                self.script(config.blankRoute);

                return;
            };

            if (action === 'recall') {
                self.isNew(true);

                var unsaved = localStorage.getItem('subroute-unsaved-route');
                var data;

                if (unsaved) {
                    data = JSON.parse(unsaved);
                };

                if (data) {
                    self.uri(data.uri);
                    self.updatePageData(data);
                };

                return;
            };

            // We have a uri, so we will be loading and saving via the API.
            self.isNew(false);
            self.savedUri(uri);
            self.uri(uri);
            self.loadRoute(uri);
            self.reloadRequests();
        };

        self.bindingComplete = function () {
            // Ensure uri gets updated globally so header will update.
            app.trigger('uri:changed', self.uri());
        };

        self.compositionComplete = function () {
            $('#toolbox').on('scroll', function () {
                $('#save-button').qtip('reposition');
            });
        };

        self.resizeEditor = function() {
            var editor = ace.edit('editor');
            editor.resize();
        };

        self.canDeactivate = function () {
            return true;
        };

        self.intellisense = function (innerEditor, session, pos, prefix, callback) {
            var uri = config.apiUrl + 'intellisense/v1?wordToComplete=' + prefix + '&character=' + pos.column + '&line=' + pos.row + '&wantSnippet=true&wantDocumentationForEveryCompletionResult=true&wantReturnType=true&wantKind=true&wantMethodHeader=true';

            ajax.request({
                url: uri,
                type: 'POST',
                data: innerEditor.getValue(),
                contentType: 'text/plain'
            }).then(function (data) {
                var index = 0;
                var completions = ko.utils.arrayMap(data, function (item) {
                    var display = item.displayText;
                    var signiture = hljs.highlight('csharp', item.displayText).value;

                    if (display.length > 60) {
                        display = display.substring(0, 60).trim() + '…';
                    };

                    return {
                        caption: display,
                        snippet: item.snippet,
                        meta: item.kind,
                        type: "snippet",
                        score: -(++index),
                        docHTML: "<b>" + signiture + "</b>\n" + item.description
                    }
                });
                callback(null, completions);
            });
        };

        self.requests = ko.observableArray([]);
        self.requestsLoading = ko.observable(false);

        self.requestsVisible = ko.computed(function () {
            return self.requests() && self.requests().length > 0;
        });

        self.requestsUri = ko.computed(function () {
            return config.apiUrl + 'routes/v1/' + self.uri() + '/requests';
        });

        self.reloadRequests = function () {
            self.requestsLoading(true);
            self.requests.removeAll();

            var uri = self.requestsUri();

            ajax.request({
                url: uri,
                type: 'GET',
                data: {
                    skip: 0,
                    take: 10,
                    from: '2001-09-11',
                    to: moment().format()
                }
            }).then(function (data) {
                ko.utils.arrayForEach(data.results, function (item) {
                    self.requests.push(item);
                });
                self.updateExecutionCount();
            }).always(function () {
                self.requestsLoading(false);
            });
        };

        /* Execution Engine */
        self.executionMethod = ko.observable('GET');
        self.executionHeaders = ko.observable('Content-Type: application/json');
        self.executionPayload = ko.observable('');
        self.executionResponse = ko.observable('');
        self.executionResponseHeaders = ko.observable('');
        self.executionStatusCode = ko.observable(0);
        self.executionStatusMessage = ko.observable('Nothing');
        self.executionRequestContentType = ko.observable('application/json');
        self.executionResponseContentType = ko.observable('');
        self.executionContentTypeRegEx = new RegExp('^Content\-Type', 'i');
        self.executionLoading = ko.observable(false);

        self.executionResponseVisible = ko.computed(function () {
            return self.executionStatusCode() > 0;
        });

        self.executionUri = ko.computed(function () {
            return config.apiUrl + 'v1/' + self.uri();
        });

        self.getContentTypeSyntax = function (contentType) {
            contentType = contentType || '';

            if (contentType.lastIndexOf('application/json', 0) === 0) {
                return 'json';
            };
            if (contentType.lastIndexOf('application/xml', 0) === 0) {
                return 'xml';
            };
            if (contentType.lastIndexOf('text/html', 0) === 0) {
                return 'html';
            };

            return "text";
        };

        self.executionRequestSyntax = ko.computed(function () {
            var contentType = self.executionRequestContentType();
            return self.getContentTypeSyntax(contentType);
        });

        self.executionResponseSyntax = ko.computed(function () {
            var contentType = self.executionResponseContentType();
            return self.getContentTypeSyntax(contentType);
        });

        self.resetResponse = function () {
            self.executionStatusCode(0);
            self.executionStatusMessage('');
            self.executionResponse('');
            self.executionResponseHeaders('');
            self.executionResponseContentType('');
        };

        self.execute = function () {
            self.resetResponse();
            self.executionLoading(true);

            var payload = null;
            if (self.executionMethod() !== 'GET') {
                payload = self.executionPayload();
            };

            // Clear existing response payload.
            self.executionResponse('');

            // We will use standard ajax call for this since it doesn't require a token
            // and our ajax implementation doesn't support the advanced scenario.
            $.ajax({
                url: self.executionUri(),
                type: self.executionMethod(),
                data: payload,
                dataType: 'text',
                beforeSend: function (xhr) {
                    var headers = self.executionHeaders() || '';
                    var lines = headers.split('\n');

                    ko.utils.arrayForEach(lines, function (line) {
                        var segments = line.split(':');

                        if (segments.length >= 2) {
                            xhr.setRequestHeader(segments[0], segments[1]);
                        };
                    });
                }
            }).done(function (data, xhr, request) {
                var headers = request.getAllResponseHeaders();
                var contentType = self.getContentTypeHeader(headers);

                self.executionStatusMessage(request.statusText);
                self.executionResponseHeaders(headers);
                self.executionResponse(request.responseText);
                self.executionResponseContentType(contentType);
                self.executionStatusCode(request.status);
            }).fail(function (error) {
                var headers = error.getAllResponseHeaders();
                var contentType = self.getContentTypeHeader(headers);

                self.executionStatusMessage(error.statusText);
                self.executionResponseHeaders(headers);
                self.executionResponse(error.responseText);
                self.executionResponseContentType(contentType);
                self.executionStatusCode(error.status);
            })
            .always(function () {
                self.executionLoading(false);
                self.updateExecutionCount();
            });
        };

        self.executionRequestContentType.subscribe(function (value) {
            var lines = (self.executionHeaders() || '').split('\n');
            var hasContentType = false;

            lines = ko.utils.arrayMap(lines, function (line) {
                if (self.executionContentTypeRegEx.test(line || '')) {
                    if (hasContentType) {
                        return false;
                    };

                    hasContentType = true;
                    return 'Content-Type: ' + value;
                }

                return line;
            });

            // Remove all blank lines.
            lines = ko.utils.arrayFilter(lines, function (line) {
                return !!line;
            });

            var result = lines.join('\n');

            if (!hasContentType) {
                result = 'Content-Type: ' + value + '\n' + result;
            };

            self.executionHeaders(result);
            self.switchDefaultPayload();
        });

        self.getContentTypeHeader = function (headers) {
            var lines = headers.split('\n');
            var contentType = ko.utils.arrayFirst(lines, function (line) {
                return self.executionContentTypeRegEx.test(line || '')
            });

            if (contentType) {
                var segments = contentType.split(':');

                if (segments.length >= 2) {
                    return segments[1].trim();
                };
            }
        };

        self.switchDefaultPayload = function () {
            var currentPayload = self.executionPayload();
            var isDefaultPayload = false;

            // Exit if the current payload is not a default payload.
            for (var contentType in self.contentTypes) {
                if (!self.contentTypes.hasOwnProperty(contentType)) {
                    continue;
                };

                var payload = self.contentTypes[contentType];

                if (currentPayload === payload) {
                    // We found a payload match, indicate, and exit loop.
                    isDefaultPayload = true;
                    break;
                };
            };

            // We can update the payload with a new default, if we 
            // found a match or there is no current payload.
            if (isDefaultPayload || currentPayload === '') {
                var currentContentType = self.executionRequestContentType();
                var replacementPayload = self.contentTypes[currentContentType];
                self.executionPayload(replacementPayload);
            };
        };
        /* End Execution Engine */
    };
});