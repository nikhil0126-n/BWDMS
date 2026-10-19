<%@ Page Title="Shop Details"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="ShopDetails.aspx.cs"
    Inherits="BWDMS.Dealer.ShopDetails" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Shop Details

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <style>

        .sd-head
        {
            display   : flex;
            align-items: center;
            gap       : 10px;
            margin    : 8px 0 26px 0;
        }

        .sd-head a
        {
            color       : #111827;
            font-size   : 24px;
            line-height : 1;
            text-decoration: none;
        }

        .sd-title
        {
            font-size   : 24px;
            font-weight : 700;
            color       : #111827;
            margin      : 0;
        }


        .sd-layout
        {
            display        : grid;
            grid-template-columns: 300px 1fr;
            gap            : 26px;
            align-items    : start;
        }


        /* Left card */

        .sd-card
        {
            border        : 1px solid #d1d5db;
            border-radius : 8px;
            background    : #fff;
            padding       : 30px 26px;
            text-align    : center;
        }

        .sd-avatar
        {
            font-size   : 72px;
            line-height : 1;
            color       : #6b7280;
            margin      : 10px 0 18px 0;
        }

        .sd-name
        {
            font-size   : 20px;
            font-weight : 700;
            color       : #111827;
            margin      : 0 0 6px 0;
        }

        .sd-status
        {
            font-size     : 16px;
            font-weight   : 600;
            color         : #16a34a;
            margin        : 0 0 26px 0;
        }

        .sd-status.inactive
        {
            color: #dc2626;
        }

        .sd-list
        {
            text-align : left;
            margin     : 0;
        }

        .sd-list > div
        {
            display        : flex;
            justify-content: space-between;
            gap           : 14px;
            padding       : 7px 0;
        }

        .sd-list span:first-child
        {
            color     : #9ca3af;
            font-size : 14px;
            white-space: nowrap;
        }

        .sd-list span:last-child
        {
            color     : #111827;
            font-size : 14px;
            font-weight: 600;
            text-align: right;
        }


        /* Right side */

        .sd-section-title
        {
            font-size   : 20px;
            font-weight : 700;
            color       : #111827;
            margin      : 0 0 14px 0;
        }

        .sd-box
        {
            border        : 1px solid #d1d5db;
            border-radius : 8px;
            background    : #fff;
            padding       : 18px;
            margin-bottom : 26px;
        }

        .sd-tiles
        {
            display   : grid;
            grid-template-columns: 1fr 1fr;
            gap       : 16px;
        }

        .sd-tile
        {
            display        : flex;
            align-items    : center;
            gap            : 14px;
            border         : 1px solid #d1d5db;
            border-radius  : 8px;
            padding        : 16px 18px;
        }

        .sd-tile i
        {
            font-size : 26px;
            color     : #6b7280;
        }

        .sd-tile .sd-tile-label
        {
            display   : block;
            font-size : 14px;
            color     : #6b7280;
        }

        .sd-tile .sd-tile-value
        {
            display     : block;
            font-size   : 17px;
            font-weight : 700;
            color       : #111827;
            margin-top  : 2px;
        }


        /* Orders table */

        .sd-table
        {
            width          : 100%;
            border-collapse: collapse;
        }

        .sd-table th
        {
            background    : #fff;
            font-size     : 16px;
            font-weight   : 700;
            color         : #111827;
            text-align    : left;
            padding       : 14px 16px;
            border-bottom : 1px solid #d1d5db;
        }

        .sd-table td
        {
            font-size     : 16px;
            color         : #374151;
            padding       : 18px 16px;
            border-bottom : 1px solid #ececec;
        }

        .sd-table tr:last-child td
        {
            border-bottom: none;
        }

    </style>


    <!-- Page Header -->

    <div class="sd-head">

        <a href="Shops.aspx" title="Back to Shops">&larr;</a>

        <h3 class="sd-title">Shop Details</h3>

    </div>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <div class="sd-layout">


        <!-- Shop Information -->

        <div class="sd-card">

            <div class="sd-avatar">
                <i class="bi bi-shop"></i>
            </div>

            <p class="sd-name">

                <asp:Label
                    ID="lblShopName"
                    runat="server"
                    Text="-">
                </asp:Label>

            </p>

            <p class="sd-status" id="statusWrap" runat="server">

                <asp:Label
                    ID="lblStatus"
                    runat="server"
                    Text="Active">
                </asp:Label>

            </p>

            <div class="sd-list">

                <div>
                    <span>Owner Name</span>
                    <span>
                        <asp:Label
                            ID="lblOwnerName"
                            runat="server"
                            Text="-">
                        </asp:Label>
                    </span>
                </div>

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
                    <span>Shop Code</span>
                    <span>
                        <asp:Label
                            ID="lblShopCode"
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
                    <span>Location</span>
                    <span>
                        <asp:Label
                            ID="lblVillage"
                            runat="server"
                            Text="-">
                        </asp:Label>
                    </span>
                </div>

                <div>
                    <span>Route</span>
                    <span>
                        <asp:Label
                            ID="lblRoute"
                            runat="server"
                            Text="-">
                        </asp:Label>
                    </span>
                </div>

                <div>
                    <span>Address</span>
                    <span>
                        <asp:Label
                            ID="lblAddress"
                            runat="server"
                            Text="-">
                        </asp:Label>
                    </span>
                </div>

                <div>
                    <span>Credit Limit</span>
                    <span>
                        <asp:Label
                            ID="lblCreditLimit"
                            runat="server"
                            Text="-">
                        </asp:Label>
                    </span>
                </div>

                <div>
                    <span>Opening Balance</span>
                    <span>
                        <asp:Label
                            ID="lblOpeningBalance"
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


        <!-- Summary + Recent Orders -->

        <div>

            <h5 class="sd-section-title">
                Shop Summary
            </h5>


            <div class="sd-box">

                <div class="sd-tiles">

                    <div class="sd-tile">

                        <i class="bi bi-cart-check"></i>

                        <div>

                            <span class="sd-tile-label">Total Orders</span>

                            <span class="sd-tile-value">

                                <asp:Label
                                    ID="lblTotalOrders"
                                    runat="server"
                                    Text="0">
                                </asp:Label>

                            </span>

                        </div>

                    </div>


                    <div class="sd-tile">

                        <i class="bi bi-graph-up"></i>

                        <div>

                            <span class="sd-tile-label">Total Sales</span>

                            <span class="sd-tile-value">

                                <asp:Label
                                    ID="lblTotalSales"
                                    runat="server"
                                    Text="0">
                                </asp:Label>

                            </span>

                        </div>

                    </div>


                    <div class="sd-tile">

                        <i class="bi bi-calendar-check"></i>

                        <div>

                            <span class="sd-tile-label">Sales This Month</span>

                            <span class="sd-tile-value">

                                <asp:Label
                                    ID="lblMonthSales"
                                    runat="server"
                                    Text="0">
                                </asp:Label>

                            </span>

                        </div>

                    </div>


                    <div class="sd-tile">

                        <i class="bi bi-arrow-counterclockwise"></i>

                        <div>

                            <span class="sd-tile-label">Return Orders</span>

                            <span class="sd-tile-value">

                                <asp:Label
                                    ID="lblReturnOrders"
                                    runat="server"
                                    Text="0">
                                </asp:Label>

                            </span>

                        </div>

                    </div>

                </div>

            </div>


            <h5 class="sd-section-title">
                Recent Orders
            </h5>


            <div class="sd-box mb-4">

                <div class="table-responsive">

                    <asp:GridView
                        ID="gvOrders"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="sd-table"
                        GridLines="None"
                        AllowPaging="true"
                        OnPageIndexChanging="gvOrders_PageIndexChanging"
                        PageSize="25"
                        PagerSettings-Mode="NumericFirstLast"
                        PagerSettings-PageButtonCount="10"
                        PagerSettings-Position="Bottom">

                        <Columns>

                            <asp:BoundField
                                DataField="OrderNumber"
                                HeaderText="Order ID" />

                            <asp:BoundField
                                DataField="OrderDateText"
                                HeaderText="Order Date" />

                            <asp:BoundField
                                DataField="GrandTotal"
                                HeaderText="Amount"
                                DataFormatString="{0:N2}" />

                            <asp:BoundField
                                DataField="Status"
                                HeaderText="Status" />

                        </Columns>


                        <EmptyDataTemplate>

                            <div class="text-center py-4">

                                <p class="text-muted mb-0">
                                    No orders found for this shop.
                                </p>

                            </div>

                        </EmptyDataTemplate>

                    </asp:GridView>

                </div>

            </div>

        </div>

    </div>

</asp:Content>
