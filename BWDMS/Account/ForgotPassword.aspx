<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="ForgotPassword.aspx.cs"
    Inherits="BWDMS.Account.ForgotPassword" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />

    <meta name="viewport"
          content="width=device-width, initial-scale=1" />

    <title>Forgot Password | BWDMS</title>

    <!-- Bootstrap 5 CSS -->
    <link
        href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"
        rel="stylesheet" />

    <!-- Bootstrap Icons -->
    <link
        rel="stylesheet"
        href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" />

    <!-- Shared Login Theme -->
    <link href="../Content/Login.css"
          rel="stylesheet" />

    <!-- Forgot Password CSS -->
    <link href="../Content/ForgotPassword.css"
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
             RIGHT SIDE - FORGOT PASSWORD
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


                <!-- ==================================
                     STEP 1 - VERIFY EMAIL
                     ================================== -->

                <asp:Panel
                    ID="pnlRequest"
                    runat="server">

                    <div class="login-heading">

                        <h2>
                            Forgot Password?
                        </h2>

                        <p>
                            Enter your registered email address
                            and we'll help you set a new password.
                        </p>

                    </div>


                    <!-- Step Indicator -->

                    <div class="fp-steps">

                        <div class="fp-step active">

                            <span class="fp-step-dot">
                                1
                            </span>

                            <span class="fp-step-label">
                                Verify Email
                            </span>

                        </div>

                        <div class="fp-step-line"></div>

                        <div class="fp-step">

                            <span class="fp-step-dot">
                                2
                            </span>

                            <span class="fp-step-label">
                                New Password
                            </span>

                        </div>

                    </div>


                    <!-- Validation Summary -->

                    <asp:ValidationSummary
                        ID="vsRequest"
                        runat="server"
                        ValidationGroup="ForgotGroup"
                        CssClass="validation-summary"
                        HeaderText="Please correct the following:"
                        DisplayMode="BulletList" />


                    <!-- ==============================
                         EMAIL
                         ============================== -->

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
                            ValidationGroup="ForgotGroup"
                            ErrorMessage="Email address is required."
                            Text="Email address is required."
                            CssClass="field-error"
                            Display="Dynamic">
                        </asp:RequiredFieldValidator>


                        <asp:RegularExpressionValidator
                            ID="revEmail"
                            runat="server"
                            ControlToValidate="txtEmail"
                            ValidationGroup="ForgotGroup"
                            ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                            ErrorMessage="Please enter a valid email address."
                            Text="Please enter a valid email address."
                            CssClass="field-error"
                            Display="Dynamic">
                        </asp:RegularExpressionValidator>

                    </div>


                    <!-- Server Message -->

                    <asp:Label
                        ID="lblRequestMessage"
                        runat="server"
                        CssClass="server-error"
                        Visible="false">
                    </asp:Label>


                    <!-- Continue Button -->

                    <asp:Button
                        ID="btnSend"
                        runat="server"
                        Text="Continue"
                        ValidationGroup="ForgotGroup"
                        CausesValidation="true"
                        CssClass="btn login-button"
                        OnClick="btnSend_Click" />

                </asp:Panel>


                <!-- ==================================
                     STEP 2 - SET NEW PASSWORD
                     ================================== -->

                <asp:Panel
                    ID="pnlReset"
                    runat="server"
                    Visible="false">

                    <div class="login-heading">

                        <h2>
                            Reset Password
                        </h2>

                        <p>
                            Choose a new password for
                            <span class="fp-email-chip">

                                <asp:Label
                                    ID="lblResetEmail"
                                    runat="server"
                                    Text="">
                                </asp:Label>

                            </span>
                        </p>

                    </div>


                    <!-- Step Indicator -->

                    <div class="fp-steps">

                        <div class="fp-step done">

                            <span class="fp-step-dot">

                                <i class="bi bi-check-lg"></i>

                            </span>

                            <span class="fp-step-label">
                                Verify Email
                            </span>

                        </div>

                        <div class="fp-step-line done"></div>

                        <div class="fp-step active">

                            <span class="fp-step-dot">
                                2
                            </span>

                            <span class="fp-step-label">
                                New Password
                            </span>

                        </div>

                    </div>


                    <!-- Validation Summary -->

                    <asp:ValidationSummary
                        ID="vsReset"
                        runat="server"
                        ValidationGroup="ResetGroup"
                        CssClass="validation-summary"
                        HeaderText="Please correct the following:"
                        DisplayMode="BulletList" />


                    <!-- ==============================
                         NEW PASSWORD
                         ============================== -->

                    <div class="mb-3">

                        <label
                            for="txtNew"
                            class="form-label">

                            New Password

                        </label>


                        <small class="fp-help">
                            At least 6 characters.
                        </small>


                        <div class="input-group custom-input-group">

                            <span class="input-group-text">

                                <i class="bi bi-lock"></i>

                            </span>


                            <asp:TextBox
                                ID="txtNew"
                                runat="server"
                                CssClass="form-control password-input"
                                TextMode="Password"
                                placeholder="Enter your new password"
                                MaxLength="50">
                            </asp:TextBox>


                            <button
                                type="button"
                                class="password-toggle"
                                onclick="togglePassword('newPasswordIcon','<%= txtNew.ClientID %>')">

                                <i
                                    id="newPasswordIcon"
                                    class="bi bi-eye">
                                </i>

                            </button>

                        </div>


                        <asp:RequiredFieldValidator
                            ID="rfvNew"
                            runat="server"
                            ControlToValidate="txtNew"
                            ValidationGroup="ResetGroup"
                            ErrorMessage="New password is required."
                            Text="New password is required."
                            CssClass="field-error"
                            Display="Dynamic">
                        </asp:RequiredFieldValidator>


                        <asp:RegularExpressionValidator
                            ID="revNew"
                            runat="server"
                            ControlToValidate="txtNew"
                            ValidationGroup="ResetGroup"
                            ValidationExpression="^.{6,}$"
                            ErrorMessage="Password must be at least 6 characters."
                            Text="Password must be at least 6 characters."
                            CssClass="field-error"
                            Display="Dynamic">
                        </asp:RegularExpressionValidator>

                    </div>


                    <!-- ==============================
                         CONFIRM PASSWORD
                         ============================== -->

                    <div class="mb-3">

                        <label
                            for="txtConfirm"
                            class="form-label">

                            Confirm New Password

                        </label>


                        <div class="input-group custom-input-group">

                            <span class="input-group-text">

                                <i class="bi bi-shield-lock"></i>

                            </span>


                            <asp:TextBox
                                ID="txtConfirm"
                                runat="server"
                                CssClass="form-control password-input"
                                TextMode="Password"
                                placeholder="Re-enter your new password"
                                MaxLength="50">
                            </asp:TextBox>


                            <button
                                type="button"
                                class="password-toggle"
                                onclick="togglePassword('confirmPasswordIcon','<%= txtConfirm.ClientID %>')">

                                <i
                                    id="confirmPasswordIcon"
                                    class="bi bi-eye">
                                </i>

                            </button>

                        </div>


                        <asp:RequiredFieldValidator
                            ID="rfvConfirm"
                            runat="server"
                            ControlToValidate="txtConfirm"
                            ValidationGroup="ResetGroup"
                            ErrorMessage="Please confirm the new password."
                            Text="Please confirm the new password."
                            CssClass="field-error"
                            Display="Dynamic">
                        </asp:RequiredFieldValidator>


                        <asp:CompareValidator
                            ID="cmpConfirm"
                            runat="server"
                            ControlToValidate="txtConfirm"
                            ControlToCompare="txtNew"
                            ValidationGroup="ResetGroup"
                            ErrorMessage="The two passwords do not match."
                            Text="The two passwords do not match."
                            CssClass="field-error"
                            Display="Dynamic">
                        </asp:CompareValidator>

                    </div>


                    <!-- Reset Button -->

                    <asp:Button
                        ID="btnReset"
                        runat="server"
                        Text="Reset Password"
                        ValidationGroup="ResetGroup"
                        CausesValidation="true"
                        CssClass="btn login-button"
                        OnClick="btnReset_Click" />


                    <!-- Back Link -->

                    <div class="fp-back-row">

                        <asp:LinkButton
                            ID="btnBack"
                            runat="server"
                            CssClass="forgot-password"
                            CausesValidation="false"
                            OnClick="btnBack_Click">

                            <i class="bi bi-arrow-left"></i>

                            Use a different email

                        </asp:LinkButton>

                    </div>

                </asp:Panel>


                <!-- ==================================
                     STEP 3 - SUCCESS
                     ================================== -->

                <asp:Panel
                    ID="pnlSuccess"
                    runat="server"
                    Visible="false">

                    <div class="fp-success">

                        <div class="fp-success-icon">

                            <i class="bi bi-check-lg"></i>

                        </div>


                        <h2>
                            Password Reset
                        </h2>


                        <p>
                            Your password has been changed
                            successfully. You can now sign in
                            with your new password.
                        </p>


                        <a href="Login.aspx"
                           class="btn login-button">

                            <i class="bi bi-box-arrow-in-right me-2"></i>

                            Back to Login

                        </a>

                    </div>

                </asp:Panel>


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

    function togglePassword(iconId, passwordId) {

        var passwordBox =
            document.getElementById(passwordId);

        var icon =
            document.getElementById(iconId);


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

</script>


</body>

</html>
