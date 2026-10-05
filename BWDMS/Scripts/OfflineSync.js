/* ============================================================================
   BWDMS - Offline order capture for salesmen
   ----------------------------------------------------------------------------
   What this does, and what it deliberately does NOT do:

     * Captures an order while the device is out of coverage and stores it
       in IndexedDB together with a client-generated operation id.
     * Shows four honest states: pending, syncing, synced, failed.
     * Never reports an order as accepted until the SERVER has confirmed it.
     * Uploads the queue when connectivity returns.
     * Retries are safe: the server keys on the client operation id, so the
       same upload twice results in one order.
     * A rejected order is KEPT with the reason so the salesman can review
       it and try again, or delete it.

   No fake "offline" toggle: if the browser cannot reach the server, the
   queue simply stays pending until it can.
   ============================================================================ */

var BwdmsOffline = (function () {
    'use strict';

    var DB_NAME = 'bwdms-offline';
    var DB_VERSION = 1;
    var STORE = 'orders';
    var SYNC_URL = '/Handlers/OrderSync.ashx';

    /* ------------------------------------------------------------------
       Operation id. crypto.randomUUID when available, otherwise a
       timestamp + random string. Either way it only has to be unique
       per browser, because the server stores it and enforces uniqueness.
       ------------------------------------------------------------------ */
    function newOperationId() {
        if (window.crypto && window.crypto.randomUUID) {
            return window.crypto.randomUUID();
        }

        return 'op-' + new Date().getTime().toString(36) + '-' +
               Math.random().toString(36).slice(2, 12);
    }

    /* ------------------------------------------------------------------
       IndexedDB
       ------------------------------------------------------------------ */
    function openDb() {
        return new Promise(function (resolve, reject) {
            if (!window.indexedDB) {
                reject(new Error('This browser has no IndexedDB.'));
                return;
            }

            var request = window.indexedDB.open(DB_NAME, DB_VERSION);

            request.onupgradeneeded = function (event) {
                var db = event.target.result;

                if (!db.objectStoreNames.contains(STORE)) {
                    var store = db.createObjectStore(STORE, {
                        keyPath: 'clientOrderId'
                    });

                    store.createIndex('status', 'status', {
                        unique: false
                    });
                    store.createIndex('capturedAt', 'capturedAt', {
                        unique: false
                    });
                }
            };

            request.onsuccess = function (event) {
                resolve(event.target.result);
            };

            request.onerror = function (event) {
                reject(event.target.error ||
                       new Error('Could not open the offline store.'));
            };
        });
    }

    function withStore(mode, action) {
        return openDb().then(function (db) {
            return new Promise(function (resolve, reject) {
                var tx = db.transaction([STORE], mode);
                var store = tx.objectStore(STORE);

                var result;

                try {
                    result = action(store);
                } catch (e) {
                    reject(e);
                    return;
                }

                tx.oncomplete = function () { resolve(result); };
                tx.onerror = function () {
                    reject(tx.error ||
                           new Error('Offline store failed.'));
                };
                tx.onabort = function () {
                    reject(tx.error ||
                           new Error('Offline store aborted.'));
                };
            });
        });
    }

    function put(order) {
        return withStore('readwrite', function (store) {
            store.put(order);
        });
    }

    function all() {
        return openDb().then(function (db) {
            return new Promise(function (resolve, reject) {
                var tx = db.transaction([STORE], 'readonly');
                var store = tx.objectStore(STORE);
                var found = [];

                var request = store.openCursor();

                request.onsuccess = function (event) {
                    var cursor = event.target.result;

                    if (cursor) {
                        found.push(cursor.value);
                        cursor.continue();
                    }
                };

                tx.oncomplete = function () {
                    found.sort(function (a, b) {
                        return a.capturedAt - b.capturedAt;
                    });

                    resolve(found);
                };

                tx.onerror = function () {
                    reject(tx.error);
                };
            });
        });
    }

    function remove(clientOrderId) {
        return withStore('readwrite', function (store) {
            store.delete(clientOrderId);
        });
    }

    function counts() {
        return all().then(function (rows) {
            var out = {
                pending: 0,
                syncing: 0,
                synced: 0,
                failed: 0
            };

            rows.forEach(function (row) {
                if (out[row.status] !== undefined) {
                    out[row.status] += 1;
                }
            });

            return out;
        });
    }

    /* ------------------------------------------------------------------
       Capture
       ------------------------------------------------------------------ */
    function capture(draft) {
        var now = new Date();

        var order = {
            clientOrderId: newOperationId(),
            capturedAt: now.getTime(),
            capturedOn: now.toISOString(),
            status: 'pending',
            attempts: 0,
            lastError: null,
            shopId: draft.shopId,
            routeScheduleId: draft.routeScheduleId,
            orderDate: draft.orderDate,
            deliveryDate: draft.deliveryDate || null,
            remarks: draft.remarks || '',
            shopName: draft.shopName || '',
            lines: (draft.lines || []).map(function (line) {
                return {
                    productVariantId: line.productVariantId,
                    variantLabel: line.variantLabel || '',
                    unitId: line.unitId,
                    unitCode: line.unitCode || '',
                    quantity: line.quantity,
                    quantityPackets: line.quantityPackets,
                    unitPrice: line.unitPrice,
                    lineTotal: line.lineTotal
                };
            })
        };

        return put(order).then(function () {
            return order;
        });
    }

    /* ------------------------------------------------------------------
       Sync
       ------------------------------------------------------------------ */
    function pendingOrders() {
        return all().then(function (rows) {
            return rows.filter(function (row) {
                return row.status === 'pending' ||
                       row.status === 'failed';
            });
        });
    }

    function mark(ids, status, error) {
        return openDb().then(function (db) {
            return new Promise(function (resolve, reject) {
                var tx = db.transaction([STORE], 'readwrite');
                var store = tx.objectStore(STORE);

                ids.forEach(function (id) {
                    var request = store.get(id);

                    request.onsuccess = function (event) {
                        var row = event.target.result;

                        if (!row) {
                            return;
                        }

                        row.status = status;
                        row.lastError = error || null;

                        if (status === 'syncing') {
                            row.attempts = (row.attempts || 0) + 1;
                        }

                        store.put(row);
                    };
                });

                tx.oncomplete = function () { resolve(); };
                tx.onerror = function () { reject(tx.error); };
            });
        });
    }

    function describeFailure(result) {
        var code = result.code || 'FAILED';
        var message = result.message || '';

        switch (code) {
            case 'PRICE_CHANGED':
                // message looks like PRICE_CHANGED:<label>:<newPrice>
                var priceParts = message.split(':');

                if (priceParts.length >= 3) {
                    return 'The price of ' + priceParts[1] +
                           ' changed to ' + priceParts[2] +
                           ' while you were offline.';
                }

                return 'A price changed while you were offline.';

            case 'INSUFFICIENT_STOCK':
                var stockParts = message.split(':');

                if (stockParts.length >= 4) {
                    return 'Not enough on the vehicle for ' +
                           stockParts[1] + ': only ' + stockParts[2] +
                           ' packet(s) left, this bill needs ' +
                           stockParts[3] + '.';
                }

                return 'Not enough stock on the vehicle.';

            case 'SHOP_INACTIVE':
                return 'That shop has been deactivated.';

            case 'SHOP_NOT_FOUND':
                return 'That shop is not on your account any more.';

            case 'ROUTE_CLOSED':
                return 'The dealer has closed this route day.';

            case 'NO_VEHICLE':
                return 'That route has no vehicle assigned.';

            case 'NOT_AUTHORIZED':
                return 'That route is no longer assigned to you.';

            case 'PRODUCT_UNAVAILABLE':
                return 'A product on this bill is no longer available.';

            default:
                return message || 'The server rejected this order.';
        }
    }

    function sync() {
        return pendingOrders().then(function (rows) {
            if (!rows.length) {
                return { attempted: 0, synced: 0, failed: 0 };
            }

            var ids = rows.map(function (row) {
                return row.clientOrderId;
            });

            return mark(ids, 'syncing', null)
                .then(function () {
                    return post(rows);
                })
                .then(function (results) {
                    var synced = [];
                    var duplicates = [];
                    var failed = [];

                    (results || []).forEach(function (result) {
                        if (result.status === 'synced' ||
                            result.status === 'duplicate') {
                            if (result.status === 'synced') {
                                synced.push(result);
                            } else {
                                duplicates.push(result);
                            }
                        } else {
                            failed.push(result);
                        }
                    });

                    var doneIds = synced
                        .concat(duplicates)
                        .map(function (r) {
                            return r.clientOrderId;
                        });

                    var chain = doneIds.length
                        ? mark(doneIds, 'synced', null)
                        : Promise.resolve();

                    var failedIds = failed.map(function (r) {
                        return r.clientOrderId;
                    });

                    failedIds.forEach(function (id) {
                        chain = chain.then(function () {
                            var one = failed.filter(function (r) {
                                return r.clientOrderId === id;
                            })[0];

                            return mark(
                                [id],
                                'failed',
                                describeFailure(one));
                        });
                    });

                    return chain.then(function () {
                        return {
                            attempted: rows.length,
                            synced: synced.length + duplicates.length,
                            failed: failed.length
                        };
                    });
                });
        });
    }

    function post(rows) {
        var body = JSON.stringify({
            orders: rows.map(function (row) {
                return {
                    clientOrderId: row.clientOrderId,
                    shopId: row.shopId,
                    routeScheduleId: row.routeScheduleId,
                    orderDate: row.orderDate,
                    deliveryDate: row.deliveryDate,
                    remarks: row.remarks,
                    lines: row.lines.map(function (line) {
                        return {
                            productVariantId: line.productVariantId,
                            unitId: line.unitId,
                            quantity: line.quantity,
                            unitPrice: line.unitPrice
                        };
                    })
                };
            })
        });

        return fetch(SYNC_URL, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json; charset=utf-8'
            },
            credentials: 'same-origin',
            body: body
        }).then(function (response) {
            return response.json();
        });
    }

    function isOnline() {
        return navigator.onLine !== false;
    }

    /* ------------------------------------------------------------------
       Public surface
       ------------------------------------------------------------------ */
    return {
        newOperationId: newOperationId,
        capture: capture,
        all: all,
        counts: counts,
        sync: sync,
        remove: remove,
        pendingOrders: pendingOrders,
        isOnline: isOnline
    };
})();

/* When connectivity comes back, push whatever is waiting. This is the only
   automatic behaviour: nothing is ever reported as accepted locally. */
if (window.addEventListener) {
    window.addEventListener('online', function () {
        BwdmsOffline.sync();
    });
}