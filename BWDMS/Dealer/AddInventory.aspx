<%@ Page Title="Add Stock"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddInventory.aspx.cs"
    Inherits="BWDMS.Dealer.AddInventory" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    <asp:Label
        ID="lblPageTitle"
        runat="server"
        Text="Add Stock">
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
                    Text="Add / Adjust Stock">
                </asp:Label>

            </h3>


            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblSubHeading"
                    runat="server"
                    Text="Stock is held in base packets and every change is written to the ledger">
                </asp:Label>

            </p>

        </div>


        <a href="Inventory.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Inventory

        </a>

    </div>


    <div class="dashboard-card">


        <div class="mb-4">

            <h5 class="fw-bold mb-1">
                Stock Entry
            </h5>

            <small class="text-muted">
                Fields marked <span class="text-danger">*</span> are required.
            </small>

        </div>


        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <asp:Label
            ID="lblCurrent"
            runat="server"
            CssClass="alert alert-info d-block"
            Visible="false">
        </asp:Label>


        <div class="row g-4">


            <div class="col-md-8">

                <label class="form-label fw-semibold">

                    Product Variant

                    <span class="text-danger">*</span>

                </label>


                <asp:DropDownList
                    ID="ddlVariant"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlVariant_SelectedIndexChanged">
                </asp:DropDownList>


                <asp:RequiredFieldValidator
                    ID="rfvVariant"
                    runat="server"
                    ControlToValidate="ddlVariant"
                    InitialValue=""
                    ErrorMessage="Select a product variant."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-4">

                <label class="form-label fw-semibold">
                    Reference No
                </label>


                <asp:TextBox
                    ID="txtReference"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="200"
                    placeholder="Invoice / challan no.">
                </asp:TextBox>

            </div>


            <div class="col-md-4">

                <label class="form-label fw-semibold">

                    Quantity (packets)

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtQuantity"
                    runat="server"
                    CssClass="form-control"
                    TextMode="Number"
                    Text="0">
                </asp:TextBox>


                <asp:RegularExpressionValidator
                    ID="revQuantity"
                    runat="server"
                    ControlToValidate="txtQuantity"
                    ValidationExpression="^\d{1,9}$"
                    ErrorMessage="Quantity must be a whole number of packets."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RegularExpressionValidator>


                <asp:RequiredFieldValidator
                    ID="rfvQuantity"
                    runat="server"
                    ControlToValidate="txtQuantity"
                    ErrorMessage="Quantity is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-4">

                <label class="form-label fw-semibold">
                    Reorder Level
                </label>


                <small class="text-muted d-block mb-1">
                    Flag stock as low at or below this many packets.
                </small>


                <asp:TextBox
                    ID="txtReorder"
                    runat="server"
                    CssClass="form-control"
                    TextMode="Number"
                    Text="0">
                </asp:TextBox>


                <asp:RegularExpressionValidator
                    ID="revReorder"
                    runat="server"
                    ControlToValidate="txtReorder"
                    ValidationExpression="^\d{1,9}$"
                    ErrorMessage="Reorder level must be a whole number."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RegularExpressionValidator>

            </div>


            <div class="col-12">

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
                    placeholder="Why this stock changed">
                </asp:TextBox>

            </div>


        </div>


        <hr class="my-4" />


        <div class="d-flex justify-content-end gap-2">

            <a href="Inventory.aspx"
               class="btn btn-light border">

                <i class="bi bi-x-lg"></i>

                Cancel

            </a>


            <asp:Button
                ID="btnSave"
                runat="server"
                Text="Save Stock"
                CssClass="btn btn-danger px-4"
                OnClick="btnSave_Click" />

        </div>


    </div>

</asp:Content>
