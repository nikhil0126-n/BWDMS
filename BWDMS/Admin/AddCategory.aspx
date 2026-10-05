<%@ Page Title="Add Category"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddCategory.aspx.cs"
    Inherits="BWDMS.Admin.AddCategory" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    <asp:Label
        ID="lblPageTitle"
        runat="server"
        Text="Add Category">
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
                    Text="Add Category">
                </asp:Label>

            </h3>


            <p class="text-muted mb-0">

                <asp:Label
                    ID="lblSubHeading"
                    runat="server"
                    Text="Create a product category for the catalogue">
                </asp:Label>

            </p>

        </div>


        <a href="Categories.aspx"
           class="btn btn-outline-secondary">

            <i class="bi bi-arrow-left"></i>

            Back to Categories

        </a>

    </div>


    <div class="dashboard-card">


        <div class="mb-4">

            <h5 class="fw-bold mb-1">
                Category Information
            </h5>

            <small class="text-muted">
                Fields marked <span class="text-danger">*</span> are required.
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

                    Category Name

                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtCategoryName"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="300"
                    placeholder="Example: Wafers">
                </asp:TextBox>


                <asp:RequiredFieldValidator
                    ID="rfvCategoryName"
                    runat="server"
                    ControlToValidate="txtCategoryName"
                    ErrorMessage="Category name is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-md-6">

                <label class="form-label fw-semibold">
                    Sort Order
                </label>


                <asp:TextBox
                    ID="txtSortOrder"
                    runat="server"
                    CssClass="form-control"
                    TextMode="Number"
                    Text="0">
                </asp:TextBox>


                <small class="text-muted">
                    Lower numbers appear first.
                </small>

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
                    placeholder="What this category covers">
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


        </div>


        <hr class="my-4" />


        <div class="d-flex justify-content-end gap-2">

            <a href="Categories.aspx"
               class="btn btn-light border">

                <i class="bi bi-x-lg"></i>

                Cancel

            </a>


            <asp:Button
                ID="btnSave"
                runat="server"
                Text="Create Category"
                CssClass="btn btn-danger px-4"
                OnClick="btnSave_Click" />

        </div>


    </div>

</asp:Content>
