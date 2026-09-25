
<%@ Page Title="Route Schedules"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="RouteSchedules.aspx.cs"
    Inherits="BWDMS.Dealer.RouteSchedules" %>

<asp:Content ID="Content1"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <link href="../Content/RouteSchedules.css" rel="stylesheet" />

    <div class="route-page">

        <div class="page-header">

            <div>
                <h2>Route Schedules</h2>

                <p>
                    Manage routes, vehicles, salesmen, and drivers.
                </p>
            </div>

            <a href="AddRouteSchedule.aspx"
               class="btn-primary">

                <span>+</span>
                Add Schedule

            </a>

        </div>

        <div class="filter-card">

            <div class="search-wrapper">

                <input type="text"
                       id="txtScheduleSearch"
                       class="search-input"
                       placeholder="Search route, day, vehicle, salesman..." />

            </div>

            <div class="filter-wrapper">

                <asp:DropDownList ID="ddlDayFilter"
                    runat="server"
                    CssClass="filter-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlDayFilter_SelectedIndexChanged">

                    <asp:ListItem Text="All Days"
                        Value="" />

                    <asp:ListItem Text="Monday"
                        Value="Monday" />

                    <asp:ListItem Text="Tuesday"
                        Value="Tuesday" />

                    <asp:ListItem Text="Wednesday"
                        Value="Wednesday" />

                    <asp:ListItem Text="Thursday"
                        Value="Thursday" />

                    <asp:ListItem Text="Friday"
                        Value="Friday" />

                    <asp:ListItem Text="Saturday"
                        Value="Saturday" />

                    <asp:ListItem Text="Sunday"
                        Value="Sunday" />

                </asp:DropDownList>

            </div>

        </div>

        <div class="table-card">

            <div class="table-header">

                <h3>All Route Schedules</h3>

                <asp:Label ID="lblTotal"
                    runat="server"
                    CssClass="record-count" />

            </div>

            <div class="table-responsive">

                <asp:GridView ID="gvSchedules"
                    runat="server"
                    AutoGenerateColumns="False"
                    CssClass="schedule-table"
                    GridLines="None"
                    EmptyDataText="No route schedules found."
                    DataKeyNames="RouteScheduleId">

                    <Columns>

                        <asp:BoundField
                            DataField="RouteName"
                            HeaderText="Route" />

                        <asp:BoundField
                            DataField="DayOfWeek"
                            HeaderText="Day" />

                        <asp:BoundField
                            DataField="VehicleNumber"
                            HeaderText="Vehicle" />

                        <asp:BoundField
                            DataField="SalesmanName"
                            HeaderText="Salesman" />

                        <asp:BoundField
                            DataField="DriverName"
                            HeaderText="Driver" />

                        <asp:TemplateField HeaderText="Status">

                            <ItemTemplate>

                                <span class='<%# Convert.ToBoolean(Eval("IsActive"))
                                    ? "status-active"
                                    : "status-inactive" %>'>

                                    <%# Convert.ToBoolean(Eval("IsActive"))
                                        ? "Active"
                                        : "Inactive" %>

                                </span>

                            </ItemTemplate>

                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Action">

                            <ItemTemplate>

                                <a class="btn-edit"
                                   href='<%# "AddRouteSchedule.aspx?id=" + Eval("RouteScheduleId") %>'>

                                    Edit

                                </a>

                            </ItemTemplate>

                        </asp:TemplateField>

                    </Columns>

                </asp:GridView>

            </div>

        </div>

    </div>

    <script src="../Scripts/RouteSchedules.js"></script>

</asp:Content>