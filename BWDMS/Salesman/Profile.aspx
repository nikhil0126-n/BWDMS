<%@ Page Title="My Profile"
    Language="C#"
    MasterPageFile="~/Master/SalesmanMaster.master"
    AutoEventWireup="true"
    CodeBehind="Profile.aspx.cs"
    Inherits="BWDMS.Salesman.Profile" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    My Profile

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">


    <style>
        .pf-identity {
            text-align: center;
        }

        .pf-avatar {
            width: 78px;
            height: 78px;
            border-radius: 50%;
            background: linear-gradient(135deg, var(--sw-primary) 0%, var(--sw-primary-2) 100%);
            color: #fff;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 30px;
            font-weight: 700;
            margin: 6px auto 14px;
        }

        .pf-name {
            margin: 0 0 8px;
            font-size: 18px;
            font-weight: 700;
            overflow-wrap: anywhere;
        }

        .pf-detail {
            display: flex;
            align-items: center;
            gap: 10px;
            padding: 10px 0;
            border-top: 1px solid var(--sw-border-soft);
            font-size: 13.5px;
            color: var(--sw-text-soft);
            text-align: left;
            overflow-wrap: anywhere;
        }

        .pf-detail i {
            color: var(--sw-primary);
            width: 18px;
            text-align: center;
            flex: none;
        }

        .pf-message {
            padding: 10px 12px;
            border-radius: 10px;
            margin-bottom: 16px;
            font-size: 13.5px;
        }

        .pf-ok {
            background: #e6f7ec;
            color: #0f7a35;
        }

        .pf-error {
            background: #fdeaea;
            color: #b42318;
        }

        .pf-readonly {
            display: flex;
            align-items: center;
            background: var(--sw-surface-2);
            color: var(--sw-text-soft);
            overflow-wrap: anywhere;
        }

        .pf-actions {
            display: flex;
            align-items: center;
            gap: 12px;
            flex-wrap: wrap;
            margin-top: 18px;
        }

        .pf-actions .btn {
            min-width: 130px;
        }

        @media (max-width: 575.98px) {
            .pf-avatar {
                width: 66px;
                height: 66px;
                font-size: 26px;
            }

            .pf-actions .btn,
            .pf-actions a {
                flex: 1 1 100%;
            }
        }
    </style>


    <div class="sw-page-head">

        <div>

            <asp:HyperLink
                ID="lnkBack"
                runat="server"
                CssClass="sw-crumb"
                NavigateUrl="~/Salesman/Dashboard.aspx">

                <i class="bi bi-arrow-left"></i>
                Back to dashboard

            </asp:HyperLink>

            <h3>My Profile</h3>

            <p>Update your personal details and manage your account.</p>

        </div>

    </div>


    <div class="row g-3">

        <div class="col-12 col-lg-4">

            <div class="sw-card pf-identity">

                <div class="pf-avatar">
                    <asp:Label ID="lblInitial" runat="server">S</asp:Label>
                </div>

                <h5 class="pf-name">
                    <asp:Label ID="lblFullName" runat="server">Salesman</asp:Label>
                </h5>

                <span class="sw-badge sw-badge-info">
                    <asp:Label ID="lblRole" runat="server">Salesman</asp:Label>
                </span>

                <div class="pf-detail">
                    <i class="bi bi-envelope"></i>
                    <asp:Label ID="lblEmail" runat="server"></asp:Label>
                </div>

                <div class="pf-detail">
                    <i class="bi bi-telephone"></i>
                    <asp:Label ID="lblPhone" runat="server"></asp:Label>
                </div>

                <div class="pf-detail">
                    <i class="bi bi-calendar3"></i>
                    <asp:Label ID="lblJoined" runat="server"></asp:Label>
                </div>

                <asp:HyperLink
                    ID="lnkChangePassword"
                    runat="server"
                    CssClass="btn btn-outline-secondary w-100 mt-3"
                    NavigateUrl="~/Account/ChangePassword.aspx">

                    <i class="bi bi-shield-lock"></i>
                    Change Password

                </asp:HyperLink>

            </div>

        </div>


        <div class="col-12 col-lg-8">

            <div class="sw-card">

                <div class="sw-card-head">

                    <div>
                        <h5>Edit profile</h5>
                        <small>Your name and phone number are visible to your dealer.</small>
                    </div>

                </div>


                <asp:Label
                    ID="lblMessage"
                    runat="server"
                    CssClass="pf-message d-none">
                </asp:Label>


                <div class="row g-3">

                    <div class="col-12">

                        <asp:Label
                            runat="server"
                            CssClass="form-label"
                            AssociatedControlID="txtFullName">
                            Full name
                        </asp:Label>

                        <asp:TextBox
                            ID="txtFullName"
                            runat="server"
                            CssClass="form-control"
                            MaxLength="200">
                        </asp:TextBox>

                    </div>


                    <div class="col-12">

                        <asp:Label
                            runat="server"
                            CssClass="form-label"
                            AssociatedControlID="txtPhone">
                            Phone number
                        </asp:Label>

                        <asp:TextBox
                            ID="txtPhone"
                            runat="server"
                            CssClass="form-control"
                            MaxLength="40">
                        </asp:TextBox>

                    </div>


                    <div class="col-12">

                        <asp:Label
                            runat="server"
                            CssClass="form-label">
                            Email address
                        </asp:Label>

                        <div class="form-control pf-readonly">
                            <asp:Label ID="lblEmailForm" runat="server"></asp:Label>
                        </div>

                    </div>

                </div>


                <div class="pf-actions">

                    <asp:Button
                        ID="btnSave"
                        runat="server"
                        CssClass="btn btn-primary"
                        Text="Save changes"
                        OnClick="btnSave_Click" />

                    <asp:HyperLink
                        ID="lnkCancel"
                        runat="server"
                        CssClass="btn btn-outline-secondary"
                        NavigateUrl="~/Salesman/Dashboard.aspx">
                        Cancel
                    </asp:HyperLink>

                </div>

            </div>

        </div>

    </div>


</asp:Content>
