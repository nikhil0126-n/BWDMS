<%@ Page Title="Route Details"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="RouteDetails.aspx.cs"
    Inherits="BWDMS.Dealer.RouteDetails" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Route Details

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Route Details
            </h3>

            <p class="text-muted mb-0">
                Manage All Routes
            </p>

        </div>


        <a href="Routes.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Routes

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


        <!-- Route Information -->

        <div class="col-md-6">

            <div class="dashboard-card mb-4">

                <div class="d-flex justify-content-between align-items-center mb-3">

                    <div>

                        <small class="text-muted d-block">
                            Route
                        </small>


                        <strong>

                            <asp:Label
                                ID="lblRouteName"
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
                            Route Code
                        </small>


                        <asp:Label
                            ID="lblRouteCode"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Route Type
                        </small>


                        <asp:Label
                            ID="lblRouteType"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Days
                        </small>


                        <asp:Label
                            ID="lblDays"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Preferred Salesman
                        </small>


                        <asp:Label
                            ID="lblSalesman"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Default Vehicle
                        </small>


                        <asp:Label
                            ID="lblVehicle"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Village Count
                        </small>


                        <asp:Label
                            ID="lblVillageCount"
                            runat="server"
                            Text="0">
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

                </div>

            </div>

        </div>


        <!-- Shops In This Route -->

        <div class="col-md-6">

            <div class="dashboard-card">

                <h5 class="fw-bold mb-1">
                    Shops in this Route
                </h5>

                <small class="text-muted d-block mb-3">
                    Shops assigned to this route
                </small>


                <div class="table-responsive">

                    <asp:GridView
                        ID="gvShops"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table table-hover align-middle mb-0"
                        GridLines="None"
                        AllowPaging="true" OnPageIndexChanging="gvShops_PageIndexChanging" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                        <Columns>

                            <asp:BoundField
                                DataField="ShopCode"
                                HeaderText="Code" />

                            <asp:BoundField
                                DataField="ShopName"
                                HeaderText="Shop" />

                            <asp:BoundField
                                DataField="VillageName"
                                HeaderText="Village" />

                        </Columns>


                        <EmptyDataTemplate>

                            <div class="text-center py-4">

                                <p class="text-muted mb-0">
                                    No shops are assigned to this route.
                                </p>

                            </div>

                        </EmptyDataTemplate>

                    </asp:GridView>

                </div>

            </div>

        </div>

    </div>


</asp:Content>
