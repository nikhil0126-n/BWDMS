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

            <a href="AddProduct.aspx"
               class="btn btn-primary">

                <i class="bi bi-plus-lg"></i>

                Add Product

            </a>

        </div>

    </div>


    <!-- Statistics -->

    <div class="row g-4">


        <!-- Dealers -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between align-items-start">

                    <div class="card-title">
                        Total Dealers
                    </div>

                    <span class="stat-icon bg-primary-subtle text-primary-emphasis">
                        <i class="bi bi-people"></i>
                    </span>

                </div>

                <h3>
                    <asp:Label
                        ID="lblDealerCount"
                        runat="server"
                        Text="0">
                    </asp:Label>
                </h3>

                <small class="text-muted">
                    Active registered dealers
                </small>

            </div>

        </div>


        <!-- Categories -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between align-items-start">

                    <div class="card-title">
                        Categories
                    </div>

                    <span class="stat-icon bg-success-subtle text-success-emphasis">
                        <i class="bi bi-tags"></i>
                    </span>

                </div>

                <h3>
                    <asp:Label
                        ID="lblCategoryCount"
                        runat="server"
                        Text="0">
                    </asp:Label>
                </h3>

                <small class="text-muted">
                    Product categories
                </small>

            </div>

        </div>


        <!-- Products -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between align-items-start">

                    <div class="card-title">
                        Total Products
                    </div>

                    <span class="stat-icon bg-warning-subtle text-warning-emphasis">
                        <i class="bi bi-box-seam"></i>
                    </span>

                </div>

                <h3>
                    <asp:Label
                        ID="lblProductCount"
                        runat="server"
                        Text="0">
                    </asp:Label>
                </h3>

                <small class="text-muted">
                    Active products
                </small>

            </div>

        </div>


        <!-- Orders -->

        <div class="col-xl-3 col-md-6">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between align-items-start">

                    <div class="card-title">
                        Total Orders
                    </div>

                    <span class="stat-icon bg-info-subtle text-info-emphasis">
                        <i class="bi bi-cart-check"></i>
                    </span>

                </div>

                <h3>
                    <asp:Label
                        ID="lblOrderCount"
                        runat="server"
                        Text="0">
                    </asp:Label>
                </h3>

                <small class="text-muted">
                    Orders received
                </small>

            </div>

        </div>

    </div>


    <!-- Product Overview + Recent Activity -->

    <div class="row mt-4">


        <!-- Product Overview (donut by category) -->

        <div class="col-lg-4">

            <div class="dashboard-card">

                <h5 class="fw-bold mb-1">
                    Product Overview
                </h5>

                <p class="text-muted small mb-3">
                    Active products grouped by category
                </p>


                <asp:Label
                    ID="lblOverviewMessage"
                    runat="server"
                    Visible="false"
                    CssClass="text-muted small d-block text-center py-5">
                </asp:Label>


                <div id="divOverview"
                     runat="server">

                    <div class="position-relative d-flex justify-content-center my-3">

                        <canvas id="productOverviewChart"
                                width="180"
                                height="180">
                        </canvas>

                        <div class="position-absolute top-50 start-50 translate-middle text-center">

                            <div class="fw-bold"
                                 style="font-size: 1.9rem; line-height: 1.1;">

                                <asp:Label
                                    ID="lblOverviewTotal"
                                    runat="server"
                                    Text="0">
                                </asp:Label>

                            </div>

                            <small class="text-muted">
                                Total Products
                            </small>

                        </div>

                    </div>


                    <asp:Repeater
                        ID="rptOverview"
                        runat="server">

                        <ItemTemplate>

                            <div class="d-flex align-items-center justify-content-between py-1">

                                <span class="d-inline-flex align-items-center">

                                    <span style='display:inline-block; width:13px; height:13px; border-radius:3px; margin-right:9px; background-color:<%# Eval("Color") %>;'>
                                    </span>

                                    <%# Eval("CategoryName") %>

                                </span>


                                <span class="text-muted small">

                                    <strong class="text-body">
                                        <%# Eval("CountText") %>
                                    </strong>

                                    (<%# Eval("PercentText") %>)

                                </span>

                            </div>

                        </ItemTemplate>

                    </asp:Repeater>

                </div>

            </div>

        </div>


        <!-- Recent Dealers -->

        <div class="col-lg-8">

            <div class="dashboard-card">

                <div class="d-flex justify-content-between align-items-start mb-1">

                    <div>

                        <h5 class="fw-bold mb-1">
                            Recent Dealers
                        </h5>

                        <p class="text-muted small mb-0">
                            Newest dealers on the system
                        </p>

                    </div>


                    <a href="Dealers.aspx"
                       class="btn btn-sm btn-outline-secondary">

                        View all

                    </a>

                </div>


                <div class="table-responsive">

                    <asp:GridView
                        ID="gvRecentDealers"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table table-hover align-middle"
                        GridLines="None"
                        EmptyDataText="No dealers have joined yet.">

                        <Columns>

                            <asp:BoundField
                                DataField="DealerName"
                                HeaderText="Dealers Name" />

                            <asp:BoundField
                                DataField="Location"
                                HeaderText="Location" />

                            <asp:TemplateField
                                HeaderText="Status">

                                <ItemTemplate>

                                    <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge bg-success" : "badge bg-danger" %>'>

                                        <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>

                                    </span>

                                </ItemTemplate>

                            </asp:TemplateField>

                            <asp:BoundField
                                DataField="JoinedOn"
                                HeaderText="Joined On" />

                        </Columns>

                    </asp:GridView>

                </div>

            </div>

        </div>


    </div>


    <%-- ============================================================
         RECENT DEALER ACTIVITY - HIDDEN, NOT DELETED.
         The panel was taken off the dashboard on request. The markup
         below is kept so it can be restored later; to bring it back,
         remove the opening comment tag above this banner and the
         closing one after the closing table div further down.
         Its loader (LoadRecentActivity) and pager handler are
         commented out in Dashboard.aspx.cs for the same reason.
         ============================================================

    Recent Dealer Activity

    <div class="row mt-4">

        <div class="col-lg-12">

            <div class="dashboard-card">

                <h5 class="fw-bold">
                    Recent Dealer Activity
                </h5>

                <p class="text-muted">
                    Latest orders created by your dealers
                </p>

                <div class="table-responsive">

                    <asp:GridView
                        ID="gvRecentActivity"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table table-hover align-middle"
                        GridLines="None"
                        EmptyDataText="No orders available yet." AllowPaging="true" OnPageIndexChanging="gvRecentActivity_PageIndexChanging" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                        <Columns>

                            <asp:BoundField
                                DataField="CreatedAtText"
                                HeaderText="Created" />

                            <asp:BoundField
                                DataField="DealerName"
                                HeaderText="Dealer" />

                            <asp:BoundField
                                DataField="OrderNumber"
                                HeaderText="Order No." />

                            <asp:BoundField
                                DataField="ShopName"
                                HeaderText="Shop" />

                            <asp:BoundField
                                DataField="GrandTotalText"
                                HeaderText="Amount"
                                DataFormatString="{0:N2}" />

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

        </div>


    </div>

    ============ end of hidden Recent Dealer Activity ============ --%>


    <script type="text/javascript"
            src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js">
    </script>

    <script type="text/javascript">
        function drawProductOverview() {

            var data = <%= ProductOverviewJson %>;
            var canvas = document.getElementById('productOverviewChart');

            if (!canvas || typeof Chart === 'undefined' || !data || !data.length) {
                return;
            }

            if (canvas.__bwChart) {
                canvas.__bwChart.destroy();
            }

            var labels = [];
            var values = [];
            var colors = [];

            for (var i = 0; i < data.length; i++) {
                labels.push(data[i].label);
                values.push(data[i].value);
                colors.push(data[i].color);
            }

            canvas.__bwChart = new Chart(canvas, {
                type: 'doughnut',
                data: {
                    labels: labels,
                    datasets: [{
                        data: values,
                        backgroundColor: colors,
                        borderWidth: 2,
                        borderColor: '#ffffff'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    cutout: '62%',
                    plugins: {
                        legend: { display: false },
                        tooltip: {
                            callbacks: {
                                label: function (ctx) {
                                    var total = ctx.dataset.data.reduce(function (a, b) { return a + b; }, 0);
                                    var pct = total ? Math.round(ctx.parsed * 1000 / total) / 10 : 0;
                                    return ' ' + ctx.label + ': ' + ctx.parsed + ' (' + pct + '%)';
                                }
                            }
                        }
                    }
                }
            });
        }

        window.addEventListener('load', drawProductOverview);
    </script>


</asp:Content>