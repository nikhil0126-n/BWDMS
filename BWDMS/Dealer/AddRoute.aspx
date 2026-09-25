<%@ Page Title="Add Route"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddRoute.aspx.cs"
    Inherits="BWDMS.Dealer.AddRoute" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    <asp:Label
        ID="lblPageTitle"
        runat="server"
        Text="Add Route">
    </asp:Label>

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- ============================================================
         PAGE HEADER
         ============================================================ -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">

                <asp:Label
                    ID="lblHeading"
                    runat="server"
                    Text="Add Route">
                </asp:Label>

            </h3>


            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblSubHeading"
                    runat="server"
                    Text="Create a new sales and delivery route">
                </asp:Label>

            </p>

        </div>


        <a href="Routes.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left me-1"></i>

            Back to Routes

        </a>

    </div>



    <!-- ============================================================
         ROUTE FORM
         ============================================================ -->

    <div class="dashboard-card">


        <div class="mb-4">

            <h5 class="fw-bold mb-1">

                <asp:Label
                    ID="lblFormTitle"
                    runat="server"
                    Text="Route Information">
                </asp:Label>

            </h5>

            <small class="text-muted">
                Enter the route details below.
            </small>

        </div>



        <!-- ========================================================
             MESSAGE
             ======================================================== -->

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



        <!-- ========================================================
             FORM
             ======================================================== -->

        <div class="row g-4">


            <!-- Route Name -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Route Name

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtRouteName"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="100"
                    placeholder="Example: Rajkot North">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvRouteName"
                    runat="server"
                    ControlToValidate="txtRouteName"
                    ErrorMessage="Route name is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>



            <!-- Route Code -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Route Code

                </label>


                <asp:TextBox
                    ID="txtRouteCode"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="50"
                    placeholder="Example: RKT-N01">
                </asp:TextBox>


                <div class="form-text">

                    Optional unique code for this route.

                </div>

            </div>



            <!-- Day -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Route Day

                    <span class="text-danger">*</span>

                </label>


                <asp:DropDownList
                    ID="ddlDayOfWeek"
                    runat="server"
                    CssClass="form-select">

                    <asp:ListItem
                        Text="-- Select Day --"
                        Value="">
                    </asp:ListItem>

                    <asp:ListItem
                        Text="Monday"
                        Value="Monday">
                    </asp:ListItem>

                    <asp:ListItem
                        Text="Tuesday"
                        Value="Tuesday">
                    </asp:ListItem>

                    <asp:ListItem
                        Text="Wednesday"
                        Value="Wednesday">
                    </asp:ListItem>

                    <asp:ListItem
                        Text="Thursday"
                        Value="Thursday">
                    </asp:ListItem>

                    <asp:ListItem
                        Text="Friday"
                        Value="Friday">
                    </asp:ListItem>

                    <asp:ListItem
                        Text="Saturday"
                        Value="Saturday">
                    </asp:ListItem>

                    <asp:ListItem
                        Text="Sunday"
                        Value="Sunday">
                    </asp:ListItem>

                </asp:DropDownList>


                <asp:RequiredFieldValidator
                    ID="rfvDayOfWeek"
                    runat="server"
                    ControlToValidate="ddlDayOfWeek"
                    InitialValue=""
                    ErrorMessage="Please select a route day."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

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



        <!-- ========================================================
             BUTTONS
             ======================================================== -->

        <div class="d-flex justify-content-end gap-2">


            <a href="Routes.aspx"
               class="btn btn-light border">

                <i class="bi bi-x-lg me-1"></i>

                Cancel

            </a>



            <asp:Button
                ID="btnSaveRoute"
                runat="server"
                Text="Create Route"
                CssClass="btn btn-danger px-4"
                OnClick="btnSaveRoute_Click">
            </asp:Button>


        </div>


    </div>

</asp:Content>