<%@ Page Title="Add Product"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddProduct.aspx.cs"
    Inherits="BWDMS.Admin.AddProduct" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    <asp:Label
        ID="lblPageTitle"
        runat="server"
        Text="Add Product">
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
                    Text="Add Product">
                </asp:Label>

            </h3>


            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblSubHeading"
                    runat="server"
                    Text="Add a product to the Balaji Wafers catalogue">
                </asp:Label>

            </p>

        </div>


        <a href="Products.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Products

        </a>

    </div>


    <div class="dashboard-card">


        <div class="mb-4">

            <h5 class="fw-bold mb-1">
                Product Information
            </h5>

            <small class="text-muted">
                Variants, units and prices are configured on the next screen.
            </small>

        </div>


        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <div class="row g-4">


            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Product Name

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtProductName"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="500"
                    placeholder="Example: Balaji Wafers Potato">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvProductName"
                    runat="server"
                    ControlToValidate="txtProductName"
                    ErrorMessage="Product name is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-6">

                <label class="form-label fw-semibold">

                    Category

                    <span class="text-danger">*</span>

                </label>


                <asp:DropDownList
                    ID="ddlCategory"
                    runat="server"
                    CssClass="form-select">
                </asp:DropDownList>


                <asp:RequiredFieldValidator
                    ID="rfvCategory"
                    runat="server"
                    ControlToValidate="ddlCategory"
                    InitialValue=""
                    ErrorMessage="Select a category."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Product Code
                </label>


                <asp:TextBox
                    ID="txtProductCode"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="200"
                    placeholder="Optional internal code">
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

                    <asp:ListItem Text="Active" Value="1" Selected="True">
                    </asp:ListItem>

                    <asp:ListItem Text="Inactive" Value="0">
                    </asp:ListItem>

                </asp:DropDownList>

            </div>


            <div class="col-12">

                <label class="form-label fw-semibold">
                    Description
                </label>


                <asp:TextBox
                    ID="txtDescription"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="1000"
                    TextMode="MultiLine"
                    Rows="3"
                    placeholder="Optional notes">
                </asp:TextBox>

            </div>


        </div>


        <hr class="my-4" />


        <div class="d-flex justify-content-end gap-2">

            <a href="Products.aspx"
               class="btn btn-light border">

                <i class="bi bi-x-lg"></i>

                Cancel

            </a>


            <asp:Button
                ID="btnSave"
                runat="server"
                Text="Create Product"
                CssClass="btn btn-danger px-4"
                OnClick="btnSave_Click" />

        </div>


    </div>

</asp:Content>
