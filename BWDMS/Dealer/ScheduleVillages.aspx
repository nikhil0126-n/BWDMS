<%@ Page Title="Schedule Villages"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="ScheduleVillages.aspx.cs"
    Inherits="BWDMS.Dealer.ScheduleVillages" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Schedule Villages

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <link href="../Content/ScheduleVillages.css"
          rel="stylesheet" />


    <!-- ============================================================
         PAGE HEADER
         ============================================================ -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Schedule Villages
            </h3>

            <p class="text-muted mb-0">
                Choose the villages this schedule visits and set the visit order
            </p>

        </div>


        <a href="RouteSchedules.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left me-1"></i>

            Back to Schedules

        </a>

    </div>



    <!-- ============================================================
         SCHEDULE SUMMARY
         ============================================================ -->

    <div class="dashboard-card mb-4">

        <div class="form-section-title">
            <i class="bi bi-calendar2-check me-2"></i>
            Schedule Information
        </div>


        <div class="row g-4">

            <div class="col-md-4">

                <label class="form-label text-muted mb-1">
                    Route
                </label>

                <div class="fw-semibold">
                    <asp:Label
                        ID="lblRouteName"
                        runat="server">
                    </asp:Label>
                </div>

            </div>


            <div class="col-md-2">

                <label class="form-label text-muted mb-1">
                    Day of Week
                </label>

                <div class="fw-semibold">
                    <asp:Label
                        ID="lblDayOfWeek"
                        runat="server">
                    </asp:Label>
                </div>

            </div>


            <div class="col-md-2">

                <label class="form-label text-muted mb-1">
                    Vehicle
                </label>

                <div class="fw-semibold">
                    <asp:Label
                        ID="lblVehicleNumber"
                        runat="server">
                    </asp:Label>
                </div>

            </div>


            <div class="col-md-2">

                <label class="form-label text-muted mb-1">
                    Salesman
                </label>

                <div class="fw-semibold">
                    <asp:Label
                        ID="lblSalesmanName"
                        runat="server">
                    </asp:Label>
                </div>

            </div>


            <div class="col-md-2">

                <label class="form-label text-muted mb-1">
                    Driver
                </label>

                <div class="fw-semibold">
                    <asp:Label
                        ID="lblDriverName"
                        runat="server">
                    </asp:Label>
                </div>

            </div>

        </div>

    </div>



    <!-- ============================================================
         MESSAGE
         ============================================================ -->

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



    <!-- ============================================================
         NO VILLAGES ON THE ROUTE
         ============================================================ -->

    <asp:Panel
        ID="pnlNoVillages"
        runat="server"
        Visible="false"
        CssClass="alert alert-warning mb-4">

        This route has no villages yet, so there is nothing to add
        to this schedule.

        <a href="RouteVillageAssignment.aspx"
           class="alert-link">

            Assign villages to the route

        </a>

        first, then come back and plan this schedule day.

    </asp:Panel>



    <!-- ============================================================
         VILLAGE PICKER + VISIT ORDER
         ============================================================ -->

    <div class="dashboard-card">

        <asp:Panel
            ID="pnlVillagePicker"
            runat="server">

            <div class="form-section-title">
                <i class="bi bi-geo-alt me-2"></i>
                Villages Visited by This Schedule
            </div>

            <p class="text-muted small mb-3">
                Tick every village this schedule visits, then type the visit
                order (1 = first stop). The order is saved for this schedule
                only, the route assignment stays unchanged.
            </p>


            <div class="village-search-wrapper mb-3">

                <input
                    type="text"
                    id="villageSearch"
                    class="form-control"
                    placeholder="Search villages..."
                    autocomplete="off" />

            </div>


            <div class="village-list-container village-order-list">

                <asp:Repeater
                    ID="repVillages"
                    runat="server">

                    <ItemTemplate>

                        <div class="village-order-row">

                            <asp:CheckBox
                                ID="chkSelect"
                                runat="server"
                                CssClass="village-order-check"
                                Checked='<%# Eval("IsScheduled") %>' />

                            <asp:HiddenField
                                ID="hfRouteVillageId"
                                runat="server"
                                Value='<%# Eval("RouteVillageId") %>' />

                            <span class="village-order-text">
                                <%# Eval("DisplayText") %>
                            </span>

                            <asp:TextBox
                                ID="txtSequence"
                                runat="server"
                                CssClass="village-order-input"
                                TextMode="Number"
                                Text='<%# Eval("SequenceValue") %>'>
                            </asp:TextBox>

                            <asp:RangeValidator
                                ID="rvSequence"
                                runat="server"
                                ControlToValidate="txtSequence"
                                Type="Integer"
                                MinimumValue="1"
                                MaximumValue="999999"
                                ErrorMessage="Order must be 1 or more."
                                CssClass="text-danger small validation-message"
                                Display="Dynamic">
                            </asp:RangeValidator>

                        </div>

                    </ItemTemplate>

                </asp:Repeater>

            </div>


            <div class="mt-3">

                <small class="text-muted">
                    Only villages permanently assigned to this route can be
                    visited by its schedules.
                </small>

            </div>

        </asp:Panel>


        <!-- ========================================================
             BUTTONS
             ======================================================== -->

        <div class="form-actions mt-4 pt-4">

            <asp:Button
                ID="btnSave"
                runat="server"
                Text="Save Schedule Villages"
                CssClass="btn btn-danger px-4"
                CausesValidation="true"
                OnClick="btnSave_Click">
            </asp:Button>


            <a href="RouteSchedules.aspx"
               class="btn btn-outline-secondary ms-2">

                Cancel

            </a>

        </div>

    </div>


    <script src="../Scripts/ScheduleVillages.js"></script>

</asp:Content>
