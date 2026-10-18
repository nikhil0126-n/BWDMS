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

            <h3 class="fw-bold mb-1 page-title">
                Route Details
            </h3>

            <p class="text-muted mb-0 page-subtitle">
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


    <div class="row g-4 mb-4">


        <!-- Route Information -->

        <div class="col-lg-4">

            <div class="profile-card text-center">

                <div class="profile-avatar">
                    <i class="bi bi-signpost-2"></i>
                </div>

                <p class="profile-name">

                    <asp:Label
                        ID="lblRouteName"
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
                        <span>Route Code</span>
                        <span>
                            <asp:Label
                                ID="lblRouteCode"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Route Type</span>
                        <span>
                            <asp:Label
                                ID="lblRouteType"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Days</span>
                        <span>
                            <asp:Label
                                ID="lblDays"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Preferred Salesman</span>
                        <span>
                            <asp:Label
                                ID="lblSalesman"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Default Vehicle</span>
                        <span>
                            <asp:Label
                                ID="lblVehicle"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Village Count</span>
                        <span>
                            <asp:Label
                                ID="lblVillageCount"
                                runat="server"
                                Text="0">
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

                </div>

            </div>

        </div>


        <!-- Shops In This Route -->

        <div class="col-lg-8">

            <div class="summary-box mb-4">

                <h5>
                    Shops in this Route
                </h5>

                <small class="text-muted d-block mb-3">
                    Shops assigned to this route
                </small>


                <div class="table-responsive mb-4">

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
