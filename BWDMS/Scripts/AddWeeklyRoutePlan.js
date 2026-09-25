
document.addEventListener("DOMContentLoaded", function () {

    const searchBox =
        document.getElementById("villageSearch");


    if (!searchBox) {
        return;
    }


    const villageList =
        document.querySelector(".village-check-list");


    if (!villageList) {
        return;
    }


    searchBox.addEventListener("input", function () {

        const searchValue =
            searchBox.value.trim().toLowerCase();


        const items =
            villageList.querySelectorAll("span");


        items.forEach(function (item) {

            const itemText =
                item.textContent.toLowerCase();


            if (itemText.includes(searchValue)) {
                item.style.display = "";
            }
            else {
                item.style.display = "none";
            }

        });

    });

});