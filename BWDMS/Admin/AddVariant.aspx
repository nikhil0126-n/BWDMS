<%@ Page Title="Add Variant"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddVariant.aspx.cs"
    Inherits="BWDMS.Admin.AddVariant" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    <asp:Label
        ID="lblPageTitle"
        runat="server"
        Text="Add Variant">
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
                    Text="Add Variant">
                </asp:Label>

            </h3>


            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblSubHeading"
                    runat="server"
                    Text="">
                </asp:Label>

            </p>

        </div>


        <asp:HyperLink
            ID="lnkBack"
            runat="server"
            CssClass="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Variants

        </asp:HyperLink>

    </div>


    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <!-- ============================================================
         VARIANT DETAILS
         ============================================================ -->

    <div class="dashboard-card mb-4">

        <div class="mb-4">

            <h5 class="fw-bold mb-1">
                Variant Details
            </h5>

            <small class="text-muted">
                A variant is one packet size or printed-price edition.
            </small>

        </div>


        <div class="row g-4">


            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Variant Name

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtVariantName"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="500"
                    placeholder="Example: 100 g Packet">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvVariantName"
                    runat="server"
                    ControlToValidate="txtVariantName"
                    ErrorMessage="Variant name is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Variant Code
                </label>


                <asp:TextBox
                    ID="txtVariantCode"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="200"
                    placeholder="Optional internal code">
                </asp:TextBox>

            </div>


            <div class="col-md-3">

                <label class="form-label fw-semibold">
                    Packet Weight
                </label>


                <asp:TextBox
                    ID="txtPacketWeight"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="9"
                    placeholder="Example: 100">
                </asp:TextBox>


                <asp:RegularExpressionValidator
                    ID="revPacketWeight"
                    runat="server"
                    ControlToValidate="txtPacketWeight"
                    ValidationExpression="^\d{0,7}(\.\d{1,2})?$"
                    ErrorMessage="Enter a valid weight."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RegularExpressionValidator>

            </div>


            <div class="col-md-3">

                <label class="form-label fw-semibold">
                    Weight Unit
                </label>


                <asp:DropDownList
                    ID="ddlWeightUnit"
                    runat="server"
                    CssClass="form-select">

                    <asp:ListItem Text="g" Value="g" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="kg" Value="kg">
                    </asp:ListItem>

                    <asp:ListItem Text="ml" Value="ml">
                    </asp:ListItem>

                    <asp:ListItem Text="L" Value="L">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>


            <div class="col-md-3">

                <label class="form-label fw-semibold">
                    Sort Order
                </label>


                <asp:TextBox
                    ID="txtSortOrder"
                    runat="server"
                    CssClass="form-control"
                    TextMode="Number"
                    Text="0">
                </asp:TextBox>

            </div>


            <div class="col-md-3">

                <label class="form-label fw-semibold">
                    Status
                </label>


                <asp:DropDownList
                    ID="ddlStatus"
                    runat="server"
                    CssClass="form-select">

                    <asp:ListItem Text="Active" Value="1" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Inactive" Value="0">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>


        </div>

    </div>



    <!-- ============================================================
         UNITS AND PRICES
         ============================================================ -->

    <div class="dashboard-card mb-4">

        <div class="mb-4">

            <h5 class="fw-bold mb-1">
                Selling Units &amp; Prices
            </h5>

            <small class="text-muted">
                Tick every unit this variant is sold in, then set the conversion
                and the three prices. Packet is the base unit.
            </small>

        </div>


        <div class="table-responsive">

            <asp:Repeater
                ID="repUnits"
                runat="server">

                <HeaderTemplate>

                    <table class="table table-hover align-middle mb-0">

                        <thead>

                            <tr>

                                <th style="width: 90px;">Sell In</th>

                                <th>Unit</th>

                                <th style="width: 170px;">

                                    Packets per Unit

                                    <i class="bi bi-question-circle"
                                       title="How many base packets make one of this unit.">
                                    </i>

                                </th>

                                <th style="width: 140px;">

                                    Printed Price

                                    <i class="bi bi-question-circle"
                                       title="MRP printed on the pack.">
                                    </i>

                                </th>

                                <th style="width: 150px;">

                                    Dealer Price

                                    <i class="bi bi-question-circle"
                                       title="Company to dealer (your purchase price).">
                                    </i>

                                </th>

                                <th style="width: 150px;">

                                    Shop Price

                                    <i class="bi bi-question-circle"
                                       title="Dealer to shop selling price.">
                                    </i>

                                </th>

                            </tr>

                        </thead>


                        <tbody>

                </HeaderTemplate>


                <ItemTemplate>

                    <tr>

                        <td>

                            <asp:CheckBox
                                ID="chkEnable"
                                runat="server"
                                Checked='<%# Convert.ToBoolean(Eval("Checked")) %>' />

                            <asp:HiddenField
                                ID="hidUnitId"
                                runat="server"
                                Value='<%# Eval("UnitId") %>' />

                        </td>


                        <td>

                            <strong>
                                <%# Eval("UnitName") %>
                            </strong>

                            <br />

                            <small class="text-muted">
                                <%# Eval("UnitCode") %>
                            </small>

                        </td>


                        <td>

                            <asp:TextBox
                                ID="txtPacketsPerUnit"
                                runat="server"
                                CssClass="form-control form-control-sm"
                                TextMode="Number"
                                Text='<%# Eval("PacketsPerUnit") %>'>
                            </asp:TextBox>

                        </td>


                        <td>

                            <asp:TextBox
                                ID="txtPrintedPrice"
                                runat="server"
                                CssClass="form-control form-control-sm"
                                Text='<%# Eval("PrintedPrice") %>'>
                            </asp:TextBox>

                        </td>


                        <td>

                            <asp:TextBox
                                ID="txtDealerPrice"
                                runat="server"
                                CssClass="form-control form-control-sm"
                                Text='<%# Eval("DealerPrice") %>'>
                            </asp:TextBox>

                        </td>


                        <td>

                            <asp:TextBox
                                ID="txtShopPrice"
                                runat="server"
                                CssClass="form-control form-control-sm"
                                Text='<%# Eval("ShopPrice") %>'>
                            </asp:TextBox>

                        </td>

                    </tr>

                </ItemTemplate>


                <FooterTemplate>

                        </tbody>

                    </table>

                </FooterTemplate>

            </asp:Repeater>

        </div>


        <small class="text-muted d-block mt-3">

            Conversions are stored per unit, so a box = 6 patti = 72 packets is
            expressed as <strong>Packets per Unit</strong> on each row.

        </small>

    </div>



    <!-- ============================================================
         ACTIONS
         ============================================================ -->

    <div class="d-flex justify-content-end gap-2">

        <asp:HyperLink
            ID="lnkCancel"
            runat="server"
            CssClass="btn btn-light border">

            <i class="bi bi-x-lg"></i>

            Cancel

        </asp:HyperLink>


        <asp:Button
            ID="btnSave"
            runat="server"
            Text="Create Variant"
            CssClass="btn btn-danger px-4"
            OnClick="btnSave_Click" />

    </div>

</asp:Content>
