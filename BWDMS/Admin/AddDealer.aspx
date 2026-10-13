<%@ Page Title="Add Dealer"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddDealer.aspx.cs"
    Inherits="BWDMS.Admin.AddDealer" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Add Dealer

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">

                <asp:Label
                    ID="lblHeading"
                    runat="server"
                    Text="Add Dealer">
                </asp:Label>

            </h3>

            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblSubHeading"
                    runat="server"
                    Text="Create a new dealer account">
                </asp:Label>

            </p>

        </div>


        <a href="Dealers.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Dealers

        </a>

    </div>


    <div class="dashboard-card">


        <div class="mb-4">

            <h5 class="fw-bold mb-1">
                Dealer Details
            </h5>

            <small class="text-muted">
                Fields marked <span class="text-danger">*</span> are required.
            </small>

        </div>


        <!-- Message -->

        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <div class="row g-4">


            <!-- Dealer Name -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Dealer Name

                    <span class="text-danger">*</span>

                </label>

                <asp:TextBox
                    ID="txtFullName"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="200"
                    placeholder="Example: Balaji Wafers">
                </asp:TextBox>

                <asp:RequiredFieldValidator
                    ID="rfvFullName"
                    runat="server"
                    ControlToValidate="txtFullName"
                    ErrorMessage="Dealer name is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <!-- Email -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Email Address

                    <span class="text-danger">*</span>

                    <small class="text-muted fw-normal">
                        used to sign in
                    </small>

                </label>

                <asp:TextBox
                    ID="txtEmail"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="300"
                    TextMode="Email"
                    placeholder="Example: dealer@balaji.com">
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
                    ValidationExpression="\S+@\S+\.\S+"
                    ErrorMessage="Enter a valid email address."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RegularExpressionValidator>

            </div>


            <!-- Phone -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Phone Number
                    <span class="text-danger">*</span>
                </label>

                <asp:TextBox
                    ID="txtPhone"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="10"
                    placeholder="Example: 9876543210">
                </asp:TextBox>

                <asp:RequiredFieldValidator
                    ID="rfvPhone"
                    runat="server"
                    ControlToValidate="txtPhone"
                    ErrorMessage="Phone number is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

                <asp:RegularExpressionValidator
                    ID="revPhone"
                    runat="server"
                    ControlToValidate="txtPhone"
                    ValidationExpression="^(\d{10})?$"
                    ErrorMessage="Enter a 10-digit mobile number."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RegularExpressionValidator>

            </div>


            <!-- Status -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Account Status
                </label>

                <asp:DropDownList
                    ID="ddlStatus"
                    runat="server"
                    CssClass="form-select">

                    <asp:ListItem
                        Text="Active"
                        Value="1"
                        Selected="True">
                    </asp:ListItem>

                    <asp:ListItem
                        Text="Inactive"
                        Value="0">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>


            <!-- Password -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Password

                    <span class="text-danger">*</span>

                    <small
                        ID="lblPasswordHint"
                        runat="server"
                        class="text-muted fw-normal">
                        minimum 6 characters
                    </small>

                </label>

                <asp:TextBox
                    ID="txtPassword"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="100"
                    TextMode="Password"
                    placeholder="Minimum 6 characters">
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


            <!-- Confirm Password -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Confirm Password

                    <span class="text-danger">*</span>

                </label>

                <asp:TextBox
                    ID="txtConfirmPassword"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="100"
                    TextMode="Password"
                    placeholder="Re-enter the password">
                </asp:TextBox>

                <asp:RequiredFieldValidator
                    ID="rfvConfirmPassword"
                    runat="server"
                    ControlToValidate="txtConfirmPassword"
                    ErrorMessage="Please confirm the password."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

                <asp:CompareValidator
                    ID="cvPassword"
                    runat="server"
                    ControlToValidate="txtConfirmPassword"
                    ControlToCompare="txtPassword"
                    ErrorMessage="Passwords do not match."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:CompareValidator>

            </div>


        </div>


        <hr class="my-4" />


        <!-- Buttons -->

        <div class="d-flex justify-content-end gap-2">

            <a href="Dealers.aspx"
               class="btn btn-light border">

                <i class="bi bi-x-lg me-1"></i>

                Cancel

            </a>


            <asp:Button
                ID="btnCreateDealer"
                runat="server"
                Text="Create Dealer"
                CssClass="btn btn-danger px-4"
                OnClick="btnCreateDealer_Click" />

        </div>

    </div>


</asp:Content>
