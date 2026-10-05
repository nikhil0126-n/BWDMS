<%@ Page Title="Inventory"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Inventory.aspx.cs"
    Inherits="BWDMS.Dealer.Inventory" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Inventory

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Inventory
            </h3>

            <p class="text-muted mb-0">
                Stock held at your godown, counted in base packets
            </p>

        </div>


        <div class="d-flex gap-2">

            <a href="StockTransactions.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-journal-text"></i>

                Stock Ledger

            </a>


            <a href="AddInventory.aspx"
               class="btn btn-danger">

                <i class="bi bi-plus-lg"></i>

                Add / Adjust Stock

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


    <!-- Stock List -->

    <div class="dashboard-card">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    Current Stock
                </h5>

                <small class="text-muted">
                    Items at or below the reorder level are flagged
                </small>

            </div>


            <div class="grid-search-box">

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Search product or variant..."
                    AutoPostBack="true"
                    OnTextChanged="txtSearch_TextChanged">
                </asp:TextBox>

            </div>

        </div>


        <div class="row g-2 mb-3">

            <div class="col-md-4">

                <asp:DropDownList
                    ID="ddlStockFilter"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">

                    <asp:ListItem Text="All Stock" Value="" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Low / Out of Stock" Value="low">
                    </asp:ListItem>

                    <asp:ListItem Text="Out of Stock" Value="out">
                    </asp:ListItem>

                    <asp:ListItem Text="In Stock" Value="ok">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvInventory"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                DataKeyNames="InventoryId" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="ProductName"
                        HeaderText="Product" />

                    <asp:BoundField
                        DataField="VariantName"
                        HeaderText="Variant" />

                    <asp:BoundField
                        DataField="WeightText"
                        HeaderText="Weight" />

                    <asp:BoundField
                        DataField="QuantityPackets"
                        HeaderText="Stock (packets)" />

                    <asp:BoundField
                        DataField="ReorderLevel"
                        HeaderText="Reorder At" />

                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Eval("StatusCss") %>'>

                                <%# Eval("StatusText") %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:BoundField
                        DataField="UpdatedAtText"
                        HeaderText="Last Updated" />

                    <asp:TemplateField
                        HeaderText="Action">

                        <ItemTemplate>

                            <asp:HyperLink
                                ID="lnkEdit"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-danger"
                                NavigateUrl='<%# Convert.ToInt32(Eval("InventoryId")) > 0
                                    ? "~/Dealer/AddInventory.aspx?id=" + Eval("InventoryId")
                                    : "~/Dealer/AddInventory.aspx?variant=" + Eval("ProductVariantId") %>'>

                                <i class="bi bi-pencil"></i>

                                <%# Convert.ToInt32(Eval("InventoryId")) > 0
                                    ? "Edit"
                                    : "Add" %>

                            </asp:HyperLink>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-box-seam fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No stock recorded yet. Use "Add / Adjust Stock".
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>
