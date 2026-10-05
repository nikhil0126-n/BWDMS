<%@ Page Title="Products"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Products.aspx.cs"
    Inherits="BWDMS.Admin.Products" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Products

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Products
            </h3>

            <p class="text-muted mb-0">
                Balaji Wafers product catalogue shared by every dealer
            </p>

        </div>


        <a href="AddProduct.aspx"
           class="btn btn-danger">

            <i class="bi bi-plus-lg"></i>

            Add Product

        </a>

    </div>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <!-- Product List -->

    <div class="dashboard-card">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    Product List
                </h5>

                <small class="text-muted">
                    Use <strong>Variants</strong> to set packet weight, units and prices
                </small>

            </div>


            <div class="grid-search-box">

                <asp:DropDownList
                    ID="ddlCategory"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="ddlCategory_SelectedIndexChanged">
                </asp:DropDownList>

            </div>

        </div>


        <div class="row g-2 mb-3">

            <div class="col-md-6">

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Search product name or code..."
                    AutoPostBack="true"
                    OnTextChanged="txtSearch_TextChanged">
                </asp:TextBox>

            </div>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvProducts"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                DataKeyNames="ProductId" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="ProductCode"
                        HeaderText="Code" />

                    <asp:BoundField
                        DataField="ProductName"
                        HeaderText="Product" />

                    <asp:BoundField
                        DataField="CategoryName"
                        HeaderText="Category" />

                    <asp:BoundField
                        DataField="VariantCount"
                        HeaderText="Variants" />

                    <asp:BoundField
                        DataField="UnitCount"
                        HeaderText="Unit Options" />

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

                            <asp:HyperLink
                                ID="lnkVariants"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-secondary"
                                NavigateUrl='<%# "~/Admin/ProductVariants.aspx?product=" + Eval("ProductId") %>'>

                                <i class="bi bi-box-seam"></i>

                                Variants

                            </asp:HyperLink>


                            <asp:HyperLink
                                ID="lnkEdit"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-danger ms-1"
                                NavigateUrl='<%# "~/Admin/AddProduct.aspx?id=" + Eval("ProductId") %>'>

                                <i class="bi bi-pencil"></i>

                                Edit

                            </asp:HyperLink>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-box fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No products found.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>
