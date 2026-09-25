document.addEventListener("DOMContentLoaded", function () {

    const sidebarToggle =
        document.getElementById("sidebarToggle");

    const sidebar =
        document.getElementById("sidebar");


    if (sidebarToggle && sidebar) {

        sidebarToggle.addEventListener("click", function () {

            sidebar.classList.toggle("show");

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