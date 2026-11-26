<%@ Page Title="Dashboard"
    Language="C#"
    MasterPageFile="~/Master/SalesmanMaster.master"
    AutoEventWireup="true"
    CodeBehind="Dashboard.aspx.cs"
    Inherits="BWDMS.Salesman.Dashboard" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Dashboard

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- ============================================================
         PAGE HEADER
         ============================================================ -->

    <div class="sw-page-head">

        <div>

            <h1 class="mb-1">

                <%= DateTime.Now.Hour < 12
                        ? "Good Morning"
                        : (DateTime.Now.Hour < 17
                            ? "Good Afternoon"
                            : "Good Evening") %>,
                <%= Server.HtmlEncode(
                        Convert.ToString(Session["FullName"])) %>

            </h1>

            <p>
                Here is your route summary for today.
            </p>

        </div>


        <div class="sw-greeting-date">

            <strong>

                <%= DateTime.Now.ToString("dd MMM yyyy") %>

            </strong>

            <small>

                <%= DateTime.Now.ToString("dddd") %>

            </small>

        </div>

    </div>



    <style>
        .sw-dashboard .sw-status-row {
            gap: 0;
        }

        .sw-dashboard .sw-status-item {
            min-width: 0;
        }

        .sw-dashboard .sw-stat-value,
        .sw-dashboard .sw-metric-val,
        .sw-dashboard .sw-route-name {
            overflow-wrap: anywhere;
            word-break: break-word;
        }

        .sw-dashboard .sw-stat > div:last-child {
            min-width: 0;
            flex: 1;
        }

        .sw-dashboard .sw-card {
            height: 100%;
        }

        @media (max-width: 575.98px) {
            .sw-dashboard .sw-status-row {
                flex-direction: column;
            }

            .sw-dashboard .sw-status-item + .sw-status-item {
                border-left: 0;
                border-top: 1px solid var(--sw-border-soft);
            }

            .sw-greeting-date {
                text-align: left;
            }
        }
    </style>

    <div class="sw-dashboard">

    <!-- ============================================================
         STAT CARDS
         ============================================================ -->

    <div class="row g-4 mb-4">

        <div class="col-6 col-md-3">

            <div class="sw-card sw-stat">

                <div class="sw-stat-icon sw-tint-primary">
                    <i class="bi bi-signpost-2"></i>
                </div>

                <div>

                    <div class="sw-stat-value">
                        <asp:Label ID="lblRouteCount" runat="server" Text="0" />
                    </div>

                    <div class="sw-stat-label">
                        Assigned Routes
                    </div>

                </div>

            </div>

        </div>


        <div class="col-6 col-md-3">

            <div class="sw-card sw-stat">

                <div class="sw-stat-icon sw-tint-green">
                    <i class="bi bi-calendar-check"></i>
                </div>

                <div>

                    <div class="sw-stat-value">
                        <asp:Label ID="lblScheduleCount" runat="server" Text="0" />
                    </div>

                    <div class="sw-stat-label">
                        Today's Visits
                    </div>

                </div>

            </div>

        </div>


        <div class="col-6 col-md-3">

            <div class="sw-card sw-stat">

                <div class="sw-stat-icon sw-tint-cyan">
                    <i class="bi bi-geo-alt"></i>
                </div>

                <div>

                    <div class="sw-stat-value">
                        <asp:Label ID="lblVillageCount" runat="server" Text="0" />
                    </div>

                    <div class="sw-stat-label">
                        Villages On Beat
                    </div>

                </div>

            </div>

        </div>


        <div class="col-6 col-md-3">

            <div class="sw-card sw-stat">

                <div class="sw-stat-icon sw-tint-orange">
                    <i class="bi bi-truck"></i>
                </div>

                <div>

                    <div class="sw-stat-value">
                        <asp:Label ID="lblVehicle" runat="server" Text="-" />
                    </div>

                    <div class="sw-stat-label">
                        Assigned Vehicle
                    </div>

                </div>

            </div>

        </div>

    </div>



    <!-- ============================================================
         TODAY'S SHOP STATUS + VEHICLE STOCK
         ============================================================ -->

    <div class="row g-4 mb-4">

        <div class="col-12 col-md-6">

            <div class="sw-card h-100">

                <div class="sw-card-title">
                    Today's Shop Status
                </div>

                <div class="sw-status-row">

                    <div class="sw-status-item">

                        <div class="sw-status-num">
                            <asp:Label ID="lblVisitedCount" runat="server" Text="0" />
                        </div>

                        <div class="sw-status-lbl">
                            <span class="sw-dot" style="background:#16a34a"></span>
                            Visited
                        </div>

                    </div>

                    <div class="sw-status-item">

                        <div class="sw-status-num">
                            <asp:Label ID="lblPendingCount" runat="server" Text="0" />
                        </div>

                        <div class="sw-status-lbl">
                            <span class="sw-dot" style="background:#f59e0b"></span>
                            Pending
                        </div>

                    </div>

                    <div class="sw-status-item">

                        <div class="sw-status-num">
                            <asp:Label ID="lblTotalShops" runat="server" Text="0" />
                        </div>

                        <div class="sw-status-lbl">
                            <span class="sw-dot" style="background:#98a2b3"></span>
                            Total Shops
                        </div>

                    </div>

                </div>

            </div>

        </div>


        <div class="col-12 col-md-6">

            <div class="sw-card h-100">

                <div class="sw-card-title">
                    Vehicle Stock
                </div>

                <div class="sw-stat">

                    <div class="sw-stat-icon sw-tint-orange">
                        <i class="bi bi-truck"></i>
                    </div>

                    <div>

                        <div class="sw-stat-value">
                            <asp:Label ID="lblStockRemaining" runat="server" Text="0 Box" />
                        </div>

                        <div class="sw-stat-label">
                            Remaining on latest load
                        </div>

                    </div>

                </div>

            </div>

        </div>

    </div>



    <!-- ============================================================
         TODAY'S ROUTE + SUMMARY
         ============================================================ -->

    <div class="row g-4 mb-4">

        <div class="col-12 col-md-6">

            <div class="sw-card h-100">

                <div class="sw-card-title">
                    Today's Route
                </div>

                <div class="sw-stat">

                    <div class="sw-stat-icon sw-tint-primary">
                        <i class="bi bi-signpost-split"></i>
                    </div>

                    <div>

                        <div class="sw-stat-value">
                            <asp:Label ID="lblTodayRoute" runat="server" Text="No route assigned" />
                        </div>

                        <div class="sw-stat-label">
                            <asp:Label ID="lblRouteSubtext" runat="server" Text="No beat assigned today" />
                        </div>

                    </div>

                </div>

            </div>

        </div>


        <div class="col-12 col-md-6">

            <div class="sw-card h-100">

                <div class="sw-card-title">
                    Sales Summary
                </div>

                <div class="sw-metric">

                    <div class="sw-metric-val">
                        <asp:Label ID="lblTodaySales" runat="server" Text="&#8377;0" />
                    </div>

                    <div class="sw-metric-lbl">
                        Today's Sales
                    </div>

                </div>

                <div class="sw-metric">

                    <div class="sw-metric-val">
                        <asp:Label ID="lblPreviousSales" runat="server" Text="&#8377;0" />
                    </div>

                    <div class="sw-metric-lbl">
                        Yesterday's Sales
                    </div>

                </div>

            </div>

        </div>

    </div>



    <!-- ============================================================
         TODAY'S BEAT
         ============================================================ -->

    <div class="sw-card">

        <div class="sw-card-head">

            <div>

                <h5>
                    Today's Beat
                </h5>

                <small>

                    <asp:Label ID="lblTodayName" runat="server" Text="" />
                    &nbsp;- villages in visit order

                </small>

            </div>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvBeat"
                runat="server"
                ClientIDMode="Static"
                AutoGenerateColumns="False"
                CssClass="sw-table align-middle mb-0"
                GridLines="None"
                EmptyDataText="No villages are scheduled for today."
                AllowPaging="true"
                OnPageIndexChanging="gvBeat_PageIndexChanging"
                PageSize="25"
                PagerSettings-Mode="NumericFirstLast"
                PagerSettings-PageButtonCount="10"
                PagerSettings-Position="TopAndBottom">
                <Columns>

                    <asp:BoundField
                        DataField="VisitSequence"
                        HeaderText="Seq" />

                    <asp:BoundField
                        DataField="VillageName"
                        HeaderText="Village" />

                    <asp:BoundField
                        DataField="Taluka"
                        HeaderText="Taluka" />

                    <asp:BoundField
                        DataField="District"
                        HeaderText="District" />

                    <asp:BoundField
                        DataField="RouteName"
                        HeaderText="Route" />

                    <asp:BoundField
                        DataField="DayOfWeek"
                        HeaderText="Day" />

                </Columns>

            </asp:GridView>

        </div>

    </div>

    </div>

</asp:Content>
