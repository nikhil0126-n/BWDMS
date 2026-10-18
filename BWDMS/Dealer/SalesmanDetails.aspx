<%@ Page Title="Salesman Details"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="SalesmanDetails.aspx.cs"
    Inherits="BWDMS.Dealer.SalesmanDetails" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Salesman Details

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1 page-title">
                Salesman Details
            </h3>

            <p class="text-muted mb-0 page-subtitle">
                Manage All Salesmen
            </p>

        </div>


        <a href="Salesmen.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Salesmen

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


        <!-- Salesman Information -->

        <div class="col-lg-4">

            <div class="profile-card text-center">

                <div class="profile-avatar">
                    <i class="bi bi-person"></i>
                </div>

                <p class="profile-name">

                    <asp:Label
                        ID="lblSalesmanName"
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
                        <span>Assigned Vehicle</span>
                        <span>
                            <asp:Label
                                ID="lblVehicle"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Joining Date</span>
                        <span>
                            <asp:Label
                                ID="lblJoiningDate"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Status</span>
                        <span>
                            <asp:Label
                                ID="lblStatusText"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                </div>

            </div>

        </div>


        <!-- Performance + Top Shops -->

        <div class="col-lg-8">

            <div class="summary-box mb-4">

                <h5>
                    Performance - This Month
                </h5>

                <small class="text-muted d-block mb-3">
                    Orders and sales taken by this salesman
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

                        <i class="bi bi-box-seam"></i>

                        <div>

                            <span class="stat-label">Packets Sold</span>

                            <span class="stat-value">

                                <asp:Label
                                    ID="lblPacketsSold"
                                    runat="server"
                                    Text="0">
                                </asp:Label>

                            </span>

                        </div>

                    </div>


                    <div class="stat-tile">

                        <i class="bi bi-truck"></i>

                        <div>

                            <span class="stat-label">Dispatches</span>

                            <span class="stat-value">

                                <asp:Label
                                    ID="lblDispatches"
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
                    Top Shops
                </h5>

                <small class="text-muted d-block mb-3">
                    Shops by order value, highest first
                </small>


                <div class="table-responsive mb-4">

                    <asp:GridView
                        ID="gvTopShops"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table table-hover align-middle mb-0"
                        GridLines="None"
                        AllowPaging="true" OnPageIndexChanging="gvTopShops_PageIndexChanging" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                        <Columns>

                            <asp:BoundField
                                DataField="ShopName"
                                HeaderText="Shop" />

                            <asp:BoundField
                                DataField="OrderCount"
                                HeaderText="Orders"
                                ItemStyle-CssClass="text-end"
                                HeaderStyle-CssClass="text-end" />

                            <asp:BoundField
                                DataField="TotalText"
                                HeaderText="Total"
                                ItemStyle-CssClass="text-end"
                                HeaderStyle-CssClass="text-end" />

                        </Columns>


                        <EmptyDataTemplate>

                            <div class="text-center py-4">

                                <p class="text-muted mb-0">
                                    No orders found for this salesman.
                                </p>

                            </div>

                        </EmptyDataTemplate>

                    </asp:GridView>

                </div>

            </div>

        </div>

    </div>


</asp:Content>
