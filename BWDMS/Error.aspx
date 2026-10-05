<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="Error.aspx.cs"
    Inherits="BWDMS.Error" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <meta charset="utf-8" />

    <meta name="viewport"
          content="width=device-width, initial-scale=1" />

    <title>Something went wrong | BWDMS</title>

    <!-- Bootstrap 5 CSS -->
    <link
        href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"
        rel="stylesheet" />

    <!-- Bootstrap Icons -->
    <link
        rel="stylesheet"
        href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" />

    <style>
        body {
            background: #f4f6f9;
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 24px;
        }

        .error-card {
            background: #fff;
            border-radius: 12px;
            box-shadow: 0 10px 30px rgba(0, 0, 0, .08);
            max-width: 560px;
            width: 100%;
            padding: 36px 32px;
            text-align: center;
        }

        .error-icon {
            font-size: 3rem;
            color: #dc3545;
        }

        .error-ref {
            font-family: Consolas, "Courier New", monospace;
            background: #f8f9fa;
            border: 1px dashed #ced4da;
            border-radius: 6px;
            padding: 6px 10px;
            display: inline-block;
            margin-top: 6px;
            font-size: .9rem;
            word-break: break-all;
        }
    </style>

</head>

<body>

    <form id="form1" runat="server">

        <div class="error-card">

            <div class="error-icon">
                <i class="bi bi-exclamation-triangle"></i>
            </div>

            <h1 class="h4 mt-3">
                Something went wrong
            </h1>

            <p class="text-secondary mt-2 mb-3">
                The incident has been logged. Please try again.
                If the problem continues, share the reference below
                with your administrator.
            </p>

            <div class="mb-1 text-secondary small">
                Reference
            </div>

            <div>
                <asp:Label ID="lblReference" runat="server"
                           CssClass="error-ref">
                </asp:Label>
            </div>

            <hr />

            <div class="d-flex flex-wrap gap-2 justify-content-center">

                <a href="javascript:history.back()"
                   class="btn btn-outline-secondary">
                    <i class="bi bi-arrow-left"></i>
                    Back
                </a>

                <asp:HyperLink ID="lnkHome" runat="server"
                               CssClass="btn btn-primary">
                    <i class="bi bi-house-door"></i>
                    Go to my page
                </asp:HyperLink>

            </div>

        </div>

    </form>

</body>

</html>
