<%@ Page Title="My Reports"
    Language="C#"
    MasterPageFile="~/Master/SalesmanMaster.master"
    AutoEventWireup="true"
    CodeBehind="Reports.aspx.cs"
    Inherits="BWDMS.Salesman.Reports" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    My Reports

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <style>
        .sw-report .sw-report-picker {
            min-width: 220px;
        }

        .sw-report .sw-stat > div:last-child {
            min-width: 0;
            flex: 1;
        }

        .sw-report .sw-stat-value,
        .sw-report .sw-card-head h5,
        .sw-report .sw-card-head small {
            overflow-wrap: anywhere;
            word-break: break-word;
        }

        @media (max-width: 767.98px) {
            .sw-report .sw-card-head {
                align-items: stretch;
            }

            .sw-report .sw-report-picker {
                width: 100%;
                min-width: 0;
            }
        }
    </style>

    <div class="sw-report">

        <div class="sw-page-head">
            <div>
                <h1 class="mb-1">My Reports</h1>
                <p>Your routes, orders, shops, and stock summary.</p>
            </div>

            <div class="sw-head-actions">
                <a href="Dashboard.aspx" class="btn btn-outline-secondary">
                    <i class="bi bi-arrow-left me-1"></i>Back to Dashboard
                </a>
            </div>
        </div>

        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block mb-4">
        </asp:Label>

        <!-- KPI tiles -->
        <div class="row g-4 mb-4">

            <div class="col-6 col-lg-3">
                <div class="sw-card sw-stat h-100">
                    <div class="sw-stat-icon sw-tint-primary">
                        <i class="bi bi-cart-check"></i>
                    </div>
                    <div>
                        <div class="sw-stat-value">
                            <asp:Label ID="lblKpiOrders" runat="server" Text="0" />
                        </div>
                        <div class="sw-stat-label">Orders Taken</div>
                    </div>
                </div>
            </div>

            <div class="col-6 col-lg-3">
                <div class="sw-card sw-stat h-100">
                    <div class="sw-stat-icon sw-tint-green">
                        <i class="bi bi-currency-rupee"></i>
                    </div>
                    <div>
                        <div class="sw-stat-value">
                            <asp:Label ID="lblKpiValue" runat="server" Text="0.00" />
                        </div>
                        <div class="sw-stat-label">Order Value</div>
                    </div>
                </div>
            </div>

            <div class="col-6 col-lg-3">
                <div class="sw-card sw-stat h-100">
                    <div class="sw-stat-icon sw-tint-orange">
                        <i class="bi bi-shop"></i>
                    </div>
                    <div>
                        <div class="sw-stat-value">
                            <asp:Label ID="lblKpiShops" runat="server" Text="0" />
                        </div>
                        <div class="sw-stat-label">Shops Served</div>
                    </div>
                </div>
            </div>

            <div class="col-6 col-lg-3">
                <div class="sw-card sw-stat h-100">
                    <div class="sw-stat-icon sw-tint-cyan">
                        <i class="bi bi-signpost-split"></i>
                    </div>
                    <div>
                        <div class="sw-stat-value">
                            <asp:Label ID="lblKpiRoutes" runat="server" Text="0" />
                        </div>
                        <div class="sw-stat-label">Routes</div>
                    </div>
                </div>
            </div>

        </div>

        <!-- Report picker -->
        <div class="sw-card">

            <div class="sw-card-head">
                <div>
                    <h5>
                        <i class="bi bi-bar-chart-line me-2"></i>
                        <asp:Label ID="lblReportTitle" runat="server" Text="Orders by Status" />
                    </h5>
                    <small>
                        <asp:Label ID="lblRecordCount" runat="server" Text="records: 0" />
                    </small>
                </div>

                <div class="sw-report-picker">
                    <asp:DropDownList
                        ID="ddlReport"
                        runat="server"
                        CssClass="form-select"
                        AutoPostBack="true"
                        OnSelectedIndexChanged="ReportChanged">
                        <asp:ListItem Text="Orders by Status" Value="orderstatus" Selected="True" />
                        <asp:ListItem Text="Orders by Day" Value="orderday" />
                        <asp:ListItem Text="Shops by Village" Value="shops" />
                        <asp:ListItem Text="Stock Loaded" Value="stock" />
                    </asp:DropDownList>
                </div>
            </div>

            <div class="table-responsive">
                <asp:GridView
                    ID="gvReport"
                    runat="server"
                    ClientIDMode="Static"
                    AutoGenerateColumns="False"
                    CssClass="sw-table align-middle mb-0"
                    GridLines="None"
                    AllowPaging="true"
                    OnPageIndexChanging="gvReport_PageIndexChanging"
                    PageSize="25"
                    PagerSettings-Mode="NumericFirstLast"
                    PagerSettings-PageButtonCount="10"
                    PagerSettings-Position="TopAndBottom">
                    <Columns>
                        <asp:BoundField DataField="Label" HeaderText="Label" />
                        <asp:BoundField DataField="Count" HeaderText="Count" />
                        <asp:BoundField DataField="Amount" HeaderText="Amount" />
                    </Columns>

                    <EmptyDataTemplate>
                        <div class="text-center py-5">
                            <i class="bi bi-bar-chart fs-1 text-muted"></i>
                            <p class="text-muted mt-3 mb-0">Nothing to report yet.</p>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>

        </div>

    </div>

</asp:Content>
