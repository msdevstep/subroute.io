define(['knockout', 'ace/ace', 'ace/ext-language_tools'], function (ko, ace, lang) {
    return {
        install: function() {
            ko.bindingHandlers.ace = {
                init: function (element, valueAccessor) {
                    var value = valueAccessor();
                    var valueFormatted = element.innerText.trim();
                    var options = ko.unwrap(value);
                    var editor = ace.edit(element.id);
                    var session = editor.getSession();

                    editor.setTheme("ace/theme/sqlserver");
                    editor.setOption("showPrintMargin", false);
                    editor.setOption('highlightActiveLine', false);
                    editor.setOption('fontSize', '12px');

                    if (options.autoHeight) {
                        editor.setOptions({
                            maxLines: Infinity
                        });
                    };

                    session.setUseWrapMode(false);
                    session.setWrapLimitRange();
                    editor.$blockScrolling = Infinity;

                    editor.commands.addCommand({
                        name: "save",
                        bindKey: {win: "Ctrl-S", mac: "Command-Option-S"},
                        exec: function(editor) {
                            if (options.save) {
                                options.save(editor);
                            };
                        }
                    });

                    editor.commands.addCommand({
                        name: "compile",
                        bindKey: { win: "Ctrl-B", mac: "Command-B" },
                        exec: function(editor) {
                            if (options.compile) {
                                options.compile(editor);
                            }
                        }
                    });

                    editor.commands.addCommand({
                        name: "run",
                        bindKey: { win: "Ctrl-E", mac: "Command-E" },
                        exec: function (editor) {
                            if (options.run) {
                                options.run(editor);
                            }
                        }
                    });

                    editor.commands.addCommand({
                        name: "publish",
                        bindKey: { win: "Ctrl-D", mac: "Command-D" },
                        exec: function (editor) {
                            if (options.publish) {
                                options.publish(editor);
                            }
                        }
                    });
                    
                    if (options.readOnly) {
                        editor.setReadOnly(true);
                        editor.setOption('highlightActiveLine', false);
                        editor.setOption('highlightGutterLine', false);
                        editor.setOption('showFoldWidgets', false);
                    }

                    // Determine if the syntax option is observable and watch for changes.
                    if (ko.isObservable(options.syntax)) {
                        options.syntax.subscribe(function(syntax) {
                            ko.bindingHandlers.ace.switchMode(editor, syntax, options);
                        });
                        ko.bindingHandlers.ace.switchMode(editor, options.syntax(), options);
                    } else {
                        ko.bindingHandlers.ace.switchMode(editor, options.syntax, options);
                    }

                    // Handle edits made in the editor
                    editor.on('input', function () {
                        var editorValue = editor.getValue();

                        if (typeof options.value === "function") {
                            var observable = options.value;
                            observable(editorValue);
                        };
                    });

                    if (typeof options.value === "function") {
                        valueFormatted = options.value();
                    };

                    editor.setValue(valueFormatted, -1);
                    
                    editor.focus();
                },
                update: function (element, valueAccessor) {
                    // Get value observable and options.
                    var valueAccess = valueAccessor();
                    var options = ko.unwrap(valueAccess);
                    var syntax = ko.unwrap(options.syntax);
                    var valueFormatted = '';

                    if (typeof options.value === "function") {
                        valueFormatted = options.value();
                    };

                    // We'll capture any formatting errors and on error, just use original value.
                    try {
                        valueFormatted = ko.bindingHandlers.ace.getFormattedCode(syntax, valueFormatted);
                    }
                    catch(ex) {
                        
                    }

                    var editor = ace.edit(element.id);
                    var editorValue = editor.getValue();

                    if (typeof options.value ==="function" && editorValue !== valueFormatted) {
                        editor.setValue(valueFormatted || '');
                        editor.gotoLine(0);
                    }
                },
                getFormattedCode: function(mode, code) {
                    switch (mode) {
                        case "json":
                            if (!code) {
                                return code;
                            };

                            var jsonObject = JSON.parse(code);
                            return JSON.stringify(jsonObject, null, 4);
                        case "xml":
                            // Todo: Add proper formatting code.
                            return code;
                        case "html":
                            // Todo: Add proper formatting code.
                            return code;
                        default:
                            return code;
                    }
                },
                switchMode: function (editor, mode, options) {
                    var language = ace.require('ace/ext/language_tools');
                    var session = editor.getSession();

                    switch (mode) {
                        case "csharp":
                            session.setMode('modes/csharp');
                            editor.setOption("scrollPastEnd", true);
                            editor.setOptions({ enableBasicAutocompletion: true, enableLiveAutocompletion: true });

                            if (options.intellisense) {
                                var intellisense = {
                                    getCompletions: options.intellisense
                                }
                                language.setCompleters([intellisense]);
                            };
                            break;
                        default:
                            session.setMode('ace/mode/' + mode);
                            editor.setOption("scrollPastEnd", false);
                            break;
                    };
                }
            };
        }
    }
});