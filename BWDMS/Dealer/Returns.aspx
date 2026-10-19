<%@ Page Title="Sales Returns"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Returns.aspx.cs"
    Inherits="BWDMS.Dealer.Returns" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Sales Returns

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1 page-title">
                Sales Returns
            </h3>

            <p class="text-muted mb-0 page-subtitle">
                Goods sent back by shops against dispatched orders
            </p>

        </div>


        <div class="d-flex gap-2">

            <a href="Reports.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-bar-chart"></i>

                Reports

            </a>


            <a href="AddReturn.aspx"
               class="btn btn-primary">

                <i class="bi bi-plus-lg"></i>

                Record Return

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


    <!-- Search -->

    <div class="grid-search-box mb-4">

        <i class="bi bi-search search-icon"></i>

        <asp:TextBox
            ID="txtSearch"
            runat="server"
            CssClass="form-control"
            placeholder="Search return number or order number..."
            AutoPostBack="true"
            OnTextChanged="txtSearch_TextChanged">
        </asp:TextBox>

    </div>


    <!-- Filters -->

    <div class="dashboard-card mb-4">

        <div class="row g-2">

            <div class="col-md-4">

                <asp:DropDownList
                    ID="ddlStatus"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">

                    <asp:ListItem Text="All Statuses" Value="" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Posted" Value="Posted">
                    </asp:ListItem>

                    <asp:ListItem Text="Draft" Value="Draft">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>

        </div>

    </div>


    <!-- Return List -->

    <div class="table-responsive">

            <asp:GridView
                ID="gvReturns"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                DataKeyNames="SalesReturnId" AllowPaging="true" OnPageIndexChanging="gvReturns_PageIndexChanging" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="ReturnNumber"
                        HeaderText="Return No." />

                    <asp:BoundField
                        DataField="ReturnDateText"
                        HeaderText="Return Date" />

                    <asp:BoundField
                        DataField="OrderNumber"
                        HeaderText="Order No." />

                    <asp:BoundField
                        DataField="ShopName"
                        HeaderText="Shop" />

                    <asp:BoundField
                        DataField="LineCount"
                        HeaderText="Lines" />

                    <asp:BoundField
                        DataField="TotalPackets"
                        HeaderText="Packets" />

                    <asp:BoundField
                        DataField="Conditions"
                        HeaderText="Condition" />

                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Eval("StatusCss") %>'>

                                <%# Eval("Status") %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:BoundField
                        DataField="Reason"
                        HeaderText="Reason" />

                    <asp:TemplateField
                        HeaderText="Action">

                        <ItemTemplate>

                            <asp:HyperLink
                                ID="lnkView"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-danger"
                                NavigateUrl='<%# "~/Dealer/AddReturn.aspx?returnId=" + Eval("SalesReturnId") %>'>

                                <i class="bi bi-eye"></i>

                                View

                            </asp:HyperLink>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-arrow-return-right fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No returns recorded yet. Use "Record Return" to log goods sent back by a shop.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>


</asp:Content>
