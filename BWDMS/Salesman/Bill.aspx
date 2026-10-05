<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="Bill.aspx.cs"
    Inherits="BWDMS.Salesman.Bill" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />

    <meta name="viewport"
          content="width=device-width, initial-scale=1" />

    <title>Bill | BWDMS</title>

    <!-- Bootstrap 5 CSS -->
    <link
        href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"
        rel="stylesheet" />

    <!-- Bootstrap Icons -->
    <link
        rel="stylesheet"
        href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" />

    <style type="text/css">
        /* Only print rules live here; the layout itself uses the same
             Bootstrap classes as every other page. */
        @media print
        {
            .no-print { display: none !important; }

            body { background: #fff !important; }

            .bill-paper {
                box-shadow: none !important;
                border: none !important;
            }
        }
    </style>

</head>

<body>

<form id="form1" runat="server">

    <div class="container py-4">

        <!-- Back / print controls (never printed) -->

        <div class="no-print d-flex justify-content-between align-items-center mb-3">

            <a href="Orders.aspx"
               class="btn btn-light border">

                <i class="bi bi-arrow-left"></i>

                Back to My Orders

            </a>

            <asp:Button
                ID="btnPrint"
                runat="server"
                Text="Print Bill"
                CssClass="btn btn-primary"
                OnClick="btnPrint_Click"
                CausesValidation="false" />

        </div>

        <asp:Label
            ID="lblMessage"
            runat="server"
            Visible="false"
            CssClass="alert d-block no-print">
        </asp:Label>

        <!-- The bill itself -->

        <div class="bill-paper bg-white border rounded p-4">

            <!-- Dealer header -->

            <div class="row align-items-start mb-3">

                <div class="col-md-6">

                    <h4 class="fw-bold mb-1">
                        Balaji Wafers
                    </h4>

                    <asp:Label
                        ID="lblDealer"
                        runat="server"
                        CssClass="text-muted">
                    </asp:Label>

                </div>

                <div class="col-md-6 text-md-end">

                    <h5 class="fw-bold mb-1">

                        <asp:Literal
                            ID="litBillTitle"
                            runat="server" />

                    </h5>

                    <div class="text-muted">
                        Bill No:
                        <strong>
                            <asp:Literal
                                ID="litBillNumber"
                                runat="server" />
                        </strong>
                    </div>

                    <div class="text-muted">
                        Date:
                        <strong>
                            <asp:Literal
                                ID="litBillDate"
                                runat="server" />
                        </strong>
                    </div>

                </div>

            </div>

            <hr />

            <!-- Shop + route + salesman -->

            <div class="row mb-3">

                <div class="col-md-6">

                    <div class="text-muted small">
                        Shop
                    </div>

                    <div class="fw-semibold">
                        <asp:Literal
                            ID="litShop"
                            runat="server" />
                    </div>

                    <asp:Label
                        ID="lblShopAddress"
                        runat="server"
                        CssClass="text-muted small" />

                </div>

                <div class="col-md-6 text-md-end">

                    <div class="text-muted small">
                        Route / Salesman
                    </div>

                    <div class="fw-semibold">
                        <asp:Literal
                            ID="litRoute"
                            runat="server" />
                    </div>

                    <div class="text-muted small">
                        <asp:Literal
                            ID="litSalesman"
                            runat="server" />
                    </div>

                </div>

            </div>

            <!-- Lines -->

            <div class="table-responsive">

                <asp:GridView
                    ID="gvLines"
                    runat="server"
                    AutoGenerateColumns="False"
                    CssClass="table table-sm table-bordered align-middle mb-0"
                    GridLines="None" AllowPaging="true" PageSize="25" PagerSettings-Mode="NumericFirstLast" PagerSettings-PageButtonCount="10" PagerSettings-Position="TopAndBottom">

                    <Columns>

                        <asp:BoundField
                            DataField="LineNo"
                            HeaderText="#"
                            ItemStyle-CssClass="text-end" />

                        <asp:BoundField
                            DataField="ProductName"
                            HeaderText="Product" />

                        <asp:BoundField
                            DataField="VariantName"
                            HeaderText="Variant" />

                        <asp:BoundField
                            DataField="UnitCode"
                            HeaderText="Unit" />

                        <asp:BoundField
                            DataField="Quantity"
                            HeaderText="Qty"
                            ItemStyle-CssClass="text-end" />

                        <asp:BoundField
                            DataField="UnitPriceText"
                            HeaderText="Rate"
                            ItemStyle-CssClass="text-end" />

                        <asp:BoundField
                            DataField="LineTotalText"
                            HeaderText="Amount"
                            ItemStyle-CssClass="text-end" />

                    </Columns>

                </asp:GridView>

            </div>

            <!-- Totals -->

            <div class="row mt-3">

                <div class="col-md-5 offset-md-7">

                    <table class="table table-sm mb-0">

                        <tr>
                            <td>Sub Total</td>
                            <td class="text-end">
                                <asp:Literal
                                    ID="litSubTotal"
                                    runat="server" />
                            </td>
                        </tr>

                        <tr>
                            <td>Discount</td>
                            <td class="text-end">
                                <asp:Literal
                                    ID="litDiscount"
                                    runat="server" />
                            </td>
                        </tr>

                        <tr class="fw-bold">
                            <td>Grand Total</td>
                            <td class="text-end">
                                <asp:Literal
                                    ID="litGrandTotal"
                                    runat="server" />
                            </td>
                        </tr>

                    </table>

                </div>

            </div>

            <hr />

            <div class="d-flex justify-content-between">

                <div class="text-muted small">
                    This bill was generated from stock carried on the
                    vehicle for this route.
                </div>

                <div class="text-muted small">
                    <asp:Literal
                        ID="litPrintedAt"
                        runat="server" />
                </div>

            </div>

        </div>

    </div>

</form>

</body>

</html>