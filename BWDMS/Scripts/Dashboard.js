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