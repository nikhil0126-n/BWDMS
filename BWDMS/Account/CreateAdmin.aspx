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

</head>

<body class="bg-light">

<form id="form1" runat="server">

    <div class="container mt-5">

        <div class="row justify-content-center">

            <div class="col-md-5">

                <div class="card shadow">

                    <div class="card-body p-4">

                        <h3 class="mb-4">
                            Create BWDMS Admin
                        </h3>

                        <div class="mb-3">

                            <label class="form-label">
                                Full Name
                            </label>

                            <asp:TextBox
                                ID="txtFullName"
                                runat="server"
                                CssClass="form-control"
                                Text="BWDMS Administrator">
                            </asp:TextBox>

                        </div>

                        <div class="mb-3">

                            <label class="form-label">
                                Email
                            </label>

                            <asp:TextBox
                                ID="txtEmail"
                                runat="server"
                                CssClass="form-control"
                                Text="admin@balaji.com">
                            </asp:TextBox>

                        </div>

                        <div class="mb-3">

                            <label class="form-label">
                                Password
                            </label>

                            <asp:TextBox
                                ID="txtPassword"
                                runat="server"
                                CssClass="form-control"
                                TextMode="Password"
                                Text="Admin@123">
                            </asp:TextBox>

                        </div>

                        <asp:Button
                            ID="btnCreate"
                            runat="server"
                            Text="Create Admin"
                            CssClass="btn btn-danger w-100"
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