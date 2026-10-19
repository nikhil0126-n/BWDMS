<%@ Page Title="Daily Closing"
    Language="C#"
    MasterPageFile="~/Master/SalesmanMaster.master"
    AutoEventWireup="true"
    CodeBehind="DailyClosing.aspx.cs"
    Inherits="BWDMS.Salesman.DailyClosing" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Daily Closing

</asp:Content>


<asp:Content ID="HeadStyle"
    ContentPlaceHolderID="HeadContent"
    runat="server">

    <style>

        /* =====================================================
           DAILY CLOSING  (matches the BALAJI DMS prototype UI)
           ===================================================== */

        .dc-page
        {
            background:#fff;
            padding:6px 4px 20px;
        }


        /* ---------- page header ---------- */

        .dc-head
        {
            display:flex;
            align-items:center;
            gap:14px;
            margin-bottom:24px;
        }

        .dc-back
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
        }

        .dc-back:hover
        {
            background:#f2f2f2;
            color:#111;
        }

        .dc-head h3
        {
            font-size:21px;
            font-weight:700;
            color:#111;
            margin:0;
            letter-spacing:.2px;
        }


        /* ---------- layout ---------- */

        .dc-layout
        {
            display        : grid;
            grid-template-columns: 270px 1fr;
            gap            : 22px;
            align-items    : start;
        }

        .dc-card
        {
            border        : 1px solid #d5d5d5;
            border-radius : 10px;
            background    : #fff;
            padding       : 22px 24px 26px;
        }

        .dc-card-title
        {
            font-size   : 19px;
            font-weight : 700;
            color       : #111;
            margin      : 0 0 34px 0;
        }


        /* ---------- trip information ---------- */

        .dc-field
        {
            margin-bottom : 22px;
        }

        .dc-field:last-child
        {
            margin-bottom : 0;
        }

        .dc-label
        {
            display   : block;
            font-size : 14px;
            color     : #9a9a9a;
            margin-bottom: 6px;
        }

        .dc-big
        {
            display     : block;
            font-size   : 21px;
            font-weight : 700;
            color       : #111;
        }


        /* ---------- closing table ---------- */

        table.dc-grid
        {
            width          : 100%;
            border-collapse: collapse;
            margin         : 0;
        }

        table.dc-grid th
        {
            color       : #111;
            font-size   : 17px;
            font-weight : 700;
            text-align  : center;
            padding     : 12px 12px 30px;
            border      : none;
        }

        table.dc-grid th:first-child,
        table.dc-grid td:first-child
        {
            text-align : left;
        }

        table.dc-grid td
        {
            color       : #111;
            font-size   : 16px;
            font-weight : 700;
            text-align  : center;
            padding     : 18px 12px;
            border      : none;
        }

        .dc-input
        {
            width           : 100px;
            border          : 1px solid #d3d3d3;
            border-radius   : 10px;
            padding         : 9px 12px;
            font-size       : 16px;
            font-weight     : 700;
            color           : #111;
            text-align      : center;
            background      : #fff;
        }

        .dc-input:focus
        {
            outline         : none;
            border-color    : #6c5ce7;
        }

        .dc-diff
        {
            font-size : 17px;
            font-weight: 700;
            color     : #111;
        }

        .dc-diff.neg
        {
            color : #ef4444;
        }


        /* ---------- submit ---------- */

        .dc-actions
        {
            display        : flex;
            justify-content: flex-end;
            margin-top     : 26px;
        }

        .dc-btn
        {
            border        : none;
            background    : #6c5ce7;
            color         : #fff;
            font-size     : 15px;
            font-weight   : 700;
            padding       : 14px 34px;
            border-radius : 8px;
            cursor        : pointer;
        }

        .dc-btn:hover
        {
            background : #5a4bd6;
            color      : #fff;
        }

        .dc-btn:disabled
        {
            background : #c7c7c7;
            color      : #fff;
        }

        @media (max-width: 900px)
        {
            .dc-layout
            {
                grid-template-columns: 1fr;
            }
        }

    </style>

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <div class="dc-page">


        <!-- PAGE HEADER -->

        <div class="dc-head">

            <a class="dc-back"
               href="<%= ResolveUrl("~/Salesman/Dashboard.aspx") %>"
               title="Back">
                <i class="bi bi-arrow-left"></i>
            </a>

            <h3>Daily Closing</h3>

        </div>


        <!-- MESSAGE -->

        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <div class="dc-layout">


            <!-- TRIP INFORMATION -->

            <div class="dc-card">

                <p class="dc-card-title">
                    Trip Information
                </p>

                <div class="dc-field">

                    <span class="dc-label">Vehicle</span>

                    <span class="dc-big">

                        <asp:Label ID="lblVehicle" runat="server" Text="-"></asp:Label>

                    </span>

                </div>

                <div class="dc-field">

                    <span class="dc-label">Route</span>

                    <span class="dc-big">

                        <asp:Label ID="lblRoute" runat="server" Text="-"></asp:Label>

                    </span>

                </div>

                <div class="dc-field">

                    <span class="dc-label">Date</span>

                    <span class="dc-big">

                        <asp:Label ID="lblDate" runat="server" Text="-"></asp:Label>

                    </span>

                </div>

                <div class="dc-field">

                    <span class="dc-label">Salesman</span>

                    <span class="dc-big">

                        <asp:Label ID="lblSalesman" runat="server" Text="-"></asp:Label>

                    </span>

                </div>

            </div>


            <!-- CLOSING SUMMARY -->

            <div class="dc-card">

                <p class="dc-card-title">
                    Closing summary
                </p>

                <div class="table-responsive">

                    <asp:GridView
                        ID="gvClosing"
                        runat="server"
                        AutoGenerateColumns="False"
                        CssClass="dc-grid"
                        GridLines="None"
                        DataKeyNames="VehicleStockId,ProductVariantId">

                        <Columns>

                            <asp:BoundField
                                DataField="ProductName"
                                HeaderText="Product" />

                            <asp:BoundField
                                DataField="Expected"
                                HeaderText="Expected(Box)" />

                            <asp:TemplateField HeaderText="Actual(Box)">

                                <ItemTemplate>

                                    <asp:TextBox
                                        ID="txtActual"
                                        runat="server"
                                        CssClass="dc-input"
                                        TextMode="Number"
                                        Text='<%# Eval("Actual") %>'
                                        oninput="calcDiff(this)">
                                    </asp:TextBox>

                                </ItemTemplate>

                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Difference">

                                <ItemTemplate>

                                    <span class="dc-diff"
                                          data-expected='<%# Eval("Expected") %>'>
                                        <%# Eval("Difference") %>
                                    </span>

                                </ItemTemplate>

                            </asp:TemplateField>

                        </Columns>


                        <EmptyDataTemplate>

                            <div class="text-center py-5">

                                <i class="bi bi-box-seam fs-1 text-muted"></i>

                                <p class="text-muted mt-3 mb-0">
                                    No stock is loaded for you yet.
                                </p>

                            </div>

                        </EmptyDataTemplate>

                    </asp:GridView>

                </div>

            </div>

        </div>


        <!-- SUBMIT -->

        <div class="dc-actions">

            <asp:Button
                ID="btnSubmit"
                runat="server"
                CssClass="dc-btn"
                Text="Submit Closing"
                OnClick="btnSubmit_Click" />

        </div>

    </div>


    <script type="text/javascript">

        function calcDiff(el) {

            var row = el.closest ? el.closest('tr') : null;

            if (!row) { return; }

            var span = row.querySelector('.dc-diff');

            if (!span) { return; }

            var expected = parseInt(span.getAttribute('data-expected'), 10) || 0;

            var actual = (el.value === '') ? 0 : (parseInt(el.value, 10) || 0);

            var diff = actual - expected;

            span.textContent = diff;

            span.className = 'dc-diff' + (diff < 0 ? ' neg' : '');
        }

    </script>

</asp:Content>
