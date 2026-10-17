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

            <h3 class="fw-bold mb-1">
                Vehicle Details
            </h3>

            <p class="text-muted mb-0">
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


    <div class="row">


        <!-- Vehicle Information -->

        <div class="col-md-6">

            <div class="dashboard-card mb-4">

                <div class="d-flex justify-content-between align-items-center mb-3">

                    <div>

                        <small class="text-muted d-block">
                            Vehicle
                        </small>


                        <strong>

                            <asp:Label
                                ID="lblVehicleNumber"
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
                            Vehicle Name
                        </small>


                        <asp:Label
                            ID="lblVehicleName"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Vehicle Type
                        </small>


                        <asp:Label
                            ID="lblVehicleType"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </div>


                    <div class="col-md-6">

                        <small class="text-muted d-block">
                            Owned By
                        </small>


                        <asp:Label
                            ID="lblOwnerText"
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

                </div>

            </div>

        </div>


        <!-- This Month Summary -->

        <div class="col-md-6">

            <div class="dashboard-card">

                <h5 class="fw-bold mb-1">
                    This Month Summary
                </h5>

                <small class="text-muted d-block mb-3">
                    Dispatches and orders this vehicle carried
                </small>


                <div class="row g-4">

                    <div class="col-md-4">

                        <small class="text-muted d-block">
                            Total Trips
                        </small>


                        <strong>

                            <asp:Label
                                ID="lblTrips"
                                runat="server"
                                Text="0">
                            </asp:Label>

                        </strong>

                    </div>


                    <div class="col-md-4">

                        <small class="text-muted d-block">
                            Total Sales
                        </small>


                        <strong>

                            <asp:Label
                                ID="lblSales"
                                runat="server"
                                Text="0">
                            </asp:Label>

                        </strong>

                    </div>


                    <div class="col-md-4">

                        <small class="text-muted d-block">
                            Total Orders
                        </small>


                        <strong>

                            <asp:Label
                                ID="lblOrders"
                                runat="server"
                                Text="0">
                            </asp:Label>

                        </strong>

                    </div>

                </div>

            </div>

        </div>

    </div>


</asp:Content>
