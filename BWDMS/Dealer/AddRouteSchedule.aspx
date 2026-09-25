
<%@ Page Title="Add Route Schedule"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="AddRouteSchedule.aspx.cs"
    Inherits="BWDMS.Dealer.AddRouteSchedule" %>

<asp:Content ID="Content1"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <link href="../Content/RouteSchedules.css" rel="stylesheet" />

    <div class="route-page">

        <div class="page-header">

            <div>
                <h2>
                    <asp:Literal ID="litPageTitle"
                        runat="server"
                        Text="Add Route Schedule" />
                </h2>

                <p>
                    Assign a route, day, vehicle, salesman, and driver.
                </p>
            </div>

            <a href="RouteSchedules.aspx"
               class="btn-secondary">

                Back to Schedules

            </a>

        </div>

        <div class="form-card">

            <asp:Label ID="lblMessage"
                runat="server"
                CssClass="message-label" />

            <div class="form-grid">

                <div class="form-group">

                    <label>Route <span>*</span></label>

                    <asp:DropDownList ID="ddlRoute"
                        runat="server"
                        CssClass="form-control">

                    </asp:DropDownList>

                    <asp:RequiredFieldValidator
                        ID="rfvRoute"
                        runat="server"
                        ControlToValidate="ddlRoute"
                        InitialValue=""
                        ErrorMessage="Please select a route."
                        CssClass="validation-error" />

                </div>

                <div class="form-group">

                    <label>Day of Week <span>*</span></label>

                    <asp:DropDownList ID="ddlDayOfWeek"
                        runat="server"
                        CssClass="form-control">

                        <asp:ListItem
                            Text="Select Day"
                            Value="" />

                        <asp:ListItem
                            Text="Monday"
                            Value="Monday" />

                        <asp:ListItem
                            Text="Tuesday"
                            Value="Tuesday" />

                        <asp:ListItem
                            Text="Wednesday"
                            Value="Wednesday" />

                        <asp:ListItem
                            Text="Thursday"
                            Value="Thursday" />

                        <asp:ListItem
                            Text="Friday"
                            Value="Friday" />

                        <asp:ListItem
                            Text="Saturday"
                            Value="Saturday" />

                        <asp:ListItem
                            Text="Sunday"
                            Value="Sunday" />

                    </asp:DropDownList>

                    <asp:RequiredFieldValidator
                        ID="rfvDay"
                        runat="server"
                        ControlToValidate="ddlDayOfWeek"
                        InitialValue=""
                        ErrorMessage="Please select a day."
                        CssClass="validation-error" />

                </div>

                <div class="form-group">

                    <label>Vehicle</label>

                    <asp:DropDownList ID="ddlVehicle"
                        runat="server"
                        CssClass="form-control">

                    </asp:DropDownList>

                </div>

                <div class="form-group">

                    <label>Salesman</label>

                    <asp:DropDownList ID="ddlSalesman"
                        runat="server"
                        CssClass="form-control">

                    </asp:DropDownList>

                </div>

                <div class="form-group">

                    <label>Driver</label>

                    <asp:DropDownList ID="ddlDriver"
                        runat="server"
                        CssClass="form-control">

                    </asp:DropDownList>

                </div>

                <div class="form-group">

                    <label>Status</label>

                    <asp:DropDownList ID="ddlStatus"
                        runat="server"
                        CssClass="form-control">

                        <asp:ListItem
                            Text="Active"
                            Value="1" />

                        <asp:ListItem
                            Text="Inactive"
                            Value="0" />

                    </asp:DropDownList>

                </div>

            </div>

            <div class="form-actions">

                <asp:Button ID="btnSave"
                    runat="server"
                    Text="Save Schedule"
                    CssClass="btn-primary"
                    OnClick="btnSave_Click" />

                <a href="RouteSchedules.aspx"
                   class="btn-secondary">

                    Cancel

                </a>

            </div>

        </div>

    </div>

</asp:Content>