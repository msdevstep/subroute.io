define(['knockout'], function (ko, ace) {

    return {
        install: function() {
            ko.bindingHandlers.toggle = {
                init: function(element, valueAccessor) {
                    $(element).on('click', function() {
                        var value = valueAccessor();
                        var currentValue = value();

                        if (currentValue !== true && currentValue !== false)
                            return false;

                        value(!value());

                        return true;
                    });
                }
            };
        }
    }
});