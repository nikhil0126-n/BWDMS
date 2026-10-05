<%@ Page Title="Orders"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Orders.aspx.cs"
    Inherits="BWDMS.Dealer.Orders" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Orders

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Orders
            </h3>

            <p class="text-muted mb-0">
                Shop orders taken for your dealership
            </p>

        </div>


        <div class="d-flex gap-2">

            <a href="Reports.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-bar-chart"></i>

                Reports

            </a>


            <a href="AddOrder.aspx"
               class="btn btn-danger">

                <i class="bi bi-plus-lg"></i>

                New Order

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


    <!-- Order List -->

    <div class="dashboard-card">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    All Orders
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
                    placeholder="Search order number or shop..."
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


            <div class="col-md-4">

                <asp:DropDownList
                    ID="ddlSource"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">

                    <asp:ListItem Text="All Sources" Value="" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Beat" Value="Beat">
                    </asp:ListItem>

                    <asp:ListItem Text="Telephone" Value="Telephone">
                    </asp:ListItem>

                    <asp:ListItem Text="Counter" Value="Counter">
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
                OnRowCommand="gvOrders_RowCommand"
                DataKeyNames="OrderId" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="OrderNumber"
                        HeaderText="Order No." />

                    <asp:BoundField
                        DataField="OrderDateText"
                        HeaderText="Order Date" />

                    <asp:BoundField
                        DataField="DeliveryDateText"
                        HeaderText="Delivery" />

                    <asp:BoundField
                        DataField="ShopName"
                        HeaderText="Shop" />

                    <asp:BoundField
                        DataField="SourceText"
                        HeaderText="Order Source" />

                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Eval("StatusCss") %>'>

                                <%# Eval("Status") %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:BoundField
                        DataField="ItemCount"
                        HeaderText="Lines" />

                    <asp:BoundField
                        DataField="GrandTotalText"
                        HeaderText="Grand Total"
                        DataFormatString="{0:N2}" />

                    <asp:TemplateField
                        HeaderText="Action">

                        <ItemTemplate>

                            <asp:HyperLink
                                ID="lnkEdit"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-danger"
                                NavigateUrl='<%# "~/Dealer/AddOrder.aspx?id=" + Eval("OrderId") %>'>

                                <i class="bi bi-pencil"></i>

                                Edit

                            </asp:HyperLink>


                            <asp:LinkButton
                                ID="lnkCancel"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-secondary"
                                CommandName="CancelOrder"
                                CommandArgument='<%# Eval("OrderId") %>'
                                OnClientClick="return confirm('Cancel this order? Stock does not return to the godown unless it was dispatched.');">

                                <i class="bi bi-x-circle"></i>

                                Cancel

                            </asp:LinkButton>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-cart-check fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No orders found. Use "New Order" to place one.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>
