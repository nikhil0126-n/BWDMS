<%@ Page Title="Dealer Details"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="DealerDetails.aspx.cs"
    Inherits="BWDMS.Admin.DealerDetails" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Dealer Details

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div class="d-flex align-items-center gap-2">

            <a href="Dealers.aspx"
               class="btn btn-outline-secondary btn-sm"
               title="Back to dealers">
                <i class="bi bi-arrow-left"></i>
            </a>

            <h3 class="fw-bold mb-0">
                Dealer Details
            </h3>

        </div>


        <div>

            <asp:HyperLink
                ID="lnkEditDealer"
                runat="server"
                CssClass="btn btn-primary">
                <i class="bi bi-pencil-square me-1"></i>
                Edit Dealer
            </asp:HyperLink>

        </div>

    </div>


    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <asp:Panel
        ID="pnlDetails"
        runat="server">


        <!-- Dealer Card -->

        <div class="dashboard-card mb-4">

            <div class="d-flex align-items-center flex-wrap gap-4">

                <div class="text-center">

                    <i class="bi bi-shop fs-1 text-primary"></i>

                </div>


                <div class="flex-grow-1">

                    <h4 class="fw-bold mb-2">
                        <asp:Label
                            ID="lblDealerName"
                            runat="server">
                        </asp:Label>
                    </h4>


                    <p class="mb-1 text-muted">
                        <i class="bi bi-person me-2"></i>
                        <strong>Owner :</strong>
                        <asp:Label
                            ID="lblOwner"
                            runat="server">
                        </asp:Label>
                    </p>


                    <p class="mb-1 text-muted">
                        <i class="bi bi-telephone me-2"></i>
                        <strong>Mobile Number :</strong>
                        <asp:Label
                            ID="lblPhone"
                            runat="server">
                        </asp:Label>
                    </p>


                    <p class="mb-1 text-muted">
                        <i class="bi bi-envelope me-2"></i>
                        <strong>Email :</strong>
                        <asp:Label
                            ID="lblEmail"
                            runat="server">
                        </asp:Label>
                    </p>


                    <p class="mb-1 text-muted">
                        <i class="bi bi-geo-alt me-2"></i>
                        <strong>Location :</strong>
                        <asp:Label
                            ID="lblLocation"
                            runat="server">
                        </asp:Label>
                    </p>


                    <p class="mb-0 text-muted">
                        <i class="bi bi-calendar-check me-2"></i>
                        <strong>Join On :</strong>
                        <asp:Label
                            ID="lblJoined"
                            runat="server">
                        </asp:Label>
                    </p>

                </div>


                <div>

                    <asp:Label
                        ID="lblStatusBadge"
                        runat="server"
                        CssClass="badge">
                    </asp:Label>

                </div>

            </div>

        </div>


        <!-- Overview Heading -->

        <h5 class="fw-bold mb-3">
            Overview
        </h5>


        <!-- Overview Statistics -->

        <div class="row g-4 mb-4">


            <div class="col-xl-3 col-md-6">

                <div class="dashboard-card">

                    <div class="d-flex align-items-center gap-3">

                        <span class="stat-icon bg-primary-subtle text-primary-emphasis">
                            <i class="bi bi-people"></i>
                        </span>

                        <div>

                            <small class="text-muted fw-semibold d-block">
                                Salesman
                            </small>

                            <h3 class="mb-0">
                                <asp:Label
                                    ID="lblSalesmanCount"
                                    runat="server"
                                    Text="0">
                                </asp:Label>
                            </h3>

                        </div>

                    </div>

                </div>

            </div>


            <div class="col-xl-3 col-md-6">

                <div class="dashboard-card">

                    <div class="d-flex align-items-center gap-3">

                        <span class="stat-icon bg-success-subtle text-success-emphasis">
                            <i class="bi bi-shop"></i>
                        </span>

                        <div>

                            <small class="text-muted fw-semibold d-block">
                                Shop
                            </small>

                            <h3 class="mb-0">
                                <asp:Label
                                    ID="lblShopCount"
                                    runat="server"
                                    Text="0">
                                </asp:Label>
                            </h3>

                        </div>

                    </div>

                </div>

            </div>


            <div class="col-xl-3 col-md-6">

                <div class="dashboard-card">

                    <div class="d-flex align-items-center gap-3">

                        <span class="stat-icon bg-warning-subtle text-warning-emphasis">
                            <i class="bi bi-cart-check"></i>
                        </span>

                        <div>

                            <small class="text-muted fw-semibold d-block">
                                Order (This Month)
                            </small>

                            <h3 class="mb-0">
                                <asp:Label
                                    ID="lblOrdersThisMonth"
                                    runat="server"
                                    Text="0">
                                </asp:Label>
                            </h3>

                        </div>

                    </div>

                </div>

            </div>


            <div class="col-xl-3 col-md-6">

                <div class="dashboard-card">

                    <div class="d-flex align-items-center gap-3">

                        <span class="stat-icon bg-info-subtle text-info-emphasis">
                            <i class="bi bi-currency-rupee"></i>
                        </span>

                        <div>

                            <small class="text-muted fw-semibold d-block">
                                Total Sales
                            </small>

                            <h3 class="mb-0">
                                <asp:Label
                                    ID="lblTotalSales"
                                    runat="server"
                                    Text="0.00">
                                </asp:Label>
                            </h3>

                        </div>

                    </div>

                </div>

            </div>


        </div>


        <!-- Recent Orders -->

        <div class="dashboard-card">

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
                    EmptyDataText="No orders yet for this dealer."
                    AllowPaging="true"
                    OnPageIndexChanging="gvRecentOrders_PageIndexChanging"
                    PageSize="10"
                    PagerSettings-Mode="NumericFirstLast"
                    PagerSettings-PageButtonCount="5"
                    PagerSettings-Position="Bottom">

                    <Columns>

                        <asp:BoundField
                            DataField="OrderNumber"
                            HeaderText="Orders ID" />

                        <asp:BoundField
                            DataField="DateText"
                            HeaderText="Date" />

                        <asp:BoundField
                            DataField="ShopName"
                            HeaderText="Shop Name" />

                        <asp:BoundField
                            DataField="GrandTotalText"
                            HeaderText="Amount" />

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


    </asp:Panel>


</asp:Content>
