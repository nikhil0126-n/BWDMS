<%@ Page Title="Routes"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Routes.aspx.cs"
    Inherits="BWDMS.Dealer.Routes" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Routes

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <style>
        /* --- PAGE HEADER (matches design) --- */
        .rt-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            margin-bottom: 28px;
        }

        .rt-title-wrap {
            display: flex;
            align-items: center;
            gap: 12px;
        }

        .rt-back {
            color: #111827;
            font-size: 22px;
            line-height: 1;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
        }

            .rt-back:hover {
                color: var(--bw-blue-600, #4f46e5);
                text-decoration: none;
            }

        .rt-title {
            font-size: 24px;
            font-weight: 700;
            color: #111827;
            margin: 0;
        }

        /* --- TABLE CARD --- */
        .rt-card {
            background: #ffffff;
            border: 1px solid #e4e4e7;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 1px 2px rgba(16, 24, 40, .04);
        }

        /* --- STATUS TEXT --- */
        .rt-status {
            font-size: 14px;
            font-weight: 500;
        }

        .rt-status.active {
            color: #16a34a;
        }

        .rt-status.inactive {
            color: #6b7280;
        }

        /* --- ROW ACTIONS --- */
        .rt-actions {
            display: inline-flex;
            align-items: center;
            gap: 16px;
        }

        /* --- PAGER --- */
        .rt-pager {
            display: flex;
            justify-content: center;
            align-items: center;
            gap: 8px;
            margin-top: 24px;
        }

        .rt-pager a,
        .rt-pager span {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            min-width: 34px;
            height: 34px;
            padding: 0 8px;
            border: 1px solid #e4e4e7;
            border-radius: 8px;
            background: #ffffff;
            color: #3f3f46;
            font-size: 14px;
            font-weight: 500;
            text-decoration: none;
            transition: .15s;
        }

        .rt-pager a:hover {
            border-color: var(--bw-blue-200, #c7d2fe);
            background: var(--bw-blue-50, #eef2ff);
            text-decoration: none;
        }

        .rt-pager .rt-pager-current {
            background: #6366f1;
            border-color: #6366f1;
            color: #ffffff;
        }

        .rt-pager .rt-pager-disabled {
            color: #c4c4cc;
        }
    </style>


    <!-- ============================================================
         PAGE HEADER
         ============================================================ -->

    <div class="rt-header">

        <div class="rt-title-wrap">

            <a class="rt-back" href="Dashboard.aspx" title="Back">
                <i class="bi bi-arrow-left"></i>
            </a>

            <h3 class="rt-title">
                Routes
            </h3>

        </div>


        <a href="AddRoute.aspx"
           class="btn btn-primary">

            <i class="bi bi-plus-lg me-1"></i>

            Add Route

        </a>

    </div>



    <!-- ============================================================
         LIVE SEARCH
         ============================================================ -->

    <div class="grid-search-box mb-4">

        <i class="bi bi-search search-icon"></i>

        <asp:TextBox
            ID="txtSearch"
            runat="server"
            CssClass="form-control"
            placeholder="Search vehicles...."
            ClientIDMode="Static">
        </asp:TextBox>

    </div>



    <!-- ============================================================
         ROUTE TABLE
         ============================================================ -->

    <div class="rt-card table-responsive">


        <asp:GridView
            ID="gvRoutes"
            runat="server"
            ClientIDMode="Static"
            AutoGenerateColumns="False"
            CssClass="table table-hover align-middle"
            GridLines="None"
            AllowPaging="true"
            OnPageIndexChanging="gvRoutes_PageIndexChanging"
            PageSize="25"
            PagerSettings-Mode="NumericFirstLast"
            PagerSettings-PageButtonCount="10"
            PagerSettings-Position="Bottom"
            PagerStyle-CssClass="rt-pager">


            <Columns>


                <asp:BoundField
                    DataField="RouteCode"
                    HeaderText="Route Code" />

                <asp:BoundField
                    DataField="RouteName"
                    HeaderText="Route Name" />

                <asp:BoundField
                    DataField="RouteType"
                    HeaderText="Type" />

                <asp:BoundField
                    DataField="Days"
                    HeaderText="Order / Dispatch Days" />

                <asp:BoundField
                    DataField="SalesmanName"
                    HeaderText="Salesmen" />

                <asp:BoundField
                    DataField="VehicleNumber"
                    HeaderText="Default Truck" />


                <asp:TemplateField
                    HeaderText="Status">

                    <ItemTemplate>

                        <span class='<%# Convert.ToBoolean(Eval("IsActive"))
                            ? "rt-status active"
                            : "rt-status inactive" %>'>

                            <%# Convert.ToBoolean(Eval("IsActive"))
                                ? "Active"
                                : "Inactive" %>

                        </span>

                    </ItemTemplate>

                </asp:TemplateField>


                <asp:TemplateField
                    HeaderText="Action">

                    <ItemTemplate>

                        <span class="rt-actions">

                            <a class="row-action"
                                title="Details"
                                href='<%# ResolveUrl("~/Dealer/RouteDetails.aspx?id=" + Eval("RouteId")) %>'>
                                <i class="bi bi-eye"></i>
                            </a>

                            <a class="row-action"
                                title="Edit"
                                href='<%# ResolveUrl("~/Dealer/AddRoute.aspx?id=" + Eval("RouteId")) %>'>
                                <i class="bi bi-pencil"></i>
                            </a>

                            <a class="row-action"
                                title="Assign Villages"
                                href='<%# ResolveUrl("~/Dealer/RouteVillageAssignment.aspx?route=" + Eval("RouteId")) %>'>
                                <i class="bi bi-geo-alt"></i>
                            </a>

                        </span>

                    </ItemTemplate>

                </asp:TemplateField>


            </Columns>


        </asp:GridView>


    </div>



</asp:Content>
