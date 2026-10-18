<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="CreateAdmin.aspx.cs"
    Inherits="BWDMS.Account.CreateAdmin" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <title>Create Admin - BWDMS</title>

    <link
        href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"
        rel="stylesheet" />

    <style>
        /* Primary action buttons - kept in step with the dashboard palette. */
        .btn-primary {
            background-color: #686ce5;
            border-color: #686ce5;
        }

            .btn-primary:hover,
            .btn-primary:focus {
                background-color: #5a5ed8;
                border-color: #5a5ed8;
            }
    </style>

</head>

<body class="bg-light">

<form id="form1" runat="server" novalidate>

    <div class="container mt-5">

        <div class="row justify-content-center">

            <div class="col-md-5">

                <div class="card shadow">

                    <div class="card-body p-4">

                        <h3 class="mb-4">
                            Create BWDMS Admin
                        </h3>

                        <asp:ValidationSummary
                            ID="valSummary"
                            runat="server"
                            CssClass="alert alert-danger d-block"
                            HeaderText="Please fix the following:"
                            ShowMessageBox="false"
                            ShowSummary="true" />

                        <div class="mb-3">

                            <label class="form-label">
                                Full Name
                                <span class="text-danger">*</span>
                            </label>

                            <asp:TextBox
                                ID="txtFullName"
                                runat="server"
                                CssClass="form-control"
                                MaxLength="100"
                                Text="BWDMS Administrator">
                            </asp:TextBox>

                            <asp:RequiredFieldValidator
                                ID="rfvFullName"
                                runat="server"
                                ControlToValidate="txtFullName"
                                ErrorMessage="Full name is required."
                                CssClass="text-danger small"
                                Display="Dynamic">
                            </asp:RequiredFieldValidator>

                        </div>

                        <div class="mb-3">

                            <label class="form-label">
                                Email
                                <span class="text-danger">*</span>
                            </label>

                            <asp:TextBox
                                ID="txtEmail"
                                runat="server"
                                CssClass="form-control"
                                MaxLength="100"
                                Text="admin@balaji.com">
                            </asp:TextBox>

                            <asp:RequiredFieldValidator
                                ID="rfvEmail"
                                runat="server"
                                ControlToValidate="txtEmail"
                                ErrorMessage="Email is required."
                                CssClass="text-danger small"
                                Display="Dynamic">
                            </asp:RequiredFieldValidator>

                            <asp:RegularExpressionValidator
                                ID="revEmail"
                                runat="server"
                                ControlToValidate="txtEmail"
                                ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                                ErrorMessage="Enter a valid email address."
                                CssClass="text-danger small"
                                Display="Dynamic">
                            </asp:RegularExpressionValidator>

                        </div>

                        <div class="mb-3">

                            <label class="form-label">
                                Password
                                <span class="text-danger">*</span>
                            </label>

                            <asp:TextBox
                                ID="txtPassword"
                                runat="server"
                                CssClass="form-control"
                                TextMode="Password"
                                MaxLength="50"
                                Text="Admin@123">
                            </asp:TextBox>

                            <asp:RequiredFieldValidator
                                ID="rfvPassword"
                                runat="server"
                                ControlToValidate="txtPassword"
                                ErrorMessage="Password is required."
                                CssClass="text-danger small"
                                Display="Dynamic">
                            </asp:RequiredFieldValidator>

                            <asp:RegularExpressionValidator
                                ID="revPassword"
                                runat="server"
                                ControlToValidate="txtPassword"
                                ValidationExpression="^.{6,}$"
                                ErrorMessage="Password must be at least 6 characters."
                                CssClass="text-danger small"
                                Display="Dynamic">
                            </asp:RegularExpressionValidator>

                        </div>

                        <asp:Button
                            ID="btnCreate"
                            runat="server"
                            Text="Create Admin"
                            CssClass="btn btn-primary w-100"
                            CausesValidation="true"
                            OnClick="btnCreate_Click" />

                        <asp:Label
                            ID="lblMessage"
                            runat="server"
                            CssClass="d-block mt-3">
                        </asp:Label>

                    </div>

                </div>

            </div>

        </div>

    </div>

</form>

</body>

</html>