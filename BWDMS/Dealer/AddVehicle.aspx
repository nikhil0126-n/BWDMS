<%@ Page Title="Add Vehicle"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddVehicle.aspx.cs"
    Inherits="BWDMS.Dealer.AddVehicle" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    <asp:Label
        ID="lblPageTitle"
        runat="server"
        Text="Add Vehicle">
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
                    Text="Add Vehicle">
                </asp:Label>

            </h3>


            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblSubHeading"
                    runat="server"
                    Text="Register a truck or tempo for route dispatch">
                </asp:Label>

            </p>

        </div>


        <a href="Vehicles.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left me-1"></i>

            Back to Vehicles

        </a>

    </div>



    <div class="dashboard-card">


        <div class="mb-4">

            <h5 class="fw-bold mb-1">

                <asp:Label
                    ID="lblFormTitle"
                    runat="server"
                    Text="Vehicle Information">
                </asp:Label>

            </h5>

            <small class="text-muted">
                Enter the vehicle details below.
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

                    Vehicle Number

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtVehicleNumber"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="100"
                    placeholder="Example: GJ-03-AB-1234">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvVehicleNumber"
                    runat="server"
                    ControlToValidate="txtVehicleNumber"
                    ErrorMessage="Vehicle number is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>




            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Vehicle Name

                </label>


                <asp:TextBox
                    ID="txtVehicleName"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="200"
                    placeholder="Example: Tata 407">
                </asp:TextBox>

            </div>




            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Vehicle Type

                </label>


                <asp:TextBox
                    ID="txtVehicleType"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="100"
                    placeholder="Example: Truck / Tempo / Auto">
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


            <a href="Vehicles.aspx"
               class="btn btn-light border">

                <i class="bi bi-x-lg me-1"></i>

                Cancel

            </a>


            <asp:Button
                ID="btnSaveVehicle"
                runat="server"
                Text="Create Vehicle"
                CssClass="btn btn-danger px-4"
                OnClick="btnSaveVehicle_Click" />

        </div>


    </div>

</asp:Content>
