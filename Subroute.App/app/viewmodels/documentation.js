define(['knockout', 'markdown'], function (ko, markdown) {
    return function () {
        var self = this;

        self.markdown = ko.observable(); 
        self.rendered = ko.observable();
        
        self.markdown.subscribe(function(value) {
            var html = markdown.toHTML(value);
            self.rendered(html);
        });
    };
});