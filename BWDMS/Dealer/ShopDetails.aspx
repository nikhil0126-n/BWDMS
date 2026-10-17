<%@ Page Title="Shop Details"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="ShopDetails.aspx.cs"
    Inherits="BWDMS.Dealer.ShopDetails" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Shop Details

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Shop Details
            </h3>

            <p class="text-muted mb-0">
                Manage All Shops
            </p>

        </div>


        <a href="Shops.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Shops

        </a>

    </div>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <div class="row">


        <!-- Shop Information -->

        <div class="col-md-6">

            <div class="dashboard-card mb-4">

                <div class="d-flex justify-content-between align-items-center mb-3">

                    <div>

                        <small class="text-muted d-block">
                            Shop
                        </small>


                        <strong>

                            <asp:Label
                                ID="lblShopName"
                                runat="server"
                                Text="-">
                            </asp:Label>

                        </strong>

                    </div>


                    <asp:Label
                        ID="lblStatus"
                        runat="server"
                        CssClass="badge bg-success"
                        Text="Active">
                    </asp:Label>

                </div>


                <div class="row g-4">

                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Shop Code
                        </small>


                        <asp:Label
                            ID="lblShopCode"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Owner
                        </small>


                        <asp:Label
                            ID="lblOwnerName"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Phone
                        </small>


                        <asp:Label
                            ID="lblPhone"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Email
                        </small>


                        <asp:Label
                            ID="lblEmail"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Village
                        </small>


                        <asp:Label
                            ID="lblVillage"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Route
                        </small>


                        <asp:Label
                            ID="lblRoute"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Address
                        </small>


                        <asp:Label
                            ID="lblAddress"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Added On
                        </small>


                        <asp:Label
                            ID="lblCreatedAt"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Opening Balance
                        </small>


                        <asp:Label
                            ID="lblOpeningBalance"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Credit Limit
                        </small>


                        <asp:Label
                            ID="lblCreditLimit"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>

                </div>

            </div>

        </div>


        <!-- Summary + Recent Orders -->

        <div class="col-md-6">

            <div class="dashboard-card mb-4">

                <h5 class="fw-bold mb-1">
                    Shop Summary
                </h5>

                <small class="text-muted d-block mb-3">
                    Lifetime figures for this shop
                </small>


                <div class="row g-4">

                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Total Orders
                        </small>


                        <strong>

                            <asp:Label
                                ID="lblTotalOrders"
                                runat="server"
                                Text="0">
                            </asp:Label>

                        </strong>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Total Sales
                        </small>


                        <strong>

                            <asp:Label
                                ID="lblTotalSales"
                                runat="server"
                                Text="0">
                            </asp:Label>

                        </strong>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Sales This Month
                        </small>


                        <strong>

                            <asp:Label
                                ID="lblMonthSales"
                                runat="server"
                                Text="0">
                            </asp:Label>

                        </strong>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Return Orders
                        </small>


                        <strong>

                            <asp:Label
                                ID="lblReturnOrders"
                                runat="server"
                                Text="0">
                            </asp:Label>

                        </strong>

                    </div>

                </div>

            </div>


            <div class="dashboard-card">

                <h5 class="fw-bold mb-1">
                    Recent Orders
                </h5>

                <small class="text-muted d-block mb-3">
                    Orders taken for this shop, newest first
                </small>


                <div class="table-responsive">

                    <asp:GridView
                        ID="gvOrders"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table table-hover align-middle mb-0"
                        GridLines="None"
                        AllowPaging="true" OnPageIndexChanging="gvOrders_PageIndexChanging" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                        <Columns>

                            <asp:BoundField
                                DataField="OrderNumber"
                                HeaderText="Order No." />

                            <asp:BoundField
                                DataField="OrderDateText"
                                HeaderText="Order Date" />

                            <asp:BoundField
                                DataField="GrandTotal"
                                HeaderText="Grand Total"
                                DataFormatString="{0:N2}"
                                ItemStyle-CssClass="text-end"
                                HeaderStyle-CssClass="text-end" />

                            <asp:BoundField
                                DataField="Status"
                                HeaderText="Status" />

                        </Columns>


                        <EmptyDataTemplate>

                            <div class="text-center py-4">

                                <p class="text-muted mb-0">
                                    No orders found for this shop.
                                </p>

                            </div>

                        </EmptyDataTemplate>

                    </asp:GridView>

                </div>

            </div>

        </div>

    </div>


</asp:Content>
