<%@ Page Title="Shop Orders"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="ShopOrders.aspx.cs"
    Inherits="BWDMS.Dealer.ShopOrders" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Shop Orders

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                <asp:Label
                    ID="lblShopName"
                    runat="server"
                    Text="Shop Orders">
                </asp:Label>
            </h3>

            <p class="text-muted mb-0">
                <asp:Label
                    ID="lblShopMeta"
                    runat="server"
                    Text="Order history for this shop">
                </asp:Label>
            </p>

        </div>


        <div class="d-flex gap-2">

            <a href="Shops.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-shop"></i>

                Shops

            </a>


            <a href="Orders.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-list-check"></i>

                All Orders

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


    <!-- Order History For One Shop -->

    <asp:Panel
        ID="pnlContent"
        runat="server">

        <div class="dashboard-card">

            <div class="d-flex justify-content-between align-items-center mb-3">

                <div>

                    <h5 class="fw-bold mb-1">
                        Order History
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
                        placeholder="Search order number..."
                        AutoPostBack="true"
                        OnTextChanged="txtSearch_TextChanged">
                    </asp:TextBox>

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

                        <asp:ListItem Text="Pending" Value="Pending">
                        </asp:ListItem>

                        <asp:ListItem Text="Confirmed" Value="Confirmed">
                        </asp:ListItem>

                        <asp:ListItem Text="Dispatched" Value="Dispatched">
                        </asp:ListItem>

                        <asp:ListItem Text="Cancelled" Value="Cancelled">
                        </asp:ListItem>

                    </asp:DropDownList>

                </div>

            </div>


            <div class="table-responsive">

                <asp:GridView
                    ID="gvOrders"
                    runat="server"
                    AutoGenerateColumns="False"
                    CssClass="table table-hover align-middle"
                    GridLines="None"
                    DataKeyNames="OrderId" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                    <Columns>

                        <asp:BoundField
                            DataField="OrderNumber"
                            HeaderText="Order No." />

                        <asp:BoundField
                            DataField="OrderDateText"
                            HeaderText="Order Date" />

                        <asp:BoundField
                            DataField="OrderSource"
                            HeaderText="Source" />

                        <asp:TemplateField
                            HeaderText="Status">

                            <ItemTemplate>

                                <span class='<%# Eval("StatusCss") %>'>

                                    <%# Eval("Status") %>

                                </span>

                            </ItemTemplate>

                        </asp:TemplateField>


                        <asp:BoundField
                            DataField="LineCount"
                            HeaderText="Lines" />

                        <asp:BoundField
                            DataField="GrandTotalText"
                            HeaderText="Grand Total"
                            DataFormatString="{0:N2}" />

                        <asp:TemplateField
                            HeaderText="Action">

                            <ItemTemplate>

                                <asp:HyperLink
                                    ID="lnkOpen"
                                    runat="server"
                                    CssClass="btn btn-sm btn-outline-danger"
                                    NavigateUrl='<%# "~/Dealer/AddOrder.aspx?id=" + Eval("OrderId") %>'>

                                    <i class="bi bi-box-arrow-in-right"></i>

                                    Open

                                </asp:HyperLink>

                            </ItemTemplate>

                        </asp:TemplateField>

                    </Columns>


                    <EmptyDataTemplate>

                        <div class="text-center py-5">

                            <i class="bi bi-cart-check fs-1 text-muted"></i>

                            <p class="text-muted mt-3 mb-0">
                                No orders for this shop yet.
                            </p>

                        </div>

                    </EmptyDataTemplate>

                </asp:GridView>

            </div>

        </div>

    </asp:Panel>


</asp:Content>
