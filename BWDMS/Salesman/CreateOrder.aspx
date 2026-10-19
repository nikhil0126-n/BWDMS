<%@ Page Title="Create Order"
    Language="C#"
    MasterPageFile="~/Master/SalesmanMaster.master"
    AutoEventWireup="true"
    CodeBehind="CreateOrder.aspx.cs"
    Inherits="BWDMS.Salesman.CreateOrder" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Create Orders

</asp:Content>


<asp:Content ID="HeadStyle"
    ContentPlaceHolderID="HeadContent"
    runat="server">

    <style>
        .co-top {
            display: flex;
            justify-content: space-between;
            align-items: center;
            gap: 1rem;
            margin-bottom: .9rem;
            padding-bottom: .85rem;
            border-bottom: 1px solid var(--bw-border);
        }

        .co-title {
            margin: 0;
            font-size: 1.18rem;
            font-weight: 700;
            color: var(--bw-text);
        }

        .co-btn-outline {
            display: inline-block;
            border: 1px solid var(--bw-primary);
            color: var(--bw-primary);
            background: #fff;
            border-radius: 8px;
            padding: .5rem 1.1rem;
            font-size: .85rem;
            font-weight: 600;
            text-decoration: none;
            cursor: pointer;
        }

        .co-btn-outline:hover,
        .co-btn-outline:focus {
            background: rgba(104, 108, 229, .08);
            color: var(--bw-primary);
        }

        .co-page-card {
            height: auto;
        }

        .co-page-card:hover {
            transform: none;
            box-shadow: var(--bw-shadow-sm);
        }

        .co-top-actions {
            display: flex;
            align-items: center;
            gap: .75rem;
        }

        .co-ghost-btn {
            border: 1px solid transparent;
            background: transparent;
            color: var(--bw-text-soft);
            font-size: .8rem;
            font-weight: 600;
            padding: .42rem .6rem;
            border-radius: 8px;
            cursor: pointer;
        }

        .co-ghost-btn:hover {
            background: #f2f4f9;
            color: var(--bw-text);
        }

        .co-ghost-btn i {
            font-size: .72rem;
            transition: transform .18s ease;
        }

        .co-ghost-btn[aria-expanded="true"] i {
            transform: rotate(180deg);
        }

        .co-details {
            display: none;
            background: #f7f9fc;
            border: 1px solid var(--bw-border);
            border-radius: 10px;
            padding: .9rem;
            margin-bottom: 1rem;
        }

        .co-details.co-open {
            display: block;
        }

        .co-details .form-label {
            font-size: .78rem;
            color: var(--bw-text-soft);
            margin-bottom: .3rem;
        }

        .co-section-head {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: .8rem;
        }

        .co-section-head h5 {
            margin: 0;
            font-weight: 700;
            font-size: 1.02rem;
        }

        .co-builder {
            display: none;
            border: 1px dashed var(--bw-border);
            background: #fbfcfe;
            border-radius: 10px;
            padding: .9rem;
            margin-bottom: 1rem;
        }

        .co-builder.co-open {
            display: block;
        }

        .co-builder.co-flash {
            animation: coFlash 1.2s ease;
        }

        @keyframes coFlash {
            0% {
                box-shadow: 0 0 0 3px rgba(104, 108, 229, .35);
            }

            100% {
                box-shadow: 0 0 0 3px rgba(104, 108, 229, 0);
            }
        }

        #btnAddProducts {
            min-width: 150px;
        }

        .co-table {
            width: 100%;
            border-collapse: separate;
            border-spacing: 0;
            margin-bottom: .6rem;
        }

        .co-table thead th {
            background: #e9edf3;
            font-weight: 700;
            font-size: .9rem;
            padding: .75rem 1rem;
            border: none;
            white-space: nowrap;
        }

        .co-table thead tr th:first-child {
            border-radius: 8px 0 0 8px;
        }

        .co-table thead tr th:last-child {
            border-radius: 0 8px 8px 0;
        }

        .co-table tbody td {
            padding: .95rem 1rem;
            border-top: 1px solid var(--bw-border-soft);
            vertical-align: middle;
        }

        .co-table tbody tr:last-child td {
            border-bottom: 1px solid var(--bw-border);
        }

        .co-prod {
            font-weight: 700;
            color: var(--bw-text);
        }

        .co-pack small {
            display: block;
            color: var(--bw-text-muted);
            font-size: .74rem;
        }

        .co-step {
            display: inline-flex;
            align-items: center;
            gap: .5rem;
        }

        .co-step a {
            width: 26px;
            height: 26px;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            border: 1px solid var(--bw-border);
            border-radius: 6px;
            background: #fff;
            color: var(--bw-text-soft);
            text-decoration: none;
            font-weight: 700;
            line-height: 1;
        }

        .co-step a:hover {
            background: var(--bw-primary);
            border-color: var(--bw-primary);
            color: #fff;
        }

        .co-qty {
            width: 38px;
            text-align: center;
            border: none;
            background: transparent;
            font-weight: 700;
            color: var(--bw-text);
            -moz-appearance: textfield;
        }

        .co-qty::-webkit-outer-spin-button,
        .co-qty::-webkit-inner-spin-button {
            -webkit-appearance: none;
            margin: 0;
        }

        .co-money {
            font-weight: 700;
            white-space: nowrap;
        }

        .co-trash {
            color: var(--bw-danger);
            border: 1px solid transparent;
            background: transparent;
            padding: .15rem .35rem;
            border-radius: 6px;
            text-decoration: none;
            font-size: .85rem;
        }

        .co-trash:hover {
            background: rgba(220, 36, 36, .1);
            color: var(--bw-danger);
        }

        .co-totals {
            display: flex;
            align-items: center;
            gap: 1.5rem;
            border: 1px solid var(--bw-border);
            border-radius: 10px;
            background: #fbfcfe;
            padding: .85rem 1.2rem;
            margin-top: 1rem;
        }

        .co-totals .co-t > span {
            display: block;
            font-size: .75rem;
            color: var(--bw-text-muted);
            margin-bottom: .15rem;
        }

        .co-totals .co-t strong {
            font-size: 1.05rem;
            color: var(--bw-text);
            white-space: nowrap;
        }

        .co-totals .co-t:nth-child(2) {
            margin-left: auto;
        }

        .co-t-amount {
            margin-left: auto;
            text-align: right;
        }

        .co-mini {
            display: flex;
            justify-content: flex-end;
            gap: 1.5rem;
            font-size: .8rem;
            color: var(--bw-text-muted);
            margin-top: .5rem;
        }

        .co-actions {
            display: flex;
            align-items: center;
            justify-content: space-between;
            flex-wrap: wrap;
            gap: .6rem;
            margin-top: 1.4rem;
            padding-top: .2rem;
        }

        .co-empty-td {
            text-align: center;
            color: var(--bw-text-muted);
            font-weight: 500;
            padding: 1.6rem 1rem !important;
            border-top: 1px solid var(--bw-border-soft) !important;
        }

        /* ---------- Orders Summary ---------- */
        .co-sum-title {
            margin: .2rem 0 1.1rem;
            font-size: 1.15rem;
            font-weight: 700;
            color: var(--bw-text);
        }

        .co-sum-box {
            border: 1px solid #cfcfcf;
            border-radius: 8px;
            overflow: hidden;
            margin-bottom: 1.1rem;
        }

        .co-sum-table {
            width: 100%;
            border-collapse: collapse;
        }

        .co-sum-table thead th {
            background: #d7d7d7;
            font-weight: 700;
            font-size: .95rem;
            color: #111;
            text-align: center;
            padding: .85rem 1rem;
        }

        .co-sum-table thead tr th:first-child {
            border-radius: 8px 0 0 0;
        }

        .co-sum-table thead tr th:last-child {
            border-radius: 0 8px 0 0;
        }

        .co-sum-table tbody td {
            padding: 1rem;
            font-size: .95rem;
            font-weight: 700;
            color: #111;
            text-align: center;
        }

        .co-sum-totals {
            display: flex;
            align-items: flex-start;
            gap: 2rem;
            border: 1px solid #cfcfcf;
            border-radius: 8px;
            padding: 1.1rem 1.4rem;
            margin-bottom: 1.4rem;
        }

        .co-sum-totals .co-sum-left {
            display: flex;
            gap: 6rem;
            flex: 1 1 auto;
        }

        .co-sum-t span {
            display: block;
            font-size: .8rem;
            color: #6b7280;
            margin-bottom: .45rem;
        }

        .co-sum-t strong {
            font-size: .95rem;
            color: #111;
        }

        .co-sum-right {
            min-width: 210px;
            text-align: left;
        }

        .co-sum-right .co-sum-line {
            display: flex;
            justify-content: space-between;
            gap: 1.6rem;
            font-size: .85rem;
            margin-bottom: .45rem;
            color: #111;
        }

        .co-sum-right .co-sum-line span {
            color: #6b7280;
            margin-bottom: 0;
        }

        .co-sum-right .co-sum-line strong {
            font-size: .9rem;
        }

        .co-sum-grand {
            color: #16a34a;
        }

        .co-pay h5 {
            margin: 0 0 .7rem;
            font-size: .95rem;
            font-weight: 700;
        }

        .co-pay-opt {
            display: inline-flex;
            align-items: center;
            gap: .4rem;
            margin-right: 1.6rem;
            font-size: .92rem;
            font-weight: 700;
            color: #111;
            cursor: pointer;
        }

        .co-pay-opt input {
            accent-color: #6c5ce7;
        }

        .co-sum-actions {
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 1rem;
            flex-wrap: wrap;
            margin-top: 1.4rem;
        }

        .co-btn-primary {
            display: inline-block;
            border: 1px solid #6c5ce7;
            background: #6c5ce7;
            color: #fff;
            border-radius: 8px;
            padding: .7rem 1.7rem;
            font-size: .92rem;
            font-weight: 700;
            cursor: pointer;
        }

        .co-btn-primary:hover,
        .co-btn-primary:focus {
            background: #5a4bd6;
            border-color: #5a4bd6;
            color: #fff;
        }

        @media (max-width: 767px) {
            .co-table thead {
                display: none;
            }

            .co-table tbody td {
                display: block;
                padding: .4rem .8rem;
                border-top: none;
            }

            .co-table tbody tr {
                display: block;
                border-top: 1px solid var(--bw-border);
                padding: .5rem 0;
            }

            .co-totals {
                flex-direction: column;
                align-items: flex-start;
            }

            .co-t-amount {
                margin-left: 0;
                text-align: left;
            }
        }
    </style>

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <div class="dashboard-card co-page-card">


        <div class="co-top">

            <h3 class="co-title">
                Create Orders
            </h3>

            <div class="co-top-actions">

                <button type="button"
                        id="btnDetails"
                        class="co-ghost-btn"
                        aria-expanded="false"
                        aria-controls="coDetails">
                    Order details
                    <i class="bi bi-chevron-down"></i>
                </button>

                <a href="VehicleStock.aspx"
                   class="co-btn-outline">
                    View Vehicle Stock
                </a>

            </div>

        </div>


        <!-- Order details -->
        <div class="co-details" id="coDetails">

        <asp:Label
            ID="lblHeading"
            runat="server"
            CssClass="d-none"
            Text="Order Details">
        </asp:Label>

        <div class="row g-2 align-items-end">

                <div class="col-md-3">

                    <label class="form-label fw-semibold">

                        Shop

                        <span class="text-danger">*</span>

                    </label>

                    <asp:DropDownList
                        ID="ddlShop"
                        runat="server"
                        CssClass="form-select form-select-sm">
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


                <div class="col-md-2">

                    <label class="form-label fw-semibold">

                        Order Date

                        <span class="text-danger">*</span>

                    </label>

                    <asp:TextBox
                        ID="txtOrderDate"
                        runat="server"
                        CssClass="form-control form-control-sm"
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

                    <asp:RegularExpressionValidator
                        ID="revOrderDate"
                        ValidationGroup="Order"
                        runat="server"
                        ControlToValidate="txtOrderDate"
                        ValidationExpression="^\d{4}-\d{2}-\d{2}$"
                        ErrorMessage="Use YYYY-MM-DD."
                        CssClass="text-danger small"
                        Display="Dynamic">
                    </asp:RegularExpressionValidator>

                </div>


                <div class="col-md-2">

                    <label class="form-label fw-semibold">
                        Delivery Date
                    </label>

                    <asp:TextBox
                        ID="txtDeliveryDate"
                        runat="server"
                        CssClass="form-control form-control-sm"
                        placeholder="YYYY-MM-DD">
                    </asp:TextBox>

                    <asp:RegularExpressionValidator
                        ID="revDeliveryDate"
                        ValidationGroup="Order"
                        runat="server"
                        ControlToValidate="txtDeliveryDate"
                        ValidationExpression="^\d{4}-\d{2}-\d{2}$"
                        ErrorMessage="Use YYYY-MM-DD."
                        CssClass="text-danger small"
                        Display="Dynamic">
                    </asp:RegularExpressionValidator>

                </div>


                <div class="col-md-2">

                    <label class="form-label fw-semibold">
                        Discount
                    </label>

                    <asp:TextBox
                        ID="txtDiscount"
                        runat="server"
                        CssClass="form-control form-control-sm"
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
                        ErrorMessage="Must be zero or more."
                        CssClass="text-danger small"
                        Display="Dynamic">
                    </asp:RegularExpressionValidator>

                </div>


                <div class="col-md-3">

                    <label class="form-label fw-semibold">
                        Remarks
                    </label>

                    <asp:TextBox
                        ID="txtRemarks"
                        runat="server"
                        CssClass="form-control form-control-sm"
                        MaxLength="1000"
                        TextMode="MultiLine"
                        Rows="2"
                        placeholder="Delivery instructions, notes...">
                    </asp:TextBox>

                </div>

        </div>

    </div>


        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <!-- ================================
             Orders Summary (after "Proceed to Summary")
             ================================ -->
        <div id="coSummary" style="display:none;">

            <h4 class="co-sum-title">
                Orders Summary
            </h4>


            <div class="co-sum-box">

                <table class="co-sum-table">

                    <thead>

                        <tr>
                            <th>Products</th>
                            <th>packing</th>
                            <th>Qty</th>
                            <th>Rate</th>
                            <th>Amount</th>
                        </tr>

                    </thead>

                    <tbody id="coSumBody">
                    </tbody>

                </table>

            </div>


            <div class="co-sum-totals">

                <div class="co-sum-left">

                    <div class="co-sum-t">
                        <span>Total Items</span>
                        <strong id="sumItems">0</strong>
                    </div>

                    <div class="co-sum-t">
                        <span>Total Quantity</span>
                        <strong id="sumQty">0</strong>
                    </div>

                </div>


                <div class="co-sum-right">

                    <div class="co-sum-line">
                        <span>Total Amount</span>
                        <strong id="sumAmount">&#8377;0</strong>
                    </div>

                    <div class="co-sum-line">
                        <span>Discount</span>
                        <strong id="sumDiscount">&#8377;0</strong>
                    </div>

                    <div class="co-sum-line">
                        <span>Grand Total</span>
                        <strong class="co-sum-grand" id="sumGrand">&#8377;0</strong>
                    </div>

                </div>

            </div>


            <div class="co-pay">

                <h5>Payment Method</h5>

                <label class="co-pay-opt">
                    <input type="radio"
                           name="coPaymentMethod"
                           value="Cash"
                           checked="checked" />
                    cash
                </label>

                <label class="co-pay-opt">
                    <input type="radio"
                           name="coPaymentMethod"
                           value="UPI" />
                    UPI
                </label>

            </div>


            <div class="co-sum-actions">

                <button type="button"
                        id="btnAddBack"
                        class="co-btn-outline">
                    + Add Products
                </button>

                <button type="button"
                        id="btnPlaceOrders"
                        class="co-btn-primary">
                    Place Orders
                </button>

            </div>

        </div>


        <!-- Add Products header -->
        <div class="co-section-head">

            <h5>
                Add Products
            </h5>

        </div>


        <!-- Line builder (toggled by + Add Products) -->
        <div class="co-builder" id="coBuilder">

            <div class="row g-3 align-items-end">

                <div class="col-md-4">

                    <label class="form-label fw-semibold">

                        Product Variant

                        <span class="text-danger">*</span>

                    </label>

                    <asp:DropDownList
                        ID="ddlVariant"
                        runat="server"
                        CssClass="form-select form-select-sm"
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
                        CssClass="form-select form-select-sm"
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
                        CssClass="form-control form-control-sm"
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
                        CssClass="form-control form-control-sm"
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
                        Text="Add"
                        CssClass="co-btn-outline w-100"
                        OnClick="btnAddLine_Click" />

                </div>

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

                        <table class="co-table">

                            <thead>

                                <tr>

                                    <th>Products</th>

                                    <th>packing</th>

                                    <th>Available</th>

                                    <th>Quantity</th>

                                    <th class="text-end">Rate</th>

                                    <th class="text-end">Amount</th>

                                </tr>

                            </thead>


                            <tbody>

                    </HeaderTemplate>


                    <ItemTemplate>

                        <tr>

                            <td class="co-prod">

                                <%# Eval("ProductName") %>

                            </td>


                            <td class="co-pack">

                                <%# Eval("VariantName") %>

                                <small>

                                    <%# Eval("UnitCode") %>

                                </small>

                            </td>


                            <td>

                                <%# Eval("QuantityPackets") %>

                            </td>


                            <td>

                                <div class="co-step">

                                    <asp:LinkButton
                                        ID="lnkDec"
                                        runat="server"
                                        CommandName="Dec"
                                        CommandArgument='<%# Eval("Index") %>'
                                        ToolTip="Decrease quantity"
                                        Text="-" />

                                    <input type="number"
                                           class="co-qty"
                                           value='<%# Eval("Quantity") %>'
                                           min="1"
                                           readonly="readonly"
                                           aria-label="Quantity" />

                                    <asp:LinkButton
                                        ID="lnkInc"
                                        runat="server"
                                        CommandName="Inc"
                                        CommandArgument='<%# Eval("Index") %>'
                                        ToolTip="Increase quantity"
                                        Text="+" />

                                </div>

                            </td>


                            <td class="text-end">

                                &#8377;<%# Convert.ToDecimal(Eval("UnitPrice")).ToString("N2") %>

                            </td>


                            <td class="text-end">

                                <span class="co-money">

                                    &#8377;<%# Convert.ToDecimal(Eval("LineTotal")).ToString("N2") %>

                                </span>

                                <asp:LinkButton
                                    ID="lnkRemove"
                                    runat="server"
                                    CssClass="co-trash ms-2"
                                    CommandName="Remove"
                                    CommandArgument='<%# Eval("Index") %>'
                                    ToolTip="Remove line">

                                    <i class="bi bi-trash"></i>

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

            <div class="table-responsive">

                <table class="co-table">

                    <thead>

                        <tr>

                            <th>Products</th>

                            <th>packing</th>

                            <th>Available</th>

                            <th>Quantity</th>

                            <th class="text-end">Rate</th>

                            <th class="text-end">Amount</th>

                        </tr>

                    </thead>

                    <tbody>

                        <tr>

                            <td colspan="6"
                                class="co-empty-td">

                                No products yet - press "+ Add Products" to start.

                            </td>

                        </tr>

                    </tbody>

                </table>

            </div>

        </asp:Panel>


        <!-- + Add Products -->
        <button type="button"
                id="btnAddProducts"
                class="co-btn-outline"
                aria-expanded="false"
                aria-controls="coBuilder">
            + Add Products
        </button>


        <!-- Totals -->
        <div class="co-totals">

            <div class="co-t">

                <span>
                    Total Items
                </span>

                <strong>
                    <asp:Label
                        ID="lblTotalItems"
                        runat="server"
                        Text="0">
                    </asp:Label>
                </strong>

            </div>


            <div class="co-t">

                <span>
                    Total Quantity
                </span>

                <strong>
                    <asp:Label
                        ID="lblTotalQty"
                        runat="server"
                        Text="0">
                    </asp:Label>
                </strong>

            </div>


            <div class="co-t co-t-amount">

                <span>
                    Total Amount
                </span>

                <strong>
                    &#8377;<asp:Label
                        ID="lblGrandTotal"
                        runat="server"
                        Text="0.00"></asp:Label>
                </strong>

            </div>

        </div>


        <div class="co-mini d-none">

            <span>
                Sub Total:
                <asp:Label
                    ID="lblSubTotal"
                    runat="server"
                    Text="0.00">
                </asp:Label>
            </span>

            <span>
                Discount:
                <asp:Label
                    ID="lblDiscount"
                    runat="server"
                    Text="0.00">
                </asp:Label>
            </span>

        </div>


    </div>


    <!-- Actions -->
    <div class="co-actions">

            <div class="d-flex align-items-center gap-2 flex-wrap">

                <a href="Orders.aspx"
                   class="btn btn-light border btn-sm">

                    <i class="bi bi-x-lg"></i>

                    Cancel

                </a>

                <!--
                  Offline capture. The button below is plain HTML on purpose:
                  it never posts back, it writes the draft straight into the
                  local queue so it works with no network at all.
                -->
                <button
                    type="button"
                    id="btnSaveOffline"
                    class="co-btn-outline">

                    <i class="bi bi-cloud-slash"></i>

                    Save Offline

                </button>

                <a href="OfflineQueue.aspx"
                   class="btn btn-link btn-sm">

                    Offline Queue

                    <span id="offlineBadge" class="badge bg-danger d-none"></span>

                </a>

            </div>


            <asp:Button
                ID="btnSave"
                ValidationGroup="Order"
                runat="server"
                Text="Proceed to Summary"
                CssClass="co-btn-outline px-4"
                OnClick="btnSave_Click" />

        </div>

        <div id="offlineNote" class="alert alert-warning d-none mt-3"></div>


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
                        'You are offline. "Proceed to Summary" will not ' +
                        'reach the server - use "Save Offline" instead. ' +
                        'Orders are kept on this device until they are ' +
                        'confirmed.';
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
                table = document.querySelector('.co-table tbody');
            }
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
                            (cells[2] ? cells[2].textContent : '1').trim(),
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
        function toggleBuilder(forceOpen, stayPut) {
            var builder = document.getElementById('coBuilder');
            if (!builder) {
                return;
            }
            var open = typeof forceOpen === 'boolean'
                ? forceOpen
                : !builder.classList.contains('co-open');
            builder.classList.toggle('co-open', open);

            var btn = document.getElementById('btnAddProducts');
            if (btn) {
                btn.setAttribute('aria-expanded', open ? 'true' : 'false');
                btn.innerHTML = open
                    ? '&times;&nbsp; Close'
                    : '+ Add Products';
            }

            if (open) {
                warnIfNoProducts(builder);
                if (!stayPut) {
                    builder.scrollIntoView({
                        behavior: 'smooth',
                        block: 'center'
                    });
                }
                var first = builder.querySelector('select, input');
                if (first && first.focus) {
                    first.focus({ preventScroll: true });
                }
                builder.classList.add('co-flash');
                window.setTimeout(function () {
                    builder.classList.remove('co-flash');
                }, 1300);
            }
        }
        function warnIfNoProducts(builder) {
            var ddl = document.getElementById('MainContent_ddlVariant');
            if (!ddl) {
                return;
            }
            var note = builder.querySelector('.co-noprods');
            if (ddl.options.length <= 1) {
                if (!note) {
                    note = document.createElement('div');
                    note.className = 'alert alert-info co-noprods mt-2 mb-0';
                    note.textContent =
                        'No products are available for you yet. Ask the ' +
                        'dealer to load products and prices.';
                    builder.appendChild(note);
                }
            } else if (note && note.parentNode) {
                note.parentNode.removeChild(note);
            }
        }
        function toggleDetails(forceOpen) {
            var details = document.getElementById('coDetails');
            var btn = document.getElementById('btnDetails');
            if (!details) {
                return;
            }
            var open = typeof forceOpen === 'boolean'
                ? forceOpen
                : !details.classList.contains('co-open');
            details.classList.toggle('co-open', open);
            if (btn) {
                btn.setAttribute('aria-expanded', open ? 'true' : 'false');
            }
        }
        function groupInvalid(group) {
            if (!window.Page_Validators) {
                return false;
            }
            for (var i = 0; i < Page_Validators.length; i++) {
                var v = Page_Validators[i];
                if (v.validationGroup === group && v.isvalid === false) {
                    return true;
                }
            }
            return false;
        }
        function lineBuilderInvalid() {
            return groupInvalid('LineAdd');
        }
        document.addEventListener('DOMContentLoaded', function () {
            refreshBadge();

            var detailsBtn = document.getElementById('btnDetails');
            if (detailsBtn) {
                detailsBtn.addEventListener('click', function () {
                    toggleDetails();
                });
            }

            // A failed "Proceed to Summary" postback hides the header
            // fields again, so open them so the errors are readable.
            if (groupInvalid('Order')) {
                toggleDetails(true);
            }

            // Reveal the right block whenever client validation fails.
            if (typeof window.Page_ClientValidate === 'function') {
                var originalClientValidate = window.Page_ClientValidate;
                window.Page_ClientValidate = function (group) {
                    var ok = originalClientValidate.apply(this, arguments);
                    if (!ok) {
                        if (group === 'Order') {
                            toggleDetails(true);
                        }
                        if (group === 'LineAdd') {
                            toggleBuilder(true);
                        }
                    }
                    return ok;
                };
            }

            var add = document.getElementById('btnAddProducts');
            if (add) {
                add.addEventListener('click', function () {
                    toggleBuilder();
                });
            }

            // A failed "Add" postback re-renders the builder closed,
            // so open it again to show the field errors.
            if (lineBuilderInvalid()) {
                toggleBuilder(true);
            }

            // A successful add collapses the panel on the server side,
            // so bring it straight back for the next product.
            var message = document.getElementById('MainContent_lblMessage');
            if (message && /Line added/i.test(message.textContent || '')) {
                toggleBuilder(true, true);
            }

            /* ------------------------------------------
               Orders Summary screen
               "Proceed to Summary" swaps the form for the
               summary; "Place Orders" does the real postback.
               ------------------------------------------ */
            var summaryOn = false;

            function sumHide(id) {
                var el = document.getElementById(id);
                if (el) {
                    el.style.display = 'none';
                }
            }

            function sumShow(id) {
                var el = document.getElementById(id);
                if (el) {
                    el.style.display = '';
                }
            }

            function sumText(id, text) {
                var el = document.getElementById(id);
                if (el) {
                    el.textContent = text;
                }
            }

            function sumMoney(value) {
                var n = parseFloat(
                    String(value).replace(/[^0-9.]/g, '')) || 0;
                return '\u20B9' + (n % 1 === 0
                    ? n.toFixed(0)
                    : n.toFixed(2));
            }

            function showSummary() {
                var rows = document.querySelectorAll(
                    '.co-table tbody tr');

                // Skip the "No products yet" empty row: it renders
                // as a table row with a single cell.
                var html = '';
                var lineCount = 0;

                Array.prototype.forEach.call(rows, function (tr) {
                    var cells = tr.querySelectorAll('td');
                    if (cells.length < 4) {
                        return;
                    }

                    lineCount++;
                    var packCell = cells[1];
                    var small = packCell.querySelector('small');
                    var unit = small ? small.textContent.trim() : '';
                    var variant = packCell.textContent.trim();
                    if (unit) {
                        variant = variant.replace(unit, '').trim();
                    }

                    var qtyInput =
                        cells[3].querySelector('input[type=number]');
                    var qty = qtyInput ? qtyInput.value : '';

                    var last = cells.length - 1;
                    var rate = cells[last - 1].textContent.trim();
                    var amountSpan =
                        cells[last].querySelector('.co-money');
                    var amount = amountSpan
                        ? amountSpan.textContent.trim()
                        : cells[last].textContent.trim();

                    html +=
                        '<tr>' +
                        '<td>' + cells[0].textContent.trim() + '</td>' +
                        '<td>' +
                            (variant + (unit
                                ? ' (' + unit + ')'
                                : '')) +
                        '</td>' +
                        '<td>' +
                            (qty + (unit ? ' ' + unit : '')) +
                        '</td>' +
                        '<td>' + rate + '</td>' +
                        '<td>' + amount + '</td>' +
                        '</tr>';
                });

                if (!lineCount) {
                    return false;
                }

                document.getElementById('coSumBody').innerHTML = html;

                sumText('sumItems',
                    (document.getElementById(
                        'MainContent_lblTotalItems') || {})
                        .textContent || '0');
                sumText('sumQty',
                    (document.getElementById(
                        'MainContent_lblTotalQty') || {})
                        .textContent || '0');
                sumText('sumAmount', sumMoney(
                    (document.getElementById(
                        'MainContent_lblSubTotal') || {})
                    .textContent || '0'));
                sumText('sumDiscount', sumMoney(
                    (document.getElementById(
                        'MainContent_lblDiscount') || {})
                    .textContent || '0'));
                sumText('sumGrand', sumMoney(
                    (document.getElementById(
                        'MainContent_lblGrandTotal') || {})
                    .textContent || '0'));

                ['coDetails', 'coBuilder', 'MainContent_pnlLines',
                 'MainContent_pnlNoLines', 'btnAddProducts',
                 'MainContent_lblMessage',
                 'offlineNote'].forEach(sumHide);

                Array.prototype.forEach.call(
                    document.querySelectorAll(
                        '.co-section-head, .co-totals, .co-mini, ' +
                        '.co-actions, .co-top-actions, ' +
                        '.co-sum-title'),
                    function (el) {
                        el.style.display = 'none';
                    });

                var title = document.querySelector('.co-title');
                if (title) {
                    title.textContent = 'Orders Summary';
                }

                document.getElementById('coSummary').style.display = '';
                window.scrollTo(0, 0);

                return true;
            }

            /* Back to the "add order" form from the summary.
               Nothing was posted, so every line, shop, date and
               remark is still exactly as it was entered. */
            function backToForm() {
                ['coDetails', 'coBuilder', 'MainContent_pnlLines',
                 'MainContent_pnlNoLines', 'btnAddProducts',
                 'MainContent_lblMessage',
                 'offlineNote'].forEach(sumShow);

                Array.prototype.forEach.call(
                    document.querySelectorAll(
                        '.co-section-head, .co-totals, .co-mini, ' +
                        '.co-actions, .co-top-actions, ' +
                        '.co-sum-title'),
                    function (el) {
                        el.style.display = '';
                    });

                var title = document.querySelector('.co-title');
                if (title) {
                    title.textContent = 'Create Orders';
                }

                document.getElementById('coSummary').style.display =
                    'none';

                summaryOn = false;

                toggleBuilder(true);
                window.scrollTo(0, 0);
            }

            /* When "Proceed to Summary" is blocked by a required
               field, open Order details and jump to the bad field
               so the user actually sees why nothing happened. */
            function revealInvalidFields() {
                var details = document.getElementById('coDetails');
                var toggle = document.getElementById('btnDetails');

                if (details && toggle &&
                    !details.classList.contains('co-open')) {
                    toggle.click();
                }

                if (!window.Page_Validators) {
                    return;
                }

                for (var i = 0; i < Page_Validators.length; i++) {
                    var v = Page_Validators[i];
                    if (v.validationGroup === 'Order' &&
                        v.isvalid === false) {
                        var ctl = document.getElementById(
                            v.controltovalidate);
                        if (ctl && ctl.focus) {
                            ctl.focus();
                        }
                        return;
                    }
                }
            }

            var backButton = document.getElementById('btnAddBack');
            if (backButton) {
                backButton.addEventListener('click', backToForm);
            }

            var saveButton = document.getElementById(
                'MainContent_btnSave');

            // Capture phase on the form, so this runs BEFORE the
            // button's own postback handler: the first click shows
            // the summary, the second one (Place Orders) posts.
            var theForm = document.forms[0];
            if (theForm && saveButton) {
                theForm.addEventListener('click', function (ev) {
                    if (!ev.target ||
                        ev.target.id !== 'MainContent_btnSave') {
                        return;
                    }

                    if (summaryOn) {
                        return;
                    }

                    ev.preventDefault();
                    ev.stopPropagation();

                    if (typeof Page_ClientValidate === 'function' &&
                        !Page_ClientValidate('Order')) {
                        revealInvalidFields();
                        return;
                    }

                    if (!showSummary()) {
                        window.alert(
                            'Add at least one order line before ' +
                            'continuing.');
                    } else {
                        summaryOn = true;
                    }
                }, true);
            }

            var placeButton = document.getElementById(
                'btnPlaceOrders');
            if (placeButton && saveButton) {
                placeButton.addEventListener('click', function () {
                    saveButton.click();
                });
            }

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
