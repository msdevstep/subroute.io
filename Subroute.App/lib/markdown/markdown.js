if (typeof define !== 'function') { var define = require('amdefine')(module) }

// Include all our dependencies and return the resulting library.

define(['../lib/markdown/parser', '../lib/markdown/markdown_helpers', '../lib/markdown/render_tree', '../lib/markdown/dialects/gruber', '../lib/markdown/dialects/maruku'], function (Markdown) {
  return Markdown;
});
