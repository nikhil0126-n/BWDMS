<%@ Page Title="Vehicle Stock"
    Language="C#"
    MasterPageFile="~/Master/Salesmanmaster.master"
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

        /* ---------- Page header ---------- */

        .vs-topbar
        {
            display        : flex;
            align-items    : center;
            gap            : 16px;
            padding        : 2px 0 16px 0;
            margin         : 0 0 22px 0;
            border-bottom  : 1px solid #e5e7eb;
        }

        .vs-back
        {
            color       : #111827;
            font-size   : 20px;
            line-height : 1;
            text-decoration : none;
            display     : inline-flex;
            align-items : center;
        }

        .vs-back:hover
        {
            color : #111827;
        }

        .vs-topbar h3
        {
            margin      : 0;
            font-size   : 21px;
            font-weight : 700;
            color       : #111827;
        }


        /* ---------- Vehicle summary card ---------- */

        .vs-card
        {
            background     : #fff;
            border         : 1px solid #d9d9d9;
            border-radius  : 8px;
            padding        : 22px 30px 24px 30px;
            margin         : 0 0 24px 0;
        }

        .vs-vehicle
        {
            display     : flex;
            align-items : center;
            gap         : 22px;
            margin      : 0 0 22px 0;
        }

        .vs-truck
        {
            width  : 66px;
            height : 50px;
            flex   : 0 0 auto;
        }

        .vs-plate
        {
            margin     : 0 0 10px 0;
            font-size  : 20px;
            font-weight: 700;
            color      : #111827;
        }

        .vs-beat
        {
            display         : inline-block;
            padding         : 5px 16px;
            font-size       : 13px;
            color           : #374151;
            background      : #fff;
            border          : 1px solid #d1d5db;
            border-radius   : 6px;
        }

        .vs-stats
        {
            display : flex;
        }

        .vs-stat
        {
            flex     : 1;
            text-align: center;
        }

        .vs-stat-lbl
        {
            font-size  : 17px;
            color      : #111827;
            margin     : 0 0 12px 0;
        }

        .vs-stat-val
        {
            font-size  : 17px;
            font-weight: 700;
            color      : #111827;
        }


        /* ---------- Stock table ---------- */

        .vs-table-wrap
        {
            border        : 1px solid #d9d9d9;
            border-radius : 8px;
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
            background    : #fff;
            font-size     : 15px;
            font-weight   : 700;
            color         : #111827;
            text-align    : left;
            padding       : 17px 22px;
            border-bottom : 1px solid #e5e7eb;
        }

        .vs-table td
        {
            font-size     : 15px;
            font-weight   : 600;
            color         : #111827;
            padding       : 16px 22px;
            border-bottom : 1px solid #ececec;
        }

        .vs-table tr:last-child td
        {
            border-bottom : none;
        }

        .vs-num
        {
            text-align : center;
        }

    </style>


    <!-- Page header -->

    <div class="vs-topbar">

        <a class="vs-back"
           href="Dashboard.aspx"
           title="Back">

            <i class="bi bi-arrow-left"></i>

        </a>

        <h3>Vehicle Stock</h3>

    </div>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block mb-3">
    </asp:Label>


    <!-- Vehicle summary -->

    <div class="vs-card">

        <div class="vs-vehicle">

            <svg class="vs-truck"
                 viewBox="0 0 64 48"
                 xmlns="http://www.w3.org/2000/svg"
                 aria-hidden="true">

                <rect x="2" y="10" width="34" height="24" rx="4" fill="#4aa3e8" />

                <path d="M36 16 h9 l11 10 v8 H36 z" fill="#63b3ed" />

                <path d="M39 18 h5 l6 6 H39 z" fill="#dbeafe" />

                <rect x="54" y="29" width="7" height="5" rx="1.5" fill="#f6c453" />

                <rect x="0" y="30" width="5" height="5" rx="1.5" fill="#f6c453" />

                <rect x="6" y="14" width="10" height="7" rx="2" fill="#93c5fd" />

                <circle cx="17" cy="36" r="6.5" fill="#1f2937" />
                <circle cx="17" cy="36" r="2.6" fill="#9ca3af" />

                <circle cx="46" cy="36" r="6.5" fill="#1f2937" />
                <circle cx="46" cy="36" r="2.6" fill="#9ca3af" />

            </svg>

            <div>

                <p class="vs-plate">

                    <asp:Label
                        ID="lblVehicleNumber"
                        runat="server"
                        Text="-">
                    </asp:Label>

                </p>

                <span class="vs-beat">

                    <asp:Label
                        ID="lblBeat"
                        runat="server"
                        Text="-">
                    </asp:Label>

                </span>

            </div>

        </div>

        <div class="vs-stats">

            <div class="vs-stat">

                <div class="vs-stat-lbl">Sold</div>

                <div class="vs-stat-val">

                    <asp:Label
                        ID="lblSold"
                        runat="server"
                        Text="0 Box">
                    </asp:Label>

                </div>

            </div>

            <div class="vs-stat">

                <div class="vs-stat-lbl">Loaded</div>

                <div class="vs-stat-val">

                    <asp:Label
                        ID="lblLoaded"
                        runat="server"
                        Text="0 Box">
                    </asp:Label>

                </div>

            </div>

            <div class="vs-stat">

                <div class="vs-stat-lbl">Remaining</div>

                <div class="vs-stat-val">

                    <asp:Label
                        ID="lblRemaining"
                        runat="server"
                        Text="0 Box">
                    </asp:Label>

                </div>

            </div>

        </div>

    </div>


    <!-- Product wise stock -->

    <div class="vs-table-wrap">

        <asp:GridView
            ID="gvStock"
            runat="server"
            AutoGenerateColumns="False"
            CssClass="vs-table"
            GridLines="None">

            <Columns>

                <asp:BoundField
                    DataField="ProductName"
                    HeaderText="Product" />

                <asp:BoundField
                    DataField="PackingText"
                    HeaderText="Packing" />

                <asp:BoundField
                    DataField="OpeningText"
                    HeaderText="opening"
                    HeaderStyle-CssClass="vs-num"
                    ItemStyle-CssClass="vs-num" />

                <asp:BoundField
                    DataField="LoadedText"
                    HeaderText="Loaded"
                    HeaderStyle-CssClass="vs-num"
                    ItemStyle-CssClass="vs-num" />

                <asp:BoundField
                    DataField="SoldText"
                    HeaderText="Sold"
                    HeaderStyle-CssClass="vs-num"
                    ItemStyle-CssClass="vs-num" />

                <asp:BoundField
                    DataField="RemainingText"
                    HeaderText="Remaining"
                    HeaderStyle-CssClass="vs-num"
                    ItemStyle-CssClass="vs-num" />

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

</asp:Content>
