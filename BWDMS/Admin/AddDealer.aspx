<%@ Page Title="Add Dealer"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddDealer.aspx.cs"
    Inherits="BWDMS.Admin.AddDealer" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Add Dealer

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <div class="mb-4">

        <h3 class="fw-bold mb-1">
            Add Dealer
        </h3>

        <p class="text-muted">
            Create a new dealer account
        </p>

    </div>


    <div class="dashboard-card"
         style="max-width: 850px;">


        <!-- Message -->

        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <div class="row g-4">


            <!-- Full Name -->

            <div class="col-md-6">

                <label class="form-label">
                    Dealer Name
                </label>

                <asp:TextBox
                    ID="txtFullName"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Enter dealer name">
                </asp:TextBox>

                <asp:RequiredFieldValidator
                    ID="rfvFullName"
                    runat="server"
                    ControlToValidate="txtFullName"
                    ErrorMessage="Dealer name is required."
                    CssClass="text-danger"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <!-- Email -->

            <div class="col-md-6">

                <label class="form-label">
                    Email Address
                </label>

                <asp:TextBox
                    ID="txtEmail"
                    runat="server"
                    CssClass="form-control"
                    TextMode="Email"
                    placeholder="dealer@example.com">
                </asp:TextBox>

                <asp:RequiredFieldValidator
                    ID="rfvEmail"
                    runat="server"
                    ControlToValidate="txtEmail"
                    ErrorMessage="Email is required."
                    CssClass="text-danger"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <!-- Phone -->

            <div class="col-md-6">

                <label class="form-label">
                    Phone Number
                </label>

                <asp:TextBox
                    ID="txtPhone"
                    runat="server"
                    CssClass="form-control"
                    placeholder="Enter phone number"
                    MaxLength="15">
                </asp:TextBox>

            </div>


            <!-- Password -->

            <div class="col-md-6">

                <label class="form-label">
                    Password
                </label>

                <asp:TextBox
                    ID="txtPassword"
                    runat="server"
                    CssClass="form-control"
                    TextMode="Password"
                    placeholder="Enter password">
                </asp:TextBox>

                <asp:RequiredFieldValidator
                    ID="rfvPassword"
                    runat="server"
                    ControlToValidate="txtPassword"
                    ErrorMessage="Password is required."
                    CssClass="text-danger"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <!-- Confirm Password -->

            <div class="col-md-6">

                <label class="form-label">
                    Confirm Password
                </label>

                <asp:TextBox
                    ID="txtConfirmPassword"
                    runat="server"
                    CssClass="form-control"
                    TextMode="Password"
                    placeholder="Confirm password">
                </asp:TextBox>


                <asp:CompareValidator
                    ID="cvPassword"
                    runat="server"
                    ControlToValidate="txtConfirmPassword"
                    ControlToCompare="txtPassword"
                    ErrorMessage="Passwords do not match."
                    CssClass="text-danger"
                    Display="Dynamic">
                </asp:CompareValidator>

            </div>


            <!-- Status -->

            <div class="col-md-6">

                <label class="form-label">
                    Account Status
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


        <!-- Buttons -->

        <div class="d-flex gap-2 mt-4">

            <asp:Button
                ID="btnCreateDealer"
                runat="server"
                Text="Create Dealer"
                CssClass="btn btn-danger"
                OnClick="btnCreateDealer_Click" />


            <a href="Dealers.aspx"
               class="btn btn-light border">

                Cancel

            </a>

        </div>

    </div>


</asp:Content>