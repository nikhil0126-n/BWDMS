<%@ Page Title="Vehicle Details"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="VehicleDetails.aspx.cs"
    Inherits="BWDMS.Dealer.VehicleDetails" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Vehicle Details

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1 page-title">
                Vehicle Details
            </h3>

            <p class="text-muted mb-0 page-subtitle">
                Manage All Vehicles
            </p>

        </div>


        <a href="Vehicles.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Vehicles

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


        <!-- Vehicle Information -->

        <div class="col-lg-4">

            <div class="profile-card text-center">

                <div class="profile-avatar">
                    <i class="bi bi-truck"></i>
                </div>

                <p class="profile-name">

                    <asp:Label
                        ID="lblVehicleNumber"
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
                        <span>Vehicle Name</span>
                        <span>
                            <asp:Label
                                ID="lblVehicleName"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Vehicle Type</span>
                        <span>
                            <asp:Label
                                ID="lblVehicleType"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Owned By</span>
                        <span>
                            <asp:Label
                                ID="lblOwnerText"
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

                </div>

            </div>

        </div>


        <!-- This Month Summary -->

        <div class="col-lg-8">

            <div class="summary-box mb-4">

                <h5>
                    This Month Summary
                </h5>

                <small class="text-muted d-block mb-3">
                    Dispatches and orders this vehicle carried
                </small>


                <div class="stat-grid">

                    <div class="stat-tile">

                        <i class="bi bi-signpost-split"></i>

                        <div>

                            <span class="stat-label">Total Trips</span>

                            <span class="stat-value">

                                <asp:Label
                                    ID="lblTrips"
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
                                    ID="lblSales"
                                    runat="server"
                                    Text="0">
                                </asp:Label>

                            </span>

                        </div>

                    </div>


                    <div class="stat-tile">

                        <i class="bi bi-cart-check"></i>

                        <div>

                            <span class="stat-label">Total Orders</span>

                            <span class="stat-value">

                                <asp:Label
                                    ID="lblOrders"
                                    runat="server"
                                    Text="0">
                                </asp:Label>

                            </span>

                        </div>

                    </div>

                </div>

            </div>

        </div>

    </div>


</asp:Content>
