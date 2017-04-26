define(['knockout', 'remarkable', 'highlight'], function (ko, remarkable, hljs) {
    return function () {
        var self = this;

        self.markdown = ko.observable(); 

        self.compositionComplete = function () {
            var md = new remarkable('full', {
                html: false,                // Enable HTML tags in source
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

            var destination = document.getElementById("documentation");
            var source = self.markdown();
            var html = md.render(source);

            destination.innerHTML = html;
        };
    };
});