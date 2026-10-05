<%@ Page Title="Company Receipt"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddCompanyReceipt.aspx.cs"
    Inherits="BWDMS.Dealer.AddCompanyReceipt" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    <asp:Label
        ID="lblPageTitle"
        runat="server"
        Text="Add Company Receipt">
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
                    Text="Add Company Receipt">
                </asp:Label>

            </h3>


            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblSubHeading"
                    runat="server"
                    Text="Goods received from Balaji Wafers - posting moves the packets into the godown">
                </asp:Label>

            </p>

        </div>


        <a href="CompanyReceipts.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Receipts

        </a>

    </div>


    <div class="dashboard-card">


        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <asp:Label
            ID="lblPostedNotice"
            runat="server"
            Visible="false"
            CssClass="alert alert-warning d-block"
            Text="This receipt is already posted and cannot be edited.">
        </asp:Label>


        <!-- Header fields -->

        <div class="row g-4 mb-4">


            <div class="col-md-4">

                <label class="form-label fw-semibold">

                    Receipt Number

                    <span class="text-danger">*</span>

                </label>


                <small class="text-muted d-block mb-1">
                    Unique for your dealership - the suggested number can be changed.
                </small>


                <asp:TextBox
                    ID="txtReceiptNumber"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="100">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvReceiptNumber"
                    ValidationGroup="Receipt"
                    runat="server"
                    ControlToValidate="txtReceiptNumber"
                    ErrorMessage="Receipt number is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-3">

                <label class="form-label fw-semibold">

                    Receipt Date

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtReceiptDate"
                    runat="server"
                    CssClass="form-control"
                    placeholder="YYYY-MM-DD">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvReceiptDate"
                    ValidationGroup="Receipt"
                    runat="server"
                    ControlToValidate="txtReceiptDate"
                    ErrorMessage="Receipt date is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-5">

                <label class="form-label fw-semibold">
                    Company Invoice No
                </label>


                <asp:TextBox
                    ID="txtCompanyInvoiceNo"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="200"
                    placeholder="Balaji Wafers invoice / challan no.">
                </asp:TextBox>

            </div>


            <div class="col-12">

                <label class="form-label fw-semibold">
                    Notes
                </label>


                <asp:TextBox
                    ID="txtNotes"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="2000"
                    TextMode="MultiLine"
                    Rows="2"
                    placeholder="Vehicle, challan or anything worth remembering">
                </asp:TextBox>

            </div>


        </div>


        <hr class="my-4" />


        <!-- Line builder -->

        <div class="mb-3">

            <h5 class="fw-bold mb-1">
                Receipt Lines
            </h5>

            <small class="text-muted">
                Stock is counted in base packets - enter the quantity in packets and the cost per packet.
            </small>

        </div>


        <asp:Panel
            ID="pnlLineBuilder"
            runat="server">

            <div class="row g-3 align-items-end mb-3">

                <div class="col-md-5">

                    <label class="form-label fw-semibold">

                        Product Variant

                        <span class="text-danger">*</span>

                    </label>


                    <asp:DropDownList
                        ID="ddlVariant"
                        runat="server"
                        CssClass="form-select">
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

                        Quantity

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


                    <asp:RangeValidator
                        ID="rngQuantity"
                        ValidationGroup="LineAdd"
                        runat="server"
                        ControlToValidate="txtQuantity"
                        Type="Integer"
                        MinimumValue="1"
                        MaximumValue="999999999"
                        ErrorMessage="Quantity must be a whole number above zero."
                        CssClass="text-danger small"
                        Display="Dynamic">
                    </asp:RangeValidator>

                </div>


                <div class="col-md-2">

                    <label class="form-label fw-semibold">

                        Unit Cost

                        <span class="text-danger">*</span>

                    </label>


                    <asp:TextBox
                        ID="txtUnitCost"
                        runat="server"
                        CssClass="form-control"
                        Text="0.00">
                    </asp:TextBox>


                    <asp:RequiredFieldValidator
                        ID="rfvUnitCost"
                        ValidationGroup="LineAdd"
                        runat="server"
                        ControlToValidate="txtUnitCost"
                        ErrorMessage="Unit cost is required."
                        CssClass="text-danger small"
                        Display="Dynamic">
                    </asp:RequiredFieldValidator>


                    <asp:RegularExpressionValidator
                        ID="revUnitCost"
                        ValidationGroup="LineAdd"
                        runat="server"
                        ControlToValidate="txtUnitCost"
                        ValidationExpression="^\d{1,7}(\.\d{1,2})?$"
                        ErrorMessage="Unit cost must be zero or more."
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

        </asp:Panel>


        <!-- Lines grid -->

        <div class="table-responsive">

            <asp:GridView
                ID="gvLines"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                DataKeyNames="Index"
                OnRowCommand="gvLines_RowCommand" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="VariantLabel"
                        HeaderText="Product Variant" />

                    <asp:BoundField
                        DataField="Quantity"
                        HeaderText="Quantity"
                        DataFormatString="{0:N0}" />

                    <asp:BoundField
                        DataField="UnitCostText"
                        HeaderText="Unit Cost" />

                    <asp:BoundField
                        DataField="LineTotalText"
                        HeaderText="Line Total" />

                    <asp:TemplateField
                        HeaderText="Action">

                        <ItemTemplate>

                            <asp:LinkButton
                                ID="lnkRemove"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-danger"
                                CommandName="Remove"
                                CommandArgument='<%# Eval("Index") %>'>

                                <i class="bi bi-trash"></i>

                                Remove

                            </asp:LinkButton>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-4">

                        <i class="bi bi-box-seam fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No lines yet - add at least one item above.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>


        <!-- Grand total - always recalculated on the server -->

        <div class="row justify-content-end mt-3">

            <div class="col-md-4">

                <div class="d-flex justify-content-between align-items-center py-2 border-top border-danger">

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


        <asp:Panel
            ID="pnlButtons"
            runat="server">

            <div class="d-flex justify-content-end gap-2">

                <a href="CompanyReceipts.aspx"
                   class="btn btn-light border">

                    <i class="bi bi-x-lg"></i>

                    Cancel

                </a>


                <asp:Button
                    ID="btnSaveDraft"
                    ValidationGroup="Receipt"
                    runat="server"
                    Text="Save Draft"
                    CssClass="btn btn-outline-secondary px-4"
                    OnClick="btnSaveDraft_Click" />


                <asp:Button
                    ID="btnPost"
                    ValidationGroup="Receipt"
                    runat="server"
                    Text="Save &amp; Post"
                    CssClass="btn btn-danger px-4"
                    OnClick="btnPost_Click" />

            </div>

        </asp:Panel>


    </div>

</asp:Content>
