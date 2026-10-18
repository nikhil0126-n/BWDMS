<%@ Page Title="Dispatch Details"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="DispatchDetails.aspx.cs"
    Inherits="BWDMS.Dealer.DispatchDetails" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Dispatch Details

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1 page-title">
                Dispatch Details
            </h3>

            <p class="text-muted mb-0 page-subtitle">
                Header, lines and the shops this vehicle load covered
            </p>

        </div>


        <a href="Dispatches.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Dispatches

        </a>

    </div>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <asp:Panel
        ID="pnlDispatch"
        runat="server">


        <!-- Identity + Summary -->

        <div class="row g-4 mb-4">

            <div class="col-lg-4">

                <div class="profile-card">

                    <div class="profile-avatar">

                        <i class="bi bi-truck"></i>

                    </div>


                    <p class="profile-name mb-1">

                        <asp:Label
                            ID="lblDispatchNumber"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </p>


                    <div class="profile-state">

                        <asp:Label
                            ID="lblStatus"
                            runat="server"
                            CssClass="badge bg-success"
                            Text="Dispatched">
                        </asp:Label>

                    </div>


                    <div class="profile-list">

                        <div>

                            <span>Dispatch Date</span>

                            <asp:Label
                                ID="lblDispatchDate"
                                runat="server"
                                Text="-">
                            </asp:Label>

                        </div>


                        <div>

                            <span>Vehicle</span>

                            <asp:Label
                                ID="lblVehicle"
                                runat="server"
                                Text="-">
                            </asp:Label>

                        </div>


                        <div>

                            <span>Salesman</span>

                            <asp:Label
                                ID="lblSalesman"
                                runat="server"
                                Text="-">
                            </asp:Label>

                        </div>


                        <div>

                            <span>Route Schedule</span>

                            <asp:Label
                                ID="lblRouteSchedule"
                                runat="server"
                                Text="-">
                            </asp:Label>

                        </div>


                        <div>

                            <span>Created</span>

                            <asp:Label
                                ID="lblCreatedAt"
                                runat="server"
                                Text="-">
                            </asp:Label>

                        </div>


                        <div>

                            <span>Remarks</span>

                            <asp:Label
                                ID="lblRemarks"
                                runat="server"
                                Text="-">
                            </asp:Label>

                        </div>

                    </div>

                </div>

            </div>


            <div class="col-lg-8">

                <div class="summary-box">

                    <h5>
                        Dispatch Summary
                    </h5>


                    <div class="stat-grid">

                        <div class="stat-tile">

                            <i class="bi bi-box-seam"></i>

                            <div>

                                <span class="stat-label">
                                    Total Qty
                                </span>


                                <span class="stat-value">

                                    <asp:Label
                                        ID="lblTotalQty"
                                        runat="server"
                                        Text="0">
                                    </asp:Label>

                                </span>

                            </div>

                        </div>


                        <div class="stat-tile">

                            <i class="bi bi-boxes"></i>

                            <div>

                                <span class="stat-label">
                                    Total Packets
                                </span>


                                <span class="stat-value">

                                    <asp:Label
                                        ID="lblTotalPackets"
                                        runat="server"
                                        Text="0">
                                    </asp:Label>

                                </span>

                            </div>

                        </div>

                    </div>

                </div>

            </div>

        </div>


        <!-- Lines -->

        <h5 class="fw-bold mb-1">
            Lines
        </h5>

        <small class="text-muted d-block mb-3">
            Packets that left the godown for this dispatch
        </small>


        <div class="table-responsive mb-4">

            <asp:GridView
                ID="gvLines"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle mb-0"
                GridLines="None" AllowPaging="true" OnPageIndexChanging="gvLines_PageIndexChanging" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="ProductName"
                        HeaderText="Product" />

                    <asp:BoundField
                        DataField="VariantName"
                        HeaderText="Variant" />

                    <asp:BoundField
                        DataField="UnitCode"
                        HeaderText="Unit" />

                    <asp:BoundField
                        DataField="Quantity"
                        HeaderText="Qty"
                        ItemStyle-CssClass="text-end"
                        HeaderStyle-CssClass="text-end" />

                    <asp:BoundField
                        DataField="QuantityPackets"
                        HeaderText="Packets"
                        ItemStyle-CssClass="text-end"
                        HeaderStyle-CssClass="text-end" />

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-4">

                        <p class="text-muted mb-0">
                            This dispatch has no lines.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>


        <!-- Orders Covered -->

        <h5 class="fw-bold mb-1">
            Orders Covered
        </h5>

        <small class="text-muted d-block mb-3">
            Shops that went out on this vehicle
        </small>


        <div class="table-responsive">

            <asp:GridView
                ID="gvOrders"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle mb-0"
                GridLines="None" AllowPaging="true" OnPageIndexChanging="gvOrders_PageIndexChanging" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="OrderNumber"
                        HeaderText="Order No." />

                    <asp:BoundField
                        DataField="ShopName"
                        HeaderText="Shop" />

                    <asp:BoundField
                        DataField="Status"
                        HeaderText="Status" />

                    <asp:BoundField
                        DataField="GrandTotalText"
                        HeaderText="Grand Total"
                        ItemStyle-CssClass="text-end"
                        HeaderStyle-CssClass="text-end" />

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-4">

                        <p class="text-muted mb-0">
                            No orders are linked to this dispatch.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>


    </asp:Panel>

</asp:Content>
