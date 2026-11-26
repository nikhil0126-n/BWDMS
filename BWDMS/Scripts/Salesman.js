/* ============================================================================
   BWDMS — SALESMAN SHELL SCRIPT
   Small helper only: opens/closes the sidebar on small screens.
   ============================================================================ */

(function () {
    "use strict";

    function ready(fn) {
        if (document.readyState !== "loading") {
            fn();
        } else {
            document.addEventListener("DOMContentLoaded", fn);
        }
    }

    ready(function () {
        var toggle = document.getElementById("swToggle");
        var sidebar = document.getElementById("swSidebar");
        var backdrop = document.getElementById("swBackdrop");

        if (!toggle || !sidebar) {
            return;
        }

        function close() {
            sidebar.classList.remove("show");
            if (backdrop) {
                backdrop.classList.remove("show");
            }
        }

        toggle.addEventListener("click", function () {
            sidebar.classList.toggle("show");
            if (backdrop) {
                backdrop.classList.toggle("show");
            }
        });

        if (backdrop) {
            backdrop.addEventListener("click", close);
        }

        /* Tapping a menu item navigates, so drop the drawer first —
           otherwise it is still open when the next page renders. */
        Array.prototype.forEach.call(
            sidebar.querySelectorAll("a"),
            function (link) {
                link.addEventListener("click", close);
            });

        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape") {
                close();
            }
        });
    });
})();
