<%@ Page Title="User Account"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddUser.aspx.cs"
    Inherits="BWDMS.Admin.AddUser" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    <asp:Label
        ID="lblPageTitle"
        runat="server"
        Text="Add User">
    </asp:Label>

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
                    Text="Add User">
                </asp:Label>

            </h3>


            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblSubHeading"
                    runat="server"
                    Text="Create a new account for the system">
                </asp:Label>

            </p>

        </div>


        <a href="Users.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Users

        </a>

    </div>


    <div class="dashboard-card">


        <div class="mb-4">

            <h5 class="fw-bold mb-1">

                <asp:Label
                    ID="lblFormTitle"
                    runat="server"
                    Text="Account Details">
                </asp:Label>

            </h5>

            <small class="text-muted">
                Fields marked <span class="text-danger">*</span> are required.
            </small>

        </div>


        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <div class="row g-4">


            <!-- Full Name -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Full Name

                    <span class="text-danger">*</span>

                </label>

                <asp:TextBox
                    ID="txtFullName"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="200"
                    placeholder="Example: Ramesh Patel">
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


            <!-- Email -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Email

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
                    placeholder="Example: user@balaji.com">
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
                    Phone
                </label>

                <asp:TextBox
                    ID="txtPhone"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="40"
                    placeholder="Example: 9876543210">
                </asp:TextBox>

            </div>


            <!-- Role -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Role

                    <span class="text-danger">*</span>

                </label>

                <%-- CK_Users_Role allows only Admin / Dealer / Salesman --%>
                <asp:DropDownList
                    ID="ddlRole"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlRole_Changed">

                    <asp:ListItem
                        Text="Admin"
                        Value="Admin">
                    </asp:ListItem>

                    <asp:ListItem
                        Text="Dealer"
                        Value="Dealer">
                    </asp:ListItem>

                    <asp:ListItem
                        Text="Salesman"
                        Value="Salesman"
                        Selected="True">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>


            <!-- Dealer (only for salesmen) -->

            <div class="col-md-6" id="divDealer" runat="server">

                <label class="form-label fw-semibold">
                    Dealer
                </label>

                <asp:DropDownList
                    ID="ddlDealer"
                    runat="server"
                    CssClass="form-select">
                </asp:DropDownList>

                <asp:RequiredFieldValidator
                    ID="rfvDealer"
                    runat="server"
                    ControlToValidate="ddlDealer"
                    InitialValue=""
                    ErrorMessage="Please select a dealer for a salesman account."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <!-- Password -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Password

                    <span class="text-danger" id="spanPasswordMark" runat="server">*</span>

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

                <asp:Label
                    ID="lblPasswordHint"
                    runat="server"
                    CssClass="form-text text-muted"
                    Visible="false"
                    Text="Leave blank while editing to keep the current password.">
                </asp:Label>

            </div>


            <!-- Status -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Status
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


        </div>


        <hr class="my-4" />


        <div class="d-flex justify-content-end gap-2">

            <a href="Users.aspx"
               class="btn btn-light border">

                <i class="bi bi-x-lg me-1"></i>

                Cancel

            </a>


            <asp:Button
                ID="btnSave"
                runat="server"
                Text="Create User"
                CssClass="btn btn-danger px-4"
                OnClick="btnSave_Click" />

        </div>


    </div>

</asp:Content>
