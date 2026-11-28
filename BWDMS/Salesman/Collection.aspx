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
                Record payments collected from shops on your routes
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


    <!-- SUMMARY CARDS -->
    <div class="row g-3 mb-4">

        <div class="col-md-4">

            <div class="dashboard-card text-center">

                <div class="text-muted small">
                    Shops on Route
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
                    Total Outstanding
                </div>


                <div class="fw-bold fs-4 text-danger">

                    <asp:Label
                        ID="lblTotalOutstanding"
                        runat="server"
                        Text="0.00">
                    </asp:Label>

                </div>

            </div>

        </div>


        <div class="col-md-4">

            <div class="dashboard-card text-center">

                <div class="text-muted small">
                    Today's Collections
                </div>


                <div class="fw-bold fs-4 text-success">

                    <asp:Label
                        ID="lblTodayCollections"
                        runat="server"
                        Text="0.00">
                    </asp:Label>

                </div>

            </div>

        </div>

    </div>


    <!-- FILTERS -->
    <div class="dashboard-card mb-4">

        <div class="d-flex justify-content-between align-items-center mb-3">

            <div>
                <h5 class="fw-bold mb-1">Shops to Collect From</h5>
                <small class="text-muted">Sorted by outstanding amount (highest first)</small>
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
            <div class="col-md-3">
                <asp:DropDownList
                    ID="ddlVillage"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">
                </asp:DropDownList>
            </div>
            <div class="col-md-3">
                <asp:DropDownList
                    ID="ddlFilter"
                    runat="server"
                    CssClass="form-select"
                    AutoPostBack="true"
                    OnSelectedIndexChanged="FilterChanged">
                    <asp:ListItem Text="All Shops" Value="all" />
                    <asp:ListItem Text="Has Outstanding" Value="outstanding" />
                    <asp:ListItem Text="Over Credit Limit" Value="overlimit" />
                    <asp:ListItem Text="Collected Today" Value="collectedtoday" />
                </asp:DropDownList>
            </div>
        </div>

    </div>


    <!-- SHOPS GRID WITH COLLECTION ENTRY -->
    <div class="dashboard-card">

        <div class="table-responsive">
            <asp:GridView
                ID="gvShops"
                runat="server"
                AutoGenerateColumns="False"
                CssClass="table table-hover align-middle"
                GridLines="None"
                AllowPaging="true"
                OnPageIndexChanging="gvShops_PageIndexChanging"
                OnRowCommand="gvShops_RowCommand"
                PageSize="25"
                PagerSettings-Mode="NumericFirstLast"
                PagerSettings-PageButtonCount="10"
                PagerSettings-Position="TopAndBottom"
                DataKeyNames="ShopId">

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

                    <asp:TemplateField HeaderText="Outstanding" ItemStyle-CssClass="text-end">
                        <ItemTemplate>
                            <span class='<%# Convert.ToDecimal(Eval("OutstandingBalance")) > 0 ? "text-danger fw-bold" : "text-success" %>'>
                                ₹<%#: Convert.ToDecimal(Eval("OutstandingBalance")).ToString("N2") %>
                            </span>
                            <br />
                            <small class="text-muted">
                                Limit: ₹<%#: Convert.ToDecimal(Eval("CreditLimit")).ToString("N2") %>
                            </small>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Today's Collection" ItemStyle-CssClass="text-end">
                        <ItemTemplate>
                            <span class="text-success fw-bold">
                                ₹<%#: Convert.ToDecimal(Eval("TodayCollection")).ToString("N2") %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Record Collection" ItemStyle-CssClass="text-center" ItemStyle-Width="200px">
                        <ItemTemplate>
                            <div class="input-group input-group-sm" style="max-width: 280px; margin: 0 auto;">
                                <asp:TextBox
                                    ID="txtCollectionAmount"
                                    runat="server"
                                    CssClass="form-control text-end"
                                    placeholder="Amount"
                                    Width="80px"
                                    ToolTip="Enter collection amount" />
                                <asp:DropDownList
                                    ID="ddlPaymentMode"
                                    runat="server"
                                    CssClass="form-select"
                                    Width="100px">
                                    <asp:ListItem Text="Cash" Value="Cash" />
                                    <asp:ListItem Text="UPI" Value="UPI" />
                                    <asp:ListItem Text="Card" Value="Card" />
                                    <asp:ListItem Text="Cheque" Value="Cheque" />
                                    <asp:ListItem Text="Bank Transfer" Value="Bank Transfer" />
                                </asp:DropDownList>
                                <asp:TextBox
                                    ID="txtReference"
                                    runat="server"
                                    CssClass="form-control"
                                    placeholder="Ref #"
                                    Width="90px"
                                    ToolTip="Reference number (cheque no, UPI ref, etc.)" />
                                <asp:Button
                                    ID="btnRecord"
                                    runat="server"
                                    CssClass="btn btn-success"
                                    Text="Save"
                                    CommandName="RecordCollection"
                                    CommandArgument='<%# Eval("ShopId") %>'
                                    OnClientClick="return confirm('Record this collection?');"
                                    UseSubmitBehavior="false" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="History" ItemStyle-CssClass="text-center" ItemStyle-Width="80px">
                        <ItemTemplate>
                            <asp:HyperLink
                                ID="lnkHistory"
                                runat="server"
                                CssClass="btn btn-sm btn-outline-primary"
                                NavigateUrl='<%# "~/Salesman/CollectionHistory.aspx?shop=" + Eval("ShopId") %>'
                                ToolTip="View collection history"
                                Text='<i class="bi bi-clock-history"></i>' />
                        </ItemTemplate>
                    </asp:TemplateField>

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


    <!-- COLLECTION HISTORY MODAL (for quick view) -->
    <div class="modal fade" id="historyModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Collection History - <span id="modalShopName"></span></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="table-responsive">
                        <table class="table table-sm table-hover">
                            <thead>
                                <tr>
                                    <th>Date</th>
                                    <th>Time</th>
                                    <th>Amount</th>
                                    <th>Mode</th>
                                    <th>Reference</th>
                                    <th>Notes</th>
                                </tr>
                            </thead>
                            <tbody id="historyTableBody"></tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>


</asp:Content>


<asp:Content ID="Content3"
    ContentPlaceHolderID="HeadContent"
    runat="server">

    <style>
        .input-group-sm .form-control,
        .input-group-sm .form-select {
            font-size: 0.875rem;
            padding: 0.25rem 0.5rem;
        }

        .input-group-sm .btn {
            padding: 0.25rem 0.5rem;
            font-size: 0.875rem;
        }

        .grid-search-box {
            max-width: 300px;
        }

        .grid-search-box .form-control {
            font-size: 0.875rem;
        }

        @media (max-width: 768px) {
            .input-group-sm {
                flex-wrap: wrap;
                gap: 0.25rem;
            }
            .input-group-sm .form-control,
            .input-group-sm .form-select {
                min-width: 70px;
            }
        }
    </style>

    <script>
        // Optional: Load collection history via AJAX when history link clicked
        document.addEventListener('DOMContentLoaded', function () {
            document.querySelectorAll('#gvShops a[id$="lnkHistory"]').forEach(function (link) {
                link.addEventListener('click', function (e) {
                    e.preventDefault();
                    var shopId = this.href.split('shop=')[1];
                    var shopName = this.closest('tr').querySelector('td:nth-child(2)').innerText;
                    document.getElementById('modalShopName').textContent = shopName;
                    loadHistory(shopId);
                    var modal = new bootstrap.Modal(document.getElementById('historyModal'));
                    modal.show();
                });
            });
        });

        function loadHistory(shopId) {
            fetch('/Salesman/CollectionHistoryHandler.ashx?shop=' + shopId)
                .then(response => response.json())
                .then(data => {
                    var tbody = document.getElementById('historyTableBody');
                    tbody.innerHTML = '';
                    data.forEach(function (item) {
                        var row = document.createElement('tr');
                        row.innerHTML = '<td>' + item.date + '</td>' +
                            '<td>' + item.time + '</td>' +
                            '<td class="text-success fw-bold">₹' + item.amount.toFixed(2) + '</td>' +
                            '<td>' + item.mode + '</td>' +
                            '<td>' + (item.ref || '-') + '</td>' +
                            '<td>' + (item.notes || '-') + '</td>';
                        tbody.appendChild(row);
                    });
                });
        }
    </script>

</asp:Content>