<%@ Page Title="Add Field Account"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddSalesman.aspx.cs"
    Inherits="BWDMS.Dealer.AddSalesman" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    <asp:Label
        ID="lblPageTitle"
        runat="server"
        Text="Add Account">
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
                    Text="Add Account">
                </asp:Label>

            </h3>


            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblSubHeading"
                    runat="server"
                    Text="Create a salesman login for your dealership">
                </asp:Label>

            </p>

        </div>


        <a href="Salesmen.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left me-1"></i>

            Back to Accounts

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




        <asp:Panel
            ID="pnlMessage"
            runat="server"
            Visible="false"
            CssClass="alert mb-4">

            <asp:Label
                ID="lblMessage"
                runat="server">
            </asp:Label>

        </asp:Panel>




        <div class="row g-4">


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
                    placeholder="Example: ramesh@balaji.com">
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




            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Role

                    <span class="text-danger">*</span>

                </label>


                <%-- CK_Users_Role allows only Salesman / Dealer / Admin,
                     so a Driver login cannot exist. Drivers are picked
                     from the salesman list in AddRouteSchedule. --%>
                <asp:DropDownList
                    ID="ddlRole"
                    runat="server"
                    CssClass="form-select">

                    <asp:ListItem
                        Text="Salesman"
                        Value="Salesman"
                        Selected="True">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>




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
                    Text="Leave blank while editing to keep the current password.">
                </asp:Label>

            </div>




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


            <a href="Salesmen.aspx"
               class="btn btn-light border">

                <i class="bi bi-x-lg me-1"></i>

                Cancel

            </a>


            <asp:Button
                ID="btnSave"
                runat="server"
                Text="Create Account"
                CssClass="btn btn-danger px-4"
                OnClick="btnSave_Click" />

        </div>


    </div>

</asp:Content>
