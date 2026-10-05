<%@ Page Title="Vehicle Loading"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="VehicleLoading.aspx.cs"
    Inherits="BWDMS.Dealer.VehicleLoading" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    <asp:Label
        ID="lblPageTitle"
        runat="server"
        Text="Vehicle Loading">
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
                    Text="Vehicle Loading">
                </asp:Label>

            </h3>


            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblSubHeading"
                    runat="server"
                    Text="Godown to vehicle - the packets leave the godown and are loaded onto the vehicle in one transaction">
                </asp:Label>

            </p>

        </div>


        <a href="Inventory.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Inventory

        </a>

    </div>


    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <!-- Load form -->

    <div class="dashboard-card">


        <!-- Already loaded for the selected vehicle -->

        <div class="mb-4">

            <h5 class="fw-bold mb-1">
                Already Loaded
            </h5>


            <asp:Label
                ID="lblSummaryHeading"
                runat="server"
                CssClass="text-muted small"
                Text="Select a vehicle to see what has already been loaded.">
            </asp:Label>

        </div>


        <div class="table-responsive mb-4">

            <asp:GridView
                ID="gvTodayLoaded"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle mb-0"
                GridLines="None" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="VariantLabel"
                        HeaderText="Product Variant" />

                    <asp:BoundField
                        DataField="LoadedPackets"
                        HeaderText="Loaded Packets" />

                </Columns>


                <EmptyDataTemplate>

                    <p class="text-muted py-2 mb-0">
                        Nothing has been loaded for this vehicle on that date yet.
                    </p>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>


        <hr class="my-4" />


        <!-- Vehicle + date -->

        <div class="row g-4 mb-3">


            <div class="col-md-5">

                <label class="form-label fw-semibold">

                    Route / Schedule

                    <span class="text-danger">*</span>

                </label>


                <!--
                  Stock is loaded FOR A ROUTE, so the route is chosen first
                  and the vehicle follows from the schedule. That is also
                  what lets the end-of-day count be checked against exactly
                  the load this route started the day with.
                -->
                <asp:DropDownList
                    ID="ddlRouteSchedule"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlRouteSchedule_SelectedIndexChanged">
                </asp:DropDownList>


                <asp:RequiredFieldValidator
                    ID="rfvRouteSchedule"
                    ValidationGroup="Load"
                    runat="server"
                    ControlToValidate="ddlRouteSchedule"
                    InitialValue=""
                    ErrorMessage="Select the route this load is for."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-5">

                <label class="form-label fw-semibold">

                    Vehicle

                    <span class="text-danger">*</span>

                </label>


                <asp:DropDownList
                    ID="ddlVehicle"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlVehicle_SelectedIndexChanged">
                </asp:DropDownList>


                <div class="form-text">
                    The vehicle assigned to the selected route.
                </div>


                <asp:RequiredFieldValidator
                    ID="rfvVehicle"
                    ValidationGroup="Load"
                    runat="server"
                    ControlToValidate="ddlVehicle"
                    InitialValue=""
                    ErrorMessage="Select a vehicle."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-3">

                <label class="form-label fw-semibold">

                    Stock Date

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtStockDate"
                    runat="server"
                    CssClass="form-control"
                    placeholder="YYYY-MM-DD">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvStockDate"
                    ValidationGroup="Load"
                    runat="server"
                    ControlToValidate="txtStockDate"
                    ErrorMessage="Stock date is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-4">

                <label class="form-label fw-semibold">
                    Reference
                </label>


                <small class="text-muted d-block">
                    The load is written to the ledger as
                    <strong>LOAD-vehicle-date</strong>.
                </small>

            </div>


        </div>


        <!-- Godown stock banner -->

        <asp:Label
            ID="lblCurrent"
            runat="server"
            CssClass="alert alert-info d-block"
            Visible="false">
        </asp:Label>


        <!-- Line builder -->

        <div class="mb-3">

            <h5 class="fw-bold mb-1">
                Load Lines
            </h5>

            <small class="text-muted">
                Stock is counted in base packets - every line is checked against the godown before the load is applied.
            </small>

        </div>


        <div class="row g-3 align-items-end mb-3">

            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Product Variant

                    <span class="text-danger">*</span>

                </label>


                <asp:DropDownList
                    ID="ddlVariant"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlVariant_SelectedIndexChanged">
                </asp:DropDownList>


                <asp:RequiredFieldValidator
                    ID="rfvVariant"
                    ValidationGroup="LineAdd"
                    runat="server"
                    ControlToValidate="ddlVariant"
                    InitialValue=""
                    ErrorMessage="Select a product variant."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-3">

                <label class="form-label fw-semibold">

                    Packets

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtQuantity"
                    runat="server"
                    CssClass="form-control"
                    TextMode="Number"
                    Text="1">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvQuantity"
                    ValidationGroup="LineAdd"
                    runat="server"
                    ControlToValidate="txtQuantity"
                    ErrorMessage="Packets are required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>


                <asp:RangeValidator
                    ID="rngQuantity"
                    ValidationGroup="LineAdd"
                    runat="server"
                    ControlToValidate="txtQuantity"
                    Type="Integer"
                    MinimumValue="1"
                    MaximumValue="999999999"
                    ErrorMessage="Packets must be a whole number above zero."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RangeValidator>

            </div>


            <div class="col-md-3">

                <asp:Button
                    ID="btnAddLine"
                    ValidationGroup="LineAdd"
                    runat="server"
                    Text="Add Line"
                    CssClass="btn btn-outline-danger w-100"
                    OnClick="btnAddLine_Click" />

            </div>

        </div>


        <!-- Lines grid -->

        <div class="table-responsive">

            <asp:GridView
                ID="gvLines"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                DataKeyNames="Index"
                OnRowCommand="gvLines_RowCommand" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="VariantLabel"
                        HeaderText="Product Variant" />

                    <asp:BoundField
                        DataField="Quantity"
                        HeaderText="Packets"
                        DataFormatString="{0:N0}" />

                    <asp:TemplateField
                        HeaderText="Action">

                        <ItemTemplate>

                            <asp:LinkButton
                                ID="lnkRemove"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-danger"
                                CommandName="Remove"
                                CommandArgument='<%# Eval("Index") %>'>

                                <i class="bi bi-trash"></i>

                                Remove

                            </asp:LinkButton>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-4">

                        <i class="bi bi-truck fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No lines yet - add at least one item above.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>


        <div class="row justify-content-end mt-3">

            <div class="col-md-4">

                <div class="d-flex justify-content-between align-items-center py-2 border-top border-danger">

                    <span class="fw-bold">
                        Total To Load
                    </span>


                    <strong class="text-danger fs-5">

                        <asp:Label
                            ID="lblTotalPackets"
                            runat="server"
                            Text="0">
                        </asp:Label>

                    </strong>

                </div>

            </div>

        </div>


        <hr class="my-4" />


        <div class="d-flex justify-content-end gap-2">

            <a href="Inventory.aspx"
               class="btn btn-light border">

                <i class="bi bi-x-lg"></i>

                Cancel

            </a>


            <asp:Button
                ID="btnLoad"
                ValidationGroup="Load"
                runat="server"
                Text="Load Vehicle"
                CssClass="btn btn-danger px-4"
                OnClick="btnLoad_Click" />

        </div>


    </div>


    <!-- Load history -->

    <div class="dashboard-card mt-4">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    Warehouse To Vehicle Loads
                </h5>

                <small class="text-muted">
                    Latest 20 loads written to the stock ledger
                </small>

            </div>


            <a href="StockTransactions.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-journal-text"></i>

                Stock Ledger

            </a>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvLoadHistory"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle mb-0"
                GridLines="None" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="ReferenceNo"
                        HeaderText="Reference" />

                    <asp:BoundField
                        DataField="LoadDateText"
                        HeaderText="Loaded At" />

                    <asp:BoundField
                        DataField="VariantLabel"
                        HeaderText="Product Variant" />

                    <asp:BoundField
                        DataField="PacketsText"
                        HeaderText="Packets" />

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-4">

                        <i class="bi bi-truck fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No warehouse to vehicle loads recorded yet.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>

</asp:Content>
