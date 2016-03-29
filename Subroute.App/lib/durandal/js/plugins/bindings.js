define(['knockout', 'moment', 'jquery', 'jquery-ui', 'qtip'], function (ko, moment, $, jqueryui, qtip) {
    return {
        install: function () {
            ko.bindingHandlers.now = {
                init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
                    var $element = $(element);
                    var format = ko.utils.unwrapObservable(valueAccessor());

                    setInterval(function() {
                        var now = moment().format(format);
                        $element.text(now);
                    }, 500);
                }
            };

            ko.bindingHandlers.loading = {
                // loading: { trigger: compiling, idle: 'fa-upload', loading: 'fa-refresh' }
                init: function (element, valueAccessor) {
                    var $element = $(element);
                    var $icon = $element;
                    var canDisable = $element.is('button') || $element.is('a');

                    // Apply the loading to the icon.
                    if ($element.has('i')) {
                        $icon = $element.find('i');
                    }

                    var value = valueAccessor();
                    var options = ko.unwrap(value);

                    // Trigger must be observable to toggle icons.
                    if (!options.trigger || !ko.isObservable(options.trigger)) {
                        return;
                    };

                    // Ensure the initial state gets indicated, as there are some race cases.
                    ko.bindingHandlers.loading.indicate($element, options.selector, $icon, canDisable, options.idle, options.loading, options.trigger());

                    options.trigger.subscribe(function(loadingValue) {
                        ko.bindingHandlers.loading.indicate($element, options.selector, $icon, canDisable, options.idle, options.loading, loadingValue);
                    });
                },
                indicate: function (element, selector, icon, canDisable, idle, loading, value) {
                    // Use default loading icon if one wasn't provided.
                    if (!loading) {
                        loading = 'fa-refresh';
                    };

                    if (value) {
                        icon.removeClass(idle).addClass(loading).addClass('fa-spin');

                        if (canDisable) {
                            element.attr('disabled', 'disabled');
                            $(selector).attr('disabled', 'disabled');
                        };
                    } else {
                        icon.removeClass(loading).removeClass('fa-spin').addClass(idle);

                        if (canDisable) {
                            element.removeAttr('disabled');
                            $(selector).removeAttr('disabled');
                        };
                    }
                }
            };

            ko.bindingHandlers.slideVisible = {
                init: function (element, valueAccessor) {
                    // Initially set the element to be instantly visible/hidden depending on the value
                    var value = valueAccessor();
                    $(element).toggle(ko.unwrap(value)); // Use "unwrapObservable" so we can handle values that may or may not be observable
                },
                update: function (element, valueAccessor, allBindingsAccessor) {
                    // Whenever the value subsequently changes, slowly fade the element in or out
                    var value = valueAccessor();
                    var easingShow = ko.utils.unwrapObservable(allBindingsAccessor()).easingShow || 'easeOutQuint';
                    var easingHide = ko.utils.unwrapObservable(allBindingsAccessor()).easingHide || 'easeInQuint';
                    ko.unwrap(value) ? $(element).show('slide', { direction: 'left', easing: easingShow }, 800) : $(element).hide();
                }
            };

            ko.bindingHandlers.fadeVisible = {
                init: function (element, valueAccessor) {
                    // Initially set the element to be instantly visible/hidden depending on the value
                    var value = valueAccessor();
                    $(element).toggle(ko.unwrap(value)); // Use "unwrapObservable" so we can handle values that may or may not be observable
                },
                update: function (element, valueAccessor) {
                    // Whenever the value subsequently changes, slowly fade the element in or out
                    var value = valueAccessor();
                    ko.unwrap(value) ? $(element).fadeIn() : $(element).fadeOut();
                }
            };

            // Simplifies setting an anchor href attribute.
            ko.bindingHandlers.href = {
                update: function (element, valueAccessor) {
                    ko.bindingHandlers.attr.update(element, function () {
                        return { href: valueAccessor() }
                    });
                }
            };

            // Simplifies setting an image source attribute.
            ko.bindingHandlers.src = {
                update: function (element, valueAccessor) {
                    ko.bindingHandlers.attr.update(element, function () {
                        return { src: valueAccessor() }
                    });
                }
            };

            // Opposite of the visible binding, so you don't have to do full expression evaluation (e.g. !someObservable()).
            ko.bindingHandlers.hidden = {
                update: function (element, valueAccessor) {
                    var value = ko.utils.unwrapObservable(valueAccessor());
                    ko.bindingHandlers.visible.update(element, function () { return !value; });
                }
            };

            ko.bindingHandlers.from = {
                init: function (element, valueAccessor, allBindingsAccessor) {
                    var $element = $(element);
                    if (!$.data($element, "from-interval-handle")) {
                        var intervalHandle = setInterval(function () {
                            var value = ko.utils.unwrapObservable(valueAccessor());
                            var fromDefault = ko.utils.unwrapObservable(allBindingsAccessor()).fromDefault;

                            if (!value) {
                                $element.text(fromDefault);
                                return;
                            }

                            var now = moment(value);
                            var from = now.fromNow();
                            var display = now.format('LLLL');
                            $element.text(from);
                            $element.attr('title', display);
                        }, 10000);
                        $.data($element, "from-interval-handle", intervalHandle);
                    }

                    ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
                        var handle = $.data($element, "from-interval-handle");
                        clearInterval(handle);
                    });
                },
                update: function (element, valueAccessor, allBindingsAccessor) {
                    var $element = $(element);
                    var value = ko.utils.unwrapObservable(valueAccessor());
                    var fromDefault = ko.utils.unwrapObservable(allBindingsAccessor()).fromDefault;

                    if (!value) {
                        $element.text(fromDefault);
                        return;
                    }

                    var now = moment(value);
                    var from = now.fromNow();
                    var display = now.format('LLLL');
                    $element.text(from);
                    $element.attr('title', display);
                }
            };

            ko.bindingHandlers.date = {
                init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
                    var $element = $(element);
                    var binding = ko.utils.unwrapObservable(valueAccessor());
                    var date = binding.value;
                    var format = binding.format;
                    var defaultValue = binding.default;

                    if (!date && !defaultValue) {
                        return;
                    }

                    if (!date) {
                        $element.text(defaultValue);
                        return;
                    }

                    var formatted = moment(date).format(format);

                    if ($element.is('input')) {
                        $element.val(formatted);
                    } else {
                        $element.text(formatted);
                    }
                }
            };

            ko.bindingHandlers.source = {
                init: function (element, valueAccessor) {
                    var observable = valueAccessor();
                    var $element = $(element);
                    observable($element.text());
                }
            };

            ko.bindingHandlers.hideTooltips = {
                init: function (element) {
                    $(element).on('click', function() {
                        $('*').qtip('hide');
                    });
                }
            }

            ko.bindingHandlers.tooltip = {
                defaultTooltipOptions: {
                    show: 'click',
                    hide: 'unfocus',
                    style: {
                        classes: 'qtip-tipsy',
                        tip: {
                            width: 12,
                            height: 6
                        }
                    },
                    position: {
                        viewport: $(window),
                        my: 'left center',
                        at: 'right center',
                        adjust: {
                            x: 12,
                            y: 0,
                            resize: false,
                            method: 'flip flip'
                        }
                    }
                },
                getBehaviour: function (bindings) {
                    var behaviour = $.extend({}, ko.bindingHandlers.tooltip.defaultTooltipOptions);
                    if (typeof bindings == 'string') {
                        behaviour.content = {
                            text: bindings
                        }
                    } else {
                        behaviour.content = {
                            text: bindings.content,
                            title: { text: bindings.title }
                        };
                        behaviour = $.extend(behaviour, bindings.options);
                    }
                    return behaviour;
                },
                init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
                    var $element = $(element);
                    var allBindings = allBindingsAccessor();
                    var tooltipBindings = allBindings.tooltip;
                    var behaviour = ko.bindingHandlers.tooltip.getBehaviour(tooltipBindings);
                    $element.qtip(behaviour);

                    // Wireup a dispose handler to destroy tooltip when DOM is removed.
                    ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
                        $element.qtip('destroy');
                    });
                }
            };
        }
    }
});