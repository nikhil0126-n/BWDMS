<%@ Page Title="Record Return"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddReturn.aspx.cs"
    Inherits="BWDMS.Dealer.AddReturn" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    <asp:Label
        ID="lblPageTitle"
        runat="server"
        Text="Record Return">
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
                    Text="Record Return">
                </asp:Label>

            </h3>


            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblSubHeading"
                    runat="server"
                    Text="Goods come back from the shop against a dispatched order - good packets return to stock, damaged packets are written off in the ledger">
                </asp:Label>

            </p>

        </div>


        <a href="Returns.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Returns

        </a>

    </div>


    <div class="dashboard-card">


        <asp:Label
            ID="lblViewInfo"
            runat="server"
            Visible="false"
            CssClass="alert alert-info d-block">
        </asp:Label>


        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <!-- Header fields -->

        <asp:Panel
            ID="pnlForm"
            runat="server">

        <div class="row g-4 mb-4">


            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Dispatched Order

                    <span class="text-danger">*</span>

                </label>


                <small class="text-muted d-block mb-1">
                    Only dispatched orders can have returns recorded.
                </small>


                <asp:DropDownList
                    ID="ddlOrder"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlOrder_SelectedIndexChanged">
                </asp:DropDownList>


                <asp:RequiredFieldValidator
                    ID="rfvOrder"
                    ValidationGroup="Return"
                    runat="server"
                    ControlToValidate="ddlOrder"
                    InitialValue=""
                    ErrorMessage="Select a dispatched order."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>


                <asp:Label
                    ID="lblOrderView"
                    runat="server"
                    Visible="false"
                    CssClass="form-control-plaintext">
                </asp:Label>

            </div>


            <div class="col-md-3">

                <label class="form-label fw-semibold">

                    Return Date

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtReturnDate"
                    runat="server"
                    CssClass="form-control"
                    placeholder="YYYY-MM-DD">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvReturnDate"
                    ValidationGroup="Return"
                    runat="server"
                    ControlToValidate="txtReturnDate"
                    ErrorMessage="Return date is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>


                <asp:RegularExpressionValidator
                    ID="revReturnDate"
                    ValidationGroup="Return"
                    runat="server"
                    ControlToValidate="txtReturnDate"
                    ValidationExpression="^\d{4}-\d{2}-\d{2}$"
                    ErrorMessage="Return date must be YYYY-MM-DD."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RegularExpressionValidator>

            </div>


            <div class="col-md-3">

                <label class="form-label fw-semibold">

                    Return Number

                </label>


                <small class="text-muted d-block mb-1">
                    Generated automatically on save.
                </small>


                <asp:Label
                    ID="lblReturnNo"
                    runat="server"
                    CssClass="form-control-plaintext fw-semibold"
                    Text="Auto-generated on save">
                </asp:Label>

            </div>


            <div class="col-12">

                <label class="form-label fw-semibold">

                    Reason

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtReason"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="500"
                    TextMode="MultiLine"
                    Rows="2"
                    placeholder="Why the goods came back from the shop">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvReason"
                    ValidationGroup="Return"
                    runat="server"
                    ControlToValidate="txtReason"
                    ErrorMessage="Reason is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


        </div>


        <!-- Returned goods line editor -->

        <div class="mb-3">

            <h5 class="fw-bold mb-1">
                Returned Goods
            </h5>

            <small class="text-muted">
                Every line of the chosen order is listed - enter the returned quantity in base packets (zero leaves the line out) and pick its condition.
            </small>

        </div>


        <asp:Panel
            ID="pnlLines"
            runat="server"
            Visible="false">

            <div class="table-responsive">

                <asp:Repeater
                    ID="rpLines"
                    runat="server"
                    OnItemDataBound="rpLines_ItemDataBound">

                    <HeaderTemplate>

                        <table class="table table-hover align-middle mb-0">

                            <thead>

                                <tr>

                                    <th>Product</th>

                                    <th class="text-end">Qty Dispatched (packets)</th>

                                    <th>Returned Quantity (packets)</th>

                                    <th>Condition</th>

                                </tr>

                            </thead>


                            <tbody>

                    </HeaderTemplate>


                    <ItemTemplate>

                        <tr>

                            <!--
                              The row identity travels with the form.
                              On a postback ASP.NET rebuilds the Repeater
                              from ViewState AFTER Page_Load has already
                              re-bound it, so RepeaterItem.DataItem is
                              always null inside the save handler. These
                              two hidden fields are the reliable source
                              of "which variant is this row".
                            -->
                            <asp:HiddenField
                                ID="hidVariantId"
                                runat="server"
                                Value='<%# Eval("ProductVariantId") %>' />

                            <asp:HiddenField
                                ID="hidVariantLabel"
                                runat="server"
                                Value='<%# Eval("VariantLabel") %>' />

                            <td>

                                <%# Eval("VariantLabel") %>

                            </td>


                            <td class="text-end">

                                <%# Eval("Dispatched") %>

                            </td>


                            <td>

                                <asp:TextBox
                                    ID="txtQty"
                                    runat="server"
                                    CssClass="form-control"
                                    TextMode="Number"
                                    Text='<%# Eval("Returned") %>'>
                                </asp:TextBox>

                            </td>


                            <td>

                                <asp:DropDownList
                                    ID="ddlCondition"
                                    runat="server"
                                    CssClass="form-select">

                                    <asp:ListItem Text="Good" Value="Good" Selected="True">
                                    </asp:ListItem>

                                    <asp:ListItem Text="Damaged" Value="Damaged">
                                    </asp:ListItem>

                                </asp:DropDownList>

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

                <i class="bi bi-arrow-return-right fs-1 text-muted"></i>

                <p class="text-muted mt-3 mb-0">

                    <asp:Label
                        ID="lblNoLines"
                        runat="server"
                        Text="Select a dispatched order above to list the goods available for return.">
                    </asp:Label>

                </p>

            </div>

        </asp:Panel>


        <asp:CustomValidator
            ID="cvLines"
            ValidationGroup="Return"
            runat="server"
            EnableClientScript="false"
            Display="Dynamic"
            CssClass="text-danger small"
            ErrorMessage="Each returned quantity must be a whole number of packets (zero or more) and at least one line must be greater than zero."
            OnServerValidate="cvLines_ServerValidate">
        </asp:CustomValidator>


        <hr class="my-4" />


        <div class="d-flex justify-content-end gap-2">

            <a id="lnkCancel"
                runat="server"
                href="Returns.aspx"
                class="btn btn-light border">

                <i class="bi bi-x-lg"></i>

                Cancel

            </a>


            <asp:Button
                ID="btnSave"
                ValidationGroup="Return"
                runat="server"
                Text="Record Return"
                CssClass="btn btn-danger px-4"
                OnClick="btnSave_Click" />

        </div>

        </asp:Panel>


    </div>

</asp:Content>
