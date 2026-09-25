
// ============================================================
// WEEKLY ROUTE PLANS LIVE SEARCH
// ============================================================

document.addEventListener("DOMContentLoaded", function () {

    const searchBox =
        document.getElementById("txtSearch");

    const routeTable =
        document.getElementById("gvWeeklyPlans");


    // Stop if elements do not exist
    if (!searchBox || !routeTable) {
        return;
    }


    // Live search while typing
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