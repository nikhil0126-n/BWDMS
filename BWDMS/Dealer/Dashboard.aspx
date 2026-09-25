<%@ Page Title="Dealer Dashboard"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Dashboard.aspx.cs"
    Inherits="BWDMS.Dealer.Dashboard" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Dealer Dashboard

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- =========================================
         PAGE HEADER
         ========================================= -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Dealer Dashboard
            </h3>

            <p class="text-muted mb-0">
                Manage your sales, shops, stock and daily operations
            </p>

        </div>


        <div>

            <button type="button"
                    class="btn btn-danger">

                <i class="bi bi-plus-lg"></i>

                Create Order

            </button>

        </div>

    </div>


    <!-- =========================================
         STATISTICS
         ========================================= -->

    <div class="row g-4">


        <!-- Total Shops -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between">

                    <div>

                        <div class="card-title">
                            Total Shops
                        </div>

                        <h3>
                            0
                        </h3>

                        <small class="text-muted">
                            Registered shops
                        </small>

                    </div>


                    <div class="dashboard-icon">

                        <i class="bi bi-shop"></i>

                    </div>

                </div>

            </div>

        </div>


        <!-- Salesmen -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between">

                    <div>

                        <div class="card-title">
                            Salesmen
                        </div>

                        <h3>
                            0
                        </h3>

                        <small class="text-muted">
                            Active salesmen
                        </small>

                    </div>


                    <div class="dashboard-icon">

                        <i class="bi bi-people"></i>

                    </div>

                </div>

            </div>

        </div>


        <!-- Vehicles -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between">

                    <div>

                        <div class="card-title">
                            Vehicles
                        </div>

                        <h3>
                            0
                        </h3>

                        <small class="text-muted">
                            Active vehicles
                        </small>

                    </div>


                    <div class="dashboard-icon">

                        <i class="bi bi-truck"></i>

                    </div>

                </div>

            </div>

        </div>


        <!-- Today's Orders -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between">

                    <div>

                        <div class="card-title">
                            Today's Orders
                        </div>

                        <h3>
                            0
                        </h3>

                        <small class="text-muted">
                            Orders received today
                        </small>

                    </div>


                    <div class="dashboard-icon">

                        <i class="bi bi-cart-check"></i>

                    </div>

                </div>

            </div>

        </div>

    </div>


    <!-- =========================================
         SECOND ROW
         ========================================= -->

    <div class="row g-4 mt-1">


        <!-- Today's Sales -->

        <div class="col-xl-8">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between align-items-center mb-4">

                    <div>

                        <h5 class="fw-bold mb-1">
                            Sales Overview
                        </h5>

                        <small class="text-muted">
                            Sales performance
                        </small>

                    </div>


                    <select class="form-select form-select-sm"
                            style="width: 130px;">

                        <option>
                            This Week
                        </option>

                        <option>
                            This Month
                        </option>

                        <option>
                            This Year
                        </option>

                    </select>

                </div>


                <!-- Chart placeholder -->

                <div class="sales-chart">

                    <div class="chart-placeholder">

                        <i class="bi bi-bar-chart-line"></i>

                        <span>
                            Sales chart will appear here
                        </span>

                    </div>

                </div>

            </div>

        </div>


        <!-- Stock Summary -->

        <div class="col-xl-4">

            <div class="dashboard-card">

                <h5 class="fw-bold mb-1">
                    Stock Summary
                </h5>

                <small class="text-muted">
                    Current inventory status
                </small>


                <div class="stock-item">

                    <div>

                        <strong>
                            Wafers
                        </strong>

                        <small>
                            0 units
                        </small>

                    </div>

                    <span class="badge bg-success">
                        Good
                    </span>

                </div>


                <div class="stock-item">

                    <div>

                        <strong>
                            Namkeen
                        </strong>

                        <small>
                            0 units
                        </small>

                    </div>

                    <span class="badge bg-success">
                        Good
                    </span>

                </div>


                <div class="stock-item">

                    <div>

                        <strong>
                            Sev
                        </strong>

                        <small>
                            0 units
                        </small>

                    </div>

                    <span class="badge bg-warning text-dark">
                        Low
                    </span>

                </div>


                <div class="stock-item">

                    <div>

                        <strong>
                            Fryums
                        </strong>

                        <small>
                            0 units
                        </small>

                    </div>

                    <span class="badge bg-success">
                        Good
                    </span>

                </div>

            </div>

        </div>

    </div>


    <!-- =========================================
         THIRD ROW
         ========================================= -->

    <div class="row g-4 mt-1">


        <!-- Recent Orders -->

        <div class="col-xl-8">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between align-items-center mb-3">

                    <div>

                        <h5 class="fw-bold mb-1">
                            Recent Orders
                        </h5>

                        <small class="text-muted">
                            Latest shop orders
                        </small>

                    </div>


                    <a href="#"
                       class="text-danger text-decoration-none">

                        View All

                    </a>

                </div>


                <div class="table-responsive">

                    <table class="table align-middle">

                        <thead>

                            <tr>

                                <th>
                                    Order
                                </th>

                                <th>
                                    Shop
                                </th>

                                <th>
                                    Salesman
                                </th>

                                <th>
                                    Amount
                                </th>

                                <th>
                                    Status
                                </th>

                            </tr>

                        </thead>


                        <tbody>

                            <tr>

                                <td colspan="5"
                                    class="text-center text-muted py-4">

                                    No orders available

                                </td>

                            </tr>

                        </tbody>

                    </table>

                </div>

            </div>

        </div>


        <!-- Quick Actions -->

        <div class="col-xl-4">

            <div class="dashboard-card">

                <h5 class="fw-bold mb-1">
                    Quick Actions
                </h5>

                <small class="text-muted">
                    Common dealer operations
                </small>


                <div class="quick-actions mt-3">


                    <button type="button"
                            class="quick-action">

                        <i class="bi bi-cart-plus"></i>

                        <span>
                            Create Order
                        </span>

                    </button>


                    <button type="button"
                            class="quick-action">

                        <i class="bi bi-shop"></i>

                        <span>
                            Add Shop
                        </span>

                    </button>


                    <button type="button"
                            class="quick-action">

                        <i class="bi bi-person-plus"></i>

                        <span>
                            Add Salesman
                        </span>

                    </button>


                    <button type="button"
                            class="quick-action">

                        <i class="bi bi-truck"></i>

                        <span>
                            Manage Vehicle
                        </span>

                    </button>


                    <button type="button"
                            class="quick-action">

                        <i class="bi bi-box-seam"></i>

                        <span>
                            Check Inventory
                        </span>

                    </button>


                    <button type="button"
                            class="quick-action">

                        <i class="bi bi-arrow-repeat"></i>

                        <span>
                            Stock Reconciliation
                        </span>

                    </button>


                </div>

            </div>

        </div>

    </div>


</asp:Content>