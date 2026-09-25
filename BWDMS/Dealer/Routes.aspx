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


    <!-- ============================================================
         PAGE HEADER
         ============================================================ -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Routes
            </h3>

            <p class="text-muted mb-0">
                Manage your delivery and sales routes
            </p>

        </div>


        <a href="AddRoute.aspx"
           class="btn btn-danger">

            <i class="bi bi-plus-lg me-1"></i>

            Add Route

        </a>

    </div>



    <!-- ============================================================
         ROUTE LIST CARD
         ============================================================ -->

    <div class="dashboard-card">


        <!-- ========================================================
             CARD HEADER
             ======================================================== -->

        <div class="d-flex justify-content-between align-items-center mb-3">


            <div>

                <h5 class="fw-bold mb-1">
                    Route List
                </h5>

                <small class="text-muted">
                    Routes assigned to your dealership
                </small>

            </div>



            <!-- ====================================================
                 LIVE SEARCH
                 ==================================================== -->

            <div style="width:250px;">

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Search route..."
                    ClientIDMode="Static">
                </asp:TextBox>

            </div>


        </div>



        <!-- ========================================================
             ROUTE TABLE
             ======================================================== -->

        <div class="table-responsive">


            <asp:GridView
                ID="gvRoutes"
                runat="server"
                ClientIDMode="Static"
                AutoGenerateColumns="False"
                CssClass="table route-table align-middle mb-0"
                GridLines="None">


                <Columns>



                    <asp:BoundField
                        DataField="RouteCode"
                        HeaderText="Code">

                        <HeaderStyle
                            CssClass="route-code-col" />

                        <ItemStyle
                            CssClass="route-code-col" />

                    </asp:BoundField>




                    <asp:BoundField
                        DataField="RouteName"
                        HeaderText="Route Name">

                        <HeaderStyle
                            CssClass="route-name-col" />

                        <ItemStyle
                            CssClass="route-name-col" />

                    </asp:BoundField>




                    <asp:BoundField
                        DataField="DayOfWeek"
                        HeaderText="Day">

                        <HeaderStyle
                            CssClass="route-day-col" />

                        <ItemStyle
                            CssClass="route-day-col" />

                    </asp:BoundField>




                    <asp:TemplateField
                        HeaderText="Status">


                        <HeaderStyle
                            CssClass="route-status-col" />


                        <ItemStyle
                            CssClass="route-status-col" />


                        <ItemTemplate>


                            <span class='<%# Convert.ToBoolean(Eval("IsActive"))
                                ? "badge bg-success"
                                : "badge bg-secondary" %>'>


                                <%# Convert.ToBoolean(Eval("IsActive"))
                                    ? "Active"
                                    : "Inactive" %>


                            </span>


                        </ItemTemplate>


                    </asp:TemplateField>




                    <asp:TemplateField
                        HeaderText="Action">


                        <HeaderStyle
                            CssClass="route-action-col" />


                        <ItemStyle
                            CssClass="route-action-col" />


                        <ItemTemplate>


                            <asp:HyperLink
                                ID="lnkEdit"
                                runat="server"
                                CssClass="btn btn-outline-danger btn-sm"
                                NavigateUrl='<%# "~/Dealer/AddRoute.aspx?id=" + Eval("RouteId") %>'>


                                <i class="bi bi-pencil me-1"></i>

                                Edit


                            </asp:HyperLink>


                        </ItemTemplate>


                    </asp:TemplateField>


                </Columns>


            </asp:GridView>


        </div>


    </div>



</asp:Content>