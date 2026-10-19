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
                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtProductCode"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="200"
                    placeholder="Internal product code">
                </asp:TextBox>

                <asp:RequiredFieldValidator
                    ID="rfvProductCode"
                    runat="server"
                    ControlToValidate="txtProductCode"
                    ErrorMessage="Product code is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

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
                    <span class="text-danger">*</span>

                </label>


                <asp:TextBox
                    ID="txtDescription"
                    runat="server"
                    CssClass="form-control"
                    MaxLength="1000"
                    TextMode="MultiLine"
                    Rows="3"
                    placeholder="Product description">
                </asp:TextBox>

                <asp:RequiredFieldValidator
                    ID="rfvDescription"
                    runat="server"
                    ControlToValidate="txtDescription"
                    ErrorMessage="Description is required."
                    CssClass="text-danger small"
                    Display="Dynamic">
                </asp:RequiredFieldValidator>

            </div>


            <div class="col-12">

                <label class="form-label fw-semibold">
                    Product Image
                </label>


                <div id="imageDrop"
                     class="image-drop<%= HasImage ? " has-image" : "" %>">

                    <div id="imageDropEmpty"
                         class="image-drop-inner"
                         style="<%= HasImage ? "display:none;" : "" %>">

                        <i class="bi bi-cloud-arrow-up image-drop-icon"></i>

                        <div class="fw-semibold">
                            Drag &amp; drop image here
                        </div>

                        <div class="text-muted small">
                            or
                            <span class="text-primary fw-semibold">
                                Browse
                            </span>
                            &middot; PNG, JPG or WEBP up to 2 MB
                        </div>

                    </div>


                    <img id="imagePreview"
                         class="image-drop-preview"
                         alt="Product image preview"
                         src="<%= PreviewUrl %>"
                         style="<%= HasImage ? "" : "display:none;" %>" />


                    <button type="button"
                            id="imageRemove"
                            class="btn btn-sm btn-light border image-drop-remove"
                            title="Remove image"
                            style="<%= HasImage ? "" : "display:none;" %>">

                        <i class="bi bi-trash"></i>

                        Remove

                    </button>

                </div>


                <asp:FileUpload
                    ID="fuProductImage"
                    runat="server"
                    CssClass="d-none"
                    accept="image/png,image/jpeg,image/webp" />


                <asp:HiddenField
                    ID="hfRemoveImage"
                    runat="server"
                    Value="0" />

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
                CssClass="btn btn-primary px-4"
                OnClick="btnSave_Click" />

        </div>


    </div>


    <%-- ==========================================================
         BULK IMPORT FROM AN INVENTORY PDF - DISABLED

         Hidden on request: the import card no longer shows on the
         Add Product screen. The panel below and its code-behind
         are kept, wrapped in a server comment, so the feature can
         be brought back by deleting the two wrapper comments.
         ========================================================== --%>

    <%-- DISABLED - import from inventory PDF. Delete this wrapper
         (and the closing one after the panel) to enable it again.

    <asp:Panel
        ID="pnlImport"
        runat="server"
        CssClass="dashboard-card mt-4">


        <div class="mb-4">

            <h5 class="fw-bold mb-1">

                <i class="bi bi-file-earmark-pdf"></i>

                Import from inventory PDF

            </h5>

            <small class="text-muted">
                Upload the warehouse sheet. Each ticked row becomes one product
                with its variant, packing (Packet, Patti, Box), prices and
                godown stock.
            </small>

        </div>


        <asp:Label
            ID="lblImportMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block">
        </asp:Label>


        <div class="row g-3 align-items-end">


            <div class="col-md-4">

                <label class="form-label fw-semibold">

                    Inventory sheet (PDF)

                    <span class="text-danger">*</span>

                </label>


                <asp:FileUpload
                    ID="fuInventoryPdf"
                    runat="server"
                    CssClass="form-control">
                </asp:FileUpload>

            </div>


            <div class="col-md-4">

                <label class="form-label fw-semibold">

                    Godown to stock

                    <span class="text-danger">*</span>

                </label>


                <asp:DropDownList
                    ID="ddlGodown"
                    runat="server"
                    CssClass="form-select">
                </asp:DropDownList>

            </div>


            <div class="col-md-4">

                <label class="form-label fw-semibold">

                    Category used when it cannot be detected

                </label>


                <asp:DropDownList
                    ID="ddlFallbackCategory"
                    runat="server"
                    CssClass="form-select">
                </asp:DropDownList>

            </div>

        </div>


        <div class="d-flex gap-2 mt-3">

            <asp:Button
                ID="btnReadSheet"
                runat="server"
                Text="Read sheet"
                CssClass="btn btn-outline-danger"
                OnClick="btnReadSheet_Click">
            </asp:Button>


            <asp:Button
                ID="btnImportRows"
                runat="server"
                Text="Import products"
                CssClass="btn btn-primary"
                Visible="false"
                OnClick="btnImportRows_Click">
            </asp:Button>

        </div>


        <asp:Panel
            ID="pnlPreview"
            runat="server"
            Visible="false"
            CssClass="mt-4">

            <div class="table-responsive"
                 style="max-height: 440px; overflow-y: auto;">

                <asp:GridView
                    ID="gvPreview"
                    runat="server"
                    AutoGenerateColumns="false"
                    GridLines="None"
                    CssClass="table table-sm table-bordered align-middle mb-0"
                    HeaderStyle-CssClass="table-light">

                    <Columns>

                        <asp:TemplateField
                            ItemStyle-Width="34"
                            ItemStyle-HorizontalAlign="Center"
                            HeaderStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:CheckBox
                                    ID="chkInclude"
                                    runat="server"
                                    Checked='<%# Eval("Include") %>'>
                                </asp:CheckBox>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField
                            DataField="Sr"
                            HeaderText="Sr." />

                        <asp:BoundField
                            DataField="Material"
                            HeaderText="Material" />

                        <asp:BoundField
                            DataField="BoxQty"
                            HeaderText="Box" />

                        <asp:BoundField
                            DataField="PattiQty"
                            HeaderText="Patti" />

                        <asp:BoundField
                            DataField="PacketQty"
                            HeaderText="Packet" />

                        <asp:BoundField
                            DataField="TotalPacket"
                            HeaderText="Total Packet" />

                        <asp:BoundField
                            DataField="PurchasePrice"
                            HeaderText="Purchase Price"
                            DataFormatString="{0:0.00}"
                            ItemStyle-HorizontalAlign="Right" />

                        <asp:BoundField
                            DataField="SalesPrice"
                            HeaderText="Sales Price"
                            DataFormatString="{0:0.00}"
                            ItemStyle-HorizontalAlign="Right" />

                    </Columns>

                </asp:GridView>

            </div>


            <p class="text-muted small mt-3 mb-0">

                <asp:Label
                    ID="litSummary"
                    runat="server">
                </asp:Label>

            </p>

        </asp:Panel>


    </asp:Panel>

    --%>


    <script type="text/javascript">

        // ============================================================
        // PRODUCT IMAGE DROP ZONE
        //
        // The real file input is hidden, so dragging or clicking the
        // tile fills it in. A dropped file is copied into the input
        // through a DataTransfer, which is the only way a script is
        // allowed to set files on a file input.
        // ============================================================

        (function () {
            var drop = document.getElementById("imageDrop");
            var input = document.getElementById("<%= fuProductImage.ClientID %>");
            var empty = document.getElementById("imageDropEmpty");
            var preview = document.getElementById("imagePreview");
            var remove = document.getElementById("imageRemove");
            var flag = document.getElementById("<%= hfRemoveImage.ClientID %>");

            if (!drop || !input || !flag) {
                return;
            }

            function showPreview(source) {
                if (source instanceof File) {
                    var reader = new FileReader();

                    reader.onload = function (loaded) {
                        preview.src = loaded.target.result;
                        paint();
                    };

                    reader.readAsDataURL(source);
                } else {
                    preview.src = source;
                    paint();
                }
            }

            function paint() {
                preview.style.display = "block";
                empty.style.display = "none";
                remove.style.display = "inline-block";
                drop.className = "image-drop has-image";
            }

            function wipe() {
                input.value = "";
                preview.removeAttribute("src");
                preview.style.display = "none";
                empty.style.display = "block";
                remove.style.display = "none";
                drop.className = "image-drop";
                flag.value = "1";
            }

            drop.addEventListener("click", function (event) {
                if (remove && (event.target === remove ||
                    remove.contains(event.target))) {
                    return;
                }

                input.click();
            });

            input.addEventListener("change", function () {
                if (input.files && input.files.length > 0) {
                    flag.value = "0";
                    showPreview(input.files[0]);
                }
            });

            remove.addEventListener("click", function (event) {
                event.preventDefault();
                event.stopPropagation();
                wipe();
            });

            ["dragenter", "dragover"].forEach(function (name) {
                drop.addEventListener(name, function (event) {
                    event.preventDefault();
                    event.stopPropagation();
                    drop.classList.add("dragging");
                });
            });

            ["dragleave", "dragend"].forEach(function (name) {
                drop.addEventListener(name, function (event) {
                    event.preventDefault();
                    event.stopPropagation();
                    drop.classList.remove("dragging");
                });
            });

            drop.addEventListener("drop", function (event) {
                event.preventDefault();
                event.stopPropagation();
                drop.classList.remove("dragging");

                var files = event.dataTransfer && event.dataTransfer.files;

                if (!files || files.length === 0) {
                    return;
                }

                var file = files[0];

                if (!/^image\//.test(file.type)) {
                    window.alert("Please drop a PNG, JPG or WEBP image.");
                    return;
                }

                try {
                    var carrier = new DataTransfer();

                    carrier.items.add(file);
                    input.files = carrier.files;
                } catch (error) {
                    window.alert("Please use Browse to pick the image.");
                    return;
                }

                flag.value = "0";
                showPreview(file);
            });
        })();

    </script>

</asp:Content>
