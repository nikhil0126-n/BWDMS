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

            <h3 class="fw-bold mb-1 page-title">
                Shops
            </h3>

            <p class="text-muted mb-0 page-subtitle">
                Manage all shops
            </p>

        </div>


        <div>

            <a href="AddShop.aspx"
               class="btn btn-primary">

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


    <!-- Search -->
    <div class="grid-search-box mb-4">

        <i class="bi bi-search search-icon"></i>

        <asp:TextBox
            ID="txtSearch"
            runat="server"
            CssClass="form-control"
            placeholder="Search Dealers...."
            AutoPostBack="true"
            OnTextChanged="txtSearch_TextChanged">
        </asp:TextBox>

    </div>


    <!-- Filters -->
    <div class="row g-2 mb-4">

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
                DataKeyNames="ShopId" AllowPaging="true" OnPageIndexChanging="gvShops_PageIndexChanging" PageSize="25" PagerSettings-Mode="Numeric" PagerSettings-PageButtonCount="10" PagerSettings-Position="Bottom" PagerSettings-PreviousPageText="&lsaquo;" PagerSettings-NextPageText="&rsaquo;">

                <Columns>

                    <asp:BoundField
                        DataField="ShopCode"
                        HeaderText="Shop Code" />

                    <asp:BoundField
                        DataField="ShopName"
                        HeaderText="Shop Name" />

                    <asp:BoundField
                        DataField="OwnerName"
                        HeaderText="Owner Name" />

                    <asp:BoundField
                        DataField="Phone"
                        HeaderText="Mobile" />

                    <asp:BoundField
                        DataField="VillageName"
                        HeaderText="Location" />

                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <%# Convert.ToBoolean(Eval("IsActive"))
                                ? "Active"
                                : "Inactive" %>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:TemplateField
                        HeaderText="Action">

                        <ItemTemplate>

                            <div class="row-actions">

                                <asp:HyperLink
                                    ID="lnkDetails"
                                    runat="server"
                                    CssClass="row-action"
                                    NavigateUrl='<%# "~/Dealer/ShopDetails.aspx?id=" + Eval("ShopId") %>'>

                                    <i class="bi bi-eye"></i>

                                </asp:HyperLink>


                                <asp:HyperLink
                                    ID="lnkEdit"
                                    runat="server"
                                    CssClass="row-action"
                                    NavigateUrl='<%# "~/Dealer/AddShop.aspx?id=" + Eval("ShopId") %>'>

                                    <i class="bi bi-pencil-square"></i>

                                </asp:HyperLink>


                                <asp:HyperLink
                                    ID="lnkOrderHistory"
                                    runat="server"
                                    CssClass="row-action"
                                    NavigateUrl='<%# "~/Dealer/ShopOrders.aspx?shopId=" + Eval("ShopId") %>'>

                                    <i class="bi bi-clock-history"></i>

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


</asp:Content>
