<%@ Page Title="Vehicles"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Vehicles.aspx.cs"
    Inherits="BWDMS.Dealer.Vehicles" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Vehicles

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
                Vehicles
            </h3>

            <p class="text-muted mb-0">
                Trucks and tempos used for route dispatch
            </p>

        </div>


        <a href="AddVehicle.aspx"
           class="btn btn-danger">

            <i class="bi bi-plus-lg me-1"></i>

            Add Vehicle

        </a>

    </div>



    <!-- ============================================================
         MESSAGE PANEL
         ============================================================ -->

    <asp:Panel
        ID="pnlMessage"
        runat="server"
        Visible="false"
        CssClass="alert mb-4">

        <asp:Label
            ID="lblMessage"
            runat="server">
        </asp:Label>

    </asp:Panel>



    <!-- ============================================================
         VEHICLE LIST CARD
         ============================================================ -->

    <div class="dashboard-card">


        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    Vehicle List
                </h5>

                <small class="text-muted">
                    Vehicles available to your dealership
                </small>

            </div>


            <div class="grid-search-box">

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Search vehicle..."
                    ClientIDMode="Static">
                </asp:TextBox>

            </div>

        </div>



        <div class="table-responsive">

            <asp:GridView
                ID="gvVehicles"
                runat="server"
                ClientIDMode="Static"
                AutoGenerateColumns="False"
                CssClass="table route-table align-middle mb-0"
                GridLines="None"
                EmptyDataText="No vehicles found. Click &quot;Add Vehicle&quot; to create the first one." AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="VehicleNumber"
                        HeaderText="Vehicle Number" />

                    <asp:BoundField
                        DataField="VehicleName"
                        HeaderText="Vehicle Name" />

                    <asp:BoundField
                        DataField="VehicleType"
                        HeaderText="Type" />

                    <asp:BoundField
                        DataField="OwnerText"
                        HeaderText="Belongs To" />

                    <asp:TemplateField
                        HeaderText="Status">

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
                        HeaderText="Used By">

                        <ItemTemplate>

                            <%# Eval("UsedByText") %>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:TemplateField
                        HeaderText="Action">

                        <ItemTemplate>

                            <asp:HyperLink
                                ID="lnkEdit"
                                runat="server"
                                CssClass="btn btn-outline-danger btn-sm"
                                NavigateUrl='<%# "~/Dealer/AddVehicle.aspx?id=" + Eval("VehicleId") %>'>

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
