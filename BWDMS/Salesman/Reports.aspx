<%@ Page Title="My Reports"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
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


    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                My Reports
            </h3>

            <p class="text-muted mb-0">
                Your routes, orders and stock
            </p>

        </div>


        <a href="Dashboard.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Dashboard

        </a>

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
                    Orders Taken
                </div>


                <div class="fw-bold fs-4">

                    <asp:Label
                        ID="lblKpiOrders"
                        runat="server"
                        Text="0">
                    </asp:Label>

                </div>

            </div>

        </div>


        <div class="col-md-3">

            <div class="dashboard-card text-center">

                <div class="text-muted small">
                    Order Value
                </div>


                <div class="fw-bold fs-4 text-danger">

                    <asp:Label
                        ID="lblKpiValue"
                        runat="server"
                        Text="0.00">
                    </asp:Label>

                </div>

            </div>

        </div>


        <div class="col-md-3">

            <div class="dashboard-card text-center">

                <div class="text-muted small">
                    Shops Served
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
                    Routes
                </div>


                <div class="fw-bold fs-4">

                    <asp:Label
                        ID="lblKpiRoutes"
                        runat="server"
                        Text="0">
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
                        Text="Orders by Status">
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


            <div class="grid-search-box">

                <asp:DropDownList
                    ID="ddlReport"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ReportChanged">

                    <asp:ListItem Text="Orders by Status" Value="orderstatus" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Orders by Day" Value="orderday">
                    </asp:ListItem>

                    <asp:ListItem Text="Shops by Village" Value="shops">
                    </asp:ListItem>

                    <asp:ListItem Text="Stock Loaded" Value="stock">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvReport"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="Label"
                        HeaderText="Label" />

                    <asp:BoundField
                        DataField="Count"
                        HeaderText="Count" />

                    <asp:BoundField
                        DataField="Amount"
                        HeaderText="Amount" />

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-bar-chart fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            Nothing to report yet.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>
