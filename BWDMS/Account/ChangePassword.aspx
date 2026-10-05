<%@ Page Title="Settings"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="ChangePassword.aspx.cs"
    Inherits="BWDMS.Account.ChangePassword" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Settings

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Settings
            </h3>

            <p class="text-muted mb-0">
                Your account
            </p>

        </div>

    </div>


    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <div class="row g-4">

        <div class="col-md-6">

            <div class="dashboard-card">

                <h5 class="fw-bold mb-1">
                    Change Password
                </h5>

                <small class="text-muted d-block mb-3">

                    Signed in as

                    <asp:Label
                        ID="lblEmail"
                        runat="server"
                        Text="">
                    </asp:Label>

                </small>


                <div class="mb-3">

                    <label class="form-label fw-semibold">

                        Current Password

                        <span class="text-danger">*</span>

                    </label>


                    <asp:TextBox
                        ID="txtCurrent"
                        runat="server"
                        CssClass="form-control"
                        TextMode="Password">
                    </asp:TextBox>


                    <asp:RequiredFieldValidator
                        ID="rfvCurrent"
                        runat="server"
                        ControlToValidate="txtCurrent"
                        ErrorMessage="Current password is required."
                        CssClass="text-danger small"
                        Display="Dynamic">
                    </asp:RequiredFieldValidator>

                </div>


                <div class="mb-3">

                    <label class="form-label fw-semibold">

                        New Password

                        <span class="text-danger">*</span>

                    </label>


                    <small class="text-muted d-block mb-1">
                        At least 6 characters.
                    </small>


                    <asp:TextBox
                        ID="txtNew"
                        runat="server"
                        CssClass="form-control"
                        TextMode="Password">
                    </asp:TextBox>


                    <asp:RequiredFieldValidator
                        ID="rfvNew"
                        runat="server"
                        ControlToValidate="txtNew"
                        ErrorMessage="New password is required."
                        CssClass="text-danger small"
                        Display="Dynamic">
                    </asp:RequiredFieldValidator>


                    <asp:RegularExpressionValidator
                        ID="revNew"
                        runat="server"
                        ControlToValidate="txtNew"
                        ValidationExpression="^.{6,}$"
                        ErrorMessage="New password must be at least 6 characters."
                        CssClass="text-danger small"
                        Display="Dynamic">
                    </asp:RegularExpressionValidator>

                </div>


                <div class="mb-4">

                    <label class="form-label fw-semibold">

                        Confirm New Password

                        <span class="text-danger">*</span>

                    </label>


                    <asp:TextBox
                        ID="txtConfirm"
                        runat="server"
                        CssClass="form-control"
                        TextMode="Password">
                    </asp:TextBox>


                    <asp:RequiredFieldValidator
                        ID="rfvConfirm"
                        runat="server"
                        ControlToValidate="txtConfirm"
                        ErrorMessage="Please confirm the new password."
                        CssClass="text-danger small"
                        Display="Dynamic">
                    </asp:RequiredFieldValidator>


                    <asp:CompareValidator
                        ID="cmpConfirm"
                        runat="server"
                        ControlToValidate="txtConfirm"
                        ControlToCompare="txtNew"
                        ErrorMessage="The two passwords do not match."
                        CssClass="text-danger small"
                        Display="Dynamic">
                    </asp:CompareValidator>

                </div>


                <div class="d-flex justify-content-end">

                    <asp:Button
                        ID="btnSave"
                        runat="server"
                        Text="Update Password"
                        CssClass="btn btn-danger px-4"
                        OnClick="btnSave_Click" />

                </div>

            </div>

        </div>


        <div class="col-md-6">

            <div class="dashboard-card">

                <h5 class="fw-bold mb-3">
                    Account Details
                </h5>


                <div class="d-flex justify-content-between py-2 border-top">

                    <span class="text-muted">
                        Name
                    </span>


                    <strong>

                        <asp:Label
                            ID="lblFullName"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </strong>

                </div>


                <div class="d-flex justify-content-between py-2 border-top">

                    <span class="text-muted">
                        Role
                    </span>


                    <strong>

                        <asp:Label
                            ID="lblRole"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </strong>

                </div>


                <div class="d-flex justify-content-between py-2 border-top">

                    <span class="text-muted">
                        Phone
                    </span>


                    <strong>

                        <asp:Label
                            ID="lblPhone"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </strong>

                </div>


                <div class="d-flex justify-content-between py-2 border-top">

                    <span class="text-muted">
                        Status
                    </span>


                    <strong>

                        <asp:Label
                            ID="lblStatus"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </strong>

                </div>

            </div>

        </div>

    </div>


</asp:Content>
