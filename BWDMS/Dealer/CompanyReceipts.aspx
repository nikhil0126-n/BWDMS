<%@ Page Title="Company Receipts"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="CompanyReceipts.aspx.cs"
    Inherits="BWDMS.Dealer.CompanyReceipts" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Company Receipts

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Company Receipts
            </h3>

            <p class="text-muted mb-0">
                Goods received from Balaji Wafers - posting a receipt raises godown stock
            </p>

        </div>


        <div class="d-flex gap-2">

            <a href="StockTransactions.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-journal-text"></i>

                Stock Ledger

            </a>


            <a href="AddCompanyReceipt.aspx"
               class="btn btn-danger">

                <i class="bi bi-plus-lg"></i>

                Add Receipt

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


    <!-- Receipt List -->

    <div class="dashboard-card">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    All Receipts
                </h5>

                <small class="text-muted">
                    Newest first
                </small>

            </div>


            <div class="d-flex gap-2 align-items-center">

                <div class="grid-search-box">

                    <asp:TextBox
                        ID="txtSearch"
                        runat="server"
                        CssClass="form-control"
                        placeholder="Search receipt or invoice...">
                    </asp:TextBox>

                </div>


                <asp:Button
                    ID="btnSearch"
                    runat="server"
                    Text="Search"
                    CssClass="btn btn-outline-secondary"
                    OnClick="btnSearch_Click">
                </asp:Button>

            </div>

        </div>


        <div class="row g-2 mb-3">

            <div class="col-md-4">

                <asp:DropDownList
                    ID="ddlStatus"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">

                    <asp:ListItem Text="All Statuses" Value="" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Draft" Value="Draft">
                    </asp:ListItem>

                    <asp:ListItem Text="Posted" Value="Posted">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvReceipts"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                DataKeyNames="ReceiptId" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="ReceiptNumber"
                        HeaderText="Receipt No." />

                    <asp:BoundField
                        DataField="ReceiptDateText"
                        HeaderText="Receipt Date" />

                    <asp:BoundField
                        DataField="InvoiceText"
                        HeaderText="Company Invoice" />

                    <asp:BoundField
                        DataField="LineCount"
                        HeaderText="Lines" />

                    <asp:BoundField
                        DataField="TotalCostText"
                        HeaderText="Total Cost" />

                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Eval("StatusCss") %>'>

                                <%# Eval("Status") %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:BoundField
                        DataField="NotesText"
                        HeaderText="Notes" />

                    <asp:TemplateField
                        HeaderText="Action">

                        <ItemTemplate>

                            <asp:HyperLink
                                ID="lnkView"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-danger"
                                NavigateUrl='<%# "~/Dealer/AddCompanyReceipt.aspx?id=" + Eval("ReceiptId") %>'>

                                <i class="bi bi-eye"></i>

                                View

                            </asp:HyperLink>


                            <asp:LinkButton
                                ID="lnkPost"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-success"
                                Visible='<%# (bool)Eval("CanPost") %>'
                                CommandArgument='<%# Eval("ReceiptId") %>'
                                OnClick="lnkPost_Click">

                                <i class="bi bi-check2-circle"></i>

                                Post

                            </asp:LinkButton>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-box-seam fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No receipts found. Use "Add Receipt" to record goods received.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>
