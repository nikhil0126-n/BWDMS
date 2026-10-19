<%@ Page Title="My Shops"
    Language="C#"
    MasterPageFile="~/Master/SalesmanMaster.master"
    AutoEventWireup="true"
    CodeBehind="Shops.aspx.cs"
    Inherits="BWDMS.Salesman.Shops" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    My Shops

</asp:Content>


<asp:Content ID="Content3"
    ContentPlaceHolderID="HeadContent"
    runat="server">

    <style>

        /* =====================================================
           MY SHOPS PAGE  (matches the BALAJI DMS prototype UI)
           ===================================================== */

        .shops-page
        {
            background:#fff;
            padding:6px 4px 20px;
        }


        /* ---------- page header ---------- */

        .shops-head
        {
            display:flex;
            align-items:center;
            gap:14px;
            margin-bottom:22px;
        }

        .shops-back
        {
            width:34px;
            height:34px;
            border:none;
            background:transparent;
            color:#111;
            font-size:19px;
            display:flex;
            align-items:center;
            justify-content:center;
            border-radius:8px;
            text-decoration:none;
            cursor:pointer;
        }

        .shops-back:hover
        {
            background:#f2f2f2;
            color:#111;
        }

        .shops-head h3
        {
            font-size:21px;
            font-weight:700;
            color:#111;
            margin:0;
            letter-spacing:.2px;
        }


        /* ---------- search + filter row ---------- */

        .shops-toolbar
        {
            display:flex;
            align-items:center;
            gap:14px;
            margin-bottom:22px;
        }

        .shops-search
        {
            flex:1;
            display:flex;
            align-items:center;
            gap:12px;
            border:1px solid #cfcfcf;
            border-radius:8px;
            padding:12px 16px;
            background:#fff;
            max-width:640px;
        }

        .shops-search i
        {
            color:#666;
            font-size:16px;
        }

        .shops-search input
        {
            border:none;
            outline:none;
            width:100%;
            font-size:14px;
            color:#333;
            background:transparent;
        }

        .shops-search input::placeholder
        {
            color:#9a9a9a;
        }

        .shops-filter
        {
            border:1px solid #cfcfcf;
            border-radius:8px;
            background:#fff;
            padding:12px 34px 12px 16px;
            font-size:14px;
            color:#333;
            cursor:pointer;
            appearance:none;
            -webkit-appearance:none;
            background-image:url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 16 16' fill='%23333'><path d='M4 6l4 4 4-4z'/></svg>");
            background-repeat:no-repeat;
            background-position:right 12px center;
        }


        /* ---------- table ---------- */

        .shops-table-card
        {
            border:1px solid #d5d5d5;
            border-radius:8px;
            overflow:hidden;
            background:#fff;
        }

        table.shops-grid
        {
            width:100%;
            border-collapse:collapse;
            margin:0;
            color:#333;
        }

        table.shops-grid > tbody > tr > th,
        table.shops-grid th
        {
            background:#e6e6e6;
            color:#222;
            font-size:14px;
            font-weight:600;
            text-align:center;
            padding:15px 18px;
            border:none;
            border-bottom:1px solid #d5d5d5;
        }

        table.shops-grid td
        {
            padding:16px 18px;
            font-size:14px;
            color:#333;
            text-align:center;
            border-top:1px solid #f0f0f0;
            vertical-align:middle;
        }

        table.shops-grid tbody tr:hover
        {
            background:#fafafa;
        }

        /* pager row - the built-in GridView pager lives in a colspan cell,
           already pill-styled by Dashboard.css; just calm it down here */

        table.shops-grid td[colspan]
        {
            padding:12px 8px;
            text-align:center;
            background:#fff;
            border-top:1px solid #f0f0f0;
        }


        /* ---------- status ---------- */

        .st-badge
        {
            display:inline-flex;
            align-items:center;
            gap:5px;
            font-size:14px;
            font-weight:500;
        }

        .st-ok
        {
            color:#16a34a;
        }

        .st-pending
        {
            color:#f59e0b;
        }


        /* ---------- action eye ---------- */

        .eye-btn
        {
            border:none;
            background:transparent;
            font-size:17px;
            color:#222;
            cursor:pointer;
            padding:4px 8px;
            border-radius:6px;
            line-height:1;
        }

        .eye-btn:hover
        {
            background:#eef1ff;
            color:#2f4ff0;
        }


    </style>

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <div class="shops-page">


        <!-- PAGE HEADER -->

        <div class="shops-head">

            <a class="shops-back"
               href="<%= ResolveUrl("~/Salesman/Dashboard.aspx") %>"
               title="Back">
                <i class="bi bi-arrow-left"></i>
            </a>

            <h3>MY Shops</h3>

        </div>


        <!-- MESSAGE -->

        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <!-- SEARCH + FILTER -->

        <div class="shops-toolbar">

            <div class="shops-search">

                <i class="bi bi-search"></i>

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    BorderStyle="None"
                    placeholder="Search Shops....."
                    AutoPostBack="true"
                    OnTextChanged="txtSearch_TextChanged">
                </asp:TextBox>

            </div>

            <asp:DropDownList
                ID="ddlStatus"
                runat="server"
                CssClass="shops-filter"
                AutoPostBack="true"
                OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged">

                <asp:ListItem Text="All" Value="" />
                <asp:ListItem Text="Visited" Value="Visited" />
                <asp:ListItem Text="Pending" Value="Pending" />

            </asp:DropDownList>

        </div>


        <!-- SHOP TABLE -->

        <div class="shops-table-card table-responsive">

            <asp:GridView
                ID="gvShops"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="shops-grid"
                GridLines="None"
                DataKeyNames="ShopId"
                AllowPaging="true"
                OnPageIndexChanging="gvShops_PageIndexChanging"
                PageSize="25"
                PagerSettings-Mode="NumericFirstLast"
                PagerSettings-PageButtonCount="10"
                PagerSettings-Position="Bottom">

                <Columns>

                    <asp:BoundField
                        DataField="ShopName"
                        HeaderText="Shop Name" />

                    <asp:BoundField
                        DataField="Location"
                        HeaderText="Location" />

                    <asp:BoundField
                        DataField="LastOrderText"
                        HeaderText="Last Order" />

                    <asp:TemplateField HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Eval("Status").ToString() == "Visited" ? "st-badge st-ok" : "st-badge st-pending" %>'>

                                <i class='bi <%# Eval("Status").ToString() == "Visited" ? "bi-check-lg" : "bi-clock-history" %>'></i>

                                <%#: Eval("Status") %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Action">

                        <ItemTemplate>

                            <asp:HyperLink
                                ID="lnkView"
                                runat="server"
                                CssClass="eye-btn"
                                title="View shop details"
                                NavigateUrl='<%# "~/Salesman/ShopDetails.aspx?id=" + Eval("ShopId") %>'>

                                <i class="bi bi-eye"></i>

                            </asp:HyperLink>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-shop fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No shops are assigned to your routes yet.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>

</asp:Content>
