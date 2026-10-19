<%@ Page Title="Orders"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Orders.aspx.cs"
    Inherits="BWDMS.Dealer.Orders" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Orders

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <style>
        /* Page shell matching the Orders screen design */
        .orders-card {
            background: #ffffff;
            border: 1px solid #d5d5da;
            border-radius: 10px;
        }

        .orders-card table {
            margin-bottom: 0;
        }

        .orders-card thead th {
            background: #e6e6e6;
            color: #222222;
            font-size: 14px;
            font-weight: 600;
            padding: 15px 22px;
            border-bottom: none;
        }

        .orders-card tbody td {
            padding: 18px 22px;
            font-size: 14px;
            color: #333333;
            border-top: 1px solid #f0f0f0;
            vertical-align: middle;
        }

        .orders-card tbody tr:first-child td {
            border-top: none;
        }

        .order-id,
        .orders-card tbody td:first-child {
            font-size: 15px;
            font-weight: 700;
            color: #111111;
        }

        /* Compact pager row under the table */
        .orders-pager td {
            text-align: center;
            padding: 12px 0;
            border-top: 1px solid #f0f0f0;
        }

        .orders-pager a {
            min-width: 30px;
            height: 30px;
            margin: 0 3px;
            padding: 0 8px;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            border: 1px solid #d5d5da;
            border-radius: 6px;
            background: #ffffff;
            color: #333333;
            font-size: 13px;
            text-decoration: none;
        }

        .orders-pager span {
            min-width: 30px;
            height: 30px;
            margin: 0 3px;
            padding: 0 8px;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            border: 1px solid #5b6ef5;
            border-radius: 6px;
            background: #5b6ef5;
            color: #ffffff;
            font-size: 13px;
        }
    </style>


    <!-- Page Header -->

    <div class="mb-4">

        <h3 class="fw-bold mb-1 page-title">
            Orders
        </h3>

        <p class="text-muted mb-0 page-subtitle">
            Manage all vehicles
        </p>

    </div>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <!-- Search -->

    <div class="grid-search-box mb-4">

        <i class="bi bi-search search-icon"></i>

        <asp:TextBox
            ID="txtSearch"
            runat="server"
            CssClass="form-control"
            placeholder="Search Orders"
            AutoPostBack="true"
            OnTextChanged="txtSearch_TextChanged">
        </asp:TextBox>

    </div>


    <!-- Filters (kept for behaviour, hidden to match the design) -->

    <div class="d-none">

        <asp:DropDownList
            ID="ddlStatus"
            runat="server"
            AutoPostBack="true"
            OnSelectedIndexChanged="FilterChanged">

            <asp:ListItem Text="All Statuses" Value="" Selected="True">
            </asp:ListItem>

            <asp:ListItem Text="Pending" Value="Pending">
            </asp:ListItem>

            <asp:ListItem Text="Confirmed" Value="Confirmed">
            </asp:ListItem>

            <asp:ListItem Text="Dispatched" Value="Dispatched">
            </asp:ListItem>

            <asp:ListItem Text="Cancelled" Value="Cancelled">
            </asp:ListItem>

        </asp:DropDownList>


        <asp:DropDownList
            ID="ddlSource"
            runat="server"
            AutoPostBack="true"
            OnSelectedIndexChanged="FilterChanged">

            <asp:ListItem Text="All Sources" Value="" Selected="True">
            </asp:ListItem>

            <asp:ListItem Text="Beat" Value="Beat">
            </asp:ListItem>

            <asp:ListItem Text="Telephone" Value="Telephone">
            </asp:ListItem>

            <asp:ListItem Text="Counter" Value="Counter">
            </asp:ListItem>

        </asp:DropDownList>

    </div>


        <div class="table-responsive orders-card">

            <asp:GridView
                ID="gvOrders"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table orders-grid align-middle mb-0"
                GridLines="None"
                OnRowCommand="gvOrders_RowCommand"
                DataKeyNames="OrderId" AllowPaging="true" OnPageIndexChanging="gvOrders_PageIndexChanging" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="Bottom" PagerStyle-CssClass="orders-pager">

                <Columns>

                    <asp:BoundField
                        DataField="OrderNumber"
                        HeaderText="Order No." />

                    <asp:BoundField
                        DataField="OrderDateText"
                        HeaderText="Order Date" />

                    <asp:BoundField
                        DataField="DeliveryDateText"
                        HeaderText="Delivery" />

                    <asp:BoundField
                        DataField="ShopName"
                        HeaderText="Shop" />

                    <asp:BoundField
                        DataField="SourceText"
                        HeaderText="Order Source" />

                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Eval("StatusCss") %>'>

                                <%# Eval("Status") %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:BoundField
                        DataField="ItemCount"
                        HeaderText="Lines" />

                    <asp:BoundField
                        DataField="GrandTotalText"
                        HeaderText="Grand Total"
                        DataFormatString="{0:N2}" />

                    <asp:TemplateField
                        HeaderText="Action">

                        <ItemTemplate>

                            <asp:HyperLink
                                ID="lnkView"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-primary"
                                NavigateUrl='<%# "~/Dealer/OrderDetails.aspx?id=" + Eval("OrderId") %>'>

                                <i class="bi bi-eye"></i>

                                View

                            </asp:HyperLink>


                            <asp:HyperLink
                                ID="lnkEdit"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-danger"
                                NavigateUrl='<%# "~/Dealer/AddOrder.aspx?id=" + Eval("OrderId") %>'>

                                <i class="bi bi-pencil"></i>

                                Edit

                            </asp:HyperLink>


                            <asp:LinkButton
                                ID="lnkCancel"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-secondary"
                                CommandName="CancelOrder"
                                CommandArgument='<%# Eval("OrderId") %>'
                                OnClientClick="return confirm('Cancel this order? Stock does not return to the godown unless it was dispatched.');">

                                <i class="bi bi-x-circle"></i>

                                Cancel

                            </asp:LinkButton>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-cart-check fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No orders found. Use "New Order" to place one.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>


</asp:Content>
