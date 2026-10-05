<%@ Page Title="Order"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddOrder.aspx.cs"
    Inherits="BWDMS.Dealer.AddOrder" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    <asp:Label
        ID="lblPageTitle"
        runat="server"
        Text="New Order">
    </asp:Label>

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">

                <asp:Label
                    ID="lblHeading"
                    runat="server"
                    Text="New Order">
                </asp:Label>

            </h3>


            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblSubHeading"
                    runat="server"
                    Text="Prices are read from the product master - no prices are typed in the system">
                </asp:Label>

            </p>

        </div>


        <a href="Orders.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Orders

        </a>

    </div>


    <div class="dashboard-card">


        <div class="d-flex justify-content-between align-items-center mb-4">

            <div>

                <small class="text-muted d-block">
                    Order Number
                </small>


                <strong>

                    <asp:Label
                        ID="lblOrderNo"
                        runat="server"
                        Text="Auto-generated on save">
                    </asp:Label>

                </strong>

            </div>

        </div>


        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <!-- Header fields -->

        <div class="row g-4 mb-4">

            <div class="col-md-4">

                <label class="form-label fw-semibold">

                    Shop

                    <span class="text-danger">*</span>

                </label>


                <asp:DropDownList
                    ID="ddlShop"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlShop_SelectedIndexChanged">
                </asp:DropDownList>


                <asp:RequiredFieldValidator
                    ID="rfvShop"
                    ValidationGroup="Order"
                    runat="server"
                    ControlToValidate="ddlShop"
                    InitialValue=""
                    ErrorMessage="Select a shop."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-4">

                <label class="form-label fw-semibold">
                    Order Type
                </label>


                <asp:DropDownList
                    ID="ddlOrderType"
                    runat="server"
                    CssClass="form-select">

                    <asp:ListItem Text="Standard" Value="Standard" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Order Taking" Value="Order Taking">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>


            <div class="col-md-4">

                <label class="form-label fw-semibold">

                    Status

                    <span class="text-danger">*</span>

                </label>


                <asp:DropDownList
                    ID="ddlStatus"
                    runat="server"
                    CssClass="form-select">

                    <asp:ListItem Text="Pending" Value="Pending" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Confirmed" Value="Confirmed">
                    </asp:ListItem>

                    <asp:ListItem Text="Dispatched" Value="Dispatched">
                    </asp:ListItem>

                    <asp:ListItem Text="Cancelled" Value="Cancelled">
                    </asp:ListItem>

                </asp:DropDownList>


                <small class="text-muted d-block mt-1">
                    Stock leaves the godown only when an order is Dispatched.
                </small>

            </div>


            <div class="col-md-4">

                <label class="form-label fw-semibold">

                    Order Source

                    <span class="text-danger">*</span>

                </label>


                <asp:DropDownList
                    ID="ddlSource"
                    runat="server"
                    CssClass="form-select">

                    <asp:ListItem Text="Counter" Value="Counter" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Beat" Value="Beat">
                    </asp:ListItem>

                    <asp:ListItem Text="Telephone" Value="Telephone">
                    </asp:ListItem>

                </asp:DropDownList>


                <asp:RequiredFieldValidator
                    ID="rfvSource"
                    ValidationGroup="Order"
                    runat="server"
                    ControlToValidate="ddlSource"
                    InitialValue=""
                    ErrorMessage="Select an order source."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-4">

                <label class="form-label fw-semibold">

                    Order Date

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtOrderDate"
                    runat="server"
                    CssClass="form-control"
                    placeholder="YYYY-MM-DD">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvOrderDate"
                    ValidationGroup="Order"
                    runat="server"
                    ControlToValidate="txtOrderDate"
                    ErrorMessage="Order date is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-4">

                <label class="form-label fw-semibold">
                    Delivery Date
                </label>


                <small class="text-muted d-block mb-1">
                    Optional.
                </small>


                <asp:TextBox
                    ID="txtDeliveryDate"
                    runat="server"
                    CssClass="form-control"
                    placeholder="YYYY-MM-DD">
                </asp:TextBox>

            </div>


            <div class="col-md-4">

                <label class="form-label fw-semibold">
                    Route Schedule
                </label>


                <asp:DropDownList
                    ID="ddlSchedule"
                    runat="server"
                    CssClass="form-select">
                </asp:DropDownList>

            </div>


            <div class="col-md-4">

                <label class="form-label fw-semibold">
                    Salesman
                </label>


                <asp:DropDownList
                    ID="ddlSalesman"
                    runat="server"
                    CssClass="form-select">
                </asp:DropDownList>

            </div>


            <div class="col-md-8">

                <label class="form-label fw-semibold">
                    Remarks
                </label>


                <asp:TextBox
                    ID="txtRemarks"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="1000"
                    TextMode="MultiLine"
                    Rows="2"
                    placeholder="Delivery instructions, notes...">
                </asp:TextBox>

            </div>


        </div>


        <!-- Line builder -->

        <div class="mb-3">

            <h5 class="fw-bold mb-1">
                Order Lines
            </h5>

            <small class="text-muted">
                Choose a variant and a selling unit; price is picked from the price master and can be overridden per order.
            </small>

        </div>


        <div class="row g-3 align-items-end mb-3">

            <div class="col-md-4">

                <label class="form-label fw-semibold">

                    Product Variant

                    <span class="text-danger">*</span>

                </label>


                <asp:DropDownList
                    ID="ddlVariant"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="LineInputsChanged">
                </asp:DropDownList>


                <asp:RequiredFieldValidator
                    ID="rfvVariant"
                    ValidationGroup="LineAdd"
                    runat="server"
                    ControlToValidate="ddlVariant"
                    InitialValue=""
                    ErrorMessage="Select a product variant."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-2">

                <label class="form-label fw-semibold">

                    Unit

                    <span class="text-danger">*</span>

                </label>


                <asp:DropDownList
                    ID="ddlUnit"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="LineInputsChanged">
                </asp:DropDownList>


                <asp:RequiredFieldValidator
                    ID="rfvUnit"
                    ValidationGroup="LineAdd"
                    runat="server"
                    ControlToValidate="ddlUnit"
                    InitialValue=""
                    ErrorMessage="Select a unit."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-2">

                <label class="form-label fw-semibold">

                    Qty

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtQuantity"
                    runat="server"
                    CssClass="form-control"
                    TextMode="Number"
                    Text="1">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvQuantity"
                    ValidationGroup="LineAdd"
                    runat="server"
                    ControlToValidate="txtQuantity"
                    ErrorMessage="Quantity is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>


                <asp:RegularExpressionValidator
                    ID="revQuantity"
                    ValidationGroup="LineAdd"
                    runat="server"
                    ControlToValidate="txtQuantity"
                    ValidationExpression="^[1-9]\d{0,8}$"
                    ErrorMessage="Quantity must be a whole number above zero."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RegularExpressionValidator>

            </div>


            <div class="col-md-2">

                <label class="form-label fw-semibold">

                    Unit Price

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtUnitPrice"
                    runat="server"
                    CssClass="form-control"
                    Text="0.00">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvUnitPrice"
                    ValidationGroup="LineAdd"
                    runat="server"
                    ControlToValidate="txtUnitPrice"
                    ErrorMessage="Unit price is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>


                <asp:RegularExpressionValidator
                    ID="revUnitPrice"
                    ValidationGroup="LineAdd"
                    runat="server"
                    ControlToValidate="txtUnitPrice"
                    ValidationExpression="^\d{1,7}(\.\d{1,2})?$"
                    ErrorMessage="Price must be a positive amount."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RegularExpressionValidator>

            </div>


            <div class="col-md-2">

                <asp:Button
                    ID="btnAddLine"
                    ValidationGroup="LineAdd"
                    runat="server"
                    Text="Add Line"
                    CssClass="btn btn-outline-danger w-100"
                    OnClick="btnAddLine_Click" />

            </div>

        </div>


        <asp:Label
            ID="lblLineInfo"
            runat="server"
            CssClass="text-muted small d-block mb-3"
            Visible="false">
        </asp:Label>


        <!-- Lines grid -->

        <asp:Panel
            ID="pnlLines"
            runat="server">

            <div class="table-responsive">

                <asp:Repeater
                    ID="rpLines"
                    runat="server"
                    OnItemCommand="rpLines_ItemCommand">

                    <HeaderTemplate>

                        <table class="table table-hover align-middle mb-0">

                            <thead>

                                <tr>

                                    <th>Product</th>

                                    <th>Variant</th>

                                    <th>Unit</th>

                                    <th class="text-end">Qty</th>

                                    <th class="text-end">Packets</th>

                                    <th class="text-end">Unit Price</th>

                                    <th class="text-end">Line Total</th>

                                    <th></th>

                                </tr>

                            </thead>


                            <tbody>

                    </HeaderTemplate>


                    <ItemTemplate>

                        <tr>

                            <td>

                                <%# Eval("ProductName") %>

                            </td>


                            <td>

                                <%# Eval("VariantName") %>

                            </td>


                            <td>

                                <%# Eval("UnitCode") %>

                            </td>


                            <td class="text-end">

                                <%# Eval("Quantity") %>

                            </td>


                            <td class="text-end">

                                <%# Eval("QuantityPackets") %>

                            </td>


                            <td class="text-end">

                                <%# Convert.ToDecimal(Eval("UnitPrice")).ToString("N2") %>

                            </td>


                            <td class="text-end">

                                <strong>

                                    <%# Convert.ToDecimal(Eval("LineTotal")).ToString("N2") %>

                                </strong>

                            </td>


                            <td class="text-end">

                                <asp:LinkButton
                                    ID="lnkRemove"
                                    runat="server"
                                    CssClass="btn btn-sm btn-outline-danger"
                                    CommandName="Remove"
                                    CommandArgument='<%# Eval("Index") %>'>

                                    <i class="bi bi-trash"></i>

                                    Remove

                                </asp:LinkButton>

                            </td>

                        </tr>

                    </ItemTemplate>


                    <FooterTemplate>

                            </tbody>

                        </table>

                    </FooterTemplate>

                </asp:Repeater>

            </div>

        </asp:Panel>


        <asp:Panel
            ID="pnlNoLines"
            runat="server"
            Visible="false">

            <div class="text-center py-4">

                <i class="bi bi-cart-plus fs-1 text-muted"></i>

                <p class="text-muted mt-3 mb-0">
                    No lines yet - add at least one item above.
                </p>

            </div>

        </asp:Panel>


        <!-- Totals -->

        <div class="row justify-content-end mt-3">

            <div class="col-md-4">

                <div class="d-flex justify-content-between py-2 border-top">

                    <span class="text-muted">
                        Sub Total
                    </span>


                    <strong>

                        <asp:Label
                            ID="lblSubTotal"
                            runat="server"
                            Text="0.00">
                        </asp:Label>

                    </strong>

                </div>


                <div class="d-flex justify-content-between align-items-center py-2 border-top">

                    <label class="text-muted mb-0 pe-2">
                        Discount
                    </label>


                    <asp:TextBox
                        ID="txtDiscount"
                        runat="server"
                        CssClass="form-control form-control-sm w-50 text-end"
                        AutoPostBack="true"
                        OnTextChanged="DiscountChanged"
                        Text="0.00">
                    </asp:TextBox>

                </div>


                <asp:RegularExpressionValidator
                    ID="revDiscount"
                    ValidationGroup="Order"
                    runat="server"
                    ControlToValidate="txtDiscount"
                    ValidationExpression="^\d{1,7}(\.\d{1,2})?$"
                    ErrorMessage="Discount must be zero or more."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RegularExpressionValidator>


                <div class="d-flex justify-content-between py-2 border-top border-danger">

                    <span class="fw-bold">
                        Grand Total
                    </span>


                    <strong class="text-danger fs-5">

                        <asp:Label
                            ID="lblGrandTotal"
                            runat="server"
                            Text="0.00">
                        </asp:Label>

                    </strong>

                </div>

            </div>

        </div>


        <hr class="my-4" />


        <div class="d-flex justify-content-end gap-2">

            <a href="Orders.aspx"
               class="btn btn-light border">

                <i class="bi bi-x-lg"></i>

                Cancel

            </a>


            <asp:Button
                ID="btnSave"
                ValidationGroup="Order"
                runat="server"
                Text="Save Order"
                CssClass="btn btn-danger px-4"
                OnClick="btnSave_Click" />

        </div>


    </div>

</asp:Content>
