<%@ Page Title="Add Weekly Route Plan"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddWeeklyRoutePlan.aspx.cs"
    Inherits="BWDMS.Dealer.AddWeeklyRoutePlan" %>

<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Add Weekly Route Plan

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <link href="../Content/AddWeeklyRoutePlan.css"
          rel="stylesheet" />

    <div class="page-header d-flex justify-content-between align-items-center mb-4">

        <div>
            <h3 class="fw-bold mb-1">
                <asp:Label
                    ID="lblPageTitle"
                    runat="server"
                    Text="Add Weekly Route Plan">
                </asp:Label>
            </h3>

            <p class="text-muted mb-0">
                Create and manage your weekly route planning
            </p>
        </div>

        <a href="WeeklyRoutePlans.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left me-1"></i>
            Back

        </a>

    </div>


    <div class="dashboard-card">

        <div class="form-section-title">
            <i class="bi bi-calendar-week me-2"></i>
            Weekly Plan Information
        </div>


        <div class="row g-4">

            <div class="col-md-6">

                <label class="form-label">
                    Select Route <span class="text-danger">*</span>
                </label>

                <asp:DropDownList
                    ID="ddlRoute"
                    runat="server"
                    CssClass="form-select">

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


            <div class="col-md-6">

                <label class="form-label">
                    Status
                </label>

                <asp:DropDownList
                    ID="ddlStatus"
                    runat="server"
                    CssClass="form-select">

                    <asp:ListItem Text="Active" Value="1" />
                    <asp:ListItem Text="Inactive" Value="0" />

                </asp:DropDownList>

            </div>


            <div class="col-md-6">

                <label class="form-label">
                    Week Start Date <span class="text-danger">*</span>
                </label>

                <asp:TextBox
                    ID="txtWeekStartDate"
                    runat="server"
                    TextMode="Date"
                    CssClass="form-control">
                </asp:TextBox>

                <asp:RequiredFieldValidator
                    ID="rfvWeekStartDate"
                    runat="server"
                    ControlToValidate="txtWeekStartDate"
                    ErrorMessage="Please select the week start date."
                    CssClass="text-danger validation-message"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-6">

                <label class="form-label">
                    Week End Date <span class="text-danger">*</span>
                </label>

                <asp:TextBox
                    ID="txtWeekEndDate"
                    runat="server"
                    TextMode="Date"
                    CssClass="form-control">
                </asp:TextBox>

                <asp:RequiredFieldValidator
                    ID="rfvWeekEndDate"
                    runat="server"
                    ControlToValidate="txtWeekEndDate"
                    ErrorMessage="Please select the week end date."
                    CssClass="text-danger validation-message"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>

        </div>


        <hr class="my-4" />


        <div class="form-section-title">
            <i class="bi bi-geo-alt me-2"></i>
            Select Villages
        </div>

        <p class="text-muted small">
            Select the villages that belong to this weekly route plan.
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


        <div class="mt-3">

            <small class="text-muted">
                Select one or more villages. Their displayed order will be used as the visit sequence.
            </small>

        </div>


        <asp:CustomValidator
            ID="cvVillages"
            runat="server"
            ErrorMessage="Please select at least one village."
            CssClass="text-danger validation-message"
            Display="Dynamic"
            OnServerValidate="cvVillages_ServerValidate">
        </asp:CustomValidator>


        <div class="form-actions mt-4 pt-4">

            <asp:Button
                ID="btnSavePlan"
                runat="server"
                Text="Save Weekly Plan"
                CssClass="btn btn-danger px-4"
                OnClick="btnSavePlan_Click"
                CausesValidation="true" />


            <a href="WeeklyRoutePlans.aspx"
               class="btn btn-outline-secondary ms-2">

                Cancel

            </a>

        </div>


        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="d-block mt-3">
        </asp:Label>

    </div>


    <script src="../Scripts/AddWeeklyRoutePlan.js"></script>

</asp:Content>