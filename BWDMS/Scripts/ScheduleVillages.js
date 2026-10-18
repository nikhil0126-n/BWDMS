
document.addEventListener("DOMContentLoaded", function () {

    const searchBox =
        document.getElementById("villageSearch");


    if (!searchBox) {
        return;
    }


    const villageList =
        document.querySelector(".village-order-list");


    if (!villageList) {
        return;
    }


    searchBox.addEventListener("input", function () {

        const searchValue =
            searchBox.value.trim().toLowerCase();


        const rows =
            villageList.querySelectorAll(".village-order-row");


        rows.forEach(function (row) {

            const label =
                row.querySelector(".village-order-text");


            const rowText =
                (label ? label.textContent : row.textContent)
                    .toLowerCase();


            if (rowText.includes(searchValue)) {
                row.style.display = "";
            }
            else {
                row.style.display = "none";
            }

        });

    });

});
