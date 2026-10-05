<%@ Page Title="Admin Dashboard"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Dashboard.aspx.cs"
    Inherits="BWDMS.Admin.Dashboard" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Admin Dashboard

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Heading -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Admin Dashboard
            </h3>

            <p class="text-muted mb-0">
                Overview of your BWDMS system
            </p>

        </div>

        <div>

            <a href="AddProduct.aspx"
               class="btn btn-danger">

                <i class="bi bi-plus-lg"></i>

                Add Product

            </a>

        </div>

    </div>


    <!-- Statistics -->

    <div class="row g-4">


        <!-- Dealers -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="card-title">
                    Total Dealers
                </div>

                <h3>
                    <asp:Label
                        ID="lblDealerCount"
                        runat="server"
                        Text="0">
                    </asp:Label>
                </h3>

                <small class="text-muted">
                    Active registered dealers
                </small>

            </div>

        </div>


        <!-- Categories -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="card-title">
                    Categories
                </div>

                <h3>
                    <asp:Label
                        ID="lblCategoryCount"
                        runat="server"
                        Text="0">
                    </asp:Label>
                </h3>

                <small class="text-muted">
                    Product categories
                </small>

            </div>

        </div>


        <!-- Products -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="card-title">
                    Total Products
                </div>

                <h3>
                    <asp:Label
                        ID="lblProductCount"
                        runat="server"
                        Text="0">
                    </asp:Label>
                </h3>

                <small class="text-muted">
                    Active products
                </small>

            </div>

        </div>


        <!-- Orders -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="card-title">
                    Total Orders
                </div>

                <h3>
                    <asp:Label
                        ID="lblOrderCount"
                        runat="server"
                        Text="0">
                    </asp:Label>
                </h3>

                <small class="text-muted">
                    Orders received
                </small>

            </div>

        </div>

    </div>


    <!-- Recent Activity -->

    <div class="row mt-4">

        <div class="col-lg-8">

            <div class="dashboard-card">

                <h5 class="fw-bold">
                    Recent Dealer Activity
                </h5>

                <p class="text-muted">
                    Latest orders created by your dealers
                </p>

                <div class="table-responsive">

                    <asp:GridView
                        ID="gvRecentActivity"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table table-hover align-middle"
                        GridLines="None"
                        EmptyDataText="No orders available yet." AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                        <Columns>

                            <asp:BoundField
                                DataField="CreatedAtText"
                                HeaderText="Created" />

                            <asp:BoundField
                                DataField="DealerName"
                                HeaderText="Dealer" />

                            <asp:BoundField
                                DataField="OrderNumber"
                                HeaderText="Order No." />

                            <asp:BoundField
                                DataField="ShopName"
                                HeaderText="Shop" />

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


        <div class="col-lg-4">

            <div class="dashboard-card">

                <h5 class="fw-bold">
                    Quick Actions
                </h5>

                <div class="d-grid gap-2 mt-3">

                    <a href="AddCategory.aspx"
                       class="btn btn-outline-danger">
                        Add Category
                    </a>

                    <a href="AddProduct.aspx"
                       class="btn btn-outline-danger">
                        Add Product
                    </a>

                    <a href="AddDealer.aspx"
                       class="btn btn-outline-danger">
                        Add Dealer
                    </a>

                </div>

            </div>

        </div>

    </div>


    <!-- Orders By Status (all dealers) -->

    <div class="row mt-4">

        <div class="col-lg-12">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between align-items-center mb-3">

                    <div>

                        <h5 class="fw-bold mb-1">
                            Orders By Status
                        </h5>

                        <small class="text-muted">
                            High-level sales summary across all dealers
                        </small>

                    </div>


                    <asp:Label
                        ID="lblStatusTotal"
                        runat="server"
                        CssClass="badge bg-secondary">
                    </asp:Label>

                </div>


                <div class="table-responsive">

                    <asp:GridView
                        ID="gvStatusSummary"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table table-hover align-middle"
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

                            <asp:BoundField
                                DataField="AmountText"
                                HeaderText="Grand Total"
                                DataFormatString="{0:N2}" />

                        </Columns>

                    </asp:GridView>

                </div>

            </div>

        </div>

    </div>


</asp:Content>