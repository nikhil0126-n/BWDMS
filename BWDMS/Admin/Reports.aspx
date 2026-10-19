<%@ Page Title="Reports"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Reports.aspx.cs"
    Inherits="BWDMS.Admin.Reports" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Reports

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-3">

        <div>

            <h3 class="fw-bold mb-1">
                Reports
            </h3>

            <p class="text-muted mb-0">
                System-wide summary
            </p>

        </div>


        <div class="d-flex gap-2 align-items-center flex-wrap">

            <a href="Users.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-people"></i>

                Users

            </a>


            <a href="Orders.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-cart-check"></i>

                Orders

            </a>


            <div class="report-range">

                <i class="bi bi-calendar3"></i>

                <asp:TextBox
                    ID="txtFromDate"
                    runat="server"
                    CssClass="form-control form-control-sm"
                    TextMode="Date">
                </asp:TextBox>

                <span class="text-muted">
                    &ndash;
                </span>

                <asp:TextBox
                    ID="txtToDate"
                    runat="server"
                    CssClass="form-control form-control-sm"
                    TextMode="Date">
                </asp:TextBox>

                <asp:Button
                    ID="btnApplyRange"
                    runat="server"
                    Text="Apply"
                    CssClass="btn btn-sm btn-primary"
                    OnClick="btnApplyRange_Click">
                </asp:Button>

            </div>

        </div>

    </div>


    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <!-- KPI tiles -->

    <div class="row g-3 mb-4">

        <div class="col-md-3">

            <div class="dashboard-card">

                <div class="d-flex align-items-center gap-3">

                    <span class="stat-icon bg-primary-subtle text-primary-emphasis">
                        <i class="bi bi-people"></i>
                    </span>

                    <div>

                        <div class="text-muted small">
                            Users
                        </div>


                        <div class="fw-bold fs-4">

                            <asp:Label
                                ID="lblKpiUsers"
                                runat="server"
                                Text="0">
                            </asp:Label>

                        </div>

                    </div>

                </div>

            </div>

        </div>


        <div class="col-md-3">

            <div class="dashboard-card">

                <div class="d-flex align-items-center gap-3">

                    <span class="stat-icon bg-success-subtle text-success-emphasis">
                        <i class="bi bi-person-vcard"></i>
                    </span>

                    <div>

                        <div class="text-muted small">
                            Dealers
                        </div>


                        <div class="fw-bold fs-4">

                            <asp:Label
                                ID="lblKpiDealers"
                                runat="server"
                                Text="0">
                            </asp:Label>

                        </div>

                    </div>

                </div>

            </div>

        </div>


        <div class="col-md-3">

            <div class="dashboard-card">

                <div class="d-flex align-items-center gap-3">

                    <span class="stat-icon bg-warning-subtle text-warning-emphasis">
                        <i class="bi bi-shop"></i>
                    </span>

                    <div>

                        <div class="text-muted small">
                            Shops
                        </div>


                        <div class="fw-bold fs-4">

                            <asp:Label
                                ID="lblKpiShops"
                                runat="server"
                                Text="0">
                            </asp:Label>

                        </div>

                    </div>

                </div>

            </div>

        </div>


        <div class="col-md-3">

            <div class="dashboard-card">

                <div class="d-flex align-items-center gap-3">

                    <span class="stat-icon bg-info-subtle text-info-emphasis">
                        <i class="bi bi-currency-rupee"></i>
                    </span>

                    <div>

                        <div class="text-muted small">
                            Orders Value
                        </div>


                        <div class="fw-bold fs-4">

                            <asp:Label
                                ID="lblKpiSales"
                                runat="server"
                                Text="0.00">
                            </asp:Label>

                        </div>

                    </div>

                </div>

            </div>

        </div>

    </div>


    <!-- Top products -->

    <div class="dashboard-card mb-4">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    Top Products
                </h5>

                <small class="text-muted">

                    <asp:Label
                        ID="lblTopProductsRange"
                        runat="server"
                        Text="Best selling products by packets sold">
                    </asp:Label>

                </small>

            </div>


            <span class="stat-icon bg-primary-subtle text-primary-emphasis">

                <i class="bi bi-trophy"></i>

            </span>

        </div>


        <asp:Repeater
            ID="rptTopProducts"
            runat="server">

            <HeaderTemplate>

                <div class="top-products-row top-products-head">

                    <span>
                        Products
                    </span>

                    <span class="text-end">
                        Sales Qty
                    </span>

                </div>

            </HeaderTemplate>


            <ItemTemplate>

                <div class="top-products-row">

                    <span class="top-products-name">

                        <span class="top-products-rank">
                            <%# Container.ItemIndex + 1 %>
                        </span>

                        <%# Server.HtmlEncode(Convert.ToString(Eval("Product"))) %>

                    </span>


                    <span class="fw-semibold text-end">
                        <%# Convert.ToInt32(Eval("SalesQty")).ToString("N0") %>
                    </span>

                </div>

            </ItemTemplate>

        </asp:Repeater>


        <asp:Panel
            ID="pnlNoTopProducts"
            runat="server"
            Visible="false">

            <p class="text-muted mb-0">
                No sales have been recorded yet.
            </p>

        </asp:Panel>

    </div>


    <!-- Report picker -->

    <div class="dashboard-card">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">

                    <asp:Label
                        ID="lblReportTitle"
                        runat="server"
                        Text="Users by Role">
                    </asp:Label>

                </h5>


                <small class="text-muted">

                    <asp:Label
                        ID="lblRecordCount"
                        runat="server"
                        Text="records: 0">
                    </asp:Label>

                </small>

            </div>


            <div class="d-flex gap-2 align-items-center">

                <div class="grid-search-box">

                    <asp:DropDownList
                        ID="ddlReport"
                        runat="server"
                        CssClass="form-select"
                        AutoPostBack="true"
                        OnSelectedIndexChanged="ReportChanged">

                        <asp:ListItem Text="Users by Role" Value="users" Selected="True">
                        </asp:ListItem>

                        <asp:ListItem Text="Orders by Status" Value="orders">
                        </asp:ListItem>

                        <asp:ListItem Text="Products by Category" Value="products">
                        </asp:ListItem>

                        <asp:ListItem Text="Shops by Dealer" Value="shops">
                        </asp:ListItem>

                        <asp:ListItem Text="Stock Value by Dealer" Value="stock">
                        </asp:ListItem>

                        <asp:ListItem Text="Orders by Source" Value="ordersource">
                        </asp:ListItem>

                        <asp:ListItem Text="Returns" Value="returns">
                        </asp:ListItem>

                        <asp:ListItem Text="Company Receipts" Value="receipts">
                        </asp:ListItem>

                    </asp:DropDownList>

                </div>


                <asp:Button
                    ID="btnExport"
                    runat="server"
                    Text="Export CSV"
                    CssClass="btn btn-outline-secondary"
                    OnClick="btnExport_Click">
                </asp:Button>

            </div>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvReport"
                runat="server"
                AutoGenerateColumns="True"
                CssClass="table table-hover align-middle"
                GridLines="None" AllowPaging="true" OnPageIndexChanging="gvReport_PageIndexChanging" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-bar-chart fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No data for the selected filters.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>
