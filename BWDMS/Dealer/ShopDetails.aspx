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

            <h3 class="fw-bold mb-1 page-title">
                Shop Details
            </h3>

            <p class="text-muted mb-0 page-subtitle">
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


    <div class="row g-4 mb-4">


        <!-- Shop Information -->

        <div class="col-lg-4">

            <div class="profile-card text-center">

                <div class="profile-avatar">
                    <i class="bi bi-shop"></i>
                </div>

                <p class="profile-name">

                    <asp:Label
                        ID="lblShopName"
                        runat="server"
                        Text="-">
                    </asp:Label>

                </p>

                <p class="profile-state mb-0">

                    <asp:Label
                        ID="lblStatus"
                        runat="server"
                        CssClass="badge bg-success"
                        Text="Active">
                    </asp:Label>

                </p>

                <div class="profile-list text-start">

                    <div>
                        <span>Shop Code</span>
                        <span>
                            <asp:Label
                                ID="lblShopCode"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Owner</span>
                        <span>
                            <asp:Label
                                ID="lblOwnerName"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Phone</span>
                        <span>
                            <asp:Label
                                ID="lblPhone"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Email</span>
                        <span>
                            <asp:Label
                                ID="lblEmail"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Village</span>
                        <span>
                            <asp:Label
                                ID="lblVillage"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Route</span>
                        <span>
                            <asp:Label
                                ID="lblRoute"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Address</span>
                        <span>
                            <asp:Label
                                ID="lblAddress"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Added On</span>
                        <span>
                            <asp:Label
                                ID="lblCreatedAt"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Opening Balance</span>
                        <span>
                            <asp:Label
                                ID="lblOpeningBalance"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Credit Limit</span>
                        <span>
                            <asp:Label
                                ID="lblCreditLimit"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                </div>

            </div>

        </div>


        <!-- Summary + Recent Orders -->

        <div class="col-lg-8">

            <div class="summary-box mb-4">

                <h5>
                    Shop Summary
                </h5>

                <small class="text-muted d-block mb-3">
                    Lifetime figures for this shop
                </small>


                <div class="stat-grid">

                    <div class="stat-tile">

                        <i class="bi bi-cart-check"></i>

                        <div>

                            <span class="stat-label">Total Orders</span>

                            <span class="stat-value">

                                <asp:Label
                                    ID="lblTotalOrders"
                                    runat="server"
                                    Text="0">
                                </asp:Label>

                            </span>

                        </div>

                    </div>


                    <div class="stat-tile">

                        <i class="bi bi-graph-up"></i>

                        <div>

                            <span class="stat-label">Total Sales</span>

                            <span class="stat-value">

                                <asp:Label
                                    ID="lblTotalSales"
                                    runat="server"
                                    Text="0">
                                </asp:Label>

                            </span>

                        </div>

                    </div>


                    <div class="stat-tile">

                        <i class="bi bi-calendar-check"></i>

                        <div>

                            <span class="stat-label">Sales This Month</span>

                            <span class="stat-value">

                                <asp:Label
                                    ID="lblMonthSales"
                                    runat="server"
                                    Text="0">
                                </asp:Label>

                            </span>

                        </div>

                    </div>


                    <div class="stat-tile">

                        <i class="bi bi-arrow-counterclockwise"></i>

                        <div>

                            <span class="stat-label">Return Orders</span>

                            <span class="stat-value">

                                <asp:Label
                                    ID="lblReturnOrders"
                                    runat="server"
                                    Text="0">
                                </asp:Label>

                            </span>

                        </div>

                    </div>

                </div>

            </div>


            <div class="summary-box mb-4">

                <h5>
                    Recent Orders
                </h5>

                <small class="text-muted d-block mb-3">
                    Orders taken for this shop, newest first
                </small>


                <div class="table-responsive mb-4">

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
