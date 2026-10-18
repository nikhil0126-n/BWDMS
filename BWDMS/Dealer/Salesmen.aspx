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
                Field accounts assigned to your dealership
            </p>

        </div>


        <a href="AddSalesman.aspx"
           class="btn btn-primary">

            <i class="bi bi-plus-lg me-1"></i>

            Add Account

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
            placeholder="Search name, email or phone..."
            ClientIDMode="Static">
        </asp:TextBox>

    </div>


    <!-- Account List -->
    <div class="table-responsive">

            <asp:GridView
                ID="gvSalesmen"
                runat="server"
                ClientIDMode="Static"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                EmptyDataText="No salesman accounts yet. Click &quot;Add Account&quot; to create one." AllowPaging="true" OnPageIndexChanging="gvSalesmen_PageIndexChanging" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="FullName"
                        HeaderText="Full Name" />

                    <asp:BoundField
                        DataField="Email"
                        HeaderText="Email" />

                    <asp:BoundField
                        DataField="Phone"
                        HeaderText="Phone" />

                    <asp:BoundField
                        DataField="Role"
                        HeaderText="Role" />

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
                        HeaderText="Route Schedules">

                        <ItemTemplate>

                            <%# Eval("ScheduleCount") %>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:TemplateField
                        HeaderText="Action">

                        <ItemTemplate>

                            <asp:HyperLink
                                ID="lnkDetails"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-primary"
                                NavigateUrl='<%# "~/Dealer/SalesmanDetails.aspx?id=" + Eval("UserId") %>'>

                                <i class="bi bi-eye"></i>

                            </asp:HyperLink>


                            <asp:HyperLink
                                ID="lnkEdit"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-primary"
                                NavigateUrl='<%# "~/Dealer/AddSalesman.aspx?id=" + Eval("UserId") %>'>

                                <i class="bi bi-pencil"></i>

                            </asp:HyperLink>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>

            </asp:GridView>

        </div>


</asp:Content>
