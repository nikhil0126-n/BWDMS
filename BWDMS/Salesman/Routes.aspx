<%@ Page Title="My Routes"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Routes.aspx.cs"
    Inherits="BWDMS.Salesman.Routes" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    My Routes

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                My Routes
            </h3>

            <p class="text-muted mb-0">
                The schedules assigned to you
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


    <div class="dashboard-card">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    Routes
                </h5>

                <small class="text-muted">
                    Villages are listed in visit order
                </small>

            </div>


            <div class="grid-search-box">

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Search route name or code..."
                    AutoPostBack="true"
                    OnTextChanged="txtSearch_TextChanged">
                </asp:TextBox>

            </div>

        </div>


        <div class="row g-2 mb-3">

            <div class="col-md-4">

                <asp:DropDownList
                    ID="ddlStatus"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">

                    <asp:ListItem Text="All Statuses" Value="" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Active" Value="1">
                    </asp:ListItem>

                    <asp:ListItem Text="Inactive" Value="0">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvRoutes"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="RouteName"
                        HeaderText="Route" />

                    <asp:BoundField
                        DataField="RouteCode"
                        HeaderText="Code" />

                    <asp:BoundField
                        DataField="DayOfWeek"
                        HeaderText="Day" />

                    <asp:BoundField
                        DataField="OrderDispatchDays"
                        HeaderText="Dispatch Days" />

                    <asp:BoundField
                        DataField="RouteType"
                        HeaderText="Type" />

                    <asp:BoundField
                        DataField="VillageCount"
                        HeaderText="Villages" />

                    <asp:BoundField
                        DataField="VehicleNumber"
                        HeaderText="Vehicle" />

                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge bg-success" : "badge bg-secondary" %>'>

                                <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:TemplateField
                        HeaderText="Action">

                        <ItemTemplate>

                            <a class="btn btn-sm btn-outline-danger"
                               href='<%# ResolveUrl("~/Dealer/ScheduleVillages.aspx?id=" + Eval("RouteScheduleId")) %>'>

                                <i class="bi bi-geo-alt"></i>

                                Villages

                            </a>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-signpost-split fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No routes are assigned to you yet.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>
