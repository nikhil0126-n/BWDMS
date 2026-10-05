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



            <!-- Route Number -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Route Number

                </label>


                <asp:TextBox
                    ID="txtRouteCode"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="50"
                    placeholder="Example: RKT-N01">
                </asp:TextBox>


                <div class="form-text">

                    Unique route number. Must be unique per route.

                </div>

            </div>



            <!-- Route Type -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Route Type

                </label>


                <asp:DropDownList
                    ID="ddlRouteType"
                    runat="server"
                    AutoPostBack="false"
                    CssClass="form-select">

                    <asp:ListItem
                        Text="-- Select Type --"
                        Value="">
                    </asp:ListItem>

                    <asp:ListItem
                        Text="Order Taking"
                        Value="Order Taking">
                    </asp:ListItem>

                    <asp:ListItem
                        Text="Delivery"
                        Value="Delivery">
                    </asp:ListItem>

                    <asp:ListItem
                        Text="Collection"
                        Value="Collection">
                    </asp:ListItem>

                    <asp:ListItem
                        Text="Mixed"
                        Value="Mixed">
                    </asp:ListItem>

                </asp:DropDownList>


                <div class="form-text">

                    Optional. Example: Order Taking.

                </div>

            </div>



            <!-- Order / Dispatch Days -->

            <div class="col-md-12">

                <label class="form-label fw-semibold">

                    Order / Dispatch Days

                    <span class="text-danger">*</span>

                </label>


                <asp:CheckBoxList
                    ID="cblDays"
                    runat="server"
                    RepeatDirection="Horizontal"
                    RepeatLayout="Flow">

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

                </asp:CheckBoxList>


                <div class="form-text">

                    Tick every day on which this route operates.

                </div>


                <asp:CustomValidator
                    ID="cvDays"
                    runat="server"
                    ServerValidate="cvDays_ServerValidate"
                    ErrorMessage="Select at least one order/dispatch day."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:CustomValidator>

            </div>



            <!-- Preferred Salesperson -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Preferred Salesperson

                </label>


                <asp:DropDownList
                    ID="ddlSalesman"
                    runat="server"
                    AutoPostBack="false"
                    CssClass="form-select">
                </asp:DropDownList>


                <div class="form-text">

                    Default salesperson for this route.

                </div>

            </div>



            <!-- Preferred Driver -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Preferred Driver

                </label>


                <asp:DropDownList
                    ID="ddlDriver"
                    runat="server"
                    AutoPostBack="false"
                    CssClass="form-select">
                </asp:DropDownList>


                <div class="form-text">

                    Default driver for this route.

                </div>

            </div>



            <!-- Default Truck -->

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Default Truck

                </label>


                <asp:DropDownList
                    ID="ddlVehicle"
                    runat="server"
                    AutoPostBack="false"
                    CssClass="form-select">
                </asp:DropDownList>


                <div class="form-text">

                    Default vehicle for this route.

                </div>

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



        <!-- ========================================================
             AUDIT INFORMATION (EDIT MODE ONLY)
             ======================================================== -->

        <asp:Panel
            ID="pnlAudit"
            runat="server"
            Visible="false"
            CssClass="border rounded p-3 mt-4 bg-light">

            <asp:Label
                ID="lblAudit"
                runat="server"
                CssClass="form-text text-muted d-block">
            </asp:Label>

        </asp:Panel>



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
