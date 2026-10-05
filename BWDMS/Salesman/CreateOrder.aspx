<%@ Page Title="Create Order"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="CreateOrder.aspx.cs"
    Inherits="BWDMS.Salesman.CreateOrder" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Create Order

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Take an Order
            </h3>

            <p class="text-muted mb-0">
                Prices come from the price master - nothing is typed in by hand
            </p>

        </div>


        <div class="d-flex gap-2">

            <a href="Orders.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-cart-check"></i>

                My Orders

            </a>


            <a href="Dashboard.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-arrow-left"></i>

                Dashboard

            </a>

        </div>

    </div>


    <div class="dashboard-card">


        <asp:Label
            ID="lblHeading"
            runat="server"
            CssClass="fw-bold d-block mb-3"
            Text="New Order">
        </asp:Label>


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
                    CssClass="form-select">
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
                    Discount
                </label>


                <asp:TextBox
                    ID="txtDiscount"
                    runat="server"
                    CssClass="form-control"
                    AutoPostBack="true"
                    OnTextChanged="DiscountChanged"
                    Text="0.00">
                </asp:TextBox>


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
                Pick a variant and a selling unit; the price is filled from the price master.
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


                <asp:RegularExpressionValidator
                    ID="revQuantity"
                    ValidationGroup="LineAdd"
                    runat="server"
                    ControlToValidate="txtQuantity"
                    ValidationExpression="^[1-9]\d{0,8}$"
                    ErrorMessage="Quantity must be above zero."
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


                <asp:RegularExpressionValidator
                    ID="revUnitPrice"
                    ValidationGroup="LineAdd"
                    runat="server"
                    ControlToValidate="txtUnitPrice"
                    ValidationExpression="^\d{1,7}(\.\d{1,2})?$"
                    ErrorMessage="Price must be positive."
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


                <div class="d-flex justify-content-between py-2 border-top">

                    <span class="text-muted">
                        Discount
                    </span>


                    <span>

                        <asp:Label
                            ID="lblDiscount"
                            runat="server"
                            Text="0.00">
                        </asp:Label>

                    </span>

                </div>


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

            <!--
              Offline capture. The button below is plain HTML on purpose:
              it never posts back, it writes the draft straight into the
              local queue so it works with no network at all.
            -->
            <button
                type="button"
                id="btnSaveOffline"
                class="btn btn-outline-secondary px-4 ms-2">

                <i class="bi bi-cloud-slash"></i>

                Save Offline

            </button>

            <a href="OfflineQueue.aspx"
               class="btn btn-link ms-2">

                Offline Queue

                <span id="offlineBadge" class="badge bg-danger d-none"></span>

            </a>

        </div>

        <div id="offlineNote" class="alert alert-warning d-none mt-3"></div>


    </div>

<script src="../Scripts/OfflineSync.js"></script>
<script type="text/javascript">
    (function () {
        function refreshBadge() {
            if (!window.BwdmsOffline) {
                return;
            }
            BwdmsOffline.counts().then(function (c) {
                var waiting = c.pending + c.syncing + c.failed;
                var badge = document.getElementById('offlineBadge');
                if (waiting > 0) {
                    badge.textContent = String(waiting);
                    badge.classList.remove('d-none');
                } else {
                    badge.classList.add('d-none');
                }
                var note = document.getElementById('offlineNote');
                if (!BwdmsOffline.isOnline()) {
                    note.className = 'alert alert-warning mt-3';
                    note.textContent =
                        'You are offline. "Save Order" will not reach the ' +
                        'server - use "Save Offline" instead. Orders are ' +
                        'kept on this device until they are confirmed.';
                } else if (c.failed > 0) {
                    note.className = 'alert alert-danger mt-3';
                    note.textContent =
                        c.failed + ' offline order(s) were rejected by the ' +
                        'server. Open the Offline Queue to see why.';
                } else {
                    note.className = 'alert d-none mt-3';
                }
            });
        }
        function currentLines() {
            var rows = [];
            var table = document.getElementById('MainContent_rpLines');
            if (!table) {
                return rows;
            }
            Array.prototype.forEach.call(
                table.querySelectorAll('tr'),
                function (tr) {
                    var qty = tr.querySelector('input[type=number]');
                    if (!qty) {
                        return;
                    }
                    var quantity = parseInt(qty.value, 10) || 0;
                    if (quantity <= 0) {
                        return;
                    }
                    var cells = tr.querySelectorAll('td');
                    var priceCell = cells[cells.length - 2];
                    var totalCell = cells[cells.length - 1];
                    rows.push({
                        quantity: quantity,
                        quantityPackets: parseInt(
                            (cells[1] ? cells[1].textContent : '1').trim(),
                            10) || quantity,
                        unitPrice: parseFloat(
                            (priceCell ? priceCell.textContent : '0')
                                .replace(/[^0-9.]/g, '')) || 0,
                        lineTotal: parseFloat(
                            (totalCell ? totalCell.textContent : '0')
                                .replace(/[^0-9.]/g, '')) || 0
                    });
                });
            return rows;
        }
        document.addEventListener('DOMContentLoaded', function () {
            refreshBadge();
            var button = document.getElementById('btnSaveOffline');
            if (!button) {
                return;
            }
            button.addEventListener('click', function () {
                var lines = currentLines();
                if (!lines.length) {
                    window.alert(
                        'Add at least one order line before saving it ' +
                        'offline.');
                    return;
                }
                var shop = document.getElementById(
                    'MainContent_ddlShop');
                var orderDate = document.getElementById(
                    'MainContent_txtOrderDate');
                var remarks = document.getElementById(
                    'MainContent_txtRemarks');
                var schedule = shop
                    ? shop.options[shop.selectedIndex]
                    : null;
                BwdmsOffline.capture({
                    shopId: shop ? shop.value : '',
                    shopName: schedule ? schedule.text : '',
                    routeScheduleId: shop
                        ? (shop.getAttribute('data-schedule') || '')
                        : '',
                    orderDate: orderDate ? orderDate.value : '',
                    remarks: remarks ? remarks.value : '',
                    lines: lines
                }).then(function (order) {
                    window.alert(
                        'Saved on this device.\n\nReference: ' +
                        order.clientOrderId +
                        '\n\nThis order has NOT reached the server yet. ' +
                        'It will be sent automatically when you are back ' +
                        'online, or from the Offline Queue.');
                    refreshBadge();
                }).catch(function (e) {
                    window.alert('Could not save offline: ' + e);
                });
            });
            window.addEventListener('online', refreshBadge);
        });
    })();
</script>
</asp:Content>
