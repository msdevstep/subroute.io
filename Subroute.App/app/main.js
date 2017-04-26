requirejs.config({
    paths: {
        'text': '../lib/requirejs-text/text',
        'durandal': '../lib/durandal/js',
        'plugins': '../lib/durandal/js/plugins',
        'transitions': '../lib/durandal/js/transitions',
        'jquery': '//code.jquery.com/jquery-2.2.3.min',
        'jquery-ui': '//code.jquery.com/ui/1.11.4/jquery-ui.min',
        'base64': '../lib/jquery.base64.js/jquery.base64',
        'knockout': '//cdnjs.cloudflare.com/ajax/libs/knockout/3.4.0/knockout-min',
        'validation': '../lib/knockout.validation/Dist/knockout.validation.min',
        'mapping': '../lib/knockout.mapping/knockout.mapping/build/knockout.mapping',
        'cookie': '../lib/jquery.cookie/jquery.cookie',
        'moment': '../lib/moment/min/moment.min',
        'ace': '//cdnjs.cloudflare.com/ajax/libs/ace/1.2.6',
        'config': 'config?noext',
        'markdown': '../lib/markdown/markdown',
        'qtip': '../lib/qtip2/jquery.qtip.min',
        'modes': '../modes',
        'highlight': '../lib/highlightjs/highlight.pack',
        'remarkable': '../lib/remarkable/dist/remarkable'
    },
    shim: {
        'base64': {
            deps: ['jquery']
        },
        'qtip': {
            deps: ['jquery']
        },
        'ace/ext-language_tools': {
            deps: ['ace/ace']
        }
    }
});

define(['require', 'config', 'durandal/system', 'durandal/app', 'durandal/viewLocator', 'jquery', 'knockout', 'validation', 'services/authentication', 'plugins/router'], function (require, config, system, app, viewLocator, $, ko, val, auth, router) {
    system.debug(config.debug);

    /* This should be updated for each version to force files to be recached for the new version */
    requirejs.config({
        urlArgs: 'v=' + config.cacheVersion
    });

    // Setup global authentication
    $.ajaxSetup({
        beforeSend: function (xhr) {
            // During debug mode, enable custom tracing to help with optimizations.
            if (system.debug()) {
                xhr.setRequestHeader('Enable-Tracing', 'true');
                system.log('Tracing Enabled');
            }

            // Don't add the header if a local ajax request added one first.
            if (this.headers && this.headers.Authorization)
                return;

            var token = auth.token();
            if (token)
                xhr.setRequestHeader('Authorization', 'Bearer ' + token);
        }
    });

    // Setup knockout validation engine.
    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: false,
        parseInputAttributes: true,
        messageTemplate: null,
        decorateInputElement: true,
        errorsAsTitle: false,
        errorElementClass: 'validation-error'
    });

    app.title = 'Subroute - Online Webhook/Microservice IDE for .NET';

    app.configurePlugins({
        router: true,
        dialog: true,
        bindings: true,
        ace: true,
        toggle: true
    });

    // Wireup to the router navigation event to update google analytics for every navigation.
    router.on('router:navigation:complete', function (instance, instruction) {
        if (instruction.queryString) {
            ga('set', 'page', '#' + instruction.fragment + '?' + instruction.queryString);
        } else {
            ga('set', 'page', '#' + instruction.fragment);
        }
        ga('send', 'pageview');
    });

    // This will force the auth module to load any tokens stored in our auth cookie.
    auth.initialize();

    app.start().then(function () {
        viewLocator.useConvention();

        app.setRoot('viewmodels/container');
    });
});