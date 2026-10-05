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

            <h3 class="fw-bold mb-1">
                Salesmen
            </h3>

            <p class="text-muted mb-0">
                Field accounts assigned to your dealership
            </p>

        </div>


        <a href="AddSalesman.aspx"
           class="btn btn-danger">

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



    <!-- ============================================================
         ACCOUNT LIST CARD
         ============================================================ -->

    <div class="dashboard-card">


        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    Account List
                </h5>

                <small class="text-muted">
                    Salesmen who can be picked in route schedules
                </small>

            </div>


            <div class="grid-search-box">

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Search name, email or phone..."
                    ClientIDMode="Static">
                </asp:TextBox>

            </div>

        </div>



        <div class="table-responsive">

            <asp:GridView
                ID="gvSalesmen"
                runat="server"
                ClientIDMode="Static"
                AutoGenerateColumns="False"
                CssClass="table route-table align-middle mb-0"
                GridLines="None"
                EmptyDataText="No salesman accounts yet. Click &quot;Add Account&quot; to create one." AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

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
                                ID="lnkEdit"
                                runat="server"
                                CssClass="btn btn-outline-danger btn-sm"
                                NavigateUrl='<%# "~/Dealer/AddSalesman.aspx?id=" + Eval("UserId") %>'>

                                <i class="bi bi-pencil me-1"></i>

                                Edit

                            </asp:HyperLink>

                        </ItemTemplate>

                    </asp:TemplateField>

                </Columns>

            </asp:GridView>

        </div>


    </div>


</asp:Content>
