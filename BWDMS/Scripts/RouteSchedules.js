
document.addEventListener("DOMContentLoaded", function () {

    const searchInput =
        document.getElementById("txtScheduleSearch");

    if (!searchInput) {
        return;
    }

    searchInput.addEventListener("input", function () {

        const searchText =
            this.value.toLowerCase().trim();

        const table =
            document.querySelector(".schedule-table");

        if (!table) {
            return;
        }

        const rows =
            table.querySelectorAll("tbody tr");

        rows.forEach(function (row) {

            const rowText =
                row.textContent.toLowerCase();

            if (rowText.includes(searchText)) {
                row.style.display = "";
            } else {
                row.style.display = "none";
            }

        });

    });

});