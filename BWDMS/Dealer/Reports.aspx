<%@ Page Title="Reports"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Reports.aspx.cs"
    Inherits="BWDMS.Dealer.Reports" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Reports

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <style>
        .vl-shell { max-width: 980px; margin: 0 auto; }
        .vl-head { display: flex; align-items: center; gap: 12px; margin-bottom: 26px; }
        .vl-head h3 { margin: 0; font-size: 1.55rem; font-weight: 700; color: #111827; }
        .vl-grid { display: grid; grid-template-columns: 300px 1fr; gap: 26px; align-items: start; }
        .vl-card { background: #fff; border: 1px solid #e3e6ec; border-radius: 10px; padding: 26px 24px; box-shadow: 0 1px 3px rgba(16,24,40,.05); }
        .vl-card h5 { margin: 0 0 22px; font-size: 1.1rem; font-weight: 600; color: #1f2937; }
        .vl-field { margin-bottom: 20px; }
        .vl-field:last-child { margin-bottom: 0; }
        .vl-field label { display: block; font-size: .95rem; color: #374151; margin-bottom: 8px; }
        .vl-field .form-select, .vl-field .form-control { width: 100%; border: 1px solid #cfd4dc; border-radius: 7px; padding: .45rem 2.1rem .45rem .75rem; font-size: .95rem; color: #374151; background-color: #fff; background-image: url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16'%3E%3Cpath fill='none' stroke='%236b7280' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='m2 5 6 6 6-6'/%3E%3C/svg%3E"); background-repeat: no-repeat; background-position: right .65rem center; background-size: 14px 14px; appearance: auto; }
        .vl-table { width: 100%; border-collapse: collapse; }
        .vl-table thead th { background: #fff; border-bottom: 1px solid #e5e7eb; color: #1f2937; font-size: 1.05rem; font-weight: 600; padding: 12px 18px; }
        .vl-table tbody td { padding: 15px 18px; font-size: 1.05rem; color: #1f2937; }
        .vl-table th:nth-child(2), .vl-table td:nth-child(2) { text-align: center; }
        .vl-table th:last-child, .vl-table td:last-child { text-align: right; }
        .vl-total { display: flex; justify-content: flex-end; align-items: center; gap: 46px; padding: 10px 18px 0; }
        .vl-total span { font-size: 1.05rem; color: #1f2937; }
        .vl-total strong { font-size: 1.05rem; color: #111827; border-top: 1px solid #9aa1ad; padding-top: 6px; min-width: 86px; text-align: right; display: inline-block; }
        .vl-actions { display: flex; justify-content: flex-end; gap: 14px; margin-top: 22px; }
        .vl-actions .btn { min-width: 150px; border-radius: 8px; padding: .55rem 1.4rem; font-size: .98rem; }
        .btn-vl-primary { background: #4f46e5; border-color: #4f46e5; color: #fff; }
        .btn-vl-primary:hover { background: #4338ca; border-color: #4338ca; color: #fff; }
        .vl-filters { display: flex; gap: 10px; flex-wrap: wrap; align-items: center; margin-bottom: 14px; }
        .vl-filters .form-select, .vl-filters .form-control { border: 1px solid #cfd4dc; border-radius: 7px; padding: .4rem .6rem; font-size: .88rem; }
        .vl-empty { text-align: center; color: #8b94a8; padding: 26px 0; }
    </style>

    <div class="vl-shell">

        <!-- Title -->
        <div class="vl-head">

            <h3>reports</h3>

        </div>


        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <div class="vl-grid">


            <!-- Left: filters -->
            <div class="vl-card">

                <div class="d-flex align-items-center gap-3">

                    <span class="stat-icon bg-primary-subtle text-primary-emphasis">
                        <i class="bi bi-box-seam"></i>
                    </span>

                    <div>

                <div class="vl-field">

                    <label>Vehicle</label>

                    <asp:DropDownList
                        ID="ddlVehicle"
                        runat="server"
                        CssClass="form-select">
                    </asp:DropDownList>

                </div>


                <div class="vl-field">

                    <label>Route</label>

                    <asp:DropDownList
                        ID="ddlRoute"
                        runat="server"
                        CssClass="form-select">
                    </asp:DropDownList>

                </div>


                <div class="vl-field">

                    <label>Salesman</label>

                    <asp:DropDownList
                        ID="ddlSalesman"
                        runat="server"
                        CssClass="form-select">
                    </asp:DropDownList>

                </div>


        <div class="col-md-3">

            <div class="dashboard-card">

                <div class="d-flex align-items-center gap-3">

                    <span class="stat-icon bg-info-subtle text-info-emphasis">
                        <i class="bi bi-currency-rupee"></i>
                    </span>

                    <div>

                        <div class="card-title">
                            Total Sales Value
                        </div>

                        <h4 class="fw-bold mb-0">
                <div class="vl-field">

                    <label>Date</label>

                    <asp:DropDownList
                        ID="ddlDate"
                        runat="server"
                        CssClass="form-select">
                    </asp:DropDownList>

                </div>

            </div>


            <!-- Right: report table -->
            <div class="vl-card">

                <h5>

                    <asp:Label
                        ID="lblReportTitle"
                        runat="server"
                        Text="Unload Products">
                    </asp:Label>

                </h5>


                <!-- Existing report filters kept functional -->
                <div class="vl-filters">

                    <asp:DropDownList
                        ID="ddlReport"
                        runat="server"
                        CssClass="form-select"
                        AutoPostBack="true"
                        OnSelectedIndexChanged="ddlReport_SelectedIndexChanged">

                        <asp:ListItem Text="Stock Summary" Value="stock" Selected="True">
                        </asp:ListItem>

                        <asp:ListItem Text="Low Stock" Value="low">
                        </asp:ListItem>

                        <asp:ListItem Text="Orders by Status" Value="orderstatus">
                        </asp:ListItem>

                        <asp:ListItem Text="Sales by Shop" Value="shop">
                        </asp:ListItem>

                        <asp:ListItem Text="Stock Movement" Value="movement">
                        </asp:ListItem>

                        <asp:ListItem Text="Orders by Date" Value="orderdate">
                        </asp:ListItem>

                        <asp:ListItem Text="Sales by Product" Value="salesproduct">
                        </asp:ListItem>

                        <asp:ListItem Text="Sales by Salesman" Value="salesbysalesman">
                        </asp:ListItem>

                        <asp:ListItem Text="Sales by Route / Village" Value="salesbyroute">
                        </asp:ListItem>

                        <asp:ListItem Text="Orders by Source" Value="ordersource">
                        </asp:ListItem>

                        <asp:ListItem Text="Company Receipts" Value="receipts">
                        </asp:ListItem>

                        <asp:ListItem Text="Vehicle Reconciliation" Value="recon">
                        </asp:ListItem>

                        <asp:ListItem Text="Returns &amp; Damaged" Value="returns">
                        </asp:ListItem>

                    </asp:DropDownList>

                    <asp:TextBox
                        ID="txtFromDate"
                        runat="server"
                        CssClass="form-control"
                        placeholder="From (yyyy-MM-dd)"
                        AutoPostBack="true"
                        OnTextChanged="DateFilterChanged">
                    </asp:TextBox>

                    <asp:RegularExpressionValidator
                        ID="revFromDate"
                        runat="server"
                        ControlToValidate="txtFromDate"
                        ValidationExpression="^\d{4}-\d{2}-\d{2}$"
                        ErrorMessage="Use yyyy-MM-dd."
                        CssClass="text-danger small"
                        Display="Dynamic">
                    </asp:RegularExpressionValidator>

                    <asp:TextBox
                        ID="txtToDate"
                        runat="server"
                        CssClass="form-control"
                        placeholder="To (yyyy-MM-dd)"
                        AutoPostBack="true"
                        OnTextChanged="DateFilterChanged">
                    </asp:TextBox>

                    <asp:RegularExpressionValidator
                        ID="revToDate"
                        runat="server"
                        ControlToValidate="txtToDate"
                        ValidationExpression="^\d{4}-\d{2}-\d{2}$"
                        ErrorMessage="Use yyyy-MM-dd."
                        CssClass="text-danger small"
                        Display="Dynamic">
                    </asp:RegularExpressionValidator>

                    <asp:Button
                        ID="btnExport"
                        runat="server"
                        Text="Export CSV"
                        CssClass="btn btn-outline-secondary"
                        OnClick="btnExport_Click">
                    </asp:Button>

                </div>


                <div class="table-responsive">

                    <asp:GridView
                        ID="gvReport"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="vl-table"
                        GridLines="None"
                        OnRowDataBound="gvReport_RowDataBound"
                        AllowPaging="true"
                        OnPageIndexChanging="gvReport_PageIndexChanging"
                        PageSize="25"
                        PagerSettings-Mode="NumericFirstLast"
                        PagerSettings-PageButtonCount="10"
                        PagerSettings-Position="TopAndBottom">
                    </asp:GridView>

                </div>


                <!-- KPI labels kept so the code-behind keeps working -->
                <div class="d-none">

                    <asp:Label ID="lblKpiProducts" runat="server" Text="0"></asp:Label>
                    <asp:Label ID="lblKpiLowStock" runat="server" Text="0"></asp:Label>
                    <asp:Label ID="lblKpiOrders" runat="server" Text="0"></asp:Label>
                    <asp:Label ID="lblKpiSales" runat="server" Text="0.00"></asp:Label>
                    <asp:Label ID="lblRecordCount" runat="server" Text="records: 0"></asp:Label>

                </div>


                <asp:Panel
                    ID="pnlEmpty"
                    runat="server"
                    Visible="false">

                    <div class="vl-empty">

                        <i class="bi bi-bar-chart fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No data for the selected filters.
                        </p>

                    </div>

                </asp:Panel>


                <!-- Total row -->
                <div class="vl-total">

                    <span>Total</span>

                    <strong>

                        <asp:Label
                            ID="lblTotalRecords"
                            runat="server"
                            Text="0">
                        </asp:Label>

                    </strong>

                </div>

            </div>

        </div>


        <!-- Actions -->
        <div class="vl-actions">

            <asp:Button
                ID="btnOk"
                runat="server"
                Text="OK"
                CssClass="btn btn-vl-primary"
                OnClick="btnOk_Click" />

        </div>

    </div>

</asp:Content>
