<%@ Page Title="Vehicle Stock"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="VehicleStock.aspx.cs"
    Inherits="BWDMS.Dealer.VehicleStock" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Vehicle Stock

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <style>

        .vs-head
        {
            margin  : 8px 0 24px 0;
        }

        .vs-title
        {
            font-size  : 24px;
            font-weight: 700;
            color      : #111827;
            margin     : 0 0 4px 0;
        }

        .vs-subtitle
        {
            font-size  : 13px;
            color      : #9ca3af;
            margin     : 0;
        }


        .vs-filter
        {
            margin  : 0 0 22px 0;
        }

        .vs-dropdown
        {
            min-width : 240px;
            padding   : 12px 40px 12px 16px;
            font-size : 16px;
            color     : #111827;
            border    : 1px solid #d1d5db;
            border-radius: 6px;
            background: #fff url("data:image/svg+xml;charset=utf8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16'%3E%3Cpath fill='%236b7280' d='M8 11L3 6h10z'/%3E%3C/svg%3E") no-repeat right 14px center;
            appearance: none;
            -webkit-appearance: none;
        }

        .vs-dropdown:focus
        {
            outline      : none;
            border-color : #6366f1;
        }


        .vs-table-wrap
        {
            border       : 1px solid #d1d5db;
            border-radius: 6px;
            overflow     : hidden;
            background   : #fff;
        }

        .vs-table
        {
            width          : 100%;
            border-collapse: collapse;
        }

        .vs-table th
        {
            background   : #d9d9d9;
            font-size    : 17px;
            font-weight  : 700;
            color        : #111827;
            text-align   : left;
            padding      : 18px 24px;
            border-bottom: 1px solid #c4c4c4;
        }

        .vs-table td
        {
            font-size  : 15px;
            color      : #374151;
            padding    : 20px 24px;
            border-bottom: 1px solid #ececec;
        }

        .vs-table tr:last-child td
        {
            border-bottom: none;
        }

        .vs-num
        {
            text-align: center;
        }


        .vs-pager
        {
            display      : flex;
            justify-content: center;
            align-items  : center;
            gap          : 10px;
            margin       : 26px 0 10px 0;
        }

        .vs-page-btn
        {
            min-width     : 34px;
            height        : 34px;
            padding       : 0 10px;
            display       : inline-flex;
            align-items   : center;
            justify-content: center;
            border        : 1px solid #d1d5db;
            border-radius : 6px;
            background    : #fff;
            color         : #374151;
            font-size     : 14px;
            text-decoration: none;
            cursor        : pointer;
        }

        .vs-page-btn:hover
        {
            background: #f3f4f6;
        }

        .vs-page-btn.active
        {
            background: #c7d2fe;
            border-color: #c7d2fe;
            color     : #111827;
        }

        .vs-page-btn.disabled
        {
            opacity   : 0.5;
            pointer-events: none;
        }

    </style>


    <!-- Header -->

    <div class="vs-head">

        <h3 class="vs-title">Vehicle Stock</h3>

        <p class="vs-subtitle">Manage All Stock movement</p>

    </div>


    <!-- Vehicle filter -->

    <div class="vs-filter">

        <asp:DropDownList
            ID="ddlVehicle"
            runat="server"
            CssClass="vs-dropdown"
            AutoPostBack="true"
            OnSelectedIndexChanged="ddlVehicle_SelectedIndexChanged">
        </asp:DropDownList>

    </div>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block mb-3">
    </asp:Label>


    <!-- Stock table -->

    <div class="vs-table-wrap">

        <table class="vs-table">

            <thead>

                <tr>
                    <th>Product</th>
                    <th class="vs-num">Opening Stock</th>
                    <th class="vs-num">Loaded</th>
                    <th class="vs-num">Sold</th>
                    <th class="vs-num">Return</th>
                    <th class="vs-num">Current Stock</th>
                </tr>

            </thead>

            <tbody>

                <asp:Repeater
                    ID="rptStock"
                    runat="server">

                    <ItemTemplate>

                        <tr>
                            <td><%# Eval("ProductName") %></td>
                            <td class="vs-num"><%# Eval("OpeningStock") %></td>
                            <td class="vs-num"><%# Eval("Loaded") %></td>
                            <td class="vs-num"><%# Eval("Sold") %></td>
                            <td class="vs-num"><%# Eval("Return") %></td>
                            <td class="vs-num"><%# Eval("CurrentStock") %></td>
                        </tr>

                    </ItemTemplate>

                </asp:Repeater>

                <tr id="rowNoData" runat="server" visible="false">

                    <td colspan="6" class="text-center text-muted py-4">

                        No stock found for this vehicle.

                    </td>

                </tr>

            </tbody>

        </table>

    </div>


    <!-- Pagination -->

    <div class="vs-pager">

        <asp:LinkButton
            ID="lnkPrev"
            runat="server"
            CssClass="vs-page-btn"
            OnClick="lnkPrev_Click">
            &lt;
        </asp:LinkButton>

        <asp:Repeater
            ID="rptPages"
            runat="server"
            OnItemCommand="rptPages_ItemCommand">

            <ItemTemplate>

                <asp:LinkButton
                    ID="lnkPage"
                    runat="server"
                    CssClass='<%# "vs-page-btn" + (Convert.ToInt32(Eval("Value")) == CurrentPage ? " active" : "") %>'
                    CommandName="Page"
                    CommandArgument='<%# Eval("Value") %>'
                    Text='<%# Eval("Text") %>'>
                </asp:LinkButton>

            </ItemTemplate>

        </asp:Repeater>

        <asp:LinkButton
            ID="lnkNext"
            runat="server"
            CssClass="vs-page-btn"
            OnClick="lnkNext_Click">
            &gt;
        </asp:LinkButton>

    </div>

</asp:Content>
