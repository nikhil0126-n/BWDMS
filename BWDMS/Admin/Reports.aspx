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


    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Reports
            </h3>

            <p class="text-muted mb-0">
                System-wide summary
            </p>

        </div>


        <div class="d-flex gap-2">

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

            <div class="dashboard-card text-center">

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


        <div class="col-md-3">

            <div class="dashboard-card text-center">

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


        <div class="col-md-3">

            <div class="dashboard-card text-center">

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


        <div class="col-md-3">

            <div class="dashboard-card text-center">

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
                GridLines="None" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

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
