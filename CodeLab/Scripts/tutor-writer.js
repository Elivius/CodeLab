/*
 * Shared by the Tutor builder pages (Lessons, Coding challenge).
 *
 * 1. Markdown writer: every element with class "cl-writer" gets
 *    - Write / Preview tabs: [data-writer-tab="write"] and [data-writer-tab="preview"]. The Preview tab is the
 *      page's server button inside an UpdatePanel; this script only switches the view and shows "loading".
 *    - Formatting buttons: [data-md="bold" | "italic" | "heading" | "ul" | "ol" | "quote" | "code" | "codeblock" | "link"]
 *    - A word count in [data-word-count], if present.
 * 2. Code boxes (class "cl-ide-input"): the Tab key adds four spaces instead of leaving the box.
 * 3. Syntax colours (highlight.js, if the page loads it) for code inside a preview, again after each update.
 */
(function () {
    'use strict';

    function highlight() {
        if (!window.hljs) { return; }
        document.querySelectorAll('.cl-writer-preview pre code').forEach(function (block) {
            block.removeAttribute('data-highlighted');
            window.hljs.highlightElement(block);
        });
    }

    function initWriter(writer) {
        var editor = writer.querySelector('.cl-md-input');
        var preview = writer.querySelector('.cl-writer-preview');
        var wordCount = writer.querySelector('[data-word-count]');
        if (!editor) { return; }

        function setMode(mode) {
            writer.setAttribute('data-mode', mode);
            writer.querySelectorAll('[data-writer-tab]').forEach(function (tab) {
                var active = tab.getAttribute('data-writer-tab') === mode;
                tab.classList.toggle('is-active', active);
                tab.setAttribute('aria-selected', active ? 'true' : 'false');
            });
        }

        function countWords() {
            if (!wordCount) { return; }
            var text = editor.value.trim();
            var words = text ? text.split(/\s+/).length : 0;
            var minutes = Math.max(1, Math.round(words / 200));
            wordCount.textContent = words + (words === 1 ? ' word' : ' words') + (words ? ' · about ' + minutes + ' min read' : '');
        }

        var writeTab = writer.querySelector('[data-writer-tab="write"]');
        var previewTab = writer.querySelector('[data-writer-tab="preview"]');

        if (writeTab) {
            writeTab.addEventListener('click', function () {
                setMode('write');
                editor.focus();
            });
        }

        // The Preview tab posts back to the server; show a loading message while it works
        if (previewTab && preview) {
            previewTab.addEventListener('click', function () {
                setMode('preview');
                preview.setAttribute('aria-busy', 'true');
                var panel = preview.querySelector('.cl-markdown');
                if (panel) {
                    panel.classList.add('is-loading');
                } else if (preview.firstElementChild) {
                    preview.firstElementChild.innerHTML = '<p class="cl-preview-loading">Rendering preview…</p>';
                }
            });
        }

        // Formatting buttons: wrap the selected text, or add a prefix to each selected line
        function wrap(before, after, placeholder) {
            var start = editor.selectionStart, end = editor.selectionEnd;
            var selected = editor.value.substring(start, end) || placeholder;
            editor.setRangeText(before + selected + after, start, end, 'end');
            editor.focus();
            editor.setSelectionRange(start + before.length, start + before.length + selected.length);
            countWords();
        }

        function prefixLines(prefix) {
            var start = editor.selectionStart, end = editor.selectionEnd;
            var lineStart = editor.value.lastIndexOf('\n', start - 1) + 1;
            var block = editor.value.substring(lineStart, end) || 'Item';
            var number = 0;
            var result = block.split('\n').map(function (line) {
                number++;
                return (prefix === '1. ' ? number + '. ' : prefix) + line;
            }).join('\n');
            editor.setRangeText(result, lineStart, end, 'end');
            editor.focus();
            countWords();
        }

        var actions = {
            bold: function () { wrap('**', '**', 'bold text'); },
            italic: function () { wrap('_', '_', 'italic text'); },
            heading: function () { prefixLines('## '); },
            ul: function () { prefixLines('- '); },
            ol: function () { prefixLines('1. '); },
            quote: function () { prefixLines('> '); },
            code: function () { wrap('`', '`', 'code'); },
            codeblock: function () { wrap('\n```\n', '\n```\n', 'code here'); },
            link: function () { wrap('[', '](https://)', 'link text'); }
        };

        writer.querySelectorAll('[data-md]').forEach(function (button) {
            button.addEventListener('click', function () {
                setMode('write');
                var action = actions[button.getAttribute('data-md')];
                if (action) { action(); }
            });
        });

        editor.addEventListener('input', countWords);
        countWords();
    }

    function initCodeBoxes() {
        document.querySelectorAll('.cl-ide-input').forEach(function (box) {
            box.addEventListener('keydown', function (e) {
                if (e.key === 'Tab' && !e.shiftKey && !box.readOnly) {
                    e.preventDefault();
                    box.setRangeText('    ', box.selectionStart, box.selectionEnd, 'end');
                }
            });
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('.cl-writer').forEach(initWriter);
        initCodeBoxes();
        highlight();

        if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
                document.querySelectorAll('.cl-writer-preview[aria-busy]').forEach(function (preview) {
                    preview.removeAttribute('aria-busy');
                });
                highlight();
            });
        }
    });
})();
