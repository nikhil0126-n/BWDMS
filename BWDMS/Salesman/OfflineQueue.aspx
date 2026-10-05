<%@ Page Title="Offline Orders"
    Language="C#"
    MasterPageFile="~/Master/DashboardMaster.master"
    AutoEventWireup="true"
    CodeBehind="OfflineQueue.aspx.cs"
    Inherits="BWDMS.Salesman.OfflineQueue" %>


<asp:Content ID="Content1"
    ContentPlaceHolderID="TitleContent"
    runat="server">

    Offline Orders

</asp:Content>


<asp:Content ID="Content3"
    ContentPlaceHolderID="HeadContent"
    runat="server">

    <script src="../Scripts/OfflineSync.js"></script>

</asp:Content>


<asp:Content ID="Content2"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <div class="d-flex justify-content-between align-items-start flex-wrap gap-2 mb-4">

        <div>

            <h3 class="fw-bold mb-1">
                Orders captured offline
            </h3>

            <p class="text-muted mb-0">
                Orders saved while out of coverage wait here until the
                server confirms them. Nothing is treated as accepted
                until then.
            </p>

        </div>

        <div class="d-flex gap-2">

            <button type="button"
                    id="btnSyncNow"
                    class="btn btn-primary">

                <i class="bi bi-cloud-arrow-up"></i>

                Send Now

            </button>

            <button type="button"
                    id="btnRefresh"
                    class="btn btn-light border">

                <i class="bi bi-arrow-clockwise"></i>

                Refresh

            </button>

        </div>

    </div>


    <!-- Counts -->

    <div class="row g-3 mb-4">

        <div class="col-6 col-md-3">
            <div class="dashboard-card p-3">
                <div class="text-muted small">Pending</div>
                <div class="fs-4 fw-bold" id="cPending">0</div>
            </div>
        </div>

        <div class="col-6 col-md-3">
            <div class="dashboard-card p-3">
                <div class="text-muted small">Syncing</div>
                <div class="fs-4 fw-bold" id="cSyncing">0</div>
            </div>
        </div>

        <div class="col-6 col-md-3">
            <div class="dashboard-card p-3">
                <div class="text-muted small">Synced</div>
                <div class="fs-4 fw-bold" id="cSynced">0</div>
            </div>
        </div>

        <div class="col-6 col-md-3">
            <div class="dashboard-card p-3">
                <div class="text-muted small">Needs attention</div>
                <div class="fs-4 fw-bold text-danger" id="cFailed">0</div>
            </div>
        </div>

    </div>


    <div id="netState" class="alert alert-secondary d-none"></div>


    <div class="dashboard-card">

        <div class="table-responsive">

            <table class="table table-hover align-middle mb-0">

                <thead>
                    <tr>
                        <th>Captured</th>
                        <th>Shop</th>
                        <th>Lines</th>
                        <th>Total</th>
                        <th>State</th>
                        <th>Detail</th>
                        <th></th>
                    </tr>
                </thead>

                <tbody id="rows"></tbody>

            </table>

        </div>

        <div id="empty" class="text-center py-5">

            <i class="bi bi-cloud-check fs-1 text-muted"></i>

            <p class="text-muted mt-2 mb-0">
                Nothing waiting. Orders you capture offline will appear
                here.
            </p>

        </div>

    </div>


    <script type="text/javascript">
        (function () {
            function money(value) {
                return Number(value || 0).toFixed(2);
            }

            function escapeHtml(text) {
                var div = document.createElement('div');
                div.appendChild(
                    document.createTextNode(text || ''));

                return div.innerHTML;
            }

            function badge(status) {
                if (status === 'synced') {
                    return '<span class="badge bg-success">Synced</span>';
                }

                if (status === 'syncing') {
                    return '<span class="badge bg-info">Syncing</span>';
                }

                if (status === 'failed') {
                    return '<span class="badge bg-danger">Needs attention</span>';
                }

                return '<span class="badge bg-warning text-dark">Pending</span>';
            }

            function addAction(td, label, className, handler) {
                var button = document.createElement('button');
                button.type = 'button';
                button.className = className;
                button.textContent = label;
                button.onclick = handler;

                td.appendChild(button);
            }

            function render() {
                if (!window.BwdmsOffline) {
                    return;
                }

                BwdmsOffline.all().then(function (orders) {
                    var body = document.getElementById('rows');
                    body.innerHTML = '';

                    var counts = {
                        pending: 0, syncing: 0,
                        synced: 0, failed: 0
                    };

                    orders.forEach(function (o) {
                        if (counts[o.status] !== undefined) {
                            counts[o.status] += 1;
                        }

                        var total = 0;

                        (o.lines || []).forEach(function (l) {
                            total += Number(l.lineTotal || 0);
                        });

                        var tr = document.createElement('tr');

                        tr.innerHTML =
                            '<td class="text-nowrap">' +
                                new Date(o.capturedAt)
                                    .toLocaleString() +
                            '</td>' +
                            '<td>' +
                                escapeHtml(
                                    o.shopName ||
                                    ('Shop ' + o.shopId)) +
                            '</td>' +
                            '<td class="text-end">' +
                                (o.lines || []).length +
                            '</td>' +
                            '<td class="text-end">' +
                                money(total) +
                            '</td>' +
                            '<td>' + badge(o.status) + '</td>' +
                            '<td>' +
                                (o.lastError
                                    ? '<span class="text-danger small">' +
                                      escapeHtml(o.lastError) +
                                      '</span>'
                                    : '<span class="text-muted small">' +
                                      (o.status === 'synced'
                                          ? 'Accepted by the server'
                                          : 'Waiting to send') +
                                      '</span>') +
                            '</td>';

                        var actions =
                            document.createElement('td');

                        actions.className = 'text-end';

                        if (o.status !== 'synced') {
                            addAction(
                                actions, 'Retry',
                                'btn btn-sm btn-outline-primary me-1',
                                function () {
                                    BwdmsOffline.sync().then(render);
                                });
                        }

                        addAction(
                            actions, 'Delete',
                            'btn btn-sm btn-outline-danger',
                            function () {
                                if (!window.confirm(
                                        'Delete this queued order? It ' +
                                        'has not been accepted by the ' +
                                        'server.')) {
                                    return;
                                }

                                BwdmsOffline
                                    .remove(o.clientOrderId)
                                    .then(render);
                            });

                        tr.appendChild(actions);
                        body.appendChild(tr);
                    });

                    document.getElementById('cPending')
                        .textContent = counts.pending;
                    document.getElementById('cSyncing')
                        .textContent = counts.syncing;
                    document.getElementById('cSynced')
                        .textContent = counts.synced;
                    document.getElementById('cFailed')
                        .textContent = counts.failed;

                    document.getElementById('empty')
                        .style.display =
                            orders.length ? 'none' : 'block';

                    var net = document.getElementById('netState');

                    if (!BwdmsOffline.isOnline()) {
                        net.className = 'alert alert-warning';
                        net.textContent =
                            'You appear to be offline. Captured orders ' +
                            'are saved on this device and will be sent ' +
                            'when you are back online.';
                    } else {
                        net.classList.add('d-none');
                    }
                });
            }

            document.addEventListener('DOMContentLoaded',
                function () {
                    var sync =
                        document.getElementById('btnSyncNow');

                    if (sync) {
                        sync.addEventListener('click', function () {
                            BwdmsOffline.sync()
                                .then(function (r) {
                                    window.alert(
                                        r.attempted
                                            ? ('Sent ' + r.attempted +
                                               ' order(s). Accepted: ' +
                                               r.synced +
                                               ', rejected: ' +
                                               r.failed)
                                            : 'Nothing waiting to send.');

                                    render();
                                })
                                .catch(function () {
                                    window.alert(
                                        'Could not reach the server. ' +
                                        'The queue is safe and will be ' +
                                        'sent when you are back online.');
                                });
                        });
                    }

                    var refresh =
                        document.getElementById('btnRefresh');

                    if (refresh) {
                        refresh.addEventListener('click', render);
                    }

                    render();
                });
        })();
    </script>

</asp:Content>