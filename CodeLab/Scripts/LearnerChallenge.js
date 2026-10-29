(function () {
    "use strict";

    function initialiseChallenge() {
        var editor = document.querySelector(".ch-code-input");
        var panel = document.getElementById("chEditorPanel");

        if (!editor || !panel || panel.dataset.initialised === "true") {
            return;
        }

        panel.dataset.initialised = "true";

        var lineNumbers = document.getElementById("chLineNumbers");
        var notice = document.getElementById("chEditorNotice");
        var resetButton = document.getElementById("chResetButton");
        var copyButton = document.getElementById("chCopyButton");
        var expandButton = document.getElementById("chExpandButton");
        var fontSize = document.getElementById("chFontSize");
        var starterCode = document.querySelector(
            'input[type="hidden"][id$="hdnStarterCode"]'
        );

        var tabs = Array.prototype.slice.call(
            document.querySelectorAll("[data-ch-tab]")
        );

        var previousBodyOverflow = "";
        var noticeTimer;

        function announce(message) {
            clearTimeout(noticeTimer);
            notice.textContent = message;

            noticeTimer = setTimeout(function () {
                notice.textContent = "";
            }, 4000);
        }

        function updateLineNumbers() {
            var count = editor.value.split("\n").length;
            var numbers = [];

            for (var i = 1; i <= count; i++) {
                numbers.push(i);
            }

            lineNumbers.textContent = numbers.join("\n");
            lineNumbers.scrollTop = editor.scrollTop;
        }

        function activateTab(selectedTab, moveFocus) {
            tabs.forEach(function (tab) {
                var selected = tab === selectedTab;
                var content = document.getElementById(
                    tab.getAttribute("data-ch-tab")
                );

                tab.classList.toggle("is-active", selected);
                tab.setAttribute("aria-selected", String(selected));
                tab.tabIndex = selected ? 0 : -1;

                if (content) {
                    content.hidden = !selected;
                }
            });

            if (moveFocus) {
                selectedTab.focus();
            }
        }

        tabs.forEach(function (tab, index) {
            tab.addEventListener("click", function () {
                activateTab(tab, false);
            });

            tab.addEventListener("keydown", function (event) {
                var targetIndex;

                switch (event.key) {
                    case "ArrowRight":
                        targetIndex = (index + 1) % tabs.length;
                        break;

                    case "ArrowLeft":
                        targetIndex =
                            (index - 1 + tabs.length) % tabs.length;
                        break;

                    case "Home":
                        targetIndex = 0;
                        break;

                    case "End":
                        targetIndex = tabs.length - 1;
                        break;

                    default:
                        return;
                }

                event.preventDefault();
                activateTab(tabs[targetIndex], true);
            });
        });

        editor.addEventListener("input", updateLineNumbers);

        editor.addEventListener("scroll", function () {
            lineNumbers.scrollTop = editor.scrollTop;
        });

        resetButton.addEventListener("click", function () {
            if (!starterCode) {
                announce("Starter code is unavailable.");
                return;
            }

            if (editor.value !== starterCode.value &&
                !window.confirm(
                    "Replace your current code with the starter code?"
                )) {
                return;
            }

            editor.value = starterCode.value;
            editor.scrollTop = 0;
            editor.scrollLeft = 0;

            updateLineNumbers();
            editor.focus();
            editor.setSelectionRange(0, 0);

            announce("Starter code restored.");
        });

        copyButton.addEventListener("click", async function () {
            try {
                if (!navigator.clipboard ||
                    !navigator.clipboard.writeText) {
                    throw new Error("Clipboard unavailable.");
                }

                await navigator.clipboard.writeText(editor.value);
                announce("Code copied.");
            } catch (error) {
                editor.focus();
                editor.select();

                announce(
                    "Press Ctrl+C, or Command+C on Mac, to copy the selected code."
                );
            }
        });

        fontSize.addEventListener("change", function () {
            var size = Number(fontSize.value);

            if ([12, 14, 16, 18].indexOf(size) === -1) {
                return;
            }

            panel.style.setProperty(
                "--ch-editor-font",
                size + "px"
            );

            updateLineNumbers();
        });

        function setExpanded(expanded) {
            if (expanded) {
                previousBodyOverflow = document.body.style.overflow;
                document.body.style.overflow = "hidden";
            } else {
                document.body.style.overflow = previousBodyOverflow;
            }

            panel.classList.toggle("is-expanded", expanded);

            expandButton.setAttribute(
                "aria-pressed",
                String(expanded)
            );

            expandButton.setAttribute(
                "aria-label",
                expanded ? "Restore editor size" : "Expand editor"
            );

            updateLineNumbers();

            if (expanded) {
                editor.focus();
            } else {
                expandButton.focus();
            }
        }

        expandButton.addEventListener("click", function () {
            setExpanded(!panel.classList.contains("is-expanded"));
        });

        document.addEventListener("keydown", function (event) {
            if (!panel.classList.contains("is-expanded")) {
                return;
            }

            if (event.key === "Escape") {
                event.preventDefault();
                setExpanded(false);
                return;
            }

            // Keep keyboard focus inside the expanded editor.
            if (event.key === "Tab") {
                var controls = Array.prototype.slice.call(
                    panel.querySelectorAll(
                        'button:not(:disabled), select:not(:disabled), ' +
                        'textarea:not(:disabled), [tabindex="0"]'
                    )
                ).filter(function (control) {
                    return control.getClientRects().length > 0;
                });

                if (!controls.length) {
                    return;
                }

                var first = controls[0];
                var last = controls[controls.length - 1];
                var active = document.activeElement;

                if (event.shiftKey &&
                    (active === first || !panel.contains(active))) {
                    event.preventDefault();
                    last.focus();
                } else if (!event.shiftKey &&
                    (active === last || !panel.contains(active))) {
                    event.preventDefault();
                    first.focus();
                }
            }
        });

        updateLineNumbers();
    }

    if (document.readyState === "loading") {
        document.addEventListener(
            "DOMContentLoaded",
            initialiseChallenge
        );
    } else {
        initialiseChallenge();
    }
})();

(function () {
    "use strict";

    function initialiseChallengeActions() {
        var editor = document.querySelector(".ch-code-input");
        var selector = document.getElementById("chChallengeSelect");
        var resetButton = document.getElementById("chResetButton");
        var notice = document.getElementById("chEditorNotice");

        var buttons = Array.prototype.slice.call(
            document.querySelectorAll("[data-ch-action]")
        );

        if (!editor || !editor.form || !buttons.length) {
            return;
        }

        var form = editor.form;

        if (form.dataset.challengeActionsReady === "true") {
            return;
        }

        form.dataset.challengeActionsReady = "true";

        var busy = false;
        var clickedButton = null;
        var initialCode = editor.value;
        var initialSelection = selector ? selector.value : "";

        var originalReadOnly = editor.readOnly;
        var originalResetDisabled = resetButton
            ? resetButton.disabled
            : false;
        var originalSelectorDisabled = selector
            ? selector.disabled
            : false;

        buttons.forEach(function (button) {
            button.dataset.originalText = button.value;

            button.addEventListener("click", function (event) {
                if (busy) {
                    event.preventDefault();
                    event.stopImmediatePropagation();
                    return;
                }

                clickedButton = button;
            }, true);
        });

        function restoreControls() {
            busy = false;
            clickedButton = null;
            editor.readOnly = originalReadOnly;

            buttons.forEach(function (button) {
                button.value = button.dataset.originalText;
                button.removeAttribute("aria-disabled");
                button.removeAttribute("aria-busy");

                button.parentElement.classList.remove("is-loading");
            });

            if (resetButton) {
                resetButton.disabled = originalResetDisabled;
            }

            if (selector) {
                selector.disabled = originalSelectorDisabled;
            }
        }

        form.addEventListener("submit", function (event) {
            if (event.defaultPrevented) {
                return;
            }

            if (busy) {
                event.preventDefault();
                return;
            }

            var button = event.submitter || clickedButton;

            if (!button || !button.hasAttribute("data-ch-action")) {
                return;
            }

            busy = true;

            var submitting =
                button.getAttribute("data-ch-action") === "submit";

            button.value = submitting ? "Submitting…" : "Running…";
            button.parentElement.classList.add("is-loading");
            button.setAttribute("aria-busy", "true");

            /*
             * Keep the submit inputs enabled so ASP.NET receives
             * the clicked button and calls the correct event handler.
             * The click and submit guards block repeated requests.
             */
            buttons.forEach(function (item) {
                item.setAttribute("aria-disabled", "true");
            });

            // Read-only fields are still included in the POST.
            editor.readOnly = true;

            if (resetButton) {
                resetButton.disabled = true;
            }

            if (selector) {
                selector.disabled = true;
            }

            if (notice) {
                notice.textContent = submitting
                    ? "Submitting your solution. Please wait…"
                    : "Running your code. Please wait…";
            }

            // Restore if another submit handler cancels navigation.
            window.setTimeout(function () {
                if (event.defaultPrevented) {
                    restoreControls();

                    if (notice) {
                        notice.textContent = "";
                    }
                }
            }, 0);
        });

        if (selector) {
            selector.addEventListener("change", function () {
                if (busy) {
                    selector.value = initialSelection;
                    return;
                }

                if (selector.value === initialSelection) {
                    return;
                }

                /*
                 * Also warn after Run: code remains in the editor,
                 * but switching challenges does not retain that code.
                 */
                var starter = document.querySelector(
                    'input[type="hidden"][id$="hdnStarterCode"]'
                );

                var hasCodeToLeave =
                    editor.value !== initialCode ||
                    (starter && editor.value !== starter.value);

                if (hasCodeToLeave && !window.confirm(
                    "Switch challenge? Your current editor code will not " +
                    "be kept on this page. Submitted attempts remain in history."
                )) {
                    selector.value = initialSelection;
                    return;
                }

                var target = new URL(window.location.href);
                target.searchParams.set("id", selector.value);
                target.hash = "";

                window.location.assign(target.href);
            });
        }

        // Clear loading state when restored with the browser Back button.
        window.addEventListener("pageshow", function (event) {
            if (event.persisted) {
                restoreControls();

                if (selector) {
                    selector.value = initialSelection;
                }

                if (notice) {
                    notice.textContent = "";
                }
            }
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener(
            "DOMContentLoaded",
            initialiseChallengeActions
        );
    } else {
        initialiseChallengeActions();
    }
})();