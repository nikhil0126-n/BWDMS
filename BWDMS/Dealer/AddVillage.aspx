<%@ Page Title="Add Village"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddVillage.aspx.cs"
    Inherits="BWDMS.Dealer.AddVillage" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    <asp:Label
        ID="lblPageTitle"
        runat="server"
        Text="Add Village">
    </asp:Label>

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <div class="d-flex justify-content-between align-items-center mb-4">

        <div>

            <h3 class="fw-bold mb-1">

                <asp:Label
                    ID="lblHeading"
                    runat="server"
                    Text="Add Village">
                </asp:Label>

            </h3>


            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblSubHeading"
                    runat="server"
                    Text="Add a new village for route planning">
                </asp:Label>

            </p>

        </div>


        <a href="Villages.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left me-1"></i>

            Back to Villages

        </a>

    </div>



    <div class="dashboard-card">


        <div class="mb-4">

            <h5 class="fw-bold mb-1">

                <asp:Label
                    ID="lblFormTitle"
                    runat="server"
                    Text="Village Information">
                </asp:Label>

            </h5>

            <small class="text-muted">
                Enter the village details below.
            </small>

        </div>




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




        <div class="row g-4">



            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Village Name

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtVillageName"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="150"
                    placeholder="Example: Kothariya">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvVillageName"
                    runat="server"
                    ControlToValidate="txtVillageName"
                    ErrorMessage="Village name is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>




            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Taluka
                </label>


                <asp:TextBox
                    ID="txtTaluka"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="100"
                    placeholder="Example: Rajkot">
                </asp:TextBox>

            </div>



            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    District
                </label>


                <asp:TextBox
                    ID="txtDistrict"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="100"
                    placeholder="Example: Rajkot">
                </asp:TextBox>

            </div>



            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Pincode
                </label>


                <asp:TextBox
                    ID="txtPincode"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="10"
                    placeholder="Example: 360005">
                </asp:TextBox>

            </div>



            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Status
                </label>


                <asp:DropDownList
                    ID="ddlStatus"
                    runat="server"
                    CssClass="form-select">

                    <asp:ListItem
                        Text="Active"
                        Value="1"
                        Selected="True">
                    </asp:ListItem>

                    <asp:ListItem
                        Text="Inactive"
                        Value="0">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>


        </div>



        <hr class="my-4" />




        <div class="d-flex justify-content-end gap-2">


            <a href="Villages.aspx"
               class="btn btn-light border">

                <i class="bi bi-x-lg me-1"></i>

                Cancel

            </a>


            <asp:Button
                ID="btnSaveVillage"
                runat="server"
                Text="Create Village"
                CssClass="btn btn-danger px-4"
                OnClick="btnSaveVillage_Click" />

        </div>


    </div>

</asp:Content>