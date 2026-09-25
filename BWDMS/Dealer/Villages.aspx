<%@ Page Title="Villages"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="Villages.aspx.cs"
    Inherits="BWDMS.Dealer.Villages" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Villages

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">



    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Villages
            </h3>

            <p class="text-muted mb-0">
                Manage villages used in your route planning
            </p>

        </div>


        <a href="AddVillage.aspx"
           class="btn btn-danger">

            <i class="bi bi-plus-lg me-1"></i>

            Add Village

        </a>

    </div>




    <div class="dashboard-card">



        <div class="d-flex justify-content-between align-items-center mb-3">


            <div>

                <h5 class="fw-bold mb-1">
                    Village List
                </h5>

                <small class="text-muted">
                    All villages available for route planning
                </small>

            </div>




            <div style="width:280px;">

                <asp:TextBox
                    ID="txtSearch"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Search village..."
                    ClientIDMode="Static">
                </asp:TextBox>

            </div>


        </div>




        <div class="table-responsive">


            <asp:GridView
                ID="gvVillages"
                runat="server"
                ClientIDMode="Static"
                AutoGenerateColumns="False"
                CssClass="table align-middle mb-0"
                GridLines="None">


                <Columns>



                    <asp:BoundField
                        DataField="VillageName"
                        HeaderText="Village">

                        <HeaderStyle
                            CssClass="fw-semibold" />

                    </asp:BoundField>




                    <asp:BoundField
                        DataField="Taluka"
                        HeaderText="Taluka">

                        <HeaderStyle
                            CssClass="fw-semibold" />

                    </asp:BoundField>




                    <asp:BoundField
                        DataField="District"
                        HeaderText="District">

                        <HeaderStyle
                            CssClass="fw-semibold" />

                    </asp:BoundField>



                    <asp:BoundField
                        DataField="Pincode"
                        HeaderText="Pincode">

                        <HeaderStyle
                            CssClass="fw-semibold" />

                    </asp:BoundField>




                    <asp:TemplateField
                        HeaderText="Status">


                        <HeaderStyle
                            CssClass="fw-semibold" />


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


                        <HeaderStyle
                            CssClass="fw-semibold" />


                        <ItemTemplate>


                            <asp:HyperLink
                                ID="lnkEdit"
                                runat="server"
                                CssClass="btn btn-outline-danger btn-sm"
                                NavigateUrl='<%# "~/Dealer/AddVillage.aspx?id=" + Eval("VillageId") %>'>


                                <i class="bi bi-pencil me-1"></i>

                                Edit


                            </asp:HyperLink>


                        </ItemTemplate>


                    </asp:TemplateField>


                </Columns>


            </asp:GridView>


        </div>


    </div>




    <script type="text/javascript">

        document.addEventListener("DOMContentLoaded", function () {

            const searchBox =
                document.getElementById("txtSearch");

            const villageTable =
                document.getElementById("gvVillages");


            if (!searchBox || !villageTable) {
                return;
            }


            searchBox.addEventListener("input", function () {

                const searchValue =
                    searchBox.value
                        .trim()
                        .toLowerCase();


                const rows =
                    villageTable.querySelectorAll("tbody tr");


                rows.forEach(function (row) {

                    const rowText =
                        row.textContent.toLowerCase();


                    if (rowText.includes(searchValue)) {

                        row.style.display = "";

                    }
                    else {

                        row.style.display = "none";

                    }

                });

            });

        });

    </script>


</asp:Content>