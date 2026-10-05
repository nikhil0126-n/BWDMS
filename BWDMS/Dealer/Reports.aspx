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


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Reports
            </h3>

            <p class="text-muted mb-0">
                Stock, sales and order reports for your dealership
            </p>

        </div>


        <div class="d-flex gap-2">

            <a href="Inventory.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-box-seam"></i>

                Inventory

            </a>


            <a href="Orders.aspx"
               class="btn btn-danger">

                <i class="bi bi-cart3"></i>

                Orders

            </a>

        </div>

    </div>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <!-- KPI Tiles -->

    <div class="row g-3 mb-4">

        <div class="col-md-3">

            <div class="dashboard-card">

                <div class="card-title">
                    Total Products
                </div>

                <h4 class="fw-bold mb-0">

                    <asp:Label
                        ID="lblKpiProducts"
                        runat="server"
                        Text="0">
                    </asp:Label>

                </h4>

            </div>

        </div>


        <div class="col-md-3">

            <div class="dashboard-card">

                <div class="card-title">
                    Low / Out of Stock
                </div>

                <h4 class="fw-bold mb-0">

                    <asp:Label
                        ID="lblKpiLowStock"
                        runat="server"
                        Text="0">
                    </asp:Label>

                </h4>

            </div>

        </div>


        <div class="col-md-3">

            <div class="dashboard-card">

                <div class="card-title">
                    Orders
                </div>

                <h4 class="fw-bold mb-0">

                    <asp:Label
                        ID="lblKpiOrders"
                        runat="server"
                        Text="0">
                    </asp:Label>

                </h4>

            </div>

        </div>


        <div class="col-md-3">

            <div class="dashboard-card">

                <div class="card-title">
                    Total Sales Value
                </div>

                <h4 class="fw-bold mb-0">

                    <asp:Label
                        ID="lblKpiSales"
                        runat="server"
                        Text="0.00">
                    </asp:Label>

                </h4>

            </div>

        </div>

    </div>


    <!-- Report -->

    <div class="dashboard-card">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">

                    <asp:Label
                        ID="lblReportTitle"
                        runat="server"
                        Text="Stock Summary">
                    </asp:Label>

                </h5>

                <small class="text-muted">
                    Pick a report below, all numbers are scoped to your dealership
                </small>

            </div>


            <div>

                <span class="text-muted">

                    <asp:Label
                        ID="lblRecordCount"
                        runat="server"
                        Text="records: 0">
                    </asp:Label>

                </span>

            </div>

        </div>


        <div class="row g-2 mb-2">

            <div class="col-md-4">

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

            </div>


            <div class="col-md-3">

                <asp:TextBox
                    ID="txtFromDate"
                    runat="server"
                    CssClass="form-control"
                    placeholder="From date (yyyy-MM-dd)"
                    AutoPostBack="true"
                    OnTextChanged="DateFilterChanged">
                </asp:TextBox>

            </div>


            <div class="col-md-3">

                <asp:TextBox
                    ID="txtToDate"
                    runat="server"
                    CssClass="form-control"
                    placeholder="To date (yyyy-MM-dd)"
                    AutoPostBack="true"
                    OnTextChanged="DateFilterChanged">
                </asp:TextBox>

            </div>


            <div class="col-md-2">

                <asp:Button
                    ID="btnExport"
                    runat="server"
                    Text="Export CSV"
                    CssClass="btn btn-outline-secondary w-100"
                    OnClick="btnExport_Click">
                </asp:Button>

            </div>

        </div>


        <p class="text-muted small mb-3">
            From / To dates apply to "Orders by Date", the sales reports, receipts, reconciliation and returns. Leave them blank for no date limit.
        </p>


        <div class="table-responsive">

            <asp:GridView
                ID="gvReport"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                OnRowDataBound="gvReport_RowDataBound" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">
            </asp:GridView>

        </div>


        <asp:Panel
            ID="pnlEmpty"
            runat="server"
            Visible="false">

            <div class="text-center py-5">

                <i class="bi bi-bar-chart fs-1 text-muted"></i>

                <p class="text-muted mt-3 mb-0">
                    No data for the selected filters.
                </p>

            </div>

        </asp:Panel>

    </div>


</asp:Content>
