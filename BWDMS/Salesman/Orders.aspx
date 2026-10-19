<%@ Page Title="My Orders"
    Language="C#"
    MasterPageFile="~/Master/SalesmanMaster.master"
    AutoEventWireup="true"
    CodeBehind="Orders.aspx.cs"
    Inherits="BWDMS.Salesman.Orders" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    My Orders

</asp:Content>


<asp:Content ID="HeadStyle"
    ContentPlaceHolderID="HeadContent"
    runat="server">

    <style>
        /* ---------- Order placed success ---------- */
        .os-wrap {
            display: flex;
            align-items: center;
            gap: 2rem;
            flex-wrap: wrap;
            padding: 1.4rem .6rem;
        }

        .os-left {
            flex: 1 1 320px;
            min-width: 260px;
        }

        .os-row {
            display: flex;
            align-items: flex-start;
            gap: .4rem;
            margin-bottom: 1.45rem;
            font-size: .95rem;
        }

        .os-row .os-label {
            color: #9a9a9a;
            min-width: 132px;
            text-align: right;
            font-weight: 500;
        }

        .os-row .os-val {
            color: #1b1b1b;
            font-weight: 700;
        }

        .os-right {
            flex: 1 1 320px;
            min-width: 300px;
            text-align: center;
        }

        .os-card {
            position: relative;
            max-width: 360px;
            margin: 0 auto;
            padding: 1.6rem 1.4rem 1.5rem;
            background: #fff;
            border: 1px solid #eceff3;
            border-radius: 12px;
            box-shadow: 0 8px 26px rgba(15, 23, 42, .06);
            overflow: hidden;
        }

        .os-check {
            width: 74px;
            height: 74px;
            margin: 0 auto .9rem;
            border-radius: 50%;
            background: #22b573;
            color: #fff;
            font-size: 2.4rem;
            font-weight: 700;
            display: flex;
            align-items: center;
            justify-content: center;
            box-shadow: 0 0 0 8px rgba(34, 181, 115, .14);
        }

        .os-card h4 {
            margin: 0 0 .35rem;
            font-size: 1.32rem;
            font-weight: 800;
            color: #10182b;
        }

        .os-card p {
            margin: 0 0 .9rem;
            font-size: .82rem;
            color: #7b8496;
        }

        .os-badge {
            display: inline-block;
            background: #eef0ff;
            color: #5b5ff0;
            border-radius: 8px;
            padding: .4rem .95rem;
            font-size: .85rem;
            font-weight: 700;
        }

        .os-big {
            margin: 1.6rem 0 1.3rem;
            font-size: 1.75rem;
            font-weight: 800;
            color: #111;
        }

        .os-actions {
            display: flex;
            justify-content: center;
            gap: 1.1rem;
            flex-wrap: wrap;
        }

        .os-btn {
            min-width: 152px;
            text-align: center;
            border-radius: 8px;
            padding: .72rem 1.4rem;
            font-size: .92rem;
            font-weight: 700;
            text-decoration: none;
            cursor: pointer;
            display: inline-block;
        }

        .os-btn-outline {
            background: #fff;
            border: 1px solid #d5d5d5;
            color: #111;
        }

        .os-btn-outline:hover {
            background: #f5f6fa;
            color: #111;
        }

        .os-btn-primary {
            background: #6c5ce7;
            border: 1px solid #6c5ce7;
            color: #fff;
        }

        .os-btn-primary:hover {
            background: #5a4bd6;
            color: #fff;
        }

        /* confetti */
        .os-confetti {
            position: absolute;
            inset: 0;
            pointer-events: none;
        }

        .os-confetti i {
            position: absolute;
            width: 7px;
            height: 7px;
            border-radius: 2px;
            display: block;
        }

        .os-actions .os-btn:focus {
            outline: none;
        }

        @media (max-width: 767px) {
            .os-row {
                justify-content: flex-start;
            }

            .os-row .os-label {
                min-width: 118px;
            }

            .os-big {
                font-size: 1.4rem;
            }
        }

        /* ---------- Orders list (matches mock) ---------- */

        .or-topbar {
            display: flex;
            align-items: center;
            gap: 16px;
            padding: 2px 0 16px 0;
            margin: 0 0 22px 0;
            border-bottom: 1px solid #e5e7eb;
        }

        .or-back {
            color: #111827;
            font-size: 20px;
            line-height: 1;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
        }

        .or-back:hover {
            color: #111827;
        }

        .or-topbar h3 {
            margin: 0;
            font-size: 21px;
            font-weight: 700;
            color: #111827;
        }

        /* search + status bar */

        .or-searchbar {
            display: flex;
            align-items: center;
            gap: 12px;
            height: 56px;
            padding: 0 8px 0 18px;
            background: #fff;
            border: 1px solid #d9d9d9;
            border-radius: 10px;
            margin: 0 0 20px 0;
        }

        .or-searchbar .or-search-icon {
            color: #9ca3af;
            font-size: 19px;
            display: inline-flex;
        }

        .or-searchbar .form-control {
            flex: 1;
            height: 54px;
            padding: 0;
            border: 0;
            background: transparent;
            box-shadow: none !important;
            font-size: 17px;
            color: #111827;
        }

        .or-searchbar .form-control:focus,
        .or-searchbar .form-control::placeholder {
            background: transparent;
            color: #9ca3af;
            box-shadow: none !important;
        }

        .or-searchbar .form-control::placeholder {
            color: #9ca3af;
            opacity: 1;
        }

        .or-filter {
            display: flex;
            align-items: center;
            padding-left: 12px;
            border-left: 1px solid #ececec;
            height: 40px;
        }

        .or-filter .form-select {
            width: auto;
            min-width: 96px;
            height: 42px;
            font-size: 16px;
            color: #111827;
            background-color: #fff;
            border: 1px solid #d9d9d9;
            border-radius: 8px;
            padding-right: 34px;
            background-image: url("data:image/svg+xml;charset=utf8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16'%3E%3Cpath fill='%236b7280' d='M8 11L3 6h10z'/%3E%3C/svg%3E");
            background-repeat: no-repeat;
            background-position: right 12px center;
            background-size: 12px 12px;
            appearance: none;
            -webkit-appearance: none;
        }

        .or-filter .form-select:focus {
            outline: none;
            border-color: #6366f1;
        }

        /* table */

        .or-table-wrap {
            border: 1px solid #d1d5db;
            border-radius: 8px;
            overflow: hidden;
            background: #fff;
        }

        .or-table {
            width: 100%;
            border-collapse: collapse;
        }

        .or-table th {
            background: #d4d4d4;
            font-size: 16px;
            font-weight: 700;
            color: #111111;
            text-align: left;
            padding: 16px 20px;
            border-bottom: 1px solid #bdbdbd;
        }

        .or-table td {
            font-size: 15px;
            color: #111111;
            padding: 16px 20px;
            border-bottom: 1px solid #ececec;
        }

        .or-table tr:last-child td {
            border-bottom: none;
        }

        .or-muted {
            color: #6b7280;
        }

        .or-amount {
            font-weight: 700;
        }

        .or-status {
            font-weight: 600;
        }

        .or-status.ok { color: #16a34a; }
        .or-status.info { color: #2563eb; }
        .or-status.warn { color: #d97706; }
        .or-status.bad { color: #dc2626; }

        .or-pager {
            display: flex;
            justify-content: center;
            align-items: center;
            gap: 10px;
            padding: 14px 0;
        }

        .or-pager a,
        .or-pager span {
            min-width: 34px;
            height: 34px;
            padding: 0 10px;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            border: 1px solid #d1d5db;
            border-radius: 6px;
            background: #fff;
            color: #374151;
            font-size: 14px;
            text-decoration: none;
        }

        .or-pager span {
            background: #c7d2fe;
            border-color: #c7d2fe;
            color: #111827;
            font-weight: 700;
        }
    </style>

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Order just placed: detail + success card -->
    <asp:Panel
        ID="pnlSuccess"
        runat="server"
        Visible="false">

        <div class="dashboard-card">

            <div class="os-wrap">

                <div class="os-left">

                    <div class="os-row">
                        <span class="os-label">Shop Name :</span>
                        <span class="os-val">
                            <asp:Label ID="lblShopName" runat="server" Text="-"></asp:Label>
                        </span>
                    </div>

                    <div class="os-row">
                        <span class="os-label">Order Date :</span>
                        <span class="os-val">
                            <asp:Label ID="lblPlacedOn" runat="server" Text="-"></asp:Label>
                        </span>
                    </div>

                    <div class="os-row">
                        <span class="os-label">Total Amount :</span>
                        <span class="os-val">
                            &#8377;<asp:Label ID="lblTotalAmount" runat="server" Text="0.00"></asp:Label>
                        </span>
                    </div>

                    <div class="os-row">
                        <span class="os-label">Payment Method :</span>
                        <span class="os-val">
                            <asp:Label ID="lblPaymentMethod" runat="server" Text="Cash"></asp:Label>
                        </span>
                    </div>

                    <div class="os-row">
                        <span class="os-label">Collated By :</span>
                        <span class="os-val">
                            <asp:Label ID="lblCollatedBy" runat="server" Text="-"></asp:Label>
                        </span>
                    </div>

                </div>


                <div class="os-right">

                    <div class="os-card">

                        <div class="os-confetti">
                            <i style="background:#2f6bff; top:12%; left:6%;"></i>
                            <i style="background:#ff4d8d; top:8%; right:10%;"></i>
                            <i style="background:#ffb020; top:26%; left:2%;"></i>
                            <i style="background:#ff4d8d; top:34%; right:4%;"></i>
                            <i style="background:#7c5cff; top:52%; left:5%;"></i>
                            <i style="background:#2f6bff; top:60%; right:7%;"></i>
                            <i style="background:#ffc107; top:74%; left:9%;"></i>
                            <i style="background:#ff4d8d; top:82%; right:12%;"></i>
                            <i style="background:#20c997; top:44%; left:14%;"></i>
                            <i style="background:#ffb020; top:18%; right:22%;"></i>
                            <i style="background:#7c5cff; top:66%; right:24%;"></i>
                            <i style="background:#2f6bff; top:90%; left:34%;"></i>
                        </div>

                        <div class="os-check">
                            <i class="bi bi-check-lg"></i>
                        </div>

                        <h4>Order Completed!</h4>

                        <p>Your order has been placed successfully.</p>

                        <span class="os-badge">
                            Order #<asp:Label ID="lblOrderNumber" runat="server" Text="-"></asp:Label>
                        </span>

                    </div>


                    <h2 class="os-big">Placed Order Successful!</h2>


                    <div class="os-actions">

                        <a runat="server"
                           id="lnkViewOrder"
                           href="#"
                           class="os-btn os-btn-outline">
                            View Order
                        </a>

                        <a href="CreateOrder.aspx"
                           class="os-btn os-btn-primary">
                            New Order
                        </a>

                    </div>

                </div>

            </div>

        </div>

    </asp:Panel>


    <asp:Panel
        ID="pnlList"
        runat="server">

    <!-- Page header -->

    <div class="or-topbar">

        <a class="or-back"
           href="Dashboard.aspx"
           title="Back">

            <i class="bi bi-arrow-left"></i>

        </a>

        <h3>MY Orders</h3>

    </div>


    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <!-- Search + status filter -->

    <div class="or-searchbar">

        <span class="or-search-icon">
            <i class="bi bi-search"></i>
        </span>

        <asp:TextBox
            ID="txtSearch"
            runat="server"
            CssClass="form-control"
            placeholder="Search Order...."
            AutoPostBack="true"
            OnTextChanged="FilterChanged">
        </asp:TextBox>

        <div class="or-filter">

            <asp:DropDownList
                ID="ddlStatus"
                runat="server"
                CssClass="form-select"
                AutoPostBack="true"
                OnSelectedIndexChanged="FilterChanged">

                <asp:ListItem Text="All" Value="" Selected="True">
                </asp:ListItem>

                <asp:ListItem Text="Pending" Value="Pending">
                </asp:ListItem>

                <asp:ListItem Text="Confirmed" Value="Confirmed">
                </asp:ListItem>

                <asp:ListItem Text="Dispatched" Value="Dispatched">
                </asp:ListItem>

                <asp:ListItem Text="Delivered" Value="Delivered">
                </asp:ListItem>

                <asp:ListItem Text="Cancelled" Value="Cancelled">
                </asp:ListItem>

            </asp:DropDownList>

        </div>

    </div>


    <!-- Orders table -->

    <div class="or-table-wrap">

        <div class="table-responsive">

            <asp:GridView
                ID="gvOrders"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="or-table"
                GridLines="None"
                AllowPaging="true"
                OnPageIndexChanging="gvOrders_PageIndexChanging"
                PageSize="25"
                PagerSettings-Mode="NumericFirstLast"
                PagerSettings-PageButtonCount="10"
                PagerSettings-Position="Bottom"
                PagerStyle-CssClass="or-pager"
                PagerStyle-HorizontalAlign="Center">

                <Columns>

                    <asp:BoundField
                        DataField="OrderNumber"
                        HeaderText="Order Id"
                        ItemStyle-CssClass="or-muted" />

                    <asp:BoundField
                        DataField="ShopName"
                        HeaderText="Shop Name" />

                    <asp:BoundField
                        DataField="OrderDateText"
                        HeaderText="Date" />

                    <asp:BoundField
                        DataField="AmountText"
                        HeaderText="Amount"
                        ItemStyle-CssClass="or-amount" />

                    <asp:BoundField
                        DataField="PaymentText"
                        HeaderText="Payment" />

                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Eval("StatusCss") %>'>

                                <%# Eval("Status") %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-cart-check fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            You have not taken any orders yet.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

        <asp:Label
            ID="lblRecordCount"
            runat="server"
            Visible="false"
            Text="records: 0">
        </asp:Label>

    </div>

    </asp:Panel>


</asp:Content>
