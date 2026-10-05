<%@ Page Title="Collection"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Collection.aspx.cs"
    Inherits="BWDMS.Salesman.Collection" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Collection

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Collection
            </h3>

            <p class="text-muted mb-0">
                Shops on your routes and their balances
            </p>

        </div>


        <div class="d-flex gap-2">

            <a href="Shops.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-shop"></i>

                All My Shops

            </a>


            <a href="Dashboard.aspx"
               class="btn btn-outline-secondary">

                <i class="bi bi-arrow-left"></i>

                Dashboard

            </a>

        </div>

    </div>


    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <div class="row g-3 mb-4">

        <div class="col-md-4">

            <div class="dashboard-card text-center">

                <div class="text-muted small">
                    Shops
                </div>


                <div class="fw-bold fs-4">

                    <asp:Label
                        ID="lblShopCount"
                        runat="server"
                        Text="0">
                    </asp:Label>

                </div>

            </div>

        </div>


        <div class="col-md-4">

            <div class="dashboard-card text-center">

                <div class="text-muted small">
                    Opening Balance Total
                </div>


                <div class="fw-bold fs-4 text-danger">

                    <asp:Label
                        ID="lblBalanceTotal"
                        runat="server"
                        Text="0.00">
                    </asp:Label>

                </div>

            </div>

        </div>


        <div class="col-md-4">

            <div class="dashboard-card text-center">

                <div class="text-muted small">
                    Credit Limit Total
                </div>


                <div class="fw-bold fs-4">

                    <asp:Label
                        ID="lblCreditTotal"
                        runat="server"
                        Text="0.00">
                    </asp:Label>

                </div>

            </div>

        </div>

    </div>


    <div class="dashboard-card">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>

                <h5 class="fw-bold mb-1">
                    Shops to Collect From
                </h5>

                <small class="text-muted">
                    Sorted by village visit order where available
                </small>

            </div>


            <div class="grid-search-box">

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Search shop, code or owner..."
                    AutoPostBack="true"
                    OnTextChanged="FilterChanged">
                </asp:TextBox>

            </div>

        </div>


        <div class="row g-2 mb-3">

            <div class="col-md-4">

                <asp:DropDownList
                    ID="ddlVillage"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">
                </asp:DropDownList>

            </div>

        </div>


        <div class="table-responsive">

            <asp:GridView
                ID="gvShops"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

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
                        DataField="VisitText"
                        HeaderText="Visit" />

                    <asp:BoundField
                        DataField="OpeningBalanceText"
                        HeaderText="Opening Balance" />

                    <asp:BoundField
                        DataField="CreditLimitText"
                        HeaderText="Credit Limit" />

                </Columns>


                <EmptyDataTemplate>

                    <div class="text-center py-5">

                        <i class="bi bi-wallet2 fs-1 text-muted"></i>

                        <p class="text-muted mt-3 mb-0">
                            No shops match your filters.
                        </p>

                    </div>

                </EmptyDataTemplate>

            </asp:GridView>

        </div>

    </div>


</asp:Content>
