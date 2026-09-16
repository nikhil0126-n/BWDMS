<%@ Page Title="Admin Dashboard"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Dashboard.aspx.cs"
    Inherits="BWDMS.Admin.Dashboard" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Admin Dashboard

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Heading -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Admin Dashboard
            </h3>

            <p class="text-muted mb-0">
                Overview of your BWDMS system
            </p>

        </div>

        <div>

            <button class="btn btn-danger">

                <i class="bi bi-plus-lg"></i>

                Add Product

            </button>

        </div>

    </div>


    <!-- Statistics -->

    <div class="row g-4">


        <!-- Dealers -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="card-title">
                    Total Dealers
                </div>

                <h3>
                    0
                </h3>

                <small class="text-muted">
                    Registered dealers
                </small>

            </div>

        </div>


        <!-- Categories -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="card-title">
                    Categories
                </div>

                <h3>
                    0
                </h3>

                <small class="text-muted">
                    Product categories
                </small>

            </div>

        </div>


        <!-- Products -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="card-title">
                    Total Products
                </div>

                <h3>
                    0
                </h3>

                <small class="text-muted">
                    Active products
                </small>

            </div>

        </div>


        <!-- Orders -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="card-title">
                    Total Orders
                </div>

                <h3>
                    0
                </h3>

                <small class="text-muted">
                    Orders received
                </small>

            </div>

        </div>

    </div>


    <!-- Recent Activity -->

    <div class="row mt-4">

        <div class="col-lg-8">

            <div class="dashboard-card">

                <h5 class="fw-bold">
                    Recent Orders
                </h5>

                <p class="text-muted">
                    No orders available yet.
                </p>

            </div>

        </div>


        <div class="col-lg-4">

            <div class="dashboard-card">

                <h5 class="fw-bold">
                    Quick Actions
                </h5>

                <div class="d-grid gap-2 mt-3">

                    <button class="btn btn-outline-danger">
                        Add Category
                    </button>

                    <button class="btn btn-outline-danger">
                        Add Product
                    </button>

                    <button class="btn btn-outline-danger">
                        Add Dealer
                    </button>

                </div>

            </div>

        </div>

    </div>


</asp:Content>