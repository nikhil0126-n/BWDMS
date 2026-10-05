document.addEventListener("DOMContentLoaded", function () {

    const sidebarToggle =
        document.getElementById("sidebarToggle");

    const sidebar =
        document.getElementById("sidebar");


    if (sidebarToggle && sidebar) {

        const backdrop =
            document.getElementById("sidebarBackdrop");


        // Open / close the sidebar and its backdrop together

        function setSidebarOpen(open) {

            sidebar.classList.toggle("show", open);

            if (backdrop) {
                backdrop.classList.toggle("show", open);
            }

        }


        sidebarToggle.addEventListener("click", function () {

            setSidebarOpen(!sidebar.classList.contains("show"));

        });


        // Click outside (on the backdrop) closes the sidebar

        if (backdrop) {

            backdrop.addEventListener("click", function () {

                setSidebarOpen(false);

            });

        }


        // Escape closes the sidebar

        document.addEventListener("keydown", function (event) {

            if (event.key === "Escape") {
                setSidebarOpen(false);
            }

        });


        // Leaving the mobile breakpoint resets the sidebar

        window.addEventListener("resize", function () {

            if (window.innerWidth > 768) {
                setSidebarOpen(false);
            }

        });

    }


    // Highlight current menu item

    const currentPage =
        window.location.pathname.toLowerCase();


    const menuItems =
        document.querySelectorAll(".menu-item");


    menuItems.forEach(function (item) {

        const href =
            item.getAttribute("href");


        if (href &&
            href !== "#" &&
            currentPage.includes(
                href.toLowerCase().replace("~", "")
            )) {

            item.classList.add("active");

        }

    });

});

// ============================================================
// ROUTE LIVE SEARCH
// ============================================================

document.addEventListener("DOMContentLoaded", function () {

    const searchBox = document.getElementById("txtSearch");
    const routeTable = document.getElementById("gvRoutes");

    if (!searchBox || !routeTable) {
        return;
    }


    searchBox.addEventListener("input", function () {

        const searchValue =
            searchBox.value.trim().toLowerCase();

        const rows =
            routeTable.querySelectorAll("tbody tr");


        rows.forEach(function (row) {

            const rowText =
                row.textContent.toLowerCase();


            if (rowText.includes(searchValue)) {

                row.style.display = "";

            } else {

                row.style.display = "none";

            }

        });

    });

});