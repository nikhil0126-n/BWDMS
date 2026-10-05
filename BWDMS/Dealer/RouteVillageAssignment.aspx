<%@ Page Title="Route Village Assignment"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="RouteVillageAssignment.aspx.cs"
    Inherits="BWDMS.Dealer.RouteVillageAssignment" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Route Village Assignment

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <link href="../Content/RouteVillageAssignment.css"
          rel="stylesheet" />


    <!-- ============================================================
         PAGE HEADER
         ============================================================ -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Route Village Assignment
            </h3>

            <p class="text-muted mb-0">
                Permanently assign villages to a route and set their visit order
            </p>

        </div>


        <a href="Routes.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left me-1"></i>

            Back to Routes

        </a>

    </div>



    <!-- ============================================================
         ASSIGNMENT FORM
         ============================================================ -->

    <div class="dashboard-card">


        <!-- ========================================================
             NO ROUTE HINT
             ======================================================== -->

        <asp:Panel
            ID="pnlNoRoutes"
            runat="server"
            Visible="false"
            CssClass="alert alert-warning mb-4">

            You do not have any active route yet.

            <a href="AddRoute.aspx"
               class="alert-link">

                Create a route first

            </a>

            , then come back to assign villages to it.

        </asp:Panel>


        <!-- ========================================================
             SELECT ROUTE
             ======================================================== -->

        <div class="form-section-title">
            <i class="bi bi-signpost me-2"></i>
            Select Route
        </div>


        <div class="row g-4">

            <div class="col-md-6">

                <label class="form-label">
                    Route <span class="text-danger">*</span>
                </label>

                <asp:DropDownList
                    ID="ddlRoute"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlRoute_SelectedIndexChanged">
                </asp:DropDownList>

                <asp:RequiredFieldValidator
                    ID="rfvRoute"
                    runat="server"
                    ControlToValidate="ddlRoute"
                    InitialValue="0"
                    ErrorMessage="Please select a route."
                    CssClass="text-danger validation-message"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>

        </div>


        <hr class="my-4" />


        <!-- ========================================================
             MESSAGE
             ======================================================== -->

        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="d-block mb-3">
        </asp:Label>


        <!-- ========================================================
             VILLAGE PICKER
             ======================================================== -->

        <div class="form-section-title">
            <i class="bi bi-geo-alt me-2"></i>
            Select Villages
        </div>

        <p class="text-muted small">
            Tick the villages that belong to this route.
            The order shown in the list becomes the permanent visit sequence.
        </p>


        <div class="village-search-wrapper mb-3">

            <input
                type="text"
                id="villageSearch"
                class="form-control"
                placeholder="Search villages..."
                autocomplete="off" />

        </div>


        <div class="village-list-container">

            <asp:CheckBoxList
                ID="cblVillages"
                runat="server"
                CssClass="village-check-list"
                RepeatDirection="Vertical"
                RepeatLayout="Flow">
            </asp:CheckBoxList>

        </div>


        <asp:CustomValidator
            ID="cvVillages"
            runat="server"
            ErrorMessage="Please select at least one village."
            CssClass="text-danger validation-message"
            Display="Dynamic"
            OnServerValidate="cvVillages_ServerValidate">
        </asp:CustomValidator>


        <div class="mt-2">

            <small class="text-muted">
                Select one or more villages. Their displayed order will be used
                as the visit sequence.
            </small>

        </div>


        <!-- ========================================================
             QUICK ADD VILLAGE
             ======================================================== -->

        <hr class="my-4" />

        <div class="form-section-title">
            <i class="bi bi-plus-circle me-2"></i>
            Add a New Village
        </div>


        <div class="row g-2">

            <div class="col-md-6">

                <asp:TextBox
                    ID="txtNewVillage"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="300"
                    placeholder="Village name...">
                </asp:TextBox>

            </div>

            <div class="col-md-6">

                <asp:Button
                    ID="btnAddVillage"
                    runat="server"
                    Text="Add Village"
                    CssClass="btn btn-outline-danger"
                    CausesValidation="false"
                    OnClick="btnAddVillage_Click">
                </asp:Button>

            </div>

        </div>

        <div class="form-text">

            Creates the village and ticks it for the route above.
            Use the Villages page to add Taluka, District and Pincode details.

        </div>


        <!-- ========================================================
             BUTTONS
             ======================================================== -->

        <div class="form-actions mt-4 pt-4">

            <asp:Button
                ID="btnSave"
                runat="server"
                Text="Save Assignment"
                CssClass="btn btn-danger px-4"
                CausesValidation="true"
                OnClick="btnSave_Click">
            </asp:Button>


            <a href="Routes.aspx"
               class="btn btn-outline-secondary ms-2">

                Cancel

            </a>

        </div>

    </div>


    <script src="../Scripts/RouteVillageAssignment.js"></script>

</asp:Content>
