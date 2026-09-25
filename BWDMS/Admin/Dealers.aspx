<%@ Page Title="Dealers"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Dealers.aspx.cs"
    Inherits="BWDMS.Admin.Dealers" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Dealers

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <!-- Page Header -->

    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Dealers
            </h3>

            <p class="text-muted mb-0">
                Manage BWDMS dealers and dealer accounts
            </p>

        </div>


        <div>

            <a href="AddDealer.aspx"
               class="btn btn-danger">

                <i class="bi bi-plus-lg"></i>

                Add Dealer

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


    <!-- Dealer List -->

    <div class="dashboard-card">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    Dealer List
                </h5>

                <small class="text-muted">
                    All registered dealers
                </small>

            </div>


            <div style="width:250px;">

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Search dealer..."
                    AutoPostBack="true"
                    OnTextChanged="txtSearch_TextChanged">
                </asp:TextBox>

            </div>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvDealers"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                DataKeyNames="UserId"
                OnRowCommand="gvDealers_RowCommand">


                <Columns>


                    <asp:BoundField
                        DataField="UserId"
                        HeaderText="ID" />


                    <asp:BoundField
                        DataField="FullName"
                        HeaderText="Dealer Name" />


                    <asp:BoundField
                        DataField="Email"
                        HeaderText="Email" />


                    <asp:BoundField
                        DataField="Phone"
                        HeaderText="Phone" />


                    <asp:TemplateField
                        HeaderText="Status">

                        <ItemTemplate>

                            <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge bg-success" : "badge bg-secondary" %>'>

                                <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>

                            </span>

                        </ItemTemplate>

                    </asp:TemplateField>


                    <asp:TemplateField
                        HeaderText="Action">

                        <ItemTemplate>

                            <asp:LinkButton
                                ID="btnToggle"
                                runat="server"
                                CommandName="ToggleStatus"
                                CommandArgument='<%# Eval("UserId") %>'
                                CssClass="btn btn-sm btn-outline-danger">

                                <i class="bi bi-power"></i>

                                <%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>

                            </asp:LinkButton>

                        </ItemTemplate>

                    </asp:TemplateField>


                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-shop fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No dealers found.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>