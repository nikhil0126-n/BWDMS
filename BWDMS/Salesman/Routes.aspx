<%@ Page Title="My Route"
    Language="C#"
    MasterPageFile="~/Master/SalesmanMaster.master"
    AutoEventWireup="true"
    CodeBehind="Routes.aspx.cs"
    Inherits="BWDMS.Salesman.Routes" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    My Routes

</asp:Content>


<asp:Content ID="Content3"
    ContentPlaceHolderID="HeadContent"
    runat="server">

    <style>

        /* white canvas for the My Routes screen */
        body.sw-body .sw-main {
            background: #ffffff;
        }


        /* ---------- page header ---------- */

        .rt-head {
            display: flex;
            align-items: center;
            gap: 14px;
            margin-bottom: 24px;
        }

        .rt-back {
            width: 34px;
            height: 34px;
            border-radius: 9px;
            border: 1px solid #e3e6ee;
            background: #fff;
            color: #101828;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            font-size: 18px;
            text-decoration: none;
            transition: .18s;
        }

        .rt-back:hover {
            background: #f4f6fb;
            color: #101828;
            text-decoration: none;
        }

        .rt-head h3 {
            margin: 0;
            font-size: 24px;
            font-weight: 400;
            color: #101828;
            letter-spacing: -.2px;
        }


        /* ---------- route summary card ---------- */

        .rt-card {
            background: #fff;
            border: 1px solid #d9dde7;
            border-radius: 12px;
            padding: 24px 28px;
            margin-bottom: 26px;
        }

        .rt-card-top {
            display: flex;
            align-items: center;
            gap: 20px;
        }

        .rt-truck {
            width: 74px;
            height: 60px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 54px;
            color: #3b82f6;
            flex: none;
        }

        .rt-beat {
            margin: 0 0 9px;
            font-size: 26px;
            font-weight: 400;
            color: #101828;
            line-height: 1.1;
        }

        .rt-pill {
            display: inline-flex;
            align-items: center;
            padding: 5px 14px;
            border-radius: 6px;
            background: #2ee07f;
            color: #ffffff;
            font-size: 12px;
            font-weight: 700;
            line-height: 1.3;
        }

        .rt-pill.off {
            background: #cbd5e1;
            color: #475467;
        }

        .rt-stats {
            display: grid;
            grid-template-columns: repeat(3, minmax(0, 1fr));
            margin-top: 30px;
        }

        .rt-stat {
            text-align: center;
            padding: 0 10px;
        }

        .rt-stat-lbl {
            font-size: 17px;
            color: #3f4a5a;
            margin-bottom: 9px;
        }

        .rt-stat-val {
            font-size: 20px;
            font-weight: 400;
            color: #101828;
            line-height: 1.2;
            overflow-wrap: anywhere;
        }


        /* ---------- stops table ---------- */

        .rt-stops-title {
            margin: 0 0 12px;
            font-size: 17px;
            font-weight: 400;
            color: #101828;
        }

        .rt-table-wrap {
            border: 1px solid #d9dde7;
            border-radius: 10px;
            overflow: hidden;
        }

        /* header + body cells are targeted through the row classes the
           GridView emits, so alignment does not depend on whether the
           renderer wraps rows in thead / tbody. */

        .sw-table.rt-table .rt-th > th,
        .sw-table.rt-table .rt-th > td {
            background: #fbfbfd;
            border-bottom: 1px solid #d9dde7;
            color: #1f2937;
            font-size: 14px;
            font-weight: 700;
            text-transform: none;
            letter-spacing: 0;
            text-align: center !important;
            vertical-align: middle;
            padding: 14px 12px;
            white-space: nowrap;
        }

        .sw-table.rt-table .rt-td > td {
            border-bottom: 1px solid #eceef3;
            text-align: center !important;
            vertical-align: middle;
            font-size: 14px;
            padding: 16px 12px;
        }

        .sw-table.rt-table tr.rt-td:last-child > td {
            border-bottom: none;
        }

        /* pager row keeps its own spacing / left alignment */
        .sw-table.rt-table .rt-pager > td {
            text-align: left !important;
            padding: 12px 8px;
            border-bottom: none;
        }

        .rt-status {
            font-weight: 400;
            font-size: 14px;
        }

        .rt-status.visited {
            color: #16a34a;
        }

        .rt-status.pending {
            color: #ef4444;
        }

        .rt-eye {
            border: none;
            background: none;
            padding: 0;
            font-size: 21px;
            line-height: 1;
            color: #101828;
            cursor: pointer;
            transition: .18s;
        }

        .rt-eye:hover {
            color: #3b82f6;
        }

        .sw-table.rt-table .pager a,
        .sw-table.rt-table .pager span {
            color: #3f4a5a;
        }

    </style>

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- ============================================================
         PAGE HEADER
         ============================================================ -->

    <div class="rt-head">

        <a class="rt-back"
           href="<%= ResolveUrl("~/Salesman/Dashboard.aspx") %>"
           title="Back">

            <i class="bi bi-arrow-left"></i>

        </a>

        <h3>
            My Routes
        </h3>

    </div>


    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>



    <!-- ============================================================
         ROUTE SUMMARY
         ============================================================ -->

    <div class="rt-card">

        <div class="rt-card-top">

            <div class="rt-truck">
                <i class="bi bi-truck"></i>
            </div>

            <div>

                <div class="rt-beat">

                    <asp:Label
                        ID="lblRouteName"
                        runat="server"
                        Text="-">
                    </asp:Label>

                </div>

                <span class="rt-pill"
                      id="rtPill"
                      runat="server">

                    <asp:Label
                        ID="lblRouteStatus"
                        runat="server"
                        Text="On Route">
                    </asp:Label>

                </span>

                <asp:Label
                    ID="lblVillages"
                    runat="server"
                    Text="0 villages"
                    Visible="false">
                </asp:Label>

            </div>

        </div>


        <div class="rt-stats">

            <div class="rt-stat">

                <div class="rt-stat-lbl">
                    Vehicle
                </div>

                <div class="rt-stat-val">

                    <asp:Label
                        ID="lblVehicle"
                        runat="server"
                        Text="-">
                    </asp:Label>

                </div>

            </div>


            <div class="rt-stat">

                <div class="rt-stat-lbl">
                    Loaded
                </div>

                <div class="rt-stat-val">

                    <asp:Label
                        ID="lblLoaded"
                        runat="server"
                        Text="0 Box">
                    </asp:Label>

                </div>

            </div>


            <div class="rt-stat">

                <div class="rt-stat-lbl">
                    Remaining
                </div>

                <div class="rt-stat-val">

                    <asp:Label
                        ID="lblRemaining"
                        runat="server"
                        Text="0 Box">
                    </asp:Label>

                </div>

            </div>

        </div>

    </div>



    <!-- ============================================================
         ROUTE STOPS
         ============================================================ -->

    <div class="rt-stops-title">

        Route Stops (<asp:Label
            ID="lblStopCount"
            runat="server"
            Text="0" />)

    </div>


    <div class="rt-table-wrap">

        <asp:GridView
            ID="gvRoutes"
            runat="server"
            AutoGenerateColumns="False"
            UseAccessibleHeader="true"
            CssClass="sw-table rt-table mb-0"
            GridLines="None"
            HeaderStyle-CssClass="rt-th"
            RowStyle-CssClass="rt-td"
            AlternatingRowStyle-CssClass="rt-td"
            PagerStyle-CssClass="rt-pager"
            AllowPaging="true"
            OnPageIndexChanging="gvRoutes_PageIndexChanging"
            PageSize="25"
            PagerSettings-Mode="NumericFirstLast"
            PagerSettings-PageButtonCount="10"
            PagerSettings-Position="Bottom">

            <Columns>

                <asp:TemplateField HeaderText="#">

                    <ItemTemplate>

                        <%# Container.DataItemIndex + 1 %>

                    </ItemTemplate>

                </asp:TemplateField>


                <asp:BoundField
                    DataField="ShopName"
                    HeaderText="Shop Name" />

                <asp:BoundField
                    DataField="VillageName"
                    HeaderText="Location" />


                <asp:TemplateField
                    HeaderText="Status">

                    <ItemTemplate>

                        <span class='rt-status <%# Convert.ToInt32(Eval("IsVisited")) == 1 ? "visited" : "pending" %>'>

                            <%# Convert.ToInt32(Eval("IsVisited")) == 1 ? "Visited" : "Pending" %>

                        </span>

                    </ItemTemplate>

                </asp:TemplateField>


                <asp:TemplateField
                    HeaderText="Action">

                    <ItemTemplate>

                        <button type="button"
                                class="rt-eye"
                                title="View stop">

                            <i class="bi bi-eye"></i>

                        </button>

                    </ItemTemplate>

                </asp:TemplateField>

            </Columns>


            <EmptyDataTemplate>

                <div class="text-center py-5">

                    <i class="bi bi-signpost-split fs-1 text-muted"></i>

                    <p class="text-muted mt-3 mb-0">
                        No stops on your routes yet.
                    </p>

                </div>

            </EmptyDataTemplate>

        </asp:GridView>

    </div>


</asp:Content>
