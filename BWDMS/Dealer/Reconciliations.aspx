<%@ Page Title="Stock Reconciliation"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Reconciliations.aspx.cs"
    Inherits="BWDMS.Dealer.Reconciliations" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Stock Reconciliation

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Stock Reconciliation
            </h3>

            <p class="text-muted mb-0">
                Daily vehicle stock counts and their variances
            </p>

        </div>


        <div class="d-flex gap-2">

            <a href="~/Dealer/Inventory.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-arrow-left"></i>

                Back to Inventory

            </a>


            <a href="AddReconciliation.aspx"
               class="btn btn-danger">

                <i class="bi bi-plus-lg"></i>

                Add Reconciliation

            </a>

        </div>

    </div>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <!-- Reconciliation List -->

    <div class="dashboard-card">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    Reconciliations
                </h5>

                <small class="text-muted">
                    One reconciliation per vehicle, per day
                </small>

            </div>


            <div class="grid-search-box">

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Search vehicle number or remarks..."
                    AutoPostBack="true"
                    OnTextChanged="txtSearch_TextChanged">
                </asp:TextBox>

            </div>

        </div>


        <div class="row g-2 mb-3">

            <div class="col-md-4">

                <asp:DropDownList
                    ID="ddlApprovedFilter"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">

                    <asp:ListItem Text="All" Value="" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Approved" Value="1">
                    </asp:ListItem>

                    <asp:ListItem Text="Pending" Value="0">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvReconciliations"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                DataKeyNames="VehicleReconciliationId" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="ReconciliationDateText"
                        HeaderText="Date" />

                    <asp:BoundField
                        DataField="VehicleNumber"
                        HeaderText="Vehicle" />

                    <asp:BoundField
                        DataField="SalesmanName"
                        HeaderText="Salesman" />

                    <asp:BoundField
                        DataField="DriverName"
                        HeaderText="Driver" />

                    <asp:BoundField
                        DataField="VarianceSummary"
                        HeaderText="Variance" />

                    <asp:BoundField
                        DataField="Remarks"
                        HeaderText="Remarks" />

                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Convert.ToBoolean(Eval("IsApproved"))
                                ? "badge bg-success"
                                : "badge bg-warning text-dark" %>'>

                                <%# Convert.ToBoolean(Eval("IsApproved"))
                                    ? "Approved"
                                    : "Pending" %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:TemplateField
                        HeaderText="Action">

                        <ItemTemplate>

                            <asp:HyperLink
                                ID="lnkEdit"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-danger"
                                NavigateUrl='<%# "~/Dealer/AddReconciliation.aspx?id=" + Eval("VehicleReconciliationId") %>'>

                                <i class="bi bi-pencil"></i>

                                Edit

                            </asp:HyperLink>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-clipboard-check fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No reconciliations found.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>
