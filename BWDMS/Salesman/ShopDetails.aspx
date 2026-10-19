<%@ Page Title="Shop Details"
    Language="C#"
    MasterPageFile="~/Master/SalesmanMaster.master"
    AutoEventWireup="true"
    CodeBehind="ShopDetails.aspx.cs"
    Inherits="BWDMS.Salesman.ShopDetails" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Shop Details

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <style>

        .sd-back
        {
            margin    : 6px 0 22px 0;
            font-size : 16px;
            font-weight: 600;
        }

        .sd-back a
        {
            color         : #111827;
            text-decoration: none;
        }

        .sd-back a:hover
        {
            text-decoration: underline;
        }


        /* Shop header : icon + name + visit badge */

        .sd-top
        {
            display     : flex;
            align-items : center;
            gap         : 26px;
            margin      : 0 0 30px 0;
        }

        .sd-icon
        {
            font-size   : 64px;
            line-height : 1;
        }

        .sd-nameline
        {
            display     : flex;
            align-items : center;
            gap         : 14px;
        }

        .sd-name
        {
            font-size   : 22px;
            font-weight : 700;
            color       : #111827;
            margin      : 0;
        }

        .sd-visited
        {
            font-size   : 15px;
            font-weight : 700;
            color       : #16a34a;
        }

        .sd-visited.not-visited
        {
            color : #9ca3af;
        }

        .sd-code
        {
            font-size   : 20px;
            font-weight : 700;
            color       : #111827;
            margin      : 8px 0 0 0;
        }


        /* Two columns : details + summary */

        .sd-layout
        {
            display        : grid;
            grid-template-columns: 1fr 320px;
            gap            : 30px;
            align-items    : start;
        }


        /* Detail list */

        .sd-list
        {
            display        : grid;
            grid-template-columns: 160px 1fr;
            row-gap        : 16px;
            column-gap     : 14px;
            max-width      : 520px;
        }

        .sd-list .sd-label
        {
            font-size : 15px;
            color     : #9ca3af;
        }

        .sd-list .sd-value
        {
            font-size   : 15px;
            font-weight : 600;
            color       : #111827;
        }


        /* Summary card */

        .sd-card
        {
            border        : 1px solid #d1d5db;
            border-radius : 8px;
            background    : #fff;
            padding       : 20px 22px;
        }

        .sd-card-title
        {
            font-size     : 17px;
            font-weight   : 700;
            color         : #111827;
            text-align    : center;
            margin        : 0 0 18px 0;
            padding-bottom: 14px;
            border-bottom : 1px solid #e5e7eb;
        }

        .sd-row
        {
            display        : flex;
            align-items    : center;
            gap            : 14px;
            margin-bottom  : 18px;
        }

        .sd-row:last-child
        {
            margin-bottom : 0;
        }

        .sd-row i
        {
            font-size : 24px;
            color     : #2563eb;
        }

        .sd-row .sd-row-label
        {
            display   : block;
            font-size : 15px;
            color     : #111827;
        }

        .sd-row .sd-row-sub
        {
            display   : block;
            font-size : 12px;
            color     : #9ca3af;
            margin-top: 2px;
        }

        .sd-row .sd-row-value
        {
            margin-left : auto;
            font-size   : 15px;
            font-weight : 700;
            color       : #111827;
            white-space : nowrap;
        }


        /* Create Orders button */

        .sd-actions
        {
            display   : flex;
            justify-content : flex-end;
            margin-top: 24px;
        }

        .sd-btn
        {
            display         : inline-block;
            background      : #2563eb;
            color           : #fff;
            font-size       : 15px;
            font-weight     : 600;
            text-decoration : none;
            padding         : 12px 26px;
            border-radius   : 8px;
        }

        .sd-btn:hover
        {
            background : #1d4ed8;
            color      : #fff;
        }

        @media (max-width: 900px)
        {
            .sd-layout
            {
                grid-template-columns: 1fr;
            }
        }

    </style>


    <!-- Back -->

    <div class="sd-back">

        <a href="Shops.aspx" title="Back to Shops">&larr; Back to Shops</a>

    </div>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <!-- Shop header -->

    <div class="sd-top">

        <div class="sd-icon">

            <i class="bi bi-shop"></i>

        </div>

        <div>

            <div class="sd-nameline">

                <h4 class="sd-name">

                    <asp:Label
                        ID="lblShopName"
                        runat="server"
                        Text="-">
                    </asp:Label>

                </h4>

                <span class="sd-visited" id="visitWrap" runat="server">

                    <asp:Label
                        ID="lblVisit"
                        runat="server"
                        Text="Visited">
                    </asp:Label>

                </span>

            </div>

            <p class="sd-code">

                <asp:Label
                    ID="lblCode"
                    runat="server"
                    Text="-">
                </asp:Label>

            </p>

        </div>

    </div>


    <div class="sd-layout">


        <!-- Shop details -->

        <div class="sd-list">

            <div class="sd-label">Owner Name</div>
            <div class="sd-value">
                <asp:Label ID="lblOwnerName" runat="server" Text="-"></asp:Label>
            </div>

            <div class="sd-label">Mobile</div>
            <div class="sd-value">
                <asp:Label ID="lblPhone" runat="server" Text="-"></asp:Label>
            </div>

            <div class="sd-label">Location</div>
            <div class="sd-value">
                <asp:Label ID="lblVillage" runat="server" Text="-"></asp:Label>
            </div>

            <div class="sd-label">Address</div>
            <div class="sd-value">
                <asp:Label ID="lblAddress" runat="server" Text="-"></asp:Label>
            </div>

            <div class="sd-label">Credit Limit</div>
            <div class="sd-value">
                <asp:Label ID="lblCreditLimit" runat="server" Text="-"></asp:Label>
            </div>

            <div class="sd-label">Opening Balance</div>
            <div class="sd-value">
                <asp:Label ID="lblOpeningBalance" runat="server" Text="-"></asp:Label>
            </div>

            <div class="sd-label">Last Order</div>
            <div class="sd-value">
                <asp:Label ID="lblLastOrder" runat="server" Text="-"></asp:Label>
            </div>

        </div>


        <!-- Summary + action -->

        <div>

            <div class="sd-card">

                <p class="sd-card-title">
                    This Month Summary
                </p>

                <div class="sd-row">

                    <i class="bi bi-receipt"></i>

                    <div>
                        <span class="sd-row-label">Total Orders</span>
                    </div>

                    <span class="sd-row-value">

                        <asp:Label ID="lblTotalOrders" runat="server" Text="0"></asp:Label>

                    </span>

                </div>

                <div class="sd-row">

                    <i class="bi bi-graph-up-arrow"></i>

                    <div>
                        <span class="sd-row-label">Total Sales</span>
                    </div>

                    <span class="sd-row-value">

                        <asp:Label ID="lblTotalSales" runat="server" Text="0"></asp:Label>

                    </span>

                </div>

                <div class="sd-row">

                    <i class="bi bi-cash-stack"></i>

                    <div>
                        <span class="sd-row-label">Outstanding</span>
                        <span class="sd-row-sub">
                            <asp:Label ID="lblOutstandingShops" runat="server" Text="-"></asp:Label>
                        </span>
                    </div>

                    <span class="sd-row-value">

                        <asp:Label ID="lblOutstanding" runat="server" Text="0"></asp:Label>

                    </span>

                </div>

            </div>

            <div class="sd-actions">

                <a class="sd-btn" href="CreateOrder.aspx">
                    Create Orders
                </a>

            </div>

        </div>

    </div>

</asp:Content>
