<%@ Page Title="Daily Closing"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="DailyClosing.aspx.cs"
    Inherits="BWDMS.Dealer.DailyClosing" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Daily Closing

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <style>
        /* Page shell matching the Closing History screen design */
        .ch-title { font-size: 1.9rem; font-weight: 700; color: #111; margin: 0 0 4px; }
        .ch-subtitle { font-size: 12px; color: #9ca3af; margin: 0 0 22px; }

        .ch-range-wrap { display: flex; justify-content: flex-end; margin-bottom: 22px; }
        .ch-range {
            display: inline-flex; align-items: center; gap: 10px;
            border: 1px solid #d5d5da; border-radius: 8px;
            padding: 11px 18px; font-size: 15px; color: #111; background: #fff;
        }

        .ch-card { background: #fff; border: 1px solid #d5d5da; border-radius: 10px; }
        .ch-card table { margin-bottom: 0; }
        .ch-card thead th {
            background: #e6e6e6; color: #222; font-size: 14px; font-weight: 600;
            padding: 15px 22px; border-bottom: none;
        }
        .ch-card tbody td {
            padding: 17px 22px; font-size: 14px; color: #333;
            border-top: 1px solid #f0f0f0; vertical-align: middle;
        }
        .ch-card tbody tr:first-child td { border-top: none; }

        .ch-pager td { text-align: center; padding: 12px 0; border-top: 1px solid #f0f0f0; }
        .ch-pager a, .ch-pager span {
            min-width: 30px; height: 30px; margin: 0 3px; padding: 0 8px;
            display: inline-flex; align-items: center; justify-content: center;
            border: 1px solid #d5d5da; border-radius: 6px;
            background: #fff; color: #333; font-size: 13px; text-decoration: none;
        }
        .ch-pager span {
            background: #5b6ef5; border-color: #5b6ef5; color: #fff;
        }

        /* Original form kept for behaviour, hidden to match the design */
        .dc-title { font-size: 1.5rem; font-weight: 700; color: #111; margin: 0 0 34px; }
        .dc-card { background: #fff; border: 1px solid #d9d9d9; border-radius: 10px; padding: 26px; }
        .dc-field { margin-bottom: 18px; }
        .dc-field label { display: block; font-size: .95rem; color: #111; margin-bottom: 8px; }
        .dc-field .dc-input { display: block; width: 100%; border: 1px solid #b9b9b9; border-radius: 8px; padding: 11px 14px; font-size: .95rem; color: #333; background: #fff; }
        .dc-summary-title { font-size: 1.3rem; font-weight: 700; color: #111; margin: 0 0 26px; }
        .dc-sum-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; font-size: 1.15rem; }
        .dc-sum-row span { color: #111; }
        .dc-sum-row b { font-weight: 700; color: #111; }
        .dc-submit-wrap { display: flex; justify-content: flex-end; margin-top: 26px; }
        .dc-submit { background: #5b2df0; border: none; color: #fff; border-radius: 8px; padding: 12px 24px; font-size: 1rem; font-weight: 600; cursor: pointer; }
        .dc-submit:hover { background: #4a24d6; }
    </style>


    <h3 class="ch-title">
        Closing History
    </h3>

    <p class="ch-subtitle">
        Manage All Stock movement
    </p>


    <!-- Message -->

    <asp:Label
        ID="lblMessage"
        runat="server"
        Visible="false"
        CssClass="alert d-block">
    </asp:Label>


    <!-- Date range -->

    <div class="ch-range-wrap">

        <div class="ch-range">

            <i class="bi bi-calendar3"></i>

            01 may 2024 - 17 may 2024

        </div>

    </div>


    <!-- Closing History table -->

    <div class="ch-card">

        <table class="table align-middle mb-0">

            <thead>

                <tr>

                    <th>Dates</th>

                    <th>Vehicle</th>

                    <th>Route</th>

                    <th>Salesman</th>

                    <th>Status</th>

                    <th>Difference</th>

                </tr>

            </thead>

            <tbody>

                <tr>

                    <td>17 May 2024</td>

                    <td>GJ05AB0034</td>

                    <td>Beat 01</td>

                    <td>Rahul Patel</td>

                    <td>Closed</td>

                    <td>-5</td>

                </tr>

                <tr>

                    <td>Simply Salted</td>

                    <td>GJ05AB0034</td>

                    <td>Beat 01</td>

                    <td>Rahul Patel</td>

                    <td>Closed</td>

                    <td>-2</td>

                </tr>

                <tr>

                    <td>Cream Onion</td>

                    <td>GJ05AB0034</td>

                    <td>Beat 01</td>

                    <td>Rahul Patel</td>

                    <td>Closed</td>

                    <td>0</td>

                </tr>

                <tr>

                    <td>Aloo Bhujia</td>

                    <td>GJ05AB0034</td>

                    <td>Beat 01</td>

                    <td>Rahul Patel</td>

                    <td>Closed</td>

                    <td>0</td>

                </tr>

                <tr>

                    <td>Moong Dal</td>

                    <td>GJ05AB0034</td>

                    <td>Beat 01</td>

                    <td>Rahul Patel</td>

                    <td>Closed</td>

                    <td>0</td>

                </tr>

            </tbody>

        </table>

    </div>


    <!-- Pager -->

    <div class="ch-card mt-4" style="border: none;">

        <table class="ch-pager" style="width: 100%;">

            <tr>

                <td>

                    <a href="#">&lsaquo;</a>

                    <span>1</span>

                    <a href="#">2</a>

                    <a href="#">&rsaquo;</a>

                </td>

            </tr>

        </table>

    </div>


    <!-- Original closing form (hidden to match the design, kept for behaviour) -->

    <div class="d-none">


        <!-- Closing Form -->

        <div class="col-lg-4 col-md-5">

            <div class="dc-card">

                <div class="dc-field">

                    <label for="<%= ddlVehicle.ClientID %>">Vehicle</label>

                    <asp:DropDownList
                        ID="ddlVehicle"
                        runat="server"
                        CssClass="dc-input">
                    </asp:DropDownList>

                </div>

                <div class="dc-field">

                    <label for="<%= ddlRoute.ClientID %>">Route</label>

                    <asp:DropDownList
                        ID="ddlRoute"
                        runat="server"
                        CssClass="dc-input">
                    </asp:DropDownList>

                </div>

                <div class="dc-field">

                    <label for="<%= ddlSalesman.ClientID %>">Salesman</label>

                    <asp:DropDownList
                        ID="ddlSalesman"
                        runat="server"
                        CssClass="dc-input">
                    </asp:DropDownList>

                </div>

                <div class="dc-field mb-0">

                    <label for="<%= txtDate.ClientID %>">Date</label>

                    <asp:TextBox
                        ID="txtDate"
                        runat="server"
                        CssClass="dc-input"
                        TextMode="Date">
                    </asp:TextBox>

                </div>

            </div>

        </div>


        <!-- Closing Summary -->

        <div class="col-lg-4 col-md-5">

            <div class="dc-card">

                <h5 class="dc-summary-title">
                    Closing Summary
                </h5>

                <div class="dc-sum-row">
                    <span>Opening Stock</span>
                    <b>
                        <asp:Label ID="lblOpeningStock" runat="server" Text="0box" />
                    </b>
                </div>

                <div class="dc-sum-row">
                    <span>Total Sales</span>
                    <b>
                        <asp:Label ID="lblTotalSales" runat="server" Text="0box" />
                    </b>
                </div>

                <div class="dc-sum-row">
                    <span>Expected Stock</span>
                    <b>
                        <asp:Label ID="lblExpectedStock" runat="server" Text="0box" />
                    </b>
                </div>

                <div class="dc-sum-row">
                    <span>Actual Stock</span>
                    <b>
                        <asp:Label ID="lblActualStock" runat="server" Text="0box" />
                    </b>
                </div>

                <div class="dc-sum-row mb-0">
                    <span>Difference</span>
                    <b>
                        <asp:Label ID="lblDifference" runat="server" Text="0 Box" />
                    </b>
                </div>

            </div>

        </div>

    <div class="dc-submit-wrap">

        <asp:Button
            ID="btnSubmit"
            runat="server"
            CssClass="dc-submit"
            Text="Submit Closing"
            OnClick="btnSubmit_Click" />

    </div>

    </div>


</asp:Content>
