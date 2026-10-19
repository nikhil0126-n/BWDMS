<%@ Page Title="Salesman Details"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="SalesmanDetails.aspx.cs"
    Inherits="BWDMS.Dealer.SalesmanDetails" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Salesman Details

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <style>
        .sm-page {
            max-width: 1060px;
            margin: 0 auto;
            padding: 10px 8px 40px;
        }

        .sm-head {
            display: flex;
            align-items: center;
            gap: 12px;
            margin: 6px 0 26px;
        }

            .sm-head a {
                color: #111827;
                text-decoration: none;
                font-size: 24px;
                line-height: 1;
            }

            .sm-head h3 {
                font-size: 24px;
                font-weight: 700;
                color: #111827;
                margin: 0;
            }

        .sm-grid {
            display: grid;
            grid-template-columns: 300px minmax(0, 1fr);
            gap: 30px;
            align-items: start;
        }

        .sm-card {
            background: #fff;
            border: 1px solid #e5e7eb;
            border-radius: 10px;
            padding: 30px 26px;
        }

        .sm-shop {
            font-size: 58px;
            line-height: 1;
            text-align: center;
            margin-bottom: 16px;
        }

        .sm-name {
            font-size: 19px;
            font-weight: 700;
            color: #111827;
            text-align: center;
            margin: 0;
        }

        .sm-status-top {
            text-align: center;
            color: #16a34a;
            font-weight: 600;
            font-size: 15px;
            margin: 6px 0 24px;
        }

        .sm-info {
            display: grid;
            gap: 15px;
        }

            .sm-info > div {
                display: flex;
                gap: 10px;
                font-size: 14px;
            }

            .sm-info span:first-child {
                color: #9ca3af;
                min-width: 96px;
            }

            .sm-info span:last-child {
                color: #1f2937;
                font-weight: 600;
            }

        .sm-section-title {
            font-size: 18px;
            font-weight: 700;
            color: #111827;
            margin: 0 0 14px;
        }

        .sm-perf {
            background: #fff;
            border: 1px solid #e5e7eb;
            border-radius: 10px;
            padding: 24px;
            margin-bottom: 24px;
        }

        .sm-stats {
            display: grid;
            grid-template-columns: repeat(2, minmax(0, 1fr));
            gap: 16px;
        }

        .sm-stat {
            display: flex;
            align-items: center;
            gap: 12px;
            border: 1px solid #e5e7eb;
            border-radius: 10px;
            padding: 16px 18px;
        }

            .sm-stat i {
                font-size: 24px;
                color: var(--bw-blue-600, #1d4ed8);
            }

            .sm-stat b {
                display: block;
                font-size: 14px;
                font-weight: 600;
                color: #111827;
            }

            .sm-stat span {
                display: block;
                font-size: 17px;
                font-weight: 700;
                color: #111827;
            }

        .sm-shops {
            background: #fff;
            border: 1px solid #e5e7eb;
            border-radius: 10px;
            padding: 24px 28px 30px;
        }

        .sm-shops .table {
            margin-bottom: 0;
        }

            .sm-shops .table thead {
                display: none;
            }

            .sm-shops .table td {
                border: none;
                padding: 12px 6px;
                font-size: 17px;
                font-weight: 700;
                color: #111827;
            }

            .sm-shops .table td.sm-amount {
                text-align: right;
            }

        @@media (max-width: 900px) {
            .sm-grid {
                grid-template-columns: 1fr;
            }
        }
    </style>


    <div class="sm-page">


        <!-- Header -->

        <div class="sm-head">

            <a href="Salesmen.aspx" title="Back to Salesmen">
                <i class="bi bi-arrow-left"></i>
            </a>

            <h3>Salesmen Details</h3>

        </div>


        <!-- Message -->

        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <div class="sm-grid">


            <!-- Salesman card -->

            <div class="sm-card">

                <div class="sm-shop">🏪</div>

                <p class="sm-name">

                    <asp:Label
                        ID="lblSalesmanName"
                        runat="server"
                        Text="-">
                    </asp:Label>

                </p>

                <p class="sm-status-top mb-0">

                    <asp:Label
                        ID="lblStatus"
                        runat="server"
                        CssClass="text-success"
                        Text="Active">
                    </asp:Label>

                </p>

                <div class="sm-info">

                    <div>
                        <span>Mobile</span>
                        <span>
                            <asp:Label
                                ID="lblPhone"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Email</span>
                        <span>
                            <asp:Label
                                ID="lblEmail"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Vehicle</span>
                        <span>
                            <asp:Label
                                ID="lblVehicle"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Joining Date</span>
                        <span>
                            <asp:Label
                                ID="lblJoiningDate"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                    <div>
                        <span>Status</span>
                        <span>
                            <asp:Label
                                ID="lblStatusText"
                                runat="server"
                                Text="-">
                            </asp:Label>
                        </span>
                    </div>

                </div>

            </div>


            <!-- Performance + Top Shops -->

            <div>

                <p class="sm-section-title">
                    Performance &mdash; This Month
                </p>

                <div class="sm-perf">

                    <div class="sm-stats">

                        <div class="sm-stat">

                            <i class="bi bi-receipt"></i>

                            <div>
                                <b>Total Orders</b>
                                <span>
                                    <asp:Label
                                        ID="lblTotalOrders"
                                        runat="server"
                                        Text="0">
                                    </asp:Label>
                                </span>
                            </div>

                        </div>

                        <div class="sm-stat">

                            <i class="bi bi-graph-up"></i>

                            <div>
                                <b>Total Sales</b>
                                <span>
                                    <asp:Label
                                        ID="lblTotalSales"
                                        runat="server"
                                        Text="0">
                                    </asp:Label>
                                </span>
                            </div>

                        </div>

                        <div class="sm-stat">

                            <i class="bi bi-box-seam"></i>

                            <div>
                                <b>Packets Sold</b>
                                <span>
                                    <asp:Label
                                        ID="lblPacketsSold"
                                        runat="server"
                                        Text="0">
                                    </asp:Label>
                                </span>
                            </div>

                        </div>

                        <div class="sm-stat">

                            <i class="bi bi-truck"></i>

                            <div>
                                <b>Dispatches</b>
                                <span>
                                    <asp:Label
                                        ID="lblDispatches"
                                        runat="server"
                                        Text="0">
                                    </asp:Label>
                                </span>
                            </div>

                        </div>

                    </div>

                </div>


                <div class="sm-shops">

                    <p class="sm-section-title">
                        Top Shops
                    </p>

                    <asp:GridView
                        ID="gvTopShops"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="table align-middle mb-0"
                        GridLines="None"
                        AllowPaging="true"
                        OnPageIndexChanging="gvTopShops_PageIndexChanging"
                        PageSize="25"
                        PagerSettings-Mode="NumericFirstLast"
                        PagerSettings-PageButtonCount="10"
                        PagerSettings-Position="TopAndBottom">

                        <Columns>

                            <asp:BoundField
                                DataField="ShopName"
                                HeaderText="Shop" />

                            <asp:BoundField
                                DataField="TotalText"
                                HeaderText="Total"
                                ItemStyle-CssClass="sm-amount"
                                HeaderStyle-CssClass="sm-amount" />

                        </Columns>


                        <EmptyDataTemplate>

                            <div class="text-center py-4">

                                <p class="text-muted mb-0">
                                    No orders found for this salesman.
                                </p>

                            </div>

                        </EmptyDataTemplate>

                    </asp:GridView>

                </div>

            </div>

        </div>

    </div>

</asp:Content>
