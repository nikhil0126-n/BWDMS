<%@ Page Title="Dealer Dashboard"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Dashboard.aspx.cs"
    Inherits="BWDMS.Dealer.Dashboard" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Dealer Dashboard

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- =========================================
         PAGE HEADER
         ========================================= -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Dealer Dashboard
            </h3>

            <p class="text-muted mb-0">
                Manage your sales, shops, stock and daily operations
            </p>

        </div>


        <div>

            <a href="AddOrder.aspx"
               class="btn btn-danger">

                <i class="bi bi-plus-lg"></i>

                Create Order

            </a>

        </div>

    </div>


    <!-- =========================================
         STATISTICS
         ========================================= -->

    <div class="row g-4">


        <!-- Total Shops -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between">

                    <div>

                        <div class="card-title">
                            Total Shops
                        </div>

                        <h3>
                            <asp:Label
                                ID="lblShopCount"
                                runat="server"
                                Text="0">
                            </asp:Label>
                        </h3>

                        <small class="text-muted">
                            Active shops
                        </small>

                    </div>


                    <div class="dashboard-icon">

                        <i class="bi bi-shop"></i>

                    </div>

                </div>

            </div>

        </div>


        <!-- Salesmen -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between">

                    <div>

                        <div class="card-title">
                            Salesmen
                        </div>

                        <h3>
                            <asp:Label
                                ID="lblSalesmanCount"
                                runat="server"
                                Text="0">
                            </asp:Label>
                        </h3>

                        <small class="text-muted">
                            Active salesmen
                        </small>

                    </div>


                    <div class="dashboard-icon">

                        <i class="bi bi-people"></i>

                    </div>

                </div>

            </div>

        </div>


        <!-- Vehicles -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between">

                    <div>

                        <div class="card-title">
                            Vehicles
                        </div>

                        <h3>
                            <asp:Label
                                ID="lblVehicleCount"
                                runat="server"
                                Text="0">
                            </asp:Label>
                        </h3>

                        <small class="text-muted">
                            Active vehicles
                        </small>

                    </div>


                    <div class="dashboard-icon">

                        <i class="bi bi-truck"></i>

                    </div>

                </div>

            </div>

        </div>


        <!-- Today's Orders -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between">

                    <div>

                        <div class="card-title">
                            Today's Orders
                        </div>

                        <h3>
                            <asp:Label
                                ID="lblTodayOrders"
                                runat="server"
                                Text="0">
                            </asp:Label>
                        </h3>

                        <small class="text-muted">
                            Orders received today
                        </small>

                    </div>


                    <div class="dashboard-icon">

                        <i class="bi bi-cart-check"></i>

                    </div>

                </div>

            </div>

        </div>

    </div>


    <!-- =========================================
         SECOND ROW
         ========================================= -->

    <div class="row g-4 mt-1">


        <!-- Sales Summary -->

        <div class="col-xl-8">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between align-items-center mb-4">

                    <div>

                        <h5 class="fw-bold mb-1">
                            Sales Overview
                        </h5>

                        <small class="text-muted">
                            <asp:Label
                                ID="lblSalesPeriod"
                                runat="server"
                                Text="Sales performance">
                            </asp:Label>
                        </small>

                    </div>


                    <asp:DropDownList
                        ID="ddlPeriod"
                        runat="server"
                        CssClass="form-select form-select-sm w-auto"
                        AutoPostBack="true">

                        <asp:ListItem Text="This Week" Value="Week">
                        </asp:ListItem>

                        <asp:ListItem Text="This Month" Value="Month" Selected="True">
                        </asp:ListItem>

                        <asp:ListItem Text="This Year" Value="Year">
                        </asp:ListItem>

                    </asp:DropDownList>

                </div>


                <div class="table-responsive">

                    <asp:GridView
                        ID="gvSalesSummary"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table align-middle mb-0"
                        GridLines="None"
                        EmptyDataText="No orders in this period." AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                        <Columns>

                            <asp:TemplateField
                                HeaderText="Status">

                                <ItemTemplate>

                                    <span class='<%# Eval("StatusCss") %>'>

                                        <%# Eval("Status") %>

                                    </span>

                                </ItemTemplate>

                            </asp:TemplateField>


                            <asp:BoundField
                                DataField="OrderCount"
                                HeaderText="Orders" />

                            <asp:BoundField
                                DataField="AmountText"
                                HeaderText="Grand Total"
                                DataFormatString="{0:N2}" />

                        </Columns>

                    </asp:GridView>

                </div>


                <div class="d-flex justify-content-between align-items-center mt-3">

                    <small class="text-muted">
                        <asp:Label
                            ID="lblSalesPeriodOrders"
                            runat="server"
                            Text="0 orders">
                        </asp:Label>
                    </small>

                    <h5 class="fw-bold mb-0">
                        <asp:Label
                            ID="lblSalesPeriodTotal"
                            runat="server"
                            Text="0.00">
                        </asp:Label>
                    </h5>

                </div>

            </div>

        </div>


        <!-- Warehouse Stock Summary -->

        <div class="col-xl-4">

            <div class="dashboard-card">

                <h5 class="fw-bold mb-1">
                    Stock Summary
                </h5>

                <small class="text-muted">
                    Current inventory status
                </small>


                <div class="row g-2 text-center mt-2 mb-2">

                    <div class="col">

                        <h6 class="fw-bold mb-0">
                            <asp:Label
                                ID="lblStockVariants"
                                runat="server"
                                Text="0">
                            </asp:Label>
                        </h6>

                        <small class="text-muted">
                            Variants
                        </small>

                    </div>


                    <div class="col">

                        <h6 class="fw-bold mb-0">
                            <asp:Label
                                ID="lblStockPackets"
                                runat="server"
                                Text="0">
                            </asp:Label>
                        </h6>

                        <small class="text-muted">
                            Packets
                        </small>

                    </div>


                    <div class="col">

                        <h6 class="fw-bold mb-0">
                            <asp:Label
                                ID="lblStockValue"
                                runat="server"
                                Text="0.00">
                            </asp:Label>
                        </h6>

                        <small class="text-muted">
                            Stock value
                        </small>

                    </div>

                </div>


                <asp:Repeater
                    ID="repStockItems"
                    runat="server">

                    <ItemTemplate>

                        <div class="stock-item">

                            <div>

                                <strong>
                                    <%# Eval("ItemName") %>
                                </strong>

                                <small>
                                    <%# Eval("PacketsText") %>
                                </small>

                            </div>

                            <span class='<%# Eval("StockStateCss") %>'>

                                <%# Eval("StockState") %>

                            </span>

                        </div>

                    </ItemTemplate>

                </asp:Repeater>

                <!--
                  Repeater has no EmptyDataTemplate (unlike GridView and
                  ListView), so the empty state is a sibling label whose
                  visibility is toggled when the repeater is bound.
                -->
                <asp:Label
                    ID="lblNoStockItems"
                    runat="server"
                    Visible="false"
                    CssClass="text-muted">
                    No stock recorded yet.
                </asp:Label>

            </div>

        </div>

    </div>


    <!-- =========================================
         THIRD ROW
         ========================================= -->

    <div class="row g-4 mt-1">


        <!-- Today's Schedules -->

        <div class="col-xl-4">

            <div class="dashboard-card">

                <h5 class="fw-bold mb-1">
                    Today's Schedules
                </h5>

                <small class="text-muted">
                    Routes running on
                    <asp:Label
                        ID="lblTodayName"
                        runat="server"
                        Text="">
                    </asp:Label>
                </small>


                <div class="table-responsive mt-3">

                    <asp:GridView
                        ID="gvSchedules"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table align-middle mb-0"
                        GridLines="None"
                        EmptyDataText="No route schedules for today." AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                        <Columns>

                            <asp:BoundField
                                DataField="RouteName"
                                HeaderText="Route" />

                            <asp:BoundField
                                DataField="DayOfWeek"
                                HeaderText="Day" />

                            <asp:BoundField
                                DataField="VehicleNumber"
                                HeaderText="Vehicle" />

                        </Columns>

                    </asp:GridView>

                </div>

            </div>

        </div>


        <!-- Orders By Status -->

        <div class="col-xl-4">

            <div class="dashboard-card">

                <h5 class="fw-bold mb-1">
                    Orders By Status
                </h5>

                <small class="text-muted">
                    All orders taken by your dealership
                </small>


                <div class="table-responsive mt-3">

                    <asp:GridView
                        ID="gvOrdersByStatus"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table align-middle mb-0"
                        GridLines="None" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                        <Columns>

                            <asp:TemplateField
                                HeaderText="Status">

                                <ItemTemplate>

                                    <span class='<%# Eval("StatusCss") %>'>

                                        <%# Eval("Status") %>

                                    </span>

                                </ItemTemplate>

                            </asp:TemplateField>


                            <asp:BoundField
                                DataField="OrderCount"
                                HeaderText="Orders" />

                        </Columns>

                    </asp:GridView>

                </div>

            </div>

        </div>


        <!-- Vehicle Loading And Reconciliation -->

        <div class="col-xl-4">

            <div class="dashboard-card">

                <h5 class="fw-bold mb-1">
                    Vehicle Loading
                </h5>

                <small class="text-muted">
                    Loading and reconciliation status for today
                </small>


                <div class="stock-item mt-3">

                    <div>

                        <strong>
                            Loaded today
                        </strong>

                        <small>
                            Packets loaded onto vehicles
                        </small>

                    </div>

                    <span class="badge bg-primary">
                        <asp:Label
                            ID="lblLoadedToday"
                            runat="server"
                            Text="0">
                        </asp:Label>
                    </span>

                </div>


                <div class="stock-item">

                    <div>

                        <strong>
                            Reconciliations complete
                        </strong>

                        <small>
                            Approved today
                        </small>

                    </div>

                    <span class="badge bg-success">
                        <asp:Label
                            ID="lblReconComplete"
                            runat="server"
                            Text="0">
                        </asp:Label>
                    </span>

                </div>


                <div class="stock-item">

                    <div>

                        <strong>
                            Reconciliations pending
                        </strong>

                        <small>
                            Awaiting approval today
                        </small>

                    </div>

                    <span class="badge bg-warning text-dark">
                        <asp:Label
                            ID="lblReconPending"
                            runat="server"
                            Text="0">
                        </asp:Label>
                    </span>

                </div>

            </div>

        </div>

    </div>


    <!-- =========================================
         FOURTH ROW
         ========================================= -->

    <div class="row g-4 mt-1">


        <!-- Recent Orders -->

        <div class="col-xl-8">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between align-items-center mb-3">

                    <div>

                        <h5 class="fw-bold mb-1">
                            Recent Orders
                        </h5>

                        <small class="text-muted">
                            Latest shop orders
                        </small>

                    </div>


                    <a href="Orders.aspx"
                       class="text-danger text-decoration-none">

                        View All

                    </a>

                </div>


                <div class="table-responsive">

                    <asp:GridView
                        ID="gvRecentOrders"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table align-middle"
                        GridLines="None"
                        EmptyDataText="No orders available" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                        <Columns>

                            <asp:BoundField
                                DataField="OrderNumber"
                                HeaderText="Order" />

                            <asp:BoundField
                                DataField="ShopName"
                                HeaderText="Shop" />

                            <asp:BoundField
                                DataField="SalesmanName"
                                HeaderText="Salesman" />

                            <asp:BoundField
                                DataField="GrandTotalText"
                                HeaderText="Amount"
                                DataFormatString="{0:N2}" />

                            <asp:TemplateField
                                HeaderText="Status">

                                <ItemTemplate>

                                    <span class='<%# Eval("StatusCss") %>'>

                                        <%# Eval("Status") %>

                                    </span>

                                </ItemTemplate>

                            </asp:TemplateField>

                        </Columns>

                    </asp:GridView>

                </div>

            </div>

        </div>


        <!-- Quick Actions -->

        <div class="col-xl-4">

            <div class="dashboard-card">

                <h5 class="fw-bold mb-1">
                    Quick Actions
                </h5>

                <small class="text-muted">
                    Common dealer operations
                </small>


                <div class="quick-actions mt-3">


                    <a href="AddOrder.aspx"
                       class="quick-action text-decoration-none">

                        <i class="bi bi-cart-plus"></i>

                        <span>
                            Create Order
                        </span>

                    </a>


                    <a href="AddShop.aspx"
                       class="quick-action text-decoration-none">

                        <i class="bi bi-shop"></i>

                        <span>
                            Add Shop
                        </span>

                    </a>


                    <a href="AddSalesman.aspx"
                       class="quick-action text-decoration-none">

                        <i class="bi bi-person-plus"></i>

                        <span>
                            Add Salesman
                        </span>

                    </a>


                    <a href="Vehicles.aspx"
                       class="quick-action text-decoration-none">

                        <i class="bi bi-truck"></i>

                        <span>
                            Manage Vehicle
                        </span>

                    </a>


                    <a href="Inventory.aspx"
                       class="quick-action text-decoration-none">

                        <i class="bi bi-box-seam"></i>

                        <span>
                            Check Inventory
                        </span>

                    </a>


                    <a href="Reconciliations.aspx"
                       class="quick-action text-decoration-none">

                        <i class="bi bi-arrow-repeat"></i>

                        <span>
                            Stock Reconciliation
                        </span>

                    </a>


                    <!--
                      PART 12 / PART 13 modules. The shared master page is
                      frozen, so the newer modules are reachable from the
                      dashboard quick-action bar instead of the sidebar.
                    -->
                    <a href="CompanyReceipts.aspx"
                       class="quick-action text-decoration-none">

                        <i class="bi bi-box-arrow-in-down"></i>

                        <span>
                            Company Receipts
                        </span>

                    </a>


                    <a href="VehicleLoading.aspx"
                       class="quick-action text-decoration-none">

                        <i class="bi bi-truck"></i>

                        <span>
                            Vehicle Loading
                        </span>

                    </a>


                    <a href="Dispatches.aspx"
                       class="quick-action text-decoration-none">

                        <i class="bi bi-send"></i>

                        <span>
                            Dispatches
                        </span>

                    </a>


                    <a href="Returns.aspx"
                       class="quick-action text-decoration-none">

                        <i class="bi bi-arrow-return-left"></i>

                        <span>
                            Returns
                        </span>

                    </a>


                </div>

            </div>

        </div>

    </div>


    <!-- =========================================
         FIFTH ROW
         ========================================= -->

    <div class="row g-4 mt-1">


        <!-- Recent Operational Activity -->

        <div class="col-xl-8">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between align-items-center mb-3">

                    <div>

                        <h5 class="fw-bold mb-1">
                            Recent Activity
                        </h5>

                        <small class="text-muted">
                            Latest orders, stock movements and company receipts
                        </small>

                    </div>

                </div>


                <div class="table-responsive">

                    <asp:GridView
                        ID="gvActivity"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table align-middle"
                        GridLines="None"
                        EmptyDataText="No activity recorded yet." AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                        <Columns>

                            <asp:BoundField
                                DataField="ActivityAtText"
                                HeaderText="When" />

                            <asp:BoundField
                                DataField="ActivityType"
                                HeaderText="Type" />

                            <asp:BoundField
                                DataField="Reference"
                                HeaderText="Reference" />

                            <asp:BoundField
                                DataField="Details"
                                HeaderText="Details" />

                        </Columns>

                    </asp:GridView>

                </div>

            </div>

        </div>


        <!-- Low Stock Alerts -->

        <div class="col-xl-4">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between align-items-center mb-3">

                    <div>

                        <h5 class="fw-bold mb-1">
                            Low Stock Alerts
                        </h5>

                        <small class="text-muted">
                            At or below reorder level
                        </small>

                    </div>


                    <asp:Label
                        ID="lblLowStockCount"
                        runat="server"
                        CssClass="badge bg-warning text-dark"
                        Text="0">
                    </asp:Label>

                </div>


                <div class="table-responsive">

                    <asp:GridView
                        ID="gvLowStock"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table align-middle mb-0"
                        GridLines="None"
                        EmptyDataText="No low-stock alerts. All items are above their reorder level." AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                        <Columns>

                            <asp:BoundField
                                DataField="ItemName"
                                HeaderText="Item" />

                            <asp:BoundField
                                DataField="QuantityPackets"
                                HeaderText="Packets" />

                            <asp:BoundField
                                DataField="ReorderLevel"
                                HeaderText="Reorder" />

                        </Columns>

                    </asp:GridView>

                </div>

            </div>

        </div>

    </div>


</asp:Content>
