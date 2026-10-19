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


    <style>
        .vd-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 30px; }
        .vd-title-wrap { display: flex; align-items: center; gap: 10px; }
        .vd-title-wrap i { font-size: 1.5rem; font-weight: 700; color: #111; }
        .vd-title { font-size: 1.5rem; font-weight: 700; color: #111; margin: 0; }
        .vd-breadcrumb { color: #9aa0a6; font-size: .8rem; margin: 4px 0 0 30px; }
        .vd-btn-edit { background: #5b6ef5; border: none; color: #fff; border-radius: 8px; padding: 11px 20px; font-size: .95rem; font-weight: 600; text-decoration: none; display: inline-flex; align-items: center; gap: 6px; }
        .vd-btn-edit:hover { background: #4a5ce0; color: #fff; text-decoration: none; }
        .vd-card { background: #fff; border: 1px solid #d9d9d9; border-radius: 10px; padding: 30px 28px; }
        .vd-vehicle-top { display: flex; align-items: center; gap: 20px; margin-bottom: 30px; }
        .vd-vehicle-icon { font-size: 3.2rem; color: #b5d89a; line-height: 1; }
        .vd-name { font-size: 1.15rem; font-weight: 700; color: #111; margin: 0; }
        .vd-status { color: #17c964; font-weight: 700; font-size: 1rem; margin: 4px 0 0; }
        .vd-info p { margin: 0 0 14px; color: #333; font-size: .95rem; }
        .vd-info p b { font-weight: 700; color: #111; }
        .vd-summary-card { background: #fff; border: 1px solid #d9d9d9; border-radius: 10px; padding: 22px; }
        .vd-summary-card h5 { font-size: 1.15rem; font-weight: 700; color: #111; text-align: center; margin: 0 0 20px; padding-bottom: 16px; border-bottom: 1px solid #d9d9d9; }
        .vd-stat-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
        .vd-stat-tile { background: #e8e8e8; border-radius: 8px; padding: 16px 18px; display: flex; align-items: center; gap: 12px; }
        .vd-stat-tile i { font-size: 1.5rem; color: #2f4ff0; }
        .vd-stat-label { display: block; font-size: .9rem; font-weight: 700; color: #111; }
        .vd-stat-value { display: block; font-size: 1.05rem; font-weight: 700; color: #111; margin-top: 2px; }
    </style>


    <!-- Page Header -->

    <div class="vd-header">

        <div>

            <div class="vd-title-wrap">

                <a href="Vehicles.aspx" style="text-decoration: none;">
                    <i class="bi bi-arrow-left"></i>
                </a>

                <h3 class="vd-title">
                    Vehicles Details
                </h3>

            </div>

            <p class="vd-breadcrumb">
                Vehicles &gt; All Vehicles
            </p>

        </div>

        <a href="AddVehicle.aspx" class="vd-btn-edit">
            <i class="bi bi-plus-lg"></i>
            Edit Route
        </a>

    </div>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <div class="row g-4">


        <!-- Vehicle Information -->

        <div class="col-lg-4">

            <div class="vd-card">

                <div class="vd-vehicle-top">

                    <div class="vd-vehicle-icon">
                        <i class="bi bi-truck"></i>
                    </div>

                    <div>

                        <p class="vd-name">

                            <asp:Label
                                ID="lblVehicleNumber"
                                runat="server"
                                Text="Vehicle 1">
                            </asp:Label>

                        </p>

                        <p class="vd-status">

                            <asp:Label
                                ID="lblStatus"
                                runat="server"
                                Text="Active">
                            </asp:Label>

                        </p>

                    </div>

                </div>

                <div class="vd-info">

                    <p>
                        <b>Type:-</b>
                        <asp:Label ID="lblVehicleType" runat="server" Text="-" />
                    </p>

                    <p>
                        <b>Vehicle Name:-</b>
                        <asp:Label ID="lblVehicleName" runat="server" Text="-" />
                    </p>

                    <p>
                        <b>Owned By:-</b>
                        <asp:Label ID="lblOwnerText" runat="server" Text="-" />
                    </p>

                    <p>
                        <b>Added On:-</b>
                        <asp:Label ID="lblCreatedAt" runat="server" Text="-" />
                    </p>

                </div>

            </div>

        </div>


        <!-- This Month Summary -->

        <div class="col-lg-8">

            <div class="vd-summary-card">

                <h5>
                    This Month Summary
                </h5>

                <div class="vd-stat-grid">

                    <div class="vd-stat-tile">

                        <i class="bi bi-signpost-split"></i>

                        <div>

                            <span class="vd-stat-label">Total Trips</span>

                            <span class="vd-stat-value">

                                <asp:Label
                                    ID="lblTrips"
                                    runat="server"
                                    Text="0">
                                </asp:Label>

                            </span>

                        </div>

                    </div>


                    <div class="vd-stat-tile">

                        <i class="bi bi-graph-up"></i>

                        <div>

                            <span class="vd-stat-label">Total Sales</span>

                            <span class="vd-stat-value">

                                <asp:Label
                                    ID="lblSales"
                                    runat="server"
                                    Text="0">
                                </asp:Label>

                            </span>

                        </div>

                    </div>


                    <div class="vd-stat-tile">

                        <i class="bi bi-cart-check"></i>

                        <div>

                            <span class="vd-stat-label">Total Orders</span>

                            <span class="vd-stat-value">

                                <asp:Label
                                    ID="lblOrders"
                                    runat="server"
                                    Text="0">
                                </asp:Label>

                            </span>

                        </div>

                    </div>


                    <div class="vd-stat-tile">

                        <i class="bi bi-geo-alt-fill"></i>

                        <div>

                            <span class="vd-stat-label">Total Distance</span>

                            <span class="vd-stat-value">0 KM</span>

                        </div>

                    </div>

                </div>

            </div>

        </div>

    </div>


</asp:Content>
