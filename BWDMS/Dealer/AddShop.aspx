<%@ Page Title="Add Shop"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddShop.aspx.cs"
    Inherits="BWDMS.Dealer.AddShop" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    <asp:Label
        ID="lblPageTitle"
        runat="server"
        Text="Add Shop">
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
                    Text="Add Shop">
                </asp:Label>

            </h3>


            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblSubHeading"
                    runat="server"
                    Text="Register a retail shop served by your dealership">
                </asp:Label>

            </p>

        </div>


        <a href="Shops.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Shops

        </a>

    </div>


    <div class="dashboard-card">


        <div class="mb-4">

            <h5 class="fw-bold mb-1">
                Shop Details
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


            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Shop Name

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtShopName"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="500"
                    placeholder="Example: Shree Ganesh Kirana">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvShopName"
                    runat="server"
                    ControlToValidate="txtShopName"
                    ErrorMessage="Shop name is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Shop Code
                </label>


                <asp:TextBox
                    ID="txtShopCode"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="200"
                    placeholder="Optional internal code">
                </asp:TextBox>

            </div>


            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Owner Name
                </label>


                <asp:TextBox
                    ID="txtOwnerName"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="400"
                    placeholder="Shop owner">
                </asp:TextBox>

            </div>


            <div class="col-md-3">

                <label class="form-label fw-semibold">
                    Phone
                </label>


                <asp:TextBox
                    ID="txtPhone"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="80"
                    placeholder="Contact number">
                </asp:TextBox>

            </div>


            <div class="col-md-3">

                <label class="form-label fw-semibold">
                    Email
                </label>


                <asp:TextBox
                    ID="txtEmail"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="600"
                    placeholder="Optional email">
                </asp:TextBox>


                <asp:RegularExpressionValidator
                    ID="revEmail"
                    runat="server"
                    ControlToValidate="txtEmail"
                    ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                    ErrorMessage="Enter a valid email address."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RegularExpressionValidator>

            </div>


            <div class="col-12">

                <label class="form-label fw-semibold">
                    Address
                </label>


                <asp:TextBox
                    ID="txtAddress"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="1000"
                    TextMode="MultiLine"
                    Rows="2"
                    placeholder="Shop address">
                </asp:TextBox>

            </div>


            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Village
                </label>


                <asp:DropDownList
                    ID="ddlVillage"
                    runat="server"
                    CssClass="form-select">
                </asp:DropDownList>

            </div>


            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Opening Balance
                </label>


                <asp:TextBox
                    ID="txtOpeningBalance"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="9"
                    placeholder="0.00">
                </asp:TextBox>


                <asp:RegularExpressionValidator
                    ID="revOpeningBalance"
                    runat="server"
                    ControlToValidate="txtOpeningBalance"
                    ValidationExpression="^-?\d{0,7}(\.\d{1,2})?$"
                    ErrorMessage="Enter a valid opening balance."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RegularExpressionValidator>

            </div>


            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Route
                </label>


                <small class="text-muted d-block mb-1">
                    Optional. Choosing a route limits the schedule list.
                </small>


                <asp:DropDownList
                    ID="ddlRoute"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlRoute_SelectedIndexChanged">
                </asp:DropDownList>

            </div>


            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Route Schedule
                </label>


                <small class="text-muted d-block mb-1">
                    The day this shop is visited.
                </small>


                <asp:DropDownList
                    ID="ddlSchedule"
                    runat="server"
                    CssClass="form-select">
                </asp:DropDownList>

            </div>


            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Credit Limit
                </label>


                <asp:TextBox
                    ID="txtCreditLimit"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="9"
                    placeholder="0.00">
                </asp:TextBox>


                <asp:RegularExpressionValidator
                    ID="revCreditLimit"
                    runat="server"
                    ControlToValidate="txtCreditLimit"
                    ValidationExpression="^\d{0,7}(\.\d{1,2})?$"
                    ErrorMessage="Credit limit cannot be negative."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RegularExpressionValidator>

            </div>


            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Status
                </label>


                <asp:DropDownList
                    ID="ddlStatus"
                    runat="server"
                    CssClass="form-select">

                    <asp:ListItem Text="Active" Value="1" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Inactive" Value="0">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>


        </div>


        <hr class="my-4" />


        <div class="d-flex justify-content-end gap-2">

            <a href="Shops.aspx"
               class="btn btn-light border">

                <i class="bi bi-x-lg"></i>

                Cancel

            </a>


            <asp:Button
                ID="btnSave"
                runat="server"
                Text="Create Shop"
                CssClass="btn btn-danger px-4"
                OnClick="btnSave_Click" />

        </div>


    </div>

</asp:Content>
