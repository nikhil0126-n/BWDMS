<%@ Page Title="Vehicle Stock"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="VehicleStock.aspx.cs"
    Inherits="BWDMS.Salesman.VehicleStock" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Vehicle Stock

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Vehicle Stock
            </h3>

            <p class="text-muted mb-0">
                What you loaded and what came back
            </p>

        </div>


        <a href="Dashboard.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Dashboard

        </a>

    </div>


    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <div class="dashboard-card">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    Loaded Stock
                </h5>


                <small class="text-muted">

                    <asp:Label
                        ID="lblRecordCount"
                        runat="server"
                        Text="records: 0">
                    </asp:Label>

                </small>

            </div>


            <div class="grid-search-box">

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Search vehicle or product..."
                    AutoPostBack="true"
                    OnTextChanged="FilterChanged">
                </asp:TextBox>

            </div>

        </div>


        <div class="row g-2 mb-3">

            <div class="col-md-3">

                <label class="form-label small text-muted mb-1">
                    From
                </label>


                <asp:TextBox
                    ID="txtFromDate"
                    runat="server"
                    CssClass="form-control"
                    placeholder="YYYY-MM-DD"
                    AutoPostBack="true"
                    OnTextChanged="FilterChanged">
                </asp:TextBox>

            </div>


            <div class="col-md-3">

                <label class="form-label small text-muted mb-1">
                    To
                </label>


                <asp:TextBox
                    ID="txtToDate"
                    runat="server"
                    CssClass="form-control"
                    placeholder="YYYY-MM-DD"
                    AutoPostBack="true"
                    OnTextChanged="FilterChanged">
                </asp:TextBox>

            </div>


            <div class="col-md-3">

                <label class="form-label small text-muted mb-1">
                    Reconciled
                </label>


                <asp:DropDownList
                    ID="ddlReconciled"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">

                    <asp:ListItem Text="All" Value="" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Pending" Value="0">
                    </asp:ListItem>

                    <asp:ListItem Text="Reconciled" Value="1">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvStock"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="StockDateText"
                        HeaderText="Date" />

                    <asp:BoundField
                        DataField="VehicleNumber"
                        HeaderText="Vehicle" />

                    <asp:BoundField
                        DataField="ProductName"
                        HeaderText="Product" />

                    <asp:BoundField
                        DataField="VariantName"
                        HeaderText="Variant" />

                    <asp:BoundField
                        DataField="LoadedPackets"
                        HeaderText="Loaded" />

                    <asp:BoundField
                        DataField="SoldPackets"
                        HeaderText="Sold" />

                    <asp:BoundField
                        DataField="ReturnedPackets"
                        HeaderText="Returned" />

                    <asp:BoundField
                        DataField="DamagedPackets"
                        HeaderText="Damaged" />

                    <asp:BoundField
                        DataField="ActualClosingText"
                        HeaderText="Closing" />

                    <asp:TemplateField
                        HeaderText="Variance">

                        <ItemTemplate>

                            <%# Eval("VarianceText") %>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Convert.ToBoolean(Eval("IsReconciled")) ? "badge bg-success" : "badge bg-warning text-dark" %>'>

                                <%# Convert.ToBoolean(Eval("IsReconciled")) ? "Reconciled" : "Pending" %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-truck fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No vehicle stock rows match your filters.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>
