<%@ Page Title="Stock Ledger"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="StockTransactions.aspx.cs"
    Inherits="BWDMS.Dealer.StockTransactions" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Stock Ledger

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Stock Ledger
            </h3>

            <p class="text-muted mb-0">
                Every stock movement recorded for this dealership
            </p>

        </div>


        <div class="d-flex gap-2">

            <a href="Inventory.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-arrow-left"></i>

                Back to Inventory

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


    <!-- Ledger -->

    <div class="dashboard-card">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    Transactions
                </h5>

                <small class="text-muted">
                    Newest first, with the variants that moved
                </small>

            </div>


            <div class="d-flex gap-2">

                <div class="grid-search-box">

                    <asp:TextBox
                        ID="txtSearch"
                        runat="server"
                        CssClass="form-control"
                        placeholder="Search reference or remarks..."
                        AutoPostBack="true"
                        OnTextChanged="txtSearch_TextChanged">
                    </asp:TextBox>

                </div>


                <asp:DropDownList
                    ID="ddlType"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">

                    <asp:ListItem Text="All Types" Value="" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Stock In" Value="Stock In">
                    </asp:ListItem>

                    <asp:ListItem Text="Stock Out" Value="Stock Out">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>

        </div>


        <asp:Repeater
            ID="repTransactions"
            runat="server"
            OnItemDataBound="repTransactions_ItemDataBound">

            <HeaderTemplate>

                <div class="table-responsive">

                    <table class="table table-hover align-middle mb-0">

                        <thead>

                            <tr>

                                <th>Date</th>

                                <th>Type</th>

                                <th>Reference</th>

                                <th>Remarks</th>

                                <th>Movements</th>

                            </tr>

                        </thead>


                        <tbody>

            </HeaderTemplate>


            <ItemTemplate>

                <tr>

                    <td class="text-nowrap">

                        <%# Eval("TransactionDateText") %>

                    </td>


                    <td>

                        <span class='<%# Eval("TypeCss") %>'>

                            <%# Eval("TransactionType") %>

                        </span>

                    </td>


                    <td>

                        <%# string.IsNullOrEmpty(
                            Convert.ToString(Eval("ReferenceNo")))
                                ? "-"
                                : Eval("ReferenceNo") %>

                    </td>


                    <td>

                        <%# string.IsNullOrEmpty(
                            Convert.ToString(Eval("Remarks")))
                                ? "-"
                                : Eval("Remarks") %>

                    </td>


                    <td>

                        <asp:Repeater
                            ID="repLines"
                            runat="server">

                            <ItemTemplate>

                                <div class="small">

                                    <strong>

                                        <%# Eval("ProductName") %>

                                    </strong>

                                    &ndash;

                                    <%# Eval("VariantName") %>

                                    &nbsp;

                                    <span class='<%# Convert.ToInt32(Eval("QuantityPackets")) >= 0
                                        ? "text-success"
                                        : "text-danger" %>'>

                                        <%# Convert.ToInt32(Eval("QuantityPackets")) >= 0 ? "+" : "" %><%# Eval("QuantityPackets") %> pkt

                                    </span>

                                </div>

                            </ItemTemplate>

                        </asp:Repeater>

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
            ID="pnlEmpty"
            runat="server"
            Visible="false">

            <div class="text-center py-5">

                <i class="bi bi-journal-text fs-1 text-muted"></i>

                <p class="text-muted mt-3 mb-0">
                    No stock transactions yet.
                </p>

            </div>

        </asp:Panel>

    </div>


</asp:Content>
