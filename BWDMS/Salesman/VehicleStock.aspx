<%@ Page Title="Vehicle Stock"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="VehicleStock.aspx.cs"
    Inherits="BWDMS.Salesman.VehicleStock" %>


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
            font-weight : 700;
            color      : #111827;
            margin     : 0 0 4px 0;
        }

        .vs-subtitle
        {
            font-size : 13px;
            color     : #9ca3af;
            margin    : 0;
        }


        .vs-filter
        {
            display        : flex;
            flex-wrap      : wrap;
            gap            : 12px;
            align-items    : flex-end;
            margin         : 0 0 22px 0;
        }

        .vs-field
        {
            display   : flex;
            flex-direction : column;
            gap       : 5px;
        }

        .vs-field-label
        {
            font-size : 12px;
            color     : #6b7280;
        }

        .vs-dropdown,
        .vs-input
        {
            min-width   : 240px;
            padding     : 12px 40px 12px 16px;
            font-size   : 15px;
            color       : #111827;
            border      : 1px solid #d1d5db;
            border-radius: 6px;
            background  : #fff;
        }

        .vs-dropdown
        {
            background-image: url("data:image/svg+xml;charset=utf8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16'%3E%3Cpath fill='%236b7280' d='M8 11L3 6h10z'/%3E%3C/svg%3E");
            background-repeat: no-repeat;
            background-position: right 14px center;
            appearance    : none;
            -webkit-appearance: none;
            padding-right : 40px;
        }

        .vs-input
        {
            padding-right : 16px;
        }

        .vs-dropdown:focus,
        .vs-input:focus
        {
            outline      : none;
            border-color : #6366f1;
        }


        .vs-table-wrap
        {
            border         : 1px solid #d1d5db;
            border-radius  : 6px;
            overflow       : hidden;
            background     : #fff;
        }

        .vs-table
        {
            width          : 100%;
            border-collapse: collapse;
        }

        .vs-table th
        {
            background     : #d9d9d9;
            font-size      : 17px;
            font-weight    : 700;
            color          : #111827;
            text-align     : left;
            padding        : 18px 24px;
            border-bottom  : 1px solid #c4c4c4;
        }

        .vs-table td
        {
            font-size      : 15px;
            color          : #374151;
            padding        : 16px 24px;
            border-bottom  : 1px solid #ececec;
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
            display         : flex;
            justify-content : center;
            align-items     : center;
            gap             : 10px;
            margin          : 26px 0 10px 0;
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

        .vs-table > tbody > tr > td
        {
            border-bottom: 1px solid #ececec;
        }

    </style>


    <!-- Header -->

    <div class="vs-head">

        <h3 class="vs-title">Vehicle Stock</h3>

        <p class="vs-subtitle">Manage All Stock movement</p>

    </div>


    <!-- Filters -->

    <div class="vs-filter">

        <div class="vs-field">

            <label class="vs-field-label">From</label>

            <asp:TextBox
                ID="txtFromDate"
                runat="server"
                CssClass="vs-input"
                placeholder="YYYY-MM-DD"
                AutoPostBack="true"
                OnTextChanged="FilterChanged">
            </asp:TextBox>

        </div>


        <div class="vs-field">

            <label class="vs-field-label">To</label>

            <asp:TextBox
                ID="txtToDate"
                runat="server"
                CssClass="vs-input"
                placeholder="YYYY-MM-DD"
                AutoPostBack="true"
                OnTextChanged="FilterChanged">
            </asp:TextBox>

        </div>


        <div class="vs-field">

            <label class="vs-field-label">Reconciled</label>

            <asp:DropDownList
                ID="ddlReconciled"
                runat="server"
                CssClass="vs-dropdown"
                AutoPostBack="true"
                OnSelectedIndexChanged="FilterChanged">

                <asp:ListItem Text="All" Value="" Selected="True">
                </asp:ListItem>

                <asp:ListItem Text="Pending" Value="0">
                </asp:ListItem>

                <asp:ListItem Text="Reconciled" Value="1">
                </asp:ListItem>

            </asp:DropDownList>

        </div>


        <div class="vs-field">

            <label class="vs-field-label">Search</label>

            <asp:TextBox
                ID="txtSearch"
                runat="server"
                CssClass="vs-input"
                placeholder="Search vehicle or product..."
                AutoPostBack="true"
                OnTextChanged="FilterChanged">
            </asp:TextBox>

        </div>

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

        <div class="table-responsive">

            <asp:GridView
                ID="gvStock"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="vs-table"
                GridLines="None"
                AllowPaging="true"
                OnPageIndexChanging="gvStock_PageIndexChanging"
                PageSize="25"
                PagerSettings-Mode="NumericFirstLast"
                PagerSettings-PageButtonCount="10"
                PagerSettings-Position="Bottom"
                PagerStyle-CssClass="vs-pager"
                PagerStyle-HorizontalAlign="Center">

                <Columns>

                    <asp:BoundField
                        DataField="StockDateText"
                        HeaderText="Date" />

                    <asp:BoundField
                        DataField="VehicleNumber"
                        HeaderText="Vehicle" />

                    <asp:BoundField
                        DataField="ProductName"
                        HeaderText="Product" />

                    <asp:BoundField
                        DataField="VariantName"
                        HeaderText="Variant" />

                    <asp:BoundField
                        DataField="LoadedPackets"
                        HeaderText="Loaded"
                        HeaderStyle-CssClass="vs-num"
                        ItemStyle-CssClass="vs-num" />

                    <asp:BoundField
                        DataField="SoldPackets"
                        HeaderText="Sold"
                        HeaderStyle-CssClass="vs-num"
                        ItemStyle-CssClass="vs-num" />

                    <asp:BoundField
                        DataField="ReturnedPackets"
                        HeaderText="Return"
                        HeaderStyle-CssClass="vs-num"
                        ItemStyle-CssClass="vs-num" />

                    <asp:BoundField
                        DataField="DamagedPackets"
                        HeaderText="Damaged"
                        HeaderStyle-CssClass="vs-num"
                        ItemStyle-CssClass="vs-num" />

                    <asp:BoundField
                        DataField="ActualClosingText"
                        HeaderText="Current Stock"
                        HeaderStyle-CssClass="vs-num"
                        ItemStyle-CssClass="vs-num" />

                    <asp:TemplateField
                        HeaderText="Variance">

                        <HeaderStyle CssClass="vs-num" />
                        <ItemStyle CssClass="vs-num" />

                        <ItemTemplate>

                            <%# Eval("VarianceText") %>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Convert.ToBoolean(Eval("IsReconciled")) ? "badge bg-success" : "badge bg-warning text-dark" %>'>

                                <%# Convert.ToBoolean(Eval("IsReconciled")) ? "Reconciled" : "Pending" %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <p class="text-muted mb-0">

                            No stock found for this vehicle.

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

</asp:Content>
