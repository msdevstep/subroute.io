define(['knockout', 'remarkable', 'highlight', 'jquery'], function (ko, remarkable, hljs, $) {
    return function () {
        var self = this;

        self.anchor = '';

        self.markdown = ko.observable(); 

        self.canReuseForRoute = function () {
            return true;
        };

        self.scrollToTop = function () {
            $('html, body').animate({
                scrollTop: 0
            }, 1000);

            history.replaceState(undefined, undefined, "#documentation");
        };

        self.scrollToAnchor = function (anchor) {
            if (!anchor)
                return;

            var element = $('#documentation-content').find('#' + anchor);

            if (element.length === 0)
                return;

            $('html, body').animate({
                scrollTop: element.offset().top - 70
            }, 1000);

            history.replaceState(undefined, undefined, "#documentation/" + anchor);
        };

        self.enableScrollLinks = function () {
            $('.scroll-top').click(self.scrollToTop);
        };

        self.activate = function (anchor) {
            self.anchor = anchor;
        }

        self.compositionComplete = function () {
            var md = new remarkable('full', {
                html: true,                 // Enable HTML tags in source
                xhtmlOut: false,            // Use '/' to close single tags (<br />)
                breaks: false,              // Convert '\n' in paragraphs into <br>
                langPrefix: 'language-',    // CSS language prefix for fenced blocks
                linkify: true,              // Autoconvert URL-like texts to links
                linkTarget: '_blank',       // Set target to open link in

                // Enable some language-neutral replacements + quotes beautification
                typographer: true,

                // Double + single quotes replacement pairs, when typographer enabled,
                // and smartquotes on. Set doubles to '«»' for Russian, '„“' for German.
                quotes: '“”‘’',

                // Highlighter function. Should return escaped HTML,
                // or '' if input not changed
                highlight: function (str, lang) {
                    if (lang && hljs.getLanguage(lang)) {
                        try {
                            return hljs.highlight(lang, str).value;
                        } catch (__) { }
                    }

                    try {
                        return hljs.highlightAuto(str).value;
                    } catch (__) { }

                    return ''; // use external default escaping
                }
            });

            var source = $('#source');
            var destination = $('#documentation-content');
            var unformatted = self.markdown();
            var html = md.render(unformatted);

            source.remove();
            destination.html(html)
            self.scrollToAnchor(self.anchor);
            self.enableScrollLinks();
        };
    };
});