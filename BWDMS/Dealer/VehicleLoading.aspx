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

    <style>
        .vl-shell { max-width: 980px; margin: 0 auto; }
        .vl-head { display: flex; align-items: center; gap: 12px; margin-bottom: 26px; }
        .vl-head a { color: #111827; text-decoration: none; font-size: 1.6rem; line-height: 1; }
        .vl-head h3 { margin: 0; font-size: 1.55rem; font-weight: 700; color: #111827; }
        .vl-grid { display: grid; grid-template-columns: 300px 1fr; gap: 26px; align-items: start; }
        .vl-card { background: #fff; border: 1px solid #e3e6ec; border-radius: 10px; padding: 26px 24px; box-shadow: 0 1px 3px rgba(16,24,40,.05); }
        .vl-card h5 { margin: 0 0 22px; font-size: 1.1rem; font-weight: 600; color: #1f2937; }
        .vl-field { margin-bottom: 20px; }
        .vl-field:last-child { margin-bottom: 0; }
        .vl-field label { display: block; font-size: .95rem; color: #374151; margin-bottom: 8px; }
        .vl-field .form-select { width: 100%; border: 1px solid #cfd4dc; border-radius: 7px; padding: .45rem 2.1rem .45rem .75rem; font-size: .95rem; color: #374151; background-color: #fff; background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16'%3E%3Cpath fill='none' stroke='%236b7280' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='m2 5 6 6 6-6'/%3E%3C/svg%3E"); background-repeat: no-repeat; background-position: right .65rem center; background-size: 14px 14px; appearance: auto; }
        .vl-linebar { display: flex; gap: 12px; align-items: flex-end; margin-bottom: 18px; }
        .vl-linebar > div { flex: 1; }
        .vl-linebar label { display: block; font-size: .85rem; color: #374151; margin-bottom: 6px; }
        .vl-linebar .form-select, .vl-linebar .form-control { width: 100%; border: 1px solid #cfd4dc; border-radius: 7px; padding: .42rem .6rem; font-size: .9rem; }
        .vl-linebar .btn { flex: 0 0 110px; border-radius: 7px; }
        .vl-table { width: 100%; border-collapse: collapse; }
        .vl-table thead th { background: #d9d9d9; color: #1f2937; font-size: 1.05rem; font-weight: 600; padding: 14px 18px; }
        .vl-table tbody td { padding: 15px 18px; font-size: 1.05rem; color: #1f2937; border-bottom: 1px solid #eceff3; }
        .vl-table th:nth-child(2), .vl-table td:nth-child(2) { text-align: center; }
        .vl-table th:last-child, .vl-table td:last-child { text-align: right; }
        .vl-total { display: flex; justify-content: flex-end; align-items: center; gap: 46px; padding: 10px 18px 0; }
        .vl-total span { font-size: 1.05rem; color: #1f2937; }
        .vl-total strong { font-size: 1.05rem; color: #111827; border-top: 1px solid #9aa1ad; padding-top: 6px; min-width: 86px; text-align: right; display: inline-block; }
        .vl-actions { display: flex; justify-content: flex-end; gap: 14px; margin-top: 22px; }
        .vl-actions .btn { min-width: 150px; border-radius: 8px; padding: .55rem 1.4rem; font-size: .98rem; }
        .btn-vl-primary { background: #4f46e5; border-color: #4f46e5; color: #fff; }
        .btn-vl-primary:hover { background: #4338ca; border-color: #4338ca; color: #fff; }
        .vl-empty { text-align: center; color: #8b94a8; padding: 26px 0; }
    </style>

    <div class="vl-shell">

        <!-- Back + title -->
        <div class="vl-head">

            <a href="Inventory.aspx" title="Back to Inventory">&#8592;</a>

            <h3>
                <asp:Label
                    ID="lblHeading"
                    runat="server"
                    Text="Vehicle Loading">
                </asp:Label>
            </h3>

        </div>


        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <div class="vl-grid">


            <!-- Left: selectors -->
            <div class="vl-card">

                <div class="vl-field">

                    <label>Select Vehicle</label>

                    <asp:DropDownList
                        ID="ddlVehicle"
                        runat="server"
                        CssClass="form-select"
                        AutoPostBack="true"
                        OnSelectedIndexChanged="ddlVehicle_SelectedIndexChanged">
                    </asp:DropDownList>

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


                <div class="vl-field">

                    <label>Select Route</label>

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


                <div class="vl-field">

                    <label>Select Salesman</label>

                    <asp:DropDownList
                        ID="ddlSalesman"
                        runat="server"
                        CssClass="form-select">
                    </asp:DropDownList>

                </div>


                <div class="vl-field">

                    <label>Date</label>

                    <asp:DropDownList
                        ID="ddlDate"
                        runat="server"
                        CssClass="form-select">
                    </asp:DropDownList>

                    <asp:TextBox
                        ID="txtStockDate"
                        runat="server"
                        CssClass="form-control d-none"
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

                    <asp:RegularExpressionValidator
                        ID="revStockDate"
                        ValidationGroup="Load"
                        runat="server"
                        ControlToValidate="txtStockDate"
                        ValidationExpression="^\d{4}-\d{2}-\d{2}$"
                        ErrorMessage="Stock date must be YYYY-MM-DD."
                        CssClass="text-danger small"
                        Display="Dynamic">
                    </asp:RegularExpressionValidator>

                </div>


                <asp:Label
                    ID="lblCurrent"
                    runat="server"
                    CssClass="alert alert-info d-block"
                    Visible="false">
                </asp:Label>

            </div>


            <!-- Right: load products -->
            <div class="vl-card">

                <h5>Load Products</h5>


                <!-- Line builder -->
                <div class="vl-linebar">

                    <div>

                        <label>Product Variant</label>

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


                    <div>

                        <label>Quantity</label>

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


                    <div>

                        <asp:Button
                            ID="btnAddLine"
                            ValidationGroup="LineAdd"
                            runat="server"
                            Text="Add Line"
                            CssClass="btn btn-outline-danger"
                            OnClick="btnAddLine_Click" />

                    </div>

                </div>


                <!-- Lines grid -->
                <div class="table-responsive">

                    <asp:GridView
                        ID="gvLines"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="vl-table"
                        GridLines="None"
                        DataKeyNames="Index"
                        OnRowCommand="gvLines_RowCommand"
                        AllowPaging="true"
                        OnPageIndexChanging="gvLines_PageIndexChanging"
                        PageSize="25"
                        PagerSettings-Mode="NumericFirstLast"
                        PagerSettings-PageButtonCount="10"
                        PagerSettings-Position="TopAndBottom">

                        <Columns>

                            <asp:BoundField
                                DataField="VariantLabel"
                                HeaderText="Product" />

                            <asp:BoundField
                                DataField="UnitLabel"
                                HeaderText="Unit" />

                            <asp:BoundField
                                DataField="Quantity"
                                HeaderText="Quantity"
                                DataFormatString="{0:N0}" />

                            <asp:TemplateField HeaderText="Action">

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

                            <div class="vl-empty">

                                <i class="bi bi-truck fs-1 text-muted"></i>

                                <p class="text-muted mt-3 mb-0">
                                    No lines yet - add at least one item above.
                                </p>

                            </div>

                        </EmptyDataTemplate>

                    </asp:GridView>

                </div>


                <!-- Total -->
                <div class="vl-total">

                    <span>Total</span>

                    <strong>

                        <asp:Label
                            ID="lblTotalPackets"
                            runat="server"
                            Text="0">
                        </asp:Label>

                        Box

                    </strong>

                </div>

            </div>

        </div>


        <!-- Actions -->
        <div class="vl-actions">

            <a href="Inventory.aspx"
               class="btn btn-light border">
                Cancel
            </a>


            <asp:Button
                ID="btnLoad"
                ValidationGroup="Load"
                runat="server"
                Text="Review Loading"
                CssClass="btn btn-vl-primary"
                OnClick="btnLoad_Click" />

        </div>


        <!-- Already loaded for the selected vehicle -->
        <div class="summary-box mt-4">

            <h5>Already Loaded</h5>


            <asp:Label
                ID="lblSummaryHeading"
                runat="server"
                CssClass="text-muted small"
                Text="Select a vehicle to see what has already been loaded.">
            </asp:Label>


            <div class="table-responsive mt-3">

                <asp:GridView
                    ID="gvTodayLoaded"
                    runat="server"
                    AutoGenerateColumns="False"
                    CssClass="table table-hover align-middle mb-0"
                    GridLines="None"
                    AllowPaging="true"
                    OnPageIndexChanging="gvTodayLoaded_PageIndexChanging"
                    PageSize="25"
                    PagerSettings-Mode="NumericFirstLast"
                    PagerSettings-PageButtonCount="10"
                    PagerSettings-Position="TopAndBottom">

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

        </div>


        <!-- Load history -->
        <div class="summary-box mt-4">

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
                    GridLines="None"
                    AllowPaging="true"
                    OnPageIndexChanging="gvLoadHistory_PageIndexChanging"
                    PageSize="25"
                    PagerSettings-Mode="NumericFirstLast"
                    PagerSettings-PageButtonCount="10"
                    PagerSettings-Position="TopAndBottom">

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

    </div>

</asp:Content>
