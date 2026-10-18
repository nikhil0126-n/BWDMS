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
            <h3 class="fw-bold mb-1 page-title">
                <asp:Label
                    ID="lblPageTitle"
                    runat="server"
                    Text="Add Weekly Route Plan">
                </asp:Label>
            </h3>

            <p class="text-muted mb-0 page-subtitle">
                Create and manage your weekly route planning
            </p>
        </div>

        <a href="WeeklyRoutePlans.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left me-1"></i>
            Back

        </a>

    </div>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="d-block mt-3">
    </asp:Label>


    <div class="row g-4">


        <div class="col-lg-4">

            <div class="summary-box">

                <h5>
                    <i class="bi bi-calendar-week me-2"></i>
                    Weekly Plan Information
                </h5>


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


            </div>

        </div>


        <div class="col-lg-8">

            <div class="summary-box">

                <h5>
                    <i class="bi bi-geo-alt me-2"></i>
                    Select Villages
                </h5>

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


            </div>

        </div>

    </div>


    <!-- Actions -->

    <div class="form-actions d-flex justify-content-end gap-2 mt-4 pt-4">

        <asp:Button
            ID="btnSavePlan"
            runat="server"
            Text="Save Weekly Plan"
            CssClass="btn btn-primary px-4"
            OnClick="btnSavePlan_Click"
            CausesValidation="true" />


        <a href="WeeklyRoutePlans.aspx"
           class="btn btn-outline-secondary">

            Cancel

        </a>

    </div>


    <script src="../Scripts/AddWeeklyRoutePlan.js"></script>

</asp:Content>