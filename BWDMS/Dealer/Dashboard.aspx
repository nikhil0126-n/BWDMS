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


    <!-- Breadcrumb -->

    <nav class="dash-breadcrumb"
         aria-label="breadcrumb">

        <a href="Dashboard.aspx">
            Dealer
        </a>

        <span>/</span>

        <span class="current">
            Dashboard
        </span>

    </nav>


    <!-- ============================================================
         HEADLINE NUMBERS
         ============================================================ -->

    <div class="row g-3 mb-4">

        <div class="col-md-3 col-6">

            <div class="dashboard-card dash-kpi">

                <div class="dash-kpi-text">

                    <div class="card-title">
                        Total Shops
                    </div>

                    <h3 class="fw-bold mb-0">
                        <asp:Label
                            ID="lblShopCount"
                            runat="server"
                            Text="0">
                        </asp:Label>
                    </h3>

                </div>

                <div class="kpi-icon kpi-blue">
                    <i class="bi bi-shop"></i>
                </div>

            </div>

        </div>


        <div class="col-md-3 col-6">

            <div class="dashboard-card dash-kpi">

                <div class="dash-kpi-text">

                    <div class="card-title">
                        Today's Orders
                    </div>

                    <h3 class="fw-bold mb-0">
                        <asp:Label
                            ID="lblTodayOrders"
                            runat="server"
                            Text="0">
                        </asp:Label>
                    </h3>

                </div>

                <div class="kpi-icon kpi-green">
                    <i class="bi bi-clipboard2-check"></i>
                </div>

            </div>

        </div>


        <div class="col-md-3 col-6">

            <div class="dashboard-card dash-kpi">

                <div class="dash-kpi-text">

                    <div class="card-title">
                        Today's Sales
                    </div>

                    <h3 class="fw-bold mb-0">
                        <asp:Label
                            ID="lblTodaySales"
                            runat="server"
                            Text="&#8377;0">
                        </asp:Label>
                    </h3>

                </div>

                <div class="kpi-icon kpi-green">
                    <i class="bi bi-currency-rupee"></i>
                </div>

            </div>

        </div>


        <div class="col-md-3 col-6">

            <div class="dashboard-card dash-kpi">

                <div class="dash-kpi-text">

                    <div class="card-title">
                        Today's Dispatched
                    </div>

                    <h3 class="fw-bold mb-0">
                        <asp:Label
                            ID="lblTodayDispatched"
                            runat="server"
                            Text="&#8377;0">
                        </asp:Label>
                    </h3>

                </div>

                <div class="kpi-icon kpi-teal">
                    <i class="bi bi-truck"></i>
                </div>

            </div>

        </div>

    </div>


    <!-- ============================================================
         SALES OVERVIEW + TOP PRODUCTS
         ============================================================ -->

    <div class="row g-3 mb-4">

        <div class="col-md-8">

            <div class="dashboard-card h-100">

                <div class="d-flex justify-content-between align-items-center mb-3">

                    <h5 class="fw-bold mb-0">
                        Sales Overview
                    </h5>

                    <span class="dash-period">
                        <i class="bi bi-calendar3"></i>

                        This Month
                    </span>

                </div>

                <div class="dash-chart">

                    <asp:Literal
                        ID="litChart"
                        runat="server">
                    </asp:Literal>

                </div>

            </div>

        </div>


        <div class="col-md-4">

            <div class="dashboard-card h-100">

                <h5 class="fw-bold mb-3">
                    Top Products
                </h5>

                <asp:Repeater
                    ID="rptTopProducts"
                    runat="server">

                    <ItemTemplate>

                        <div class="dash-top-item">

                            <span class="name">
                                <%# Eval("ProductName") %>
                            </span>

                            <span class="qty">
                                <%# Eval("QuantityText") %> Packets
                            </span>

                        </div>

                    </ItemTemplate>

                </asp:Repeater>

                <asp:Label
                    ID="lblNoProducts"
                    runat="server"
                    Visible="false"
                    CssClass="text-muted small"
                    Text="No orders yet this month.">
                </asp:Label>

            </div>

        </div>

    </div>


    <!-- ============================================================
         VEHICLE STATUS + RECENT ORDERS
         ============================================================ -->

    <div class="row g-3">

        <div class="col-md-4">

            <div class="dashboard-card h-100">

                <h5 class="fw-bold mb-3">
                    Vehicle Status
                </h5>

                <div class="row g-3">

                    <div class="col-6">

                        <div class="dash-vs">

                            <span>
                                Available
                            </span>

                            <strong>
                                <asp:Label
                                    ID="lblVsAvailable"
                                    runat="server"
                                    Text="0">
                                </asp:Label>
                            </strong>

                        </div>

                    </div>


                    <div class="col-6">

                        <div class="dash-vs">

                            <span>
                                Running
                            </span>

                            <strong>
                                <asp:Label
                                    ID="lblVsRunning"
                                    runat="server"
                                    Text="0">
                                </asp:Label>
                            </strong>

                        </div>

                    </div>


                    <div class="col-6">

                        <div class="dash-vs">

                            <span>
                                Loading
                            </span>

                            <strong>
                                <asp:Label
                                    ID="lblVsLoading"
                                    runat="server"
                                    Text="0">
                                </asp:Label>
                            </strong>

                        </div>

                    </div>


                    <div class="col-6">

                        <div class="dash-vs">

                            <span>
                                Returning
                            </span>

                            <strong>
                                <asp:Label
                                    ID="lblVsReturning"
                                    runat="server"
                                    Text="0">
                                </asp:Label>
                            </strong>

                        </div>

                    </div>

                </div>


                <div class="dash-vs-stock">

                    <span>
                        Warehouse Stock Value
                    </span>

                    <strong>
                        &#8377;<asp:Label
                            ID="lblStockValue"
                            runat="server"
                            Text="0">
                        </asp:Label>
                    </strong>

                </div>

            </div>

        </div>


        <div class="col-md-8">

            <div class="dashboard-card h-100">

                <h5 class="fw-bold mb-3">
                    Recent Orders
                </h5>

                <div class="table-responsive">

                    <asp:GridView
                        ID="gvRecentOrders"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table table-hover align-middle"
                        GridLines="None"
                        AllowPaging="true"
                        OnPageIndexChanging="gvRecentOrders_PageIndexChanging"
                        PageSize="5"
                        PagerSettings-Mode="NumericFirstLast"
                        PagerSettings-PageButtonCount="5"
                        PagerSettings-Position="Bottom">

                        <Columns>

                            <asp:BoundField
                                DataField="OrderNumber"
                                HeaderText="Order ID" />

                            <asp:BoundField
                                DataField="ShopName"
                                HeaderText="Shop Name" />

                            <asp:BoundField
                                DataField="SalesmanName"
                                HeaderText="Salesman" />

                            <asp:BoundField
                                DataField="GrandTotalText"
                                HeaderText="Amount"
                                HtmlEncode="False" />

                            <asp:TemplateField
                                HeaderText="Status">

                                <ItemTemplate>

                                    <span class='<%# Eval("StatusCss") %>'>

                                        <%# Eval("Status") %>

                                    </span>

                                </ItemTemplate>

                            </asp:TemplateField>

                        </Columns>


                        <EmptyDataTemplate>

                            <div class="text-center py-4">

                                <i class="bi bi-cart-x fs-1 text-muted"></i>

                                <p class="text-muted mt-3 mb-0">
                                    No orders yet.
                                </p>

                            </div>

                        </EmptyDataTemplate>

                    </asp:GridView>

                </div>

            </div>

        </div>

    </div>


</asp:Content>
