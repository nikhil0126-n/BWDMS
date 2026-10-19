<%@ Page Title="Route Details"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="RouteDetails.aspx.cs"
    Inherits="BWDMS.Dealer.RouteDetails" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Route Details

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <style>
        .rd-back { display: inline-flex; align-items: center; gap: 10px; font-size: 1.6rem; font-weight: 700; color: #111; text-decoration: none; }
        .rd-back:hover { color: #111; text-decoration: none; }
        .rd-back i { font-size: 1.5rem; }
        .rd-subtitle { color: #9aa0a6; font-size: .85rem; margin: 2px 0 0 2px; }
        .rd-heading { display: flex; align-items: center; gap: 12px; font-size: 1.5rem; font-weight: 700; color: #111; margin: 34px 0 26px; }
        .rd-heading i { font-size: 1.4rem; }
        .rd-card { background: #fff; border: 1px solid #d9d9d9; border-radius: 10px; padding: 26px; }
        .rd-profile { display: flex; align-items: center; gap: 34px; padding: 26px 30px; }
        .rd-avatar { width: 92px; height: 92px; border-radius: 50%; background: linear-gradient(135deg,#8fd3f7,#4fb6e8); display: flex; align-items: center; justify-content: center; color: #2f3a45; font-size: 2.6rem; flex: 0 0 92px; }
        .rd-name { font-size: 1.15rem; font-weight: 700; color: #111; margin: 0; }
        .rd-status { color: #17c964; font-weight: 700; font-size: 1rem; margin: 6px 0 0; }
        .rd-info { margin-top: 30px; padding: 0 6px; }
        .rd-info p { margin: 0 0 16px; color: #333; font-size: .95rem; }
        .rd-info p b { font-weight: 700; }
        .rd-table-card { border: 1px solid #d9d9d9; border-radius: 10px; overflow: hidden; }
        .rd-table-card h5 { font-size: 1.15rem; font-weight: 700; color: #111; margin: 0; padding: 20px 24px; border-bottom: 1px solid #d9d9d9; }
        .rd-table { width: 100%; border-collapse: collapse; }
        .rd-table th, .rd-table td { border: 1px solid #d9d9d9; padding: 16px 24px; text-align: left; font-size: 1rem; }
        .rd-table th { font-weight: 700; color: #111; background: #fff; }
        .rd-table td { color: #111; font-weight: 600; }
    </style>

    <!-- Page Header -->

    <a href="Routes.aspx" class="rd-back">
        <i class="bi bi-arrow-left"></i> Back
    </a>

    <p class="rd-subtitle">Manage All Routes</p>

    <h3 class="rd-heading">
        <i class="bi bi-arrow-left"></i> Routes Details
    </h3>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <div class="row g-4 mb-4">


        <!-- Route Information -->

        <div class="col-lg-5">

            <div class="rd-card rd-profile">

                <div class="rd-avatar">
                    <i class="bi bi-geo-alt-fill"></i>
                </div>

                <div>

                    <p class="rd-name">

                        <asp:Label
                            ID="lblRouteName"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </p>

                    <p class="rd-status">

                        <asp:Label
                            ID="lblStatus"
                            runat="server"
                            Text="Active">
                        </asp:Label>

                    </p>

                </div>

            </div>

            <div class="rd-card" style="border-top: 0; border-radius: 0 0 10px 10px; margin-top: -12px; padding-top: 34px;">

                <div class="rd-info">

                    <p>
                        description&nbsp; :&nbsp;
                        <asp:Label ID="lblRouteType" runat="server" Text="-" />
                        ,
                        <asp:Label ID="lblDays" runat="server" Text="-" />
                    </p>

                    <p>
                        No.&nbsp; of shops&nbsp; :-&nbsp;
                        <asp:Label ID="lblVillageCount" runat="server" Text="0" />
                    </p>

                    <p>
                        Sales Man&nbsp; :&nbsp;
                        <asp:Label ID="lblSalesman" runat="server" Text="-" />
                    </p>

                    <p>
                        vehicles:&nbsp;
                        <asp:Label ID="lblVehicle" runat="server" Text="-" />
                    </p>

                    <p class="text-muted mb-0" style="font-size: .85rem;">
                        Route Code :
                        <asp:Label ID="lblRouteCode" runat="server" Text="-" />
                        &nbsp;|&nbsp; Added On :
                        <asp:Label ID="lblCreatedAt" runat="server" Text="-" />
                    </p>

                </div>

            </div>

        </div>


        <!-- Shops In This Route -->

        <div class="col-lg-7">

            <div class="rd-table-card">

                <h5>
                    Shop in this Route
                </h5>

                <asp:GridView
                    ID="gvShops"
                    runat="server"
                    AutoGenerateColumns="False"
                    CssClass="rd-table mb-0"
                    GridLines="None"
                    AllowPaging="true" OnPageIndexChanging="gvShops_PageIndexChanging" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                    <Columns>

                        <asp:BoundField
                            DataField="ShopName"
                            HeaderText="Shop name" />

                        <asp:BoundField
                            DataField="VillageName"
                            HeaderText="Location" />

                    </Columns>


                    <EmptyDataTemplate>

                        <div class="text-center py-4">

                            <p class="text-muted mb-0">
                                No shops are assigned to this route.
                            </p>

                        </div>

                    </EmptyDataTemplate>

                </asp:GridView>

            </div>

        </div>

    </div>


</asp:Content>
