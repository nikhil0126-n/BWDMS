<%@ Page Title="Product Variants"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="ProductVariants.aspx.cs"
    Inherits="BWDMS.Admin.ProductVariants" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Product Variants

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Product Variants
            </h3>

            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblProductName"
                    runat="server"
                    Text="">
                </asp:Label>

            </p>

        </div>


        <div class="d-flex gap-2">

            <a href="Products.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-arrow-left"></i>

                Back to Products

            </a>


            <asp:HyperLink
                ID="lnkAddVariant"
                runat="server"
                CssClass="btn btn-danger">

                <i class="bi bi-plus-lg"></i>

                Add Variant

            </asp:HyperLink>

        </div>

    </div>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <!-- Variant List -->

    <div class="dashboard-card">

        <div class="mb-3">

            <h5 class="fw-bold mb-1">
                Variant List
            </h5>

            <small class="text-muted">
                Each variant has its own packet weight, selling units and prices.
            </small>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvVariants"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                DataKeyNames="ProductVariantId" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="VariantCode"
                        HeaderText="Code" />

                    <asp:BoundField
                        DataField="VariantName"
                        HeaderText="Variant" />

                    <asp:TemplateField
                        HeaderText="Packet Weight">

                        <ItemTemplate>

                            <%# Eval("WeightText") %>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:TemplateField
                        HeaderText="Selling Units">

                        <ItemTemplate>

                            <%# Eval("UnitText") %>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:BoundField
                        DataField="DealerPriceText"
                        HeaderText="Dealer Price" />

                    <asp:BoundField
                        DataField="ShopPriceText"
                        HeaderText="Shop Price" />

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
                                ID="lnkEdit"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-danger"
                                NavigateUrl='<%# "~/Admin/AddVariant.aspx?id=" + Eval("ProductVariantId") %>'>

                                <i class="bi bi-pencil"></i>

                                Edit

                            </asp:HyperLink>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-box-seam fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No variants yet. Click "Add Variant" to define a packet size.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>
