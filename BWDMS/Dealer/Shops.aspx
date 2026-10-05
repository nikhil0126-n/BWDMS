<%@ Page Title="Shops"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Shops.aspx.cs"
    Inherits="BWDMS.Dealer.Shops" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Shops

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Shops
            </h3>

            <p class="text-muted mb-0">
                Retail shops served by your dealership
            </p>

        </div>


        <div>

            <a href="AddShop.aspx"
               class="btn btn-danger">

                <i class="bi bi-plus-lg"></i>

                Add Shop

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


    <!-- Shop List -->

    <div class="dashboard-card">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    Shop List
                </h5>

                <small class="text-muted">
                    Shops assigned to your routes and villages
                </small>

            </div>


            <div class="grid-search-box">

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Search shop, owner or phone..."
                    AutoPostBack="true"
                    OnTextChanged="txtSearch_TextChanged">
                </asp:TextBox>

            </div>

        </div>


        <div class="row g-2 mb-3">

            <div class="col-md-4">

                <asp:DropDownList
                    ID="ddlVillageFilter"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">
                </asp:DropDownList>

            </div>


            <div class="col-md-4">

                <asp:DropDownList
                    ID="ddlStatusFilter"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">

                    <asp:ListItem Text="All Statuses" Value="" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Active" Value="1">
                    </asp:ListItem>

                    <asp:ListItem Text="Inactive" Value="0">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvShops"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                DataKeyNames="ShopId" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="ShopCode"
                        HeaderText="Code" />

                    <asp:BoundField
                        DataField="ShopName"
                        HeaderText="Shop" />

                    <asp:BoundField
                        DataField="OwnerName"
                        HeaderText="Owner" />

                    <asp:BoundField
                        DataField="Phone"
                        HeaderText="Phone" />

                    <asp:BoundField
                        DataField="VillageName"
                        HeaderText="Village" />

                    <asp:BoundField
                        DataField="RouteName"
                        HeaderText="Route" />

                    <asp:BoundField
                        DataField="OrderCount"
                        HeaderText="Orders" />

                    <asp:BoundField
                        DataField="OpeningBalanceText"
                        HeaderText="Opening Bal." />

                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Convert.ToBoolean(Eval("IsActive"))
                                ? "badge bg-success"
                                : "badge bg-secondary" %>'>

                                <%# Convert.ToBoolean(Eval("IsActive"))
                                    ? "Active"
                                    : "Inactive" %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:TemplateField
                        HeaderText="Action">

                        <ItemTemplate>

                            <div class="d-flex gap-2">

                                <asp:HyperLink
                                    ID="lnkEdit"
                                    runat="server"
                                    CssClass="btn btn-sm btn-outline-danger"
                                    NavigateUrl='<%# "~/Dealer/AddShop.aspx?id=" + Eval("ShopId") %>'>

                                    <i class="bi bi-pencil"></i>

                                    Edit

                                </asp:HyperLink>


                                <asp:HyperLink
                                    ID="lnkOrderHistory"
                                    runat="server"
                                    CssClass="btn btn-sm btn-outline-danger"
                                    NavigateUrl='<%# "~/Dealer/ShopOrders.aspx?shopId=" + Eval("ShopId") %>'>

                                    <i class="bi bi-clock-history"></i>

                                    Order History

                                </asp:HyperLink>

                            </div>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-shop fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No shops found.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>
