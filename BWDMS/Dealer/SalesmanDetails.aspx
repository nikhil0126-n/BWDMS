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

            <h3 class="fw-bold mb-1">
                Salesman Details
            </h3>

            <p class="text-muted mb-0">
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


    <div class="row">


        <!-- Salesman Information -->

        <div class="col-md-6">

            <div class="dashboard-card mb-4">

                <div class="d-flex justify-content-between align-items-center mb-3">

                    <div>

                        <small class="text-muted d-block">
                            Salesman
                        </small>


                        <strong>

                            <asp:Label
                                ID="lblSalesmanName"
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
                            Assigned Vehicle
                        </small>


                        <asp:Label
                            ID="lblVehicle"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Joining Date
                        </small>


                        <asp:Label
                            ID="lblJoiningDate"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Status
                        </small>


                        <asp:Label
                            ID="lblStatusText"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>

                </div>

            </div>

        </div>


        <!-- Performance + Top Shops -->

        <div class="col-md-6">

            <div class="dashboard-card mb-4">

                <h5 class="fw-bold mb-1">
                    Performance - This Month
                </h5>

                <small class="text-muted d-block mb-3">
                    Orders and sales taken by this salesman
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
                            Packets Sold
                        </small>


                        <strong>

                            <asp:Label
                                ID="lblPacketsSold"
                                runat="server"
                                Text="0">
                            </asp:Label>

                        </strong>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Dispatches
                        </small>


                        <strong>

                            <asp:Label
                                ID="lblDispatches"
                                runat="server"
                                Text="0">
                            </asp:Label>

                        </strong>

                    </div>

                </div>

            </div>


            <div class="dashboard-card">

                <h5 class="fw-bold mb-1">
                    Top Shops
                </h5>

                <small class="text-muted d-block mb-3">
                    Shops by order value, highest first
                </small>


                <div class="table-responsive">

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
