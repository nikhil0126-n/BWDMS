<%@ Page Title="Route History"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="RouteHistory.aspx.cs"
    Inherits="BWDMS.Dealer.RouteHistory" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Route History

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1 page-title">
                Route History
            </h3>

            <p class="text-muted mb-0 page-subtitle">
                Every run of every route: loading, orders and whether the day was closed
            </p>

        </div>


        <div class="d-flex gap-2">

            <a href="Routes.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-signpost"></i>

                Routes

            </a>


            <a href="Reports.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-journal-text"></i>

                Reports

            </a>

        </div>

    </div>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <!-- Route Runs -->

    <div class="summary-box mb-4">

        <h5>
            Route runs
        </h5>

        <p class="text-muted small mb-3">
            One row for each date a route loaded, took orders or was closed
        </p>


        <!-- Filters -->

        <div class="row g-2 mb-0">

            <div class="col-md-3">

                <label class="form-label">
                    From
                </label>

                <asp:TextBox
                    ID="txtFromDate"
                    runat="server"
                    CssClass="form-control"
                    TextMode="Date">
                </asp:TextBox>

                <asp:CompareValidator
                    ID="cvFromDate"
                    runat="server"
                    ControlToValidate="txtFromDate"
                    Operator="DataTypeCheck"
                    Type="Date"
                    Display="Dynamic"
                    CssClass="text-danger small"
                    ErrorMessage="Enter a valid date.">
                </asp:CompareValidator>

            </div>


            <div class="col-md-3">

                <label class="form-label">
                    To
                </label>

                <asp:TextBox
                    ID="txtToDate"
                    runat="server"
                    CssClass="form-control"
                    TextMode="Date">
                </asp:TextBox>

                <asp:CompareValidator
                    ID="cvToDate"
                    runat="server"
                    ControlToValidate="txtToDate"
                    Operator="DataTypeCheck"
                    Type="Date"
                    Display="Dynamic"
                    CssClass="text-danger small"
                    ErrorMessage="Enter a valid date.">
                </asp:CompareValidator>

            </div>


            <div class="col-md-3">

                <label class="form-label">
                    Route
                </label>

                <asp:DropDownList
                    ID="ddlRoute"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">
                </asp:DropDownList>

            </div>


            <div class="col-md-3 d-flex align-items-end">

                <asp:Button
                    ID="btnShow"
                    runat="server"
                    Text="Show"
                    CssClass="btn btn-primary w-100"
                    OnClick="btnShow_Click">
                </asp:Button>

            </div>

        </div>

    </div>


    <div class="table-responsive">

            <asp:GridView
                ID="gvRouteHistory"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                DataKeyNames="ActivityDateText"
                AllowPaging="true"
                OnPageIndexChanging="gvRouteHistory_PageIndexChanging"
                PageSize="25"
                PagerSettings-Mode="NumericFirstLast"
                PagerSettings-PageButtonCount="10"
                PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="ActivityDateText"
                        HeaderText="Date" />

                    <asp:BoundField
                        DataField="DayOfWeek"
                        HeaderText="Day" />

                    <asp:BoundField
                        DataField="RouteName"
                        HeaderText="Route" />

                    <asp:BoundField
                        DataField="VehicleNumber"
                        HeaderText="Vehicle" />

                    <asp:BoundField
                        DataField="SalesmanName"
                        HeaderText="Salesman" />

                    <asp:BoundField
                        DataField="DriverName"
                        HeaderText="Driver" />

                    <asp:BoundField
                        DataField="LoadedPackets"
                        HeaderText="Loaded" />

                    <asp:BoundField
                        DataField="SoldPackets"
                        HeaderText="Sold" />

                    <asp:BoundField
                        DataField="OrderCount"
                        HeaderText="Orders" />

                    <asp:BoundField
                        DataField="OrderValue"
                        DataFormatString="{0:N2}"
                        HeaderText="Order Value" />

                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Convert.ToInt32(Eval("IsClosed")) == 1
                                ? "badge bg-success"
                                : "badge bg-warning text-dark" %>'>

                                <%# Convert.ToInt32(Eval("IsClosed")) == 1
                                    ? "Closed"
                                    : "Open" %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-signpost fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No route runs in this period.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

    </div>


</asp:Content>
