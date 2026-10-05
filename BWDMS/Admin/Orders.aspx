<%@ Page Title="All Orders"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Orders.aspx.cs"
    Inherits="BWDMS.Admin.Orders" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    All Orders

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                All Orders
            </h3>

            <p class="text-muted mb-0">
                Every order across all dealers
            </p>

        </div>


        <div class="d-flex gap-2">

            <a href="Users.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-people"></i>

                Users

            </a>


            <a href="Reports.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-bar-chart"></i>

                Reports

            </a>

        </div>

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
                    Orders
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
                    placeholder="Search order, shop or dealer..."
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
                GridLines="None" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="OrderNumber"
                        HeaderText="Order No." />

                    <asp:BoundField
                        DataField="DealerName"
                        HeaderText="Dealer" />

                    <asp:BoundField
                        DataField="ShopName"
                        HeaderText="Shop" />

                    <asp:BoundField
                        DataField="OrderDateText"
                        HeaderText="Order Date" />

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
                        HeaderText="Grand Total" />

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-cart-check fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No orders found.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>
