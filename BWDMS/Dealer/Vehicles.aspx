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

            <h3 class="fw-bold mb-1 page-title">
                Vehicles
            </h3>

            <p class="text-muted mb-0 page-subtitle">
                Trucks and tempos used for route dispatch
            </p>

        </div>


        <a href="AddVehicle.aspx"
           class="btn btn-primary">

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
         SEARCH
         ============================================================ -->

    <div class="grid-search-box mb-4">

        <i class="bi bi-search search-icon"></i>

        <asp:TextBox
            ID="txtSearch"
            runat="server"
            CssClass="form-control"
            placeholder="Search vehicle..."
            ClientIDMode="Static">
        </asp:TextBox>

    </div>



    <!-- ============================================================
         VEHICLE LIST CARD
         ============================================================ -->

    <div class="table-responsive">

            <asp:GridView
                ID="gvVehicles"
                runat="server"
                ClientIDMode="Static"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                EmptyDataText="No vehicles found. Click &quot;Add Vehicle&quot; to create the first one." AllowPaging="true" OnPageIndexChanging="gvVehicles_PageIndexChanging" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

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
                                ID="lnkDetails"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-primary"
                                NavigateUrl='<%# "~/Dealer/VehicleDetails.aspx?id=" + Eval("VehicleId") %>'>

                                <i class="bi bi-eye"></i>

                            </asp:HyperLink>


                            <asp:HyperLink
                                ID="lnkEdit"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-danger"
                                NavigateUrl='<%# "~/Dealer/AddVehicle.aspx?id=" + Eval("VehicleId") %>'>

                                <i class="bi bi-pencil"></i>

                                Edit

                            </asp:HyperLink>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>

            </asp:GridView>

    </div>


</asp:Content>
