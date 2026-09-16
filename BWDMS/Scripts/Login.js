function togglePassword(passwordId) {

    var passwordBox =
        document.getElementById(passwordId);

    var icon =
        document.getElementById("passwordIcon");


    if (!passwordBox || !icon) {
        return;
    }


    if (passwordBox.type === "password") {

        passwordBox.type = "text";

        icon.classList.remove("bi-eye");

        icon.classList.add("bi-eye-slash");

    }
    else {

        passwordBox.type = "password";

        icon.classList.remove("bi-eye-slash");

        icon.classList.add("bi-eye");

    }
}