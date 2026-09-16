<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="Login.aspx.cs"
    Inherits="BWDMS.Account.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />

    <meta name="viewport"
          content="width=device-width, initial-scale=1" />

    <title>Login | BWDMS</title>

    <!-- Bootstrap 5 CSS -->
    <link
        href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"
        rel="stylesheet" />

    <!-- Bootstrap Icons -->
    <link
        rel="stylesheet"
        href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" />

    <!-- Custom CSS -->
    <link href="../Content/login.css"
          rel="stylesheet" />

</head>


<body>

<form id="form1" runat="server">

    <div class="login-page">

        <!-- ==========================================
             LEFT SIDE - BRANDING
             ========================================== -->

        <div class="brand-panel">

            <div class="brand-overlay"></div>

            <div class="brand-content">

                <!-- Logo -->
                <div class="brand-logo">

                    <div class="logo-balaji">
                        BALAJI
                    </div>

                    <div class="logo-wafers">
                        WAFERS
                    </div>

                </div>


                <h1>
                    Dealer Management System
                </h1>


                <p class="brand-description">
                    Manage dealers, salesmen, products,
                    orders and business operations
                    from one powerful platform.
                </p>


                <div class="brand-line"></div>


                <div class="brand-tagline">

                    <span>
                        Distribute
                    </span>

                    <i class="bi bi-dot"></i>

                    <span>
                        Grow
                    </span>

                    <i class="bi bi-dot"></i>

                    <span>
                        Together
                    </span>

                </div>

            </div>

        </div>


        <!-- ==========================================
             RIGHT SIDE - LOGIN
             ========================================== -->

        <div class="login-panel">

            <div class="login-container">


                <!-- Mobile Logo -->

                <div class="mobile-logo">

                    <div class="mobile-logo-main">
                        BALAJI
                    </div>

                    <div class="mobile-logo-sub">
                        WAFERS
                    </div>

                </div>


                <!-- Heading -->

                <div class="login-heading">

                    <h2>
                        Welcome to BWDMS
                    </h2>

                    <p>
                        Sign in to continue to your dashboard
                    </p>

                </div>


                <!-- Validation Summary -->

                <asp:ValidationSummary
                    ID="vsLogin"
                    runat="server"
                    ValidationGroup="LoginGroup"
                    CssClass="validation-summary"
                    HeaderText="Please correct the following:"
                    DisplayMode="BulletList" />


                <!-- ==================================
                     EMAIL
                     ================================== -->

                <div class="mb-3">

                    <label
                        for="txtEmail"
                        class="form-label">

                        Email Address

                    </label>


                    <div class="input-group custom-input-group">

                        <span class="input-group-text">

                            <i class="bi bi-envelope"></i>

                        </span>


                        <asp:TextBox
                            ID="txtEmail"
                            runat="server"
                            CssClass="form-control"
                            placeholder="Enter your email address"
                            MaxLength="100">
                        </asp:TextBox>

                    </div>


                    <asp:RequiredFieldValidator
                        ID="rfvEmail"
                        runat="server"
                        ControlToValidate="txtEmail"
                        ValidationGroup="LoginGroup"
                        ErrorMessage="Email address is required."
                        Text="Email address is required."
                        CssClass="field-error"
                        Display="Dynamic">
                    </asp:RequiredFieldValidator>


                    <asp:RegularExpressionValidator
                        ID="revEmail"
                        runat="server"
                        ControlToValidate="txtEmail"
                        ValidationGroup="LoginGroup"
                        ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                        ErrorMessage="Please enter a valid email address."
                        Text="Please enter a valid email address."
                        CssClass="field-error"
                        Display="Dynamic">
                    </asp:RegularExpressionValidator>

                </div>


                <!-- ==================================
                     PASSWORD
                     ================================== -->

                <div class="mb-3">

                    <div class="password-label-row">

                        <label
                            for="txtPassword"
                            class="form-label">

                            Password

                        </label>


                        <a href="#"
                           class="forgot-password">

                            Forgot Password?

                        </a>

                    </div>


                    <div class="input-group custom-input-group">

                        <span class="input-group-text">

                            <i class="bi bi-lock"></i>

                        </span>


                        <asp:TextBox
                            ID="txtPassword"
                            runat="server"
                            CssClass="form-control password-input"
                            TextMode="Password"
                            placeholder="Enter your password"
                            MaxLength="50">
                        </asp:TextBox>


                        <button
                            type="button"
                            class="password-toggle"
                            onclick="togglePassword()">

                            <i
                                id="passwordIcon"
                                class="bi bi-eye">
                            </i>

                        </button>

                    </div>


                    <asp:RequiredFieldValidator
                        ID="rfvPassword"
                        runat="server"
                        ControlToValidate="txtPassword"
                        ValidationGroup="LoginGroup"
                        ErrorMessage="Password is required."
                        Text="Password is required."
                        CssClass="field-error"
                        Display="Dynamic">
                    </asp:RequiredFieldValidator>

                </div>


                <!-- ==================================
                     REMEMBER ME
                     ================================== -->

                <div class="remember-row">

                    <asp:CheckBox
                        ID="chkRemember"
                        runat="server" />

                    <label for="<%= chkRemember.ClientID %>">

                        Remember me

                    </label>

                </div>


                <!-- ==================================
                     SERVER MESSAGE
                     ================================== -->

                <asp:Label
                    ID="lblMessage"
                    runat="server"
                    CssClass="server-error"
                    Visible="false">
                </asp:Label>


                <!-- ==================================
                     LOGIN BUTTON
                     ================================== -->

                <asp:Button
                    ID="btnLogin"
                    runat="server"
                    Text="Login"
                    ValidationGroup="LoginGroup"
                    CausesValidation="true"
                    CssClass="btn login-button"
                    OnClick="btnLogin_Click" />


                <!-- Footer -->

                <div class="login-footer">

                    <p>
                        © 2026 BWDMS. All Rights Reserved.
                    </p>

                </div>

            </div>

        </div>

    </div>

</form>


<!-- ==========================================
     JAVASCRIPT
     Only normal JavaScript - NO jQuery
     ========================================== -->

<script>

    function togglePassword() {

        var passwordBox =
            document.getElementById('<%= txtPassword.ClientID %>');

        var icon =
            document.getElementById('passwordIcon');


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

</script>


</body>

</html>