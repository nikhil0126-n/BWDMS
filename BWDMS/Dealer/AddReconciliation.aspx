<%@ Page Title="Stock Reconciliation"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddReconciliation.aspx.cs"
    Inherits="BWDMS.Dealer.AddReconciliation" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    <asp:Label
        ID="lblPageTitle"
        runat="server"
        Text="Add Reconciliation">
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
                    Text="Add Reconciliation">
                </asp:Label>

            </h3>


            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblSubHeading"
                    runat="server"
                    Text="Count the packets loaded on a vehicle for a day">
                </asp:Label>

            </p>

        </div>


        <a href="Reconciliations.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Reconciliations

        </a>

    </div>


    <div class="dashboard-card">


        <div class="mb-4">

            <h5 class="fw-bold mb-1">
                Reconciliation Details
            </h5>

            <small class="text-muted">
                Fields marked <span class="text-danger">*</span> are required.
                One reconciliation is kept per vehicle, per day.
            </small>

        </div>


        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <div class="row g-4">


            <div class="col-md-4">

                <label class="form-label fw-semibold">

                    Date

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtDate"
                    runat="server"
                    CssClass="form-control"
                    TextMode="Date"
                    AutoPostBack="true"
                    OnTextChanged="txtDate_TextChanged">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvDate"
                    runat="server"
                    ControlToValidate="txtDate"
                    ErrorMessage="Reconciliation date is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-4">

                <label class="form-label fw-semibold">

                    Route / Schedule

                </label>


                <!--
                  The end-of-day count closes one route out. Leaving this
                  blank counts every load that vehicle had on the day; picking
                  a route limits it to what that route loaded.
                -->
                <asp:DropDownList
                    ID="ddlRouteSchedule"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlRouteSchedule_SelectedIndexChanged">
                </asp:DropDownList>

            </div>


            <div class="col-md-4">

                <label class="form-label fw-semibold">

                    Vehicle

                    <span class="text-danger">*</span>

                </label>


                <asp:DropDownList
                    ID="ddlVehicle"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlVehicle_SelectedIndexChanged">
                </asp:DropDownList>


                <asp:RequiredFieldValidator
                    ID="rfvVehicle"
                    runat="server"
                    ControlToValidate="ddlVehicle"
                    ErrorMessage="Select a vehicle."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-4">

                <label class="form-label fw-semibold">

                    Status

                </label>


                <asp:DropDownList
                    ID="ddlApproved"
                    runat="server"
                    CssClass="form-select">

                    <asp:ListItem Text="Pending" Value="0" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Approved" Value="1">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>


            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Salesman

                </label>


                <small class="text-muted d-block mb-1">
                    Optional.
                </small>


                <asp:DropDownList
                    ID="ddlSalesman"
                    runat="server"
                    CssClass="form-select">
                </asp:DropDownList>

            </div>


            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Driver

                </label>


                <small class="text-muted d-block mb-1">
                    Optional.
                </small>


                <asp:DropDownList
                    ID="ddlDriver"
                    runat="server"
                    CssClass="form-select">
                </asp:DropDownList>

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
                    placeholder="Optional notes about this count">
                </asp:TextBox>

            </div>

        </div>


        <hr class="my-4" />


        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    Stock Lines
                </h5>

                <small class="text-muted">
                    Expected packets come from the vehicle stock for the
                    selected vehicle and date. Leave a count blank to treat
                    it as 0.
                </small>

            </div>

        </div>


        <asp:Repeater
            ID="repDetails"
            runat="server">

            <HeaderTemplate>

                <div class="table-responsive">

                    <table class="table table-hover align-middle mb-0">

                        <thead>

                            <tr>

                                <th>Product</th>

                                <th>Variant</th>

                                <th class="text-end">Expected</th>

                                <th style="width: 140px;">Actual</th>

                                <th>Variance</th>

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

                        &nbsp;

                        <span class="text-muted small">

                            <%# Eval("WeightText") %>

                        </span>


                        <asp:HiddenField
                            ID="hfVariantId"
                            runat="server"
                            Value='<%# Eval("ProductVariantId") %>' />

                    </td>


                    <td class="text-end">

                        <asp:Label
                            ID="lblExpected"
                            runat="server"
                            CssClass="fw-semibold"
                            Text='<%# Eval("ExpectedPackets") %>'>
                        </asp:Label>

                    </td>


                    <td>

                        <asp:TextBox
                            ID="txtActual"
                            runat="server"
                            CssClass="form-control form-control-sm"
                            MaxLength="9"
                            placeholder="0"
                            Text='<%# Eval("ActualPackets") %>'>
                        </asp:TextBox>


                        <asp:RegularExpressionValidator
                            ID="revActual"
                            runat="server"
                            ControlToValidate="txtActual"
                            ValidationExpression="^\d*$"
                            ErrorMessage="Whole number only."
                            CssClass="text-danger small"
                            Display="Dynamic">
                        </asp:RegularExpressionValidator>

                    </td>


                    <td>

                        <span class='<%# Eval("VarianceCss") %>'>

                            <%# Eval("VarianceText") %>

                        </span>

                    </td>

                </tr>

            </ItemTemplate>


            <FooterTemplate>

                        </tbody>

                    </table>

                </div>

            </FooterTemplate>

        </asp:Repeater>


        <asp:Panel
            ID="pnlNoVariants"
            runat="server"
            Visible="false">

            <div class="text-center py-5">

                <i class="bi bi-box-seam fs-1 text-muted"></i>

                <p class="text-muted mt-3 mb-0">
                    No active product variants found.
                </p>

            </div>

        </asp:Panel>


        <hr class="my-4" />


        <div class="d-flex justify-content-end gap-2">

            <a href="Reconciliations.aspx"
               class="btn btn-light border">

                <i class="bi bi-x-lg"></i>

                Cancel

            </a>


            <asp:Button
                ID="btnSave"
                runat="server"
                Text="Create Reconciliation"
                CssClass="btn btn-danger px-4"
                OnClick="btnSave_Click" />

        </div>


    </div>

</asp:Content>
