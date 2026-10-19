<%@ Page Title="Warehouse Stock"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Inventory.aspx.cs"
    Inherits="BWDMS.Dealer.Inventory" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Warehouse Stock

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <style>

        /* Page title block */
        .wh-head { margin: 6px 0 24px 0; }

        .wh-title {
            font-size   : 26px;
            font-weight : 700;
            color       : #111827;
            margin      : 0 0 4px 0;
        }

        .wh-subtitle {
            font-size : 13px;
            color     : #9ca3af;
            margin    : 0;
        }

        /* Search box */
        .wh-search {
            display     : flex;
            align-items : center;
            gap         : 12px;
            max-width   : 420px;
            padding     : 14px 18px;
            border      : 1px solid #d1d5db;
            border-radius : 10px;
            background  : #ffffff;
            margin      : 0 0 26px 0;
        }

        .wh-search i { color: #6b7280; font-size: 18px; }

        .wh-search .form-control {
            border     : none;
            padding    : 0;
            font-size  : 15px;
            color      : #111827;
            background : transparent;
            box-shadow : none;
        }

        .wh-search .form-control:focus {
            box-shadow : none;
            background : transparent;
        }

        .wh-search .form-control::placeholder { color: #6b7280; }

        /* Table card */
        .wh-card {
            background   : #ffffff;
            border       : 1px solid #d1d5db;
            border-radius: 10px;
            overflow     : hidden;
        }

        .wh-card table { margin-bottom: 0; }

        .wh-card thead th {
            background   : #d9d9d9;
            color        : #111827;
            font-size    : 17px;
            font-weight  : 700;
            text-align   : left;
            padding      : 18px 24px;
            border-bottom: 1px solid #c4c4c4;
        }

        .wh-card tbody td {
            padding      : 20px 24px;
            font-size    : 15px;
            color        : #374151;
            border-bottom: 1px solid #ececec;
            vertical-align: middle;
        }

        .wh-card tbody tr:last-child td { border-bottom: none; }

        .wh-card tbody td:first-child {
            font-size : 16px;
            color     : #111827;
        }

        .col-right { text-align: right; }

        /* Pager */
        .wh-pager td {
            text-align : center;
            padding    : 14px 0;
            border-top : 1px solid #ececec;
        }

        .wh-pager a {
            min-width      : 34px;
            height         : 34px;
            margin         : 0 4px;
            padding        : 0 10px;
            display        : inline-flex;
            align-items    : center;
            justify-content: center;
            border         : 1px solid #d1d5db;
            border-radius  : 6px;
            background     : #ffffff;
            color          : #374151;
            font-size      : 14px;
            text-decoration: none;
        }

        .wh-pager a:hover { background: #f3f4f6; }

        .wh-pager span {
            min-width      : 34px;
            height         : 34px;
            margin         : 0 4px;
            padding        : 0 10px;
            display        : inline-flex;
            align-items    : center;
            justify-content: center;
            border         : 1px solid #6366f1;
            border-radius  : 6px;
            background     : #6366f1;
            color          : #ffffff;
            font-size      : 14px;
        }

    </style>


    <!-- Page Header -->

    <div class="wh-head">

        <h3 class="wh-title">
            Warehouse Stock
        </h3>

        <p class="wh-subtitle">
            Manage All Vehicles
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

    <div class="wh-search">

        <i class="bi bi-search"></i>

        <asp:TextBox
            ID="txtSearch"
            runat="server"
            CssClass="form-control"
            placeholder="Search Product"
            AutoPostBack="true"
            OnTextChanged="txtSearch_TextChanged">
        </asp:TextBox>

    </div>


    <!-- Stock Filter (kept for behaviour, hidden to match the design) -->

    <div class="d-none">

        <asp:DropDownList
            ID="ddlStockFilter"
            runat="server"
            AutoPostBack="true"
            OnSelectedIndexChanged="FilterChanged">

            <asp:ListItem Text="All Stock" Value="" Selected="True">
            </asp:ListItem>

            <asp:ListItem Text="Low / Out of Stock" Value="low">
            </asp:ListItem>

            <asp:ListItem Text="Out of Stock" Value="out">
            </asp:ListItem>

            <asp:ListItem Text="In Stock" Value="ok">
            </asp:ListItem>

        </asp:DropDownList>

    </div>


    <!-- Stock List -->

    <div class="table-responsive wh-card">

            <asp:GridView
                ID="gvInventory"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table align-middle mb-0"
                GridLines="None"
                DataKeyNames="InventoryId"
                AllowPaging="true"
                OnPageIndexChanging="gvInventory_PageIndexChanging"
                PageSize="5"
                PagerSettings-Mode="NumericFirstLast"
                PagerSettings-PageButtonCount="4"
                PagerSettings-Position="Bottom"
                PagerStyle-CssClass="wh-pager">

                <Columns>

                    <asp:BoundField
                        DataField="ProductName"
                        HeaderText="Product" />

                    <asp:BoundField
                        DataField="BoxText"
                        HeaderText="Box" />

                    <asp:BoundField
                        DataField="PantiText"
                        HeaderText="Panti" />

                    <asp:BoundField
                        DataField="PacketText"
                        HeaderText="Packet"
                        ItemStyle-CssClass="col-right"
                        HeaderStyle-CssClass="col-right" />

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-box-seam fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No stock found.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

    </div>


</asp:Content>
