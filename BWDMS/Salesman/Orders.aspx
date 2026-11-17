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
        /* ============================================================
           MY ORDERS — Exact redesign matching screenshots
           ============================================================ */

        /* Page header */
        .or-header {
            display: flex;
            align-items: center;
            gap: 16px;
            padding: 8px 0 16px 0;
            margin: 0 0 24px 0;
            border-bottom: 1px solid #e5e7eb;
        }

        .or-back {
            color: #111827;
            font-size: 22px;
            line-height: 1;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            padding: 8px;
            border-radius: 8px;
            transition: background 0.15s ease;
        }

        .or-back:hover {
            background: #f3f4f6;
            color: #111827;
        }

        .or-header h2 {
            margin: 0;
            font-size: 22px;
            font-weight: 700;
            color: #111827;
        }

        /* Search + filter bar */
        .or-toolbar {
            display: flex;
            align-items: center;
            gap: 12px;
            flex-wrap: wrap;
            background: #fff;
            border: 1px solid #e5e7eb;
            border-radius: 12px;
            padding: 14px 16px;
            margin-bottom: 20px;
        }

        .or-search {
            flex: 1;
            min-width: 260px;
            position: relative;
        }

        .or-search input {
            width: 100%;
            height: 44px;
            padding: 0 16px 0 44px;
            border: 1px solid #d1d5db;
            border-radius: 10px;
            background: #f9fafb;
            font-size: 15px;
            color: #111827;
            transition: border-color 0.15s ease, box-shadow 0.15s ease, background 0.15s ease;
        }

        .or-search input:focus {
            outline: none;
            border-color: #6366f1;
            background: #fff;
            box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.15);
        }

        .or-search input::placeholder {
            color: #9ca3af;
        }

        .or-search-icon {
            position: absolute;
            left: 14px;
            top: 50%;
            transform: translateY(-50%);
            color: #9ca3af;
            font-size: 18px;
            pointer-events: none;
        }

        .or-filter {
            display: flex;
            align-items: center;
            gap: 8px;
            padding-left: 12px;
            border-left: 1px solid #e5e7eb;
            height: 44px;
        }

        .or-filter-label {
            font-size: 14px;
            color: #6b7280;
            font-weight: 500;
            white-space: nowrap;
        }

        .or-filter select {
            min-width: 140px;
            height: 44px;
            font-size: 14px;
            color: #111827;
            background: #fff;
            border: 1px solid #d1d5db;
            border-radius: 8px;
            padding: 0 36px 0 14px;
            appearance: none;
            background-image: url("data:image/svg+xml;charset=utf8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16'%3E%3Cpath fill='%236b7280' d='M8 11L3 6h10z'/%3E%3C/svg%3E");
            background-repeat: no-repeat;
            background-position: right 12px center;
            background-size: 12px 12px;
            cursor: pointer;
            transition: border-color 0.15s ease;
        }

        .or-filter select:focus {
            outline: none;
            border-color: #6366f1;
        }

        /* Table */
        .or-table-wrap {
            background: #fff;
            border: 1px solid #e5e7eb;
            border-radius: 12px;
            overflow: hidden;
        }

        .or-table {
            width: 100%;
            border-collapse: collapse;
        }

        .or-table thead {
            background: #f3f4f6;
        }

        .or-table th {
            font-size: 13px;
            font-weight: 600;
            color: #374151;
            text-align: left;
            padding: 14px 16px;
            border-bottom: 1px solid #e5e7eb;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .or-table td {
            font-size: 14px;
            color: #111827;
            padding: 14px 16px;
            border-bottom: 1px solid #f3f4f6;
            vertical-align: middle;
        }

        .or-table tbody tr:last-child td {
            border-bottom: none;
        }

        .or-table tbody tr {
            transition: background 0.1s ease;
        }

        .or-table tbody tr:hover {
            background: #fafafa;
        }

        /* Column widths */
        .or-col-id { width: 140px; }
        .or-col-shop { width: 220px; }
        .or-col-date { width: 150px; }
        .or-col-items { width: 90px; text-align: center; }
        .or-col-amount { width: 140px; text-align: right; font-weight: 600; }
        .or-col-status { width: 130px; text-align: center; }
        .or-col-action { width: 60px; text-align: center; }

        /* Status badges - pill style matching screenshots */
        .or-badge {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            min-width: 88px;
            height: 28px;
            border-radius: 20px;
            font-size: 12px;
            font-weight: 600;
            line-height: 1;
            padding: 0 10px;
        }

        .or-badge.pending {
            background: #fef3c7;
            color: #b45309;
        }
        .or-badge.confirmed {
            background: #dbeafe;
            color: #1d4ed8;
        }
        .or-badge.dispatched {
            background: #e0e7ff;
            color: #3730a3;
        }
        .or-badge.delivered {
            background: #dcfce7;
            color: #166534;
        }
        .or-badge.cancelled {
            background: #fee2e2;
            color: #b91c1c;
        }

        /* Amount */
        .or-amount {
            font-weight: 700;
            color: #111827;
            font-variant-numeric: tabular-nums;
        }

        /* Action button */
        .or-view-btn {
            width: 36px;
            height: 36px;
            border-radius: 8px;
            border: 1px solid #e5e7eb;
            background: #fff;
            color: #6b7280;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            text-decoration: none;
            transition: all 0.15s ease;
        }

        .or-view-btn:hover {
            background: #f3f4f6;
            border-color: #d1d5db;
            color: #111827;
        }

        .or-view-btn i {
            font-size: 16px;
        }

        /* Empty state */
        .or-empty {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            padding: 60px 24px;
            text-align: center;
        }

        .or-empty i {
            font-size: 48px;
            color: #d1d5db;
            margin-bottom: 16px;
        }

        .or-empty h3 {
            margin: 0 0 8px;
            font-size: 18px;
            font-weight: 600;
            color: #111827;
        }

        .or-empty p {
            margin: 0;
            color: #9ca3af;
            font-size: 14px;
        }

        .or-empty .or-btn-primary {
            margin-top: 16px;
        }

        /* Pagination */
        .or-pager {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 6px;
            padding: 16px 0;
        }

        .or-pager a,
        .or-pager span {
            min-width: 36px;
            height: 36px;
            padding: 0 12px;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            border: 1px solid #e5e7eb;
            border-radius: 8px;
            background: #fff;
            color: #374151;
            font-size: 14px;
            font-weight: 500;
            text-decoration: none;
            transition: all 0.15s ease;
        }

        .or-pager a:hover {
            background: #f3f4f6;
            border-color: #d1d5db;
            color: #111827;
        }

        .or-pager span.current {
            background: #6366f1;
            border-color: #6366f1;
            color: #fff;
        }

        .or-pager .or-pager-ellipsis {
            color: #9ca3af;
            border-color: transparent;
            background: transparent;
            cursor: default;
        }

        .or-pager .or-pager-ellipsis:hover {
            background: transparent;
            color: #9ca3af;
        }

        /* ============================================================
           SUCCESS CARD — Two-column layout matching Screenshot 3
           ============================================================ */
        .os-wrap {
            display: flex;
            align-items: flex-start;
            gap: 32px;
            flex-wrap: wrap;
            padding: 8px 0;
        }

        .os-left {
            flex: 1 1 340px;
            min-width: 280px;
        }

        .os-right {
            flex: 1 1 340px;
            min-width: 300px;
            display: flex;
            flex-direction: column;
            align-items: center;
        }

        .os-detail-row {
            display: flex;
            align-items: flex-start;
            gap: 12px;
            margin-bottom: 18px;
            font-size: 15px;
        }

        .os-detail-label {
            color: #6b7280;
            min-width: 140px;
            text-align: right;
            font-weight: 500;
        }

        .os-detail-value {
            color: #111827;
            font-weight: 600;
            flex: 1;
        }

        .os-card {
            width: 100%;
            max-width: 380px;
            background: #fff;
            border: 1px solid #e5e7eb;
            border-radius: 16px;
            padding: 28px 24px;
            text-align: center;
            box-shadow: 0 10px 30px rgba(15, 23, 42, 0.06);
            position: relative;
            overflow: hidden;
        }

        .os-check {
            width: 80px;
            height: 80px;
            margin: 0 auto 16px;
            border-radius: 50%;
            background: #22c55e;
            color: #fff;
            font-size: 30px;
            font-weight: 700;
            display: flex;
            align-items: center;
            justify-content: center;
            box-shadow: 0 0 0 8px rgba(34, 197, 94, 0.12);
        }

        .os-card h3 {
            margin: 0 0 8px;
            font-size: 20px;
            font-weight: 800;
            color: #111827;
        }

        .os-card p {
            margin: 0 0 16px;
            font-size: 14px;
            color: #6b7280;
        }

        .os-badge-order {
            display: inline-block;
            background: #eef2ff;
            color: #4f46e5;
            border-radius: 8px;
            padding: 8px 16px;
            font-size: 14px;
            font-weight: 700;
            margin-bottom: 20px;
        }

        .os-total {
            font-size: 28px;
            font-weight: 800;
            color: #111827;
            margin: 8px 0 20px;
        }

        .os-actions {
            display: flex;
            justify-content: center;
            gap: 12px;
            flex-wrap: wrap;
        }

        .os-btn {
            min-width: 150px;
            text-align: center;
            border-radius: 10px;
            padding: 12px 20px;
            font-size: 15px;
            font-weight: 600;
            text-decoration: none;
            cursor: pointer;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
            transition: all 0.15s ease;
        }

        .os-btn-outline {
            background: #fff;
            border: 1px solid #d1d5db;
            color: #374151;
        }

        .os-btn-outline:hover {
            background: #f9fafb;
            border-color: #9ca3af;
            color: #111827;
        }

        .os-btn-primary {
            background: #6366f1;
            border: 1px solid #6366f1;
            color: #fff;
        }

        .os-btn-primary:hover {
            background: #4f46e5;
            border-color: #4f46e5;
            color: #fff;
        }

        /* Confetti decoration on success card */
        .os-confetti {
            position: absolute;
            inset: 0;
            pointer-events: none;
            overflow: hidden;
        }

        .os-confetti i {
            position: absolute;
            width: 8px;
            height: 8px;
            border-radius: 2px;
            display: block;
            opacity: 0.7;
        }

        /* Responsive */
        @media (max-width: 767px) {
            .or-header {
                flex-wrap: wrap;
                gap: 12px;
            }

            .or-header h2 {
                font-size: 18px;
            }

            .or-toolbar {
                flex-direction: column;
                align-items: stretch;
            }

            .or-search {
                min-width: 0;
            }

            .or-filter {
                border-left: none;
                border-top: 1px solid #e5e7eb;
                padding-left: 0;
                padding-top: 12px;
                height: auto;
                justify-content: space-between;
            }

            .or-filter-label {
                display: none;
            }

            .or-filter select {
                flex: 1;
            }

            .or-table thead {
                display: none;
            }

            .or-table,
            .or-table tbody,
            .or-table tr,
            .or-table td {
                display: block;
                width: 100%;
            }

            .or-table tbody tr {
                border: 1px solid #e5e7eb;
                border-radius: 10px;
                margin-bottom: 12px;
                padding: 12px;
                background: #fff;
            }

            .or-table td {
                border: none;
                padding: 6px 0;
                display: flex;
                justify-content: space-between;
                align-items: center;
            }

            .or-table td::before {
                content: attr(data-label);
                font-size: 12px;
                font-weight: 600;
                color: #6b7280;
                text-transform: uppercase;
                letter-spacing: 0.3px;
            }

            .or-col-items,
            .or-col-amount,
            .or-col-status,
            .or-col-action {
                text-align: right;
            }

            .os-wrap {
                flex-direction: column;
                align-items: stretch;
            }

            .os-left,
            .os-right {
                min-width: 0;
            }

            .os-detail-row {
                flex-wrap: wrap;
            }

            .os-detail-label {
                min-width: 120px;
                text-align: left;
            }
        }
    </style>

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- ============================================================
         ORDER JUST PLACED — Success Card (Two-column layout from Screenshot 3)
         ============================================================ -->
    <asp:Panel ID="pnlSuccess" runat="server" Visible="false">

        <div class="dashboard-card">

            <div class="os-wrap">

                <div class="os-left">

                    <div class="os-detail-row">
                        <span class="os-detail-label">Shop Name :</span>
                        <span class="os-detail-value">
                            <asp:Label ID="lblShopName" runat="server" Text="-"></asp:Label>
                        </span>
                    </div>

                    <div class="os-detail-row">
                        <span class="os-detail-label">Order Date :</span>
                        <span class="os-detail-value">
                            <asp:Label ID="lblPlacedOn" runat="server" Text="-"></asp:Label>
                        </span>
                    </div>

                    <div class="os-detail-row">
                        <span class="os-detail-label">Total Amount :</span>
                        <span class="os-detail-value">
                            &#8377;<asp:Label ID="lblTotalAmountSuccess" runat="server" Text="0.00"></asp:Label>
                        </span>
                    </div>

                    <div class="os-detail-row">
                        <span class="os-detail-label">Payment Method :</span>
                        <span class="os-detail-value">
                            <asp:Label ID="lblPaymentMethod" runat="server" Text="Cash"></asp:Label>
                        </span>
                    </div>

                    <div class="os-detail-row">
                        <span class="os-detail-label">Collated By :</span>
                        <span class="os-detail-value">
                            <asp:Label ID="lblCollatedBy" runat="server" Text="-"></asp:Label>
                        </span>
                    </div>

            <div class="or-records" style="margin-left:auto; font-weight:600; color:#6b7280;">
                <asp:Label ID="lblRecordCount" runat="server" Text="records: 0"></asp:Label>
            </div>

                </div>


                <div class="os-right">

                    <div class="os-card">

                        <div class="os-confetti">
                            <i style="background:#2f6bff; top:10%; left:5%;"></i>
                            <i style="background:#ff4d8d; top:8%; right:8%;"></i>
                            <i style="background:#ffb020; top:22%; left:3%;"></i>
                            <i style="background:#ff4d8d; top:30%; right:5%;"></i>
                            <i style="background:#7c5cff; top:48%; left:4%;"></i>
                            <i style="background:#2f6bff; top:55%; right:6%;"></i>
                            <i style="background:#ffc107; top:70%; left:8%;"></i>
                            <i style="background:#ff4d8d; top:78%; right:10%;"></i>
                            <i style="background:#20c997; top:40%; left:12%;"></i>
                            <i style="background:#ffb020; top:15%; right:18%;"></i>
                            <i style="background:#7c5cff; top:62%; right:20%;"></i>
                            <i style="background:#2f6bff; top:88%; left:30%;"></i>
                        </div>

                        <div class="os-check">
                            <i class="bi bi-check-lg"></i>
                        </div>

                        <h3>Order Completed!</h3>

                        <p>Your order has been placed successfully.</p>

                        <span class="os-badge-order">
                            Order #<asp:Label ID="lblOrderNumber" runat="server" Text="-"></asp:Label>
                        </span>

                        <div class="os-total">
                            &#8377;<asp:Label ID="lblTotalAmount" runat="server" Text="0.00"></asp:Label>
                        </div>

                        <div class="os-actions">

                            <a runat="server"
                               id="lnkViewOrder"
                               href="#"
                               class="os-btn os-btn-outline">
                                <i class="bi bi-eye"></i> View Order
                            </a>

                            <a href="CreateOrder.aspx"
                               class="os-btn os-btn-primary">
                                <i class="bi bi-plus-lg"></i> New Order
                            </a>

                        </div>

                    </div>

                </div>

            </div>

        </div>

    </asp:Panel>


    <!-- ============================================================
         ORDERS LIST
         ============================================================ -->
    <asp:Panel ID="pnlList" runat="server">

        <!-- Page Header -->
        <div class="or-header">
            <a class="or-back" href="Dashboard.aspx" title="Back">
                <i class="bi bi-arrow-left"></i>
            </a>
            <h2>My Orders</h2>
        </div>


        <!-- Message -->
        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block mb-4">
        </asp:Label>


        <!-- Search + Filter Toolbar -->
        <div class="or-toolbar">

            <div class="or-search">
                <i class="bi bi-search or-search-icon"></i>
                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Search order number, shop..."
                    AutoPostBack="true"
                    OnTextChanged="FilterChanged">
                </asp:TextBox>
            </div>

            <div class="or-filter">
                <span class="or-filter-label">Status</span>
                <asp:DropDownList
                    ID="ddlStatus"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">

                    <asp:ListItem Text="All" Value="" Selected="True"></asp:ListItem>
                    <asp:ListItem Text="Pending" Value="Pending"></asp:ListItem>
                    <asp:ListItem Text="Confirmed" Value="Confirmed"></asp:ListItem>
                    <asp:ListItem Text="Dispatched" Value="Dispatched"></asp:ListItem>
                    <asp:ListItem Text="Delivered" Value="Delivered"></asp:ListItem>
                    <asp:ListItem Text="Cancelled" Value="Cancelled"></asp:ListItem>

                </asp:DropDownList>
            </div>

        </div>


        <!-- Orders Table -->
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
                    PagerStyle-HorizontalAlign="Center"
                    ShowHeaderWhenEmpty="true">

                    <Columns>

                        <asp:TemplateField HeaderText="Order ID" ItemStyle-CssClass="or-col-id">
                            <ItemTemplate>
                                <span data-label="Order ID"><%# Eval("OrderNumber") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Shop" ItemStyle-CssClass="or-col-shop">
                            <ItemTemplate>
                                <span data-label="Shop"><%# Eval("ShopName") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Date" ItemStyle-CssClass="or-col-date">
                            <ItemTemplate>
                                <span data-label="Date"><%# Eval("OrderDateText") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Items" ItemStyle-CssClass="or-col-items">
                            <ItemTemplate>
                                <span data-label="Items"><%# Eval("ItemCount") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Amount" ItemStyle-CssClass="or-col-amount">
                            <ItemTemplate>
                                <span class="or-amount" data-label="Amount"><%# Eval("AmountText") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Status" ItemStyle-CssClass="or-col-status">
                            <ItemTemplate>
                                <span class="or-badge <%# Eval("StatusCss") %>" data-label="Status">
                                    <%# Eval("Status") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="" ItemStyle-CssClass="or-col-action">
                            <ItemTemplate>
                                <a href='<%# "Bill.aspx?orderId=" + Eval("OrderId") %>'
                                   class="or-view-btn"
                                   title="View Details">
                                    <i class="bi bi-eye"></i>
                                </a>
                            </ItemTemplate>
                        </asp:TemplateField>

                    </Columns>

                    <EmptyDataTemplate>
                        <div class="or-empty">
                            <i class="bi bi-cart-check"></i>
                            <h3>No orders yet</h3>
                            <p>You haven't placed any orders. Start by creating your first order.</p>
                            <a href="CreateOrder.aspx" class="os-btn os-btn-primary or-btn-primary">
                                <i class="bi bi-plus-lg"></i> Create Order
                            </a>
                        </div>
                    </EmptyDataTemplate>

                </asp:GridView>
            </div>
        </div>


        <!-- Pagination is handled by GridView PagerStyle -->

    </asp:Panel>

</asp:Content>