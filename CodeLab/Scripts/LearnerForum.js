(function () {
    "use strict";

    function initialiseForum() {
        var root = document.querySelector(".lf-wrap");

        if (!root || root.dataset.initialised === "true") {
            return;
        }

        root.dataset.initialised = "true";

        var searchInput = document.getElementById("txtSearch");
        var searchButton = document.getElementById("btnSearch");

        var clearButton = root.querySelector(
            '[id$="btnClearFilters"]'
        );

        var postContent = root.querySelector(
            '[id$="txtPostContent"]'
        );

        var searchTimer = null;
        var pendingSearch = false;
        var composing = false;
        var leavingPage = false;
        var dialogOpeners = new WeakMap();

        var requestManager =
            window.Sys &&
                Sys.WebForms &&
                Sys.WebForms.PageRequestManager
                ? Sys.WebForms.PageRequestManager.getInstance()
                : null;

        /*
         * The server also validates this limit.
         * ASP.NET multiline TextBox controls may not render MaxLength.
         */
        if (postContent) {
            postContent.maxLength = 20000;
        }

        // ------------------------------
        // Dialog opening and closing
        // ------------------------------

        root.addEventListener("click", function (event) {
            var openButton = event.target.closest(
                "[data-forum-open]"
            );

            if (openButton) {
                var openId = openButton.getAttribute(
                    "data-forum-open"
                );

                var dialog = document.getElementById(openId);

                if (dialog && !dialog.open) {
                    event.preventDefault();

                    dialogOpeners.set(dialog, openButton);
                    dialog.showModal();

                    var firstField = dialog.querySelector(
                        "select:not(:disabled), " +
                        "input:not(:disabled):not([type='hidden']), " +
                        "textarea:not(:disabled)"
                    );

                    if (firstField) {
                        firstField.focus();
                    }
                }

                return;
            }

            var closeButton = event.target.closest(
                "[data-forum-close]"
            );

            if (closeButton) {
                var closeId = closeButton.getAttribute(
                    "data-forum-close"
                );

                var openDialog = document.getElementById(closeId);

                if (openDialog && openDialog.open) {
                    event.preventDefault();
                    openDialog.close();
                }
            }
        });

        root.querySelectorAll("dialog").forEach(function (dialog) {
            dialog.addEventListener("close", function () {
                var opener = dialogOpeners.get(dialog);

                if (opener && opener.isConnected) {
                    opener.focus();
                }
            });
        });

        /*
         * Native dialogs provide Escape-to-close and keep keyboard
         * focus inside the popup. Closing preserves the draft fields.
         */

        // ------------------------------
        // Live discussion search
        // ------------------------------

        function cancelSearchTimer() {
            if (searchTimer !== null) {
                window.clearTimeout(searchTimer);
                searchTimer = null;
            }
        }

        function requestSearch() {
            cancelSearchTimer();

            if (leavingPage || !searchInput || !searchButton) {
                return;
            }

            if (composing) {
                return;
            }

            if (requestManager &&
                requestManager.get_isInAsyncPostBack()) {
                pendingSearch = true;
                return;
            }

            pendingSearch = false;

            /*
             * Uses the existing ASP.NET button so server events,
             * ViewState, and UpdatePanel behaviour remain intact.
             */
            searchButton.click();
        }

        function scheduleSearch() {
            cancelSearchTimer();

            if (composing || leavingPage) {
                return;
            }

            searchTimer = window.setTimeout(
                requestSearch,
                400
            );
        }

        if (searchInput && searchButton) {
            searchInput.addEventListener(
                "input",
                scheduleSearch
            );

            searchInput.addEventListener(
                "compositionstart",
                function () {
                    composing = true;
                    cancelSearchTimer();
                }
            );

            searchInput.addEventListener(
                "compositionend",
                function () {
                    composing = false;
                    scheduleSearch();
                }
            );

            searchInput.addEventListener(
                "keydown",
                function (event) {
                    if (event.key !== "Enter" ||
                        event.isComposing ||
                        composing) {
                        return;
                    }

                    // Prevent the panel's default button firing twice.
                    event.preventDefault();
                    requestSearch();
                }
            );
        }

        if (requestManager) {
            requestManager.add_endRequest(function () {
                if (pendingSearch && !leavingPage) {
                    requestSearch();
                }
            });
        }

        // ------------------------------
        // Full-page actions
        // ------------------------------

        if (clearButton) {
            clearButton.addEventListener("click", function () {
                cancelSearchTimer();
                pendingSearch = false;
                leavingPage = true;
            });
        }

        var publishButton = root.querySelector(
            '[id$="btnPublish"]'
        );

        if (publishButton) {
            publishButton.addEventListener("click", function () {
                cancelSearchTimer();
                pendingSearch = false;
            });
        }

        window.addEventListener("pagehide", function () {
            leavingPage = true;
            pendingSearch = false;
            cancelSearchTimer();
        });

        window.addEventListener("pageshow", function (event) {
            if (event.persisted) {
                leavingPage = false;
                pendingSearch = false;
            }
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener(
            "DOMContentLoaded",
            initialiseForum
        );
    } else {
        initialiseForum();
    }
})();