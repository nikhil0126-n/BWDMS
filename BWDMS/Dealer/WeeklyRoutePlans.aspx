
<%@ Page Title="Weekly Route Plans"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="WeeklyRoutePlans.aspx.cs"
    Inherits="BWDMS.Dealer.WeeklyRoutePlans" %>

<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Weekly Route Plans

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <link href="../Content/WeeklyRoutePlans.css"
          rel="stylesheet" />

    <div class="page-header d-flex justify-content-between align-items-center mb-4">

        <div>
            <h3 class="fw-bold mb-1">
                Weekly Route Plans
            </h3>

            <p class="text-muted mb-0">
                Plan your weekly routes and assign villages
            </p>
        </div>

        <a href="AddWeeklyRoutePlan.aspx"
           class="btn btn-danger">

            <i class="bi bi-plus-lg me-1"></i>
            Add Weekly Plan

        </a>

    </div>


    <div class="dashboard-card">

        <div class="route-plan-header d-flex justify-content-between align-items-center mb-3">

            <div>
                <h5 class="fw-bold mb-1">
                    Weekly Plan List
                </h5>

                <small class="text-muted">
                    Manage your weekly route planning
                </small>
            </div>

            <div class="search-container">

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    ClientIDMode="Static"
                    CssClass="form-control"
                    placeholder="Search route or date..."
                    autocomplete="off">
                </asp:TextBox>

            </div>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvWeeklyPlans"
                runat="server"
                ClientIDMode="Static"
                AutoGenerateColumns="False"
                CssClass="table weekly-plan-table align-middle mb-0"
                GridLines="None"
                EmptyDataText="No weekly route plans found.">

                <Columns>

                    <asp:BoundField
                        DataField="WeeklyRoutePlanId"
                        HeaderText="ID">
                    </asp:BoundField>


                    <asp:BoundField
                        DataField="RouteCode"
                        HeaderText="Route Code">
                    </asp:BoundField>


                    <asp:BoundField
                        DataField="RouteName"
                        HeaderText="Route Name">
                    </asp:BoundField>


                    <asp:BoundField
                        DataField="WeekStartDate"
                        HeaderText="Week Start"
                        DataFormatString="{0:dd-MM-yyyy}">
                    </asp:BoundField>


                    <asp:BoundField
                        DataField="WeekEndDate"
                        HeaderText="Week End"
                        DataFormatString="{0:dd-MM-yyyy}">
                    </asp:BoundField>


                    <asp:TemplateField HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge bg-success" : "badge bg-secondary" %>'>

                                <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:TemplateField HeaderText="Action">

                        <ItemTemplate>

                            <a class="btn btn-outline-danger btn-sm"
                               href='<%# "AddWeeklyRoutePlan.aspx?id=" + Eval("WeeklyRoutePlanId") %>'>

                                <i class="bi bi-pencil me-1"></i>
                                Edit

                            </a>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>

            </asp:GridView>

        </div>

    </div>


    <script src="../Scripts/WeeklyRoutePlans.js"></script>

</asp:Content>