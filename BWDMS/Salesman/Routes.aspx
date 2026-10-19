<%@ Page Title="My Route"
    Language="C#"
    MasterPageFile="~/Master/SalesmanMaster.master"
    AutoEventWireup="true"
    CodeBehind="Routes.aspx.cs"
    Inherits="BWDMS.Salesman.Routes" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    My Route

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- ============================================================
         PAGE HEADER
         ============================================================ -->

    <div class="sw-page-head">

        <div>

            <h3>
                My Route
            </h3>

            <p>
                Your assigned beat and stop list
            </p>

        </div>

    </div>


    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>



    <!-- ============================================================
         ROUTE SUMMARY
         ============================================================ -->

    <div class="sw-card mb-4">

        <div class="sw-route-head">

            <div class="sw-stat-icon sw-tint-blue">

                <i class="bi bi-signpost-split"></i>

            </div>


            <div>

                <div class="sw-route-name">

                    <asp:Label
                        ID="lblRouteName"
                        runat="server"
                        Text="-">
                    </asp:Label>

                </div>


                <div class="sw-route-meta">

                    <span class="sw-pill sw-pill-teal">

                        <asp:Label
                            ID="lblRouteStatus"
                            runat="server"
                            Text="On Route">
                        </asp:Label>

                    </span>


                    <asp:Label
                        ID="lblVillages"
                        runat="server"
                        Text="0 villages">
                    </asp:Label>

                </div>

            </div>

        </div>


        <div class="sw-route-stats">

            <div class="sw-route-stat">

                <div class="sw-route-stat-lbl">
                    Vehicle
                </div>

                <div class="sw-route-stat-val">

                    <asp:Label
                        ID="lblVehicle"
                        runat="server"
                        Text="-">
                    </asp:Label>

                </div>

            </div>


            <div class="sw-route-stat">

                <div class="sw-route-stat-lbl">
                    Loaded
                </div>

                <div class="sw-route-stat-val">
                    &mdash;
                </div>

            </div>


            <div class="sw-route-stat">

                <div class="sw-route-stat-lbl">
                    Remaining
                </div>

                <div class="sw-route-stat-val">
                    &mdash;
                </div>

            </div>

        </div>

    </div>



    <!-- ============================================================
         ROUTE STOPS
         ============================================================ -->

    <div class="sw-card">

        <div class="sw-card-title">

            Route Stops (<asp:Label
                ID="lblStopCount"
                runat="server"
                Text="0" />)

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvRoutes"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="sw-table"
                GridLines="None"
                AllowPaging="true"
                OnPageIndexChanging="gvRoutes_PageIndexChanging"
                PageSize="25"
                PagerSettings-Mode="NumericFirstLast"
                PagerSettings-PageButtonCount="10"
                PagerSettings-Position="Bottom">

                <Columns>

                    <asp:TemplateField HeaderText="#">

                        <ItemTemplate>

                            <span class="sw-num">
                                <%# Container.DataItemIndex + 1 %>
                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:BoundField
                        DataField="ShopName"
                        HeaderText="Shop Name" />

                    <asp:BoundField
                        DataField="VillageName"
                        HeaderText="Location" />


                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <span class="sw-badge sw-badge-warning">
                                Pending
                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:TemplateField
                        HeaderText="Action">

                        <ItemTemplate>

                            <button type="button"
                                    class="sw-icon-btn"
                                    title="Open stop">

                                <i class="bi bi-chevron-right"></i>

                            </button>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-signpost-split fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No stops on your routes yet.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>
