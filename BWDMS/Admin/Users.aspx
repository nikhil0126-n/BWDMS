<%@ Page Title="Users"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Users.aspx.cs"
    Inherits="BWDMS.Admin.Users" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Users

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Users
            </h3>

            <p class="text-muted mb-0">
                Every account on the system
            </p>

        </div>


        <div class="d-flex gap-2">

            <a href="Dealers.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-shop"></i>

                Dealers

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
                    All Accounts
                </h5>

                <small class="text-muted">
                    Admins, dealers and salesmen
                </small>

            </div>


            <div class="grid-search-box">

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Search name, email or phone..."
                    AutoPostBack="true"
                    OnTextChanged="txtSearch_TextChanged">
                </asp:TextBox>

            </div>

        </div>


        <div class="row g-2 mb-3">

            <div class="col-md-4">

                <asp:DropDownList
                    ID="ddlRole"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">

                    <asp:ListItem Text="All Roles" Value="" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Admin" Value="Admin">
                    </asp:ListItem>

                    <asp:ListItem Text="Dealer" Value="Dealer">
                    </asp:ListItem>

                    <asp:ListItem Text="Salesman" Value="Salesman">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvUsers"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                <Columns>

                    <asp:BoundField
                        DataField="FullName"
                        HeaderText="Name" />

                    <asp:BoundField
                        DataField="Email"
                        HeaderText="Email" />

                    <asp:BoundField
                        DataField="Phone"
                        HeaderText="Phone" />

                    <asp:TemplateField
                        HeaderText="Role">

                        <ItemTemplate>

                            <span class="badge bg-secondary">

                                <%# Eval("Role") %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:BoundField
                        DataField="DealerName"
                        HeaderText="Dealer" />

                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge bg-success" : "badge bg-danger" %>'>

                                <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:BoundField
                        DataField="CreatedAtText"
                        HeaderText="Created" />

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-people fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No users match your search.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>
