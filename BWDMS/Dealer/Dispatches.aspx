<%@ Page Title="Dispatches"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Dispatches.aspx.cs"
    Inherits="BWDMS.Dealer.Dispatches" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Dispatches

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Dispatches
            </h3>

            <p class="text-muted mb-0">
                Vehicle loads that moved packets out of the godown
            </p>

        </div>


        <div class="d-flex gap-2">

            <a href="StockTransactions.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-journal-text"></i>

                Stock Ledger

            </a>


            <a href="AddDispatch.aspx"
               class="btn btn-danger">

                <i class="bi bi-plus-lg"></i>

                Create Dispatch

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


    <!-- Dispatch List -->

    <div class="dashboard-card">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    All Dispatches
                </h5>

                <small class="text-muted">
                    Newest first
                </small>

            </div>


            <div class="grid-search-box">

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Search dispatch number..."
                    AutoPostBack="true"
                    OnTextChanged="txtSearch_TextChanged">
                </asp:TextBox>

            </div>

        </div>


        <div class="row g-2 mb-3">

            <div class="col-md-3">

                <asp:DropDownList
                    ID="ddlStatus"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">

                    <asp:ListItem Text="All Statuses" Value="" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Dispatched" Value="Dispatched">
                    </asp:ListItem>

                    <asp:ListItem Text="Cancelled" Value="Cancelled">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>


            <div class="col-md-3">

                <asp:TextBox
                    ID="txtDateFrom"
                    runat="server"
                    CssClass="form-control"
                    placeholder="From YYYY-MM-DD"
                    AutoPostBack="true"
                    OnTextChanged="FilterChanged">
                </asp:TextBox>

            </div>


            <div class="col-md-3">

                <asp:TextBox
                    ID="txtDateTo"
                    runat="server"
                    CssClass="form-control"
                    placeholder="To YYYY-MM-DD"
                    AutoPostBack="true"
                    OnTextChanged="FilterChanged">
                </asp:TextBox>

            </div>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvDispatches"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                DataKeyNames="DispatchId" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="DispatchNumber"
                        HeaderText="Dispatch No." />

                    <asp:BoundField
                        DataField="DispatchDateText"
                        HeaderText="Dispatch Date" />

                    <asp:BoundField
                        DataField="VehicleNumber"
                        HeaderText="Vehicle" />

                    <asp:BoundField
                        DataField="SalesmanName"
                        HeaderText="Salesman" />

                    <asp:BoundField
                        DataField="OrderCount"
                        HeaderText="Orders" />

                    <asp:BoundField
                        DataField="TotalPackets"
                        HeaderText="Packets" />

                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Eval("StatusCss") %>'>

                                <%# Eval("Status") %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:BoundField
                        DataField="Remarks"
                        HeaderText="Remarks" />

                    <asp:TemplateField
                        HeaderText="Action">

                        <ItemTemplate>

                            <asp:HyperLink
                                ID="lnkView"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-danger"
                                NavigateUrl='<%# "~/Dealer/DispatchDetails.aspx?id=" + Eval("DispatchId") %>'>

                                <i class="bi bi-eye"></i>

                                View

                            </asp:HyperLink>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-truck fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No dispatches yet. Use "Create Dispatch" to load a vehicle.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>
