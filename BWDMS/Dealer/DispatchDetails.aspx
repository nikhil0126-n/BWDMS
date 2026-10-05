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


    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Dispatch Details
            </h3>

            <p class="text-muted mb-0">
                Header, lines and the shops this vehicle load covered
            </p>

        </div>


        <a href="Dispatches.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Dispatches

        </a>

    </div>


    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <asp:Panel
        ID="pnlDispatch"
        runat="server">


        <div class="dashboard-card mb-4">

            <div class="d-flex justify-content-between align-items-center mb-3">

                <div>

                    <small class="text-muted d-block">
                        Dispatch Number
                    </small>


                    <strong>

                        <asp:Label
                            ID="lblDispatchNumber"
                            runat="server"
                            Text="-">
                        </asp:Label>

                    </strong>

                </div>


                <asp:Label
                    ID="lblStatus"
                    runat="server"
                    CssClass="badge bg-success"
                    Text="Dispatched">
                </asp:Label>

            </div>


            <div class="row g-4">

                <div class="col-md-3">

                    <small class="text-muted d-block">
                        Dispatch Date
                    </small>


                    <asp:Label
                        ID="lblDispatchDate"
                        runat="server"
                        CssClass="fw-semibold"
                        Text="-">
                    </asp:Label>

                </div>


                <div class="col-md-3">

                    <small class="text-muted d-block">
                        Vehicle
                    </small>


                    <asp:Label
                        ID="lblVehicle"
                        runat="server"
                        CssClass="fw-semibold"
                        Text="-">
                    </asp:Label>

                </div>


                <div class="col-md-3">

                    <small class="text-muted d-block">
                        Salesman
                    </small>


                    <asp:Label
                        ID="lblSalesman"
                        runat="server"
                        CssClass="fw-semibold"
                        Text="-">
                    </asp:Label>

                </div>


                <div class="col-md-3">

                    <small class="text-muted d-block">
                        Route Schedule
                    </small>


                    <asp:Label
                        ID="lblRouteSchedule"
                        runat="server"
                        CssClass="fw-semibold"
                        Text="-">
                    </asp:Label>

                </div>


                <div class="col-md-3">

                    <small class="text-muted d-block">
                        Created
                    </small>


                    <asp:Label
                        ID="lblCreatedAt"
                        runat="server"
                        Text="-">
                    </asp:Label>

                </div>


                <div class="col-md-9">

                    <small class="text-muted d-block">
                        Remarks
                    </small>


                    <asp:Label
                        ID="lblRemarks"
                        runat="server"
                        Text="-">
                    </asp:Label>

                </div>

            </div>

        </div>


        <div class="dashboard-card mb-4">

            <h5 class="fw-bold mb-1">
                Lines
            </h5>

            <small class="text-muted d-block mb-3">
                Packets that left the godown for this dispatch
            </small>


            <div class="table-responsive">

                <asp:GridView
                    ID="gvLines"
                    runat="server"
                    AutoGenerateColumns="False"
                    CssClass="table table-hover align-middle mb-0"
                    GridLines="None" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

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


            <div class="d-flex justify-content-end py-2 border-top mt-3">

                <span class="text-muted pe-3">

                    Total Qty

                    <strong>

                        <asp:Label
                            ID="lblTotalQty"
                            runat="server"
                            Text="0">
                        </asp:Label>

                    </strong>

                </span>


                <span class="text-muted ps-3">

                    Total Packets

                    <strong>

                        <asp:Label
                            ID="lblTotalPackets"
                            runat="server"
                            Text="0">
                        </asp:Label>

                    </strong>

                </span>

            </div>

        </div>


        <div class="dashboard-card">

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
                    GridLines="None" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

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

        </div>


    </asp:Panel>

</asp:Content>
