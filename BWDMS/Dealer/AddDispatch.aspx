<%@ Page Title="Dispatch"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddDispatch.aspx.cs"
    Inherits="BWDMS.Dealer.AddDispatch" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    New Dispatch

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                New Dispatch
            </h3>

            <p class="text-muted mb-0">
                Load a vehicle - the selected orders go out and the packets leave the godown
            </p>

        </div>


        <a href="Dispatches.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Dispatches

        </a>

    </div>


    <div class="dashboard-card">


        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <!-- Header fields -->

        <div class="row g-4 mb-4">

            <div class="col-md-3">

                <label class="form-label fw-semibold">

                    Dispatch Date

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtDispatchDate"
                    runat="server"
                    CssClass="form-control"
                    placeholder="YYYY-MM-DD">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvDispatchDate"
                    ValidationGroup="Dispatch"
                    runat="server"
                    ControlToValidate="txtDispatchDate"
                    ErrorMessage="Dispatch date is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>


                <asp:RegularExpressionValidator
                    ID="revDispatchDate"
                    ValidationGroup="Dispatch"
                    runat="server"
                    ControlToValidate="txtDispatchDate"
                    ValidationExpression="^\d{4}-\d{2}-\d{2}$"
                    ErrorMessage="Dispatch date must be YYYY-MM-DD."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RegularExpressionValidator>

            </div>


            <div class="col-md-3">

                <label class="form-label fw-semibold">

                    Vehicle

                </label>


                <asp:DropDownList
                    ID="ddlVehicle"
                    runat="server"
                    CssClass="form-select">
                </asp:DropDownList>

            </div>


            <div class="col-md-3">

                <label class="form-label fw-semibold">

                    Route Schedule

                </label>


                <asp:DropDownList
                    ID="ddlRouteSchedule"
                    runat="server"
                    CssClass="form-select">
                </asp:DropDownList>

            </div>


            <div class="col-md-3">

                <label class="form-label fw-semibold">

                    Salesman

                </label>


                <asp:DropDownList
                    ID="ddlSalesman"
                    runat="server"
                    CssClass="form-select">
                </asp:DropDownList>

            </div>


            <div class="col-md-12">

                <label class="form-label fw-semibold">

                    Remarks

                </label>


                <asp:TextBox
                    ID="txtRemarks"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="500"
                    TextMode="MultiLine"
                    Rows="2"
                    placeholder="Delivery instructions, notes...">
                </asp:TextBox>

            </div>

        </div>


        <!-- Order picker -->

        <div class="mb-3">

            <h5 class="fw-bold mb-1">
                Orders going out
            </h5>

            <small class="text-muted">
                Tick every order that is being loaded on this vehicle. Stock is deducted once for the whole dispatch.
            </small>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvOrders"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                DataKeyNames="OrderId" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:TemplateField
                        HeaderText="Take">

                        <ItemTemplate>

                            <asp:CheckBox
                                ID="chkSelect"
                                runat="server" />

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:BoundField
                        DataField="OrderNumber"
                        HeaderText="Order No." />

                    <asp:BoundField
                        DataField="ShopName"
                        HeaderText="Shop" />

                    <asp:BoundField
                        DataField="LineCount"
                        HeaderText="Lines" />

                    <asp:BoundField
                        DataField="GrandTotalText"
                        HeaderText="Grand Total" />

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-cart-check fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No pending or confirmed orders are waiting to go out.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>


        <asp:CustomValidator
            ID="cvOrders"
            runat="server"
            ValidationGroup="Dispatch"
            ServerValidate="cvOrders_ServerValidate"
            ErrorMessage="Select at least one order to dispatch."
            CssClass="text-danger small"
            Display="Dynamic">
        </asp:CustomValidator>


        <hr class="my-4" />


        <div class="d-flex justify-content-end gap-2">

            <a href="Dispatches.aspx"
               class="btn btn-light border">

                <i class="bi bi-x-lg"></i>

                Cancel

            </a>


            <asp:Button
                ID="btnSave"
                ValidationGroup="Dispatch"
                runat="server"
                Text="Save Dispatch"
                CssClass="btn btn-danger px-4"
                OnClick="btnSave_Click" />

        </div>


    </div>

</asp:Content>
