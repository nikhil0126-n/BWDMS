<%@ Page Title="Order Details"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="OrderDetails.aspx.cs"
    Inherits="BWDMS.Dealer.OrderDetails" %>

<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Order Details

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <style>

        .od-back-link
        {
            display        : inline-flex;
            align-items    : center;
            gap            : 8px;
            font-size      : 20px;
            font-weight    : 700;
            color          : #111827;
            text-decoration: none;
            margin         : 10px 0 26px 40px;
        }

        .od-back-link:hover
        {
            color       : #2563eb;
            text-decoration: none;
        }


        .od-title-row
        {
            display  : flex;
            align-items: center;
            gap      : 18px;
            margin   : 0 0 22px 40px;
        }

        .od-title
        {
            font-size  : 22px;
            font-weight: 700;
            color      : #111827;
            margin     : 0;
        }

        .od-status
        {
            font-size  : 16px;
            font-weight: 500;
            color      : #16a34a;
        }


        .od-info
        {
            margin  : 0 0 30px 40px;
            display : grid;
            grid-template-columns: 130px 1fr;
            row-gap : 14px;
            font-size  : 15px;
            color      : #111827;
        }

        .od-info .od-label
        {
            font-weight: 700;
            color      : #111827;
        }

        .od-info .od-value
        {
            font-weight: 400;
            color      : #1f2937;
        }


        .od-table-wrap
        {
            border       : 1px solid #d1d5db;
            border-radius: 4px;
            overflow     : hidden;
            margin       : 0 20px 20px 20px;
        }

        .od-table
        {
            width       : 100%;
            border-collapse: collapse;
        }

        .od-table th
        {
            background   : #d9d9d9;
            font-size    : 18px;
            font-weight  : 700;
            color        : #111827;
            text-align   : left;
            padding      : 16px 24px;
            border-bottom: 1px solid #c4c4c4;
        }

        .od-table td
        {
            font-size  : 15px;
            color      : #374151;
            padding    : 18px 24px;
            border-bottom: 1px solid #ececec;
        }

        .od-table tr:last-child td
        {
            border-bottom: none;
        }

        .od-table .od-unit
        {
            font-weight: 700;
            color      : #111827;
        }

        .od-num
        {
            text-align: center;
        }

        .od-amt
        {
            text-align: right;
        }

    </style>


    <!-- Back link -->

    <a href="Orders.aspx" class="od-back-link">
        <i class="bi bi-arrow-left"></i>
        Back to&nbsp; Order
    </a>


    <!-- Order number + status -->

    <div class="od-title-row">

        <h2 class="od-title">
            Order
            #
            <asp:Label
                ID="lblOrderNumber"
                runat="server"
                Text="-">
            </asp:Label>
        </h2>

        <asp:Label
            ID="lblStatus"
            runat="server"
            CssClass="od-status"
            Text="-">
        </asp:Label>

    </div>


    <!-- Order info -->

    <div class="od-info">

        <div class="od-label">Shop&nbsp; :</div>
        <div class="od-value">
            <asp:Label
                ID="lblShopName"
                runat="server"
                Text="-">
            </asp:Label>
        </div>

        <div class="od-label">Salesman&nbsp; :</div>
        <div class="od-value">
            <asp:Label
                ID="lblSalesman"
                runat="server"
                Text="-">
            </asp:Label>
        </div>

        <div class="od-label">Date&nbsp; :</div>
        <div class="od-value">
            <asp:Label
                ID="lblDate"
                runat="server"
                Text="-">
            </asp:Label>
        </div>

        <div class="od-label">Payment&nbsp; :</div>
        <div class="od-value">
            <asp:Label
                ID="lblPayment"
                runat="server"
                Text="-">
            </asp:Label>
        </div>

        <div class="od-label">Collection&nbsp; :</div>
        <div class="od-value">
            <asp:Label
                ID="lblCollection"
                runat="server"
                Text="-">
            </asp:Label>
        </div>

    </div>


    <!-- Lines -->

    <div class="od-table-wrap">

        <table class="od-table">

            <thead>

                <tr>
                    <th>Product</th>
                    <th>Unit</th>
                    <th class="od-num">Qty</th>
                    <th>Rate</th>
                    <th class="od-amt">Amount</th>
                </tr>

            </thead>

            <tbody>

                <asp:Repeater
                    ID="rptLines"
                    runat="server">

                    <ItemTemplate>

                        <tr>
                            <td><%# Eval("ProductName") %></td>
                            <td class="od-unit"><%# Eval("Unit") %></td>
                            <td class="od-num"><%# Eval("Qty") %></td>
                            <td>₹<%# Eval("Rate") %></td>
                            <td class="od-amt">₹<%# Eval("Amount") %></td>
                        </tr>

                    </ItemTemplate>

                </asp:Repeater>


                <asp:Label
                    ID="lblNoLines"
                    runat="server"
                    Visible="false"
                    Text="No products in this order.">
                </asp:Label>

            </tbody>

        </table>

    </div>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>

</asp:Content>
