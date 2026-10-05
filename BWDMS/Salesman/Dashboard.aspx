<%@ Page Title="Salesman Dashboard"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Dashboard.aspx.cs"
    Inherits="BWDMS.Salesman.Dashboard" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Salesman Dashboard

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
                Salesman Dashboard
            </h3>

            <p class="text-muted mb-0">
                Your assigned routes, visits and orders
            </p>

        </div>

    </div>


    <!-- ============================================================
         STAT CARDS
         ============================================================ -->

    <div class="row g-4 mb-4">

        <div class="col-sm-6 col-xl-3">

            <div class="dashboard-card text-center">

                <h2 class="fw-bold mb-0">
                    <asp:Label ID="lblRouteCount" runat="server" Text="0" />
                </h2>

                <small class="text-muted">
                    Assigned Routes
                </small>

            </div>

        </div>


        <div class="col-sm-6 col-xl-3">

            <div class="dashboard-card text-center">

                <h2 class="fw-bold mb-0">
                    <asp:Label ID="lblScheduleCount" runat="server" Text="0" />
                </h2>

                <small class="text-muted">
                    Today's Visits
                </small>

            </div>

        </div>


        <div class="col-sm-6 col-xl-3">

            <div class="dashboard-card text-center">

                <h2 class="fw-bold mb-0">
                    <asp:Label ID="lblVillageCount" runat="server" Text="0" />
                </h2>

                <small class="text-muted">
                    Villages On Beat
                </small>

            </div>

        </div>


        <div class="col-sm-6 col-xl-3">

            <div class="dashboard-card text-center">

                <h2 class="fw-bold mb-0">
                    <asp:Label ID="lblVehicle" runat="server" Text="-" />
                </h2>

                <small class="text-muted">
                    Assigned Vehicle
                </small>

            </div>

        </div>

    </div>


    <!-- ============================================================
         TODAY'S BEAT
         ============================================================ -->

    <div class="dashboard-card">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    Today's Beat
                </h5>

                <small class="text-muted">
                    <asp:Label ID="lblTodayName" runat="server" Text="" />
                    — villages in visit order
                </small>

            </div>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvBeat"
                runat="server"
                ClientIDMode="Static"
                AutoGenerateColumns="False"
                CssClass="table route-table align-middle mb-0"
                GridLines="None"
                EmptyDataText="No villages are scheduled for today." AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">
                <Columns>

                    <asp:BoundField
                        DataField="VisitSequence"
                        HeaderText="Seq">
                        <HeaderStyle CssClass="route-code-col" />
                        <ItemStyle CssClass="route-code-col" />
                    </asp:BoundField>

                    <asp:BoundField
                        DataField="VillageName"
                        HeaderText="Village">
                        <HeaderStyle CssClass="route-name-col" />
                        <ItemStyle CssClass="route-name-col" />
                    </asp:BoundField>

                    <asp:BoundField
                        DataField="Taluka"
                        HeaderText="Taluka" />

                    <asp:BoundField
                        DataField="District"
                        HeaderText="District" />

                    <asp:BoundField
                        DataField="RouteName"
                        HeaderText="Route" />

                    <asp:BoundField
                        DataField="DayOfWeek"
                        HeaderText="Day" />

                </Columns>
            </asp:GridView>

        </div>

    </div>


</asp:Content>
