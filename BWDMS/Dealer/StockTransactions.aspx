<%@ Page Title="Stock Movement"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="StockTransactions.aspx.cs"
    Inherits="BWDMS.Dealer.StockTransactions" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Stock Movement

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <style>

        .vs-head
        {
            margin : 8px 0 24px 0;
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
            font-size : 13px;
            color     : #9ca3af;
            margin    : 0;
        }


        .vs-search
        {
            position     : relative;
            margin       : 0 0 22px 0;
            max-width    : 380px;
        }

        .vs-search i
        {
            position : absolute;
            left     : 14px;
            top      : 50%;
            transform: translateY(-50%);
            color    : #6b7280;
        }

        .vs-search input
        {
            width       : 100%;
            padding     : 13px 16px 13px 44px;
            font-size   : 15px;
            color       : #111827;
            border      : 1px solid #d1d5db;
            border-radius: 8px;
            background  : #fff;
        }

        .vs-search input:focus
        {
            outline      : none;
            border-color : #6366f1;
        }


        .vs-table-wrap
        {
            border        : 1px solid #d1d5db;
            border-radius : 6px;
            overflow      : hidden;
            background    : #fff;
        }

        .vs-table
        {
            width          : 100%;
            border-collapse: collapse;
        }

        .vs-table th
        {
            background    : #d9d9d9;
            font-size     : 17px;
            font-weight   : 700;
            color         : #111827;
            text-align    : left;
            padding       : 18px 24px;
            border-bottom : 1px solid #c4c4c4;
        }

        .vs-table td
        {
            font-size     : 15px;
            color         : #374151;
            padding       : 20px 24px;
            border-bottom : 1px solid #ececec;
        }

        .vs-num
        {
            text-align: center;
        }


        .vs-pill
        {
            display        : inline-block;
            padding        : 6px 16px;
            border-radius  : 999px;
            background     : #f3f4f6;
            color          : #374151;
            font-size      : 13px;
            font-weight    : 600;
        }


        .vs-pager
        {
            display         : flex;
            justify-content : center;
            align-items     : center;
            gap             : 10px;
            margin          : 26px 0 10px 0;
        }

        .vs-pager table
        {
            margin : 0 auto;
        }

        .vs-page-btn
        {
            min-width      : 34px;
            height         : 34px;
            padding        : 0 10px;
            display        : inline-flex;
            align-items    : center;
            justify-content: center;
            border         : 1px solid #d1d5db;
            border-radius  : 6px;
            background     : #fff;
            color          : #374151;
            font-size      : 14px;
            text-decoration: none;
            cursor         : pointer;
            margin         : 0 4px;
        }

        .vs-page-btn:hover
        {
            background: #f3f4f6;
        }

        .vs-page-btn.active
        {
            background  : #c7d2fe;
            border-color: #c7d2fe;
            color       : #111827;
        }

    </style>


    <!-- Page Header -->

    <div class="vs-head">

        <h3 class="vs-title">Stock Movement</h3>

        <p class="vs-subtitle">Manage All Stock movement</p>

    </div>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block mb-3">
    </asp:Label>


    <!-- Search -->

    <div class="vs-search">

        <i class="bi bi-search"></i>

        <asp:TextBox
            ID="txtSearch"
            runat="server"
            placeholder="Search Stock"
            AutoPostBack="true"
            OnTextChanged="txtSearch_TextChanged">
        </asp:TextBox>

    </div>


    <!-- Ledger -->

    <div class="vs-table-wrap">

        <div class="table-responsive">

            <asp:GridView
                ID="gvTransactions"
                runat="server"
                ClientIDMode="Static"
                AutoGenerateColumns="False"
                CssClass="vs-table"
                GridLines="None"
                EmptyDataText="No stock transactions yet."
                AllowPaging="true"
                OnPageIndexChanging="gvTransactions_PageIndexChanging"
                PageSize="10"
                PagerSettings-Mode="NumericFirstLast"
                PagerSettings-PageButtonCount="10"
                PagerSettings-Position="Bottom"
                PagerStyle-CssClass="vs-pager">

                <Columns>

                    <asp:BoundField
                        DataField="TransactionDateText"
                        HeaderText="Dates"
                        DataFormatString="{0:dd MMM yyyy}" />

                    <asp:TemplateField
                        HeaderText="Type">

                        <ItemTemplate>

                            <span class="vs-pill">

                                <%# Eval("TransactionType") %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>

                    <asp:BoundField
                        DataField="ReferenceNo"
                        HeaderText="Ref.No" />

                    <asp:BoundField
                        DataField="ProductName"
                        HeaderText="Product" />

                    <asp:BoundField
                        DataField="UnitName"
                        HeaderText="Unit" />

                    <asp:TemplateField
                        HeaderText="Qty">

                        <HeaderStyle CssClass="vs-num" />
                        <ItemStyle CssClass="vs-num" />

                        <ItemTemplate>

                            <span class='<%# Convert.ToInt32(Eval("SignedQty")) >= 0
                                ? "text-success"
                                : "text-danger" %>'>

                                <%# Convert.ToInt32(Eval("SignedQty")) >= 0
                                    ? "+" + Eval("SignedQty")
                                    : Eval("SignedQty") %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>

            </asp:GridView>

        </div>

    </div>

</asp:Content>
