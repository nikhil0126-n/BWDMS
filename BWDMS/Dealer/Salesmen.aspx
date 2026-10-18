<%@ Page Title="Salesmen"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Salesmen.aspx.cs"
    Inherits="BWDMS.Dealer.Salesmen" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Salesmen

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- ============================================================
         PAGE HEADER
         ============================================================ -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1 page-title">
                Salesmen
            </h3>

            <p class="text-muted mb-0 page-subtitle">
                Manage All salesmen
            </p>

        </div>


        <a href="AddSalesman.aspx"
           class="btn btn-primary">

            <i class="bi bi-plus-lg me-1"></i>

            Add salesmen

        </a>

    </div>



    <!-- ============================================================
         MESSAGE PANEL
         ============================================================ -->

    <asp:Panel
        ID="pnlMessage"
        runat="server"
        Visible="false"
        CssClass="alert mb-4">

        <asp:Label
            ID="lblMessage"
            runat="server">
        </asp:Label>

    </asp:Panel>



    <!-- Search -->
    <div class="grid-search-box mb-4">

        <i class="bi bi-search search-icon"></i>

        <asp:TextBox
            ID="txtSearch"
            runat="server"
            CssClass="form-control"
            placeholder="Search salesmen...."
            ClientIDMode="Static">
        </asp:TextBox>

    </div>


    <!-- Salesmen List -->
    <div class="table-responsive">

            <asp:GridView
                ID="gvSalesmen"
                runat="server"
                ClientIDMode="Static"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                EmptyDataText="No salesmen found. Click &quot;Add salesmen&quot; to create one."
                AllowPaging="true"
                OnPageIndexChanging="gvSalesmen_PageIndexChanging"
                OnRowCommand="gvSalesmen_RowCommand"
                PageSize="25"
                PagerSettings-Mode="NumericFirstLast"
                PagerSettings-PageButtonCount="10"
                PagerSettings-Position="Bottom">

                <Columns>

                    <asp:BoundField
                        DataField="SalesmanCode"
                        HeaderText="Code" />

                    <asp:BoundField
                        DataField="FullName"
                        HeaderText="Salesman Name" />

                    <asp:BoundField
                        DataField="Phone"
                        HeaderText="Mobile" />

                    <asp:BoundField
                        DataField="VehicleNumber"
                        HeaderText="Vehicle" />

                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <small class='<%# Convert.ToBoolean(Eval("IsActive"))
                                ? "detail-status"
                                : "text-muted" %>'>

                                <%# Convert.ToBoolean(Eval("IsActive"))
                                    ? "Active"
                                    : "Inactive" %>

                            </small>

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
                                    ToolTip="View Details"
                                    NavigateUrl='<%# "~/Dealer/SalesmanDetails.aspx?id=" + Eval("UserId") %>'>

                                    <i class="bi bi-eye"></i>

                                </asp:HyperLink>


                                <asp:LinkButton
                                    ID="lnkEdit"
                                    runat="server"
                                    CssClass="row-action"
                                    CommandName="EditSalesman"
                                    CommandArgument='<%# Eval("UserId") %>'
                                    ToolTip="Edit">

                                    <i class="bi bi-pencil-square"></i>

                                </asp:LinkButton>


                                <asp:LinkButton
                                    ID="lnkDelete"
                                    runat="server"
                                    CssClass="row-action"
                                    CommandName="DeleteSalesman"
                                    CommandArgument='<%# Eval("UserId") %>'
                                    ToolTip="Delete"
                                    OnClientClick="return confirm('Delete this salesman?');">

                                    <i class="bi bi-trash"></i>

                                </asp:LinkButton>

                            </div>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>

            </asp:GridView>

        </div>


</asp:Content>
