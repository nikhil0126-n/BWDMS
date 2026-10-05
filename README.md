# BWDMS — Balaji Wafers Dealer Management System

A distributor/dealer management application for **Balaji Wafers**, built as an
**ASP.NET Web Forms** application on **.NET Framework 4.7.2**, **C#**,
**SQL Server LocalDB**, **ADO.NET** with parameterized SQL, and **Bootstrap 5**.

It manages the complete distribution workflow: dealers and catalogue, dealers'
staff/vehicles/routes/villages/shops, company stock receipts and warehouse
inventory, vehicle loading, beat/telephone/counter orders, dispatch, returns,
vehicle reconciliation and reporting.

---

## 1. Prerequisites

| Requirement | Version used / minimum |
|---|---|
| Windows | 10 / Server 2016 or later |
| Visual Studio | 2022 (18.x) with the **ASP.NET and web development** workload |
| MSBuild | ships with Visual Studio |
| .NET Framework | 4.7.2 targeting pack (4.8 runtime is fine) |
| SQL Server LocalDB | 15.0 (`MSSQLLocalDB`) |
| Browser | any current Chrome/Edge/Firefox |

NuGet packages are restored by MSBuild. The only package is the Microsoft
CodeDom compiler provider, which `packages.config` already references.

---

## 2. Database setup

### 2.1 The connection string

`BWDMS\Web.config`:

```xml
<add name="BWDMSConnection"
     connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Database1.mdf;Integrated Security=True"
     providerName="System.Data.SqlClient" />
```

> **There is no database named `BWDMS`.**
> The connection attaches `BWDMS\App_Data\Database1.mdf` by path. A script that
> begins with `USE BWDMS` fails with *"Cannot open database … because it does
> not exist"*, which is why **no script in `Database\` contains a `USE`
> statement**.

`App_Data` is already deployed with `Database1.mdf`, so a fresh clone works
immediately. To rebuild the database from scratch, delete the two files in
`BWDMS\App_Data` and run the deployment script below.

### 2.2 Apply the scripts (ordered)

```powershell
powershell -ExecutionPolicy Bypass -File Database\Deploy-Database.ps1
```

The script reads the connection string out of `Web.config`, resolves
`|DataDirectory|`, **prints the database name it actually selected**
(verification step), then runs every `.sql` file in `Database\` in filename
order, and re-checks the object counts afterwards.

Preview only, without touching the database:

```powershell
powershell -ExecutionPolicy Bypass -File Database\Deploy-Database.ps1 -WhatIf
```

### 2.3 Script order

| File | Purpose |
|---|---|
| `00_CreateDatabase.sql` | Database creation / attachment check. Documents both deployment shapes and verifies which one is live. |
| `01_Schema.sql` | All 30 tables, missing-column backfills, foreign keys, check constraints, indexes, demo seed and reporting views. |
| `02_ReceiptsAndOrderSource.sql` | Company stock receipts (`CompanyStockReceipts`, `CompanyStockReceiptDetails`) and `Orders.OrderSource`. |
| `03_RouteFlow.sql` | The route-day model: `VehicleStock.RouteScheduleId`, one-vehicle-per-route-per-weekday, village/route uniqueness, route-scoped reconciliation. |
| `04_RouteDayAndPricing.sql` | Route-day closure (`RouteSchedules.IsClosed/ClosedAt/ClosedBy/StockDate`), offline-sync idempotency (`UNIQUE` on `Orders.ClientOrderId`), and real price history (`UNIQUE` on `(ProductOptionId, EffectiveFrom)`). |

**Every script is idempotent** — each is guarded with `IF OBJECT_ID(...) IS
NULL` / `IF COL_LENGTH(...) IS NULL`, so it only creates what is missing and can
be re-run safely against an existing or a brand-new database.

### 2.4 Why the scripts are not split four ways

PART 18 of the specification suggests separate *table*, *constraints/index* and
*seed* scripts. In this schema a `UNIQUE` index must be created **after** its
table and **before** rows exist, and the check constraints sit inside their
`CREATE TABLE`, so splitting them into standalone files would produce scripts
that cannot run on a fresh database — which the same requirement forbids. The
objects therefore stay together inside `01_Schema.sql`, numbered so the
required order is still explicit and applied automatically.

---

## 3. Running the application

### 3.1 From Visual Studio

1. Open `BWDMS.slnx`.
2. Restore NuGet packages if prompted.
3. Press **F5**. The site opens on `Account\Login.aspx`.

### 3.2 Without Visual Studio (IIS Express)

```powershell
Start-Process "C:\Program Files\IIS Express\iisexpress.exe" `
  -ArgumentList '/path:C:\ASP.NET\BWDMS\BWDMS','/port:8099' -WindowStyle Hidden
```

Then browse to <http://localhost:8099/Account/Login.aspx>.

> **Rebuilding drops sessions.** A recompile restarts the ASP.NET application,
> which clears `InProc` session state. Log in again after every rebuild.

---

## 4. Initial administrator

The application is **session-authenticated** (not Forms auth). An administrator
is created through a documented, one-time setup page rather than by a
hard-coded password baked into the application.

* **`Account\CreateAdmin.aspx`** — creates the first administrator when no user
  exists. It refuses to run once a user is present, so it cannot be used to
  escalate privileges later.
* The demo seed in `01_Schema.sql` (SECTION 10) creates the sample accounts
  below **for demonstration only**.

| Email | Password | Role | UserId |
|---|---|---|---|
| `admin@balaji.com` | `dealer123` | Admin | 1 |
| `dealer@balaji.com` | `dealer123` | Dealer (DealerId 2) | 2 |
| `salesman@balaji.com` | `dealer123` | Salesman (2 route schedules) | 3 |
| `suresh@balaji.com` | `sales1234` | Salesman (no schedules — empty states) | 5 |

**Change these before any real deployment.** Each account can be changed from
**Settings → Change Password** (`Account\ChangePassword.aspx`), which hashes the
new value with `PasswordHelper` (PBKDF2, salted, 100 000 iterations). Plain-text
passwords are never stored and `Users.PasswordHash` is never selected into a
grid, report or export.

---

## 5. Roles and authorization

| Role | Home | Scope |
|---|---|---|
| **Admin** | `Admin\Dashboard.aspx` | system-wide, includes dealer administration |
| **Dealer** | `Dealer\Dashboard.aspx` | every dealer-owned record carries `DealerId = Session["UserId"]` |
| **Salesman** | `Salesman\Dashboard.aspx` | every query filters on `SalesmanId = Session["UserId"]` |

Authorization is enforced **server-side on every page and every query**, not by
hiding buttons. Every protected page repeats the same guard:

```csharp
if (Session["UserId"] == null) { Response.Redirect("~/Account/Login.aspx", false); CompleteRequest(); return; }
if (Session["UserRole"] != "Dealer") { Response.Redirect(AppAuth.HomeUrl(...), false); CompleteRequest(); return; }
```

Dealer ownership is re-checked inside every statement (`WHERE DealerId = @DealerId`),
so changing an ID in the query string cannot reach another dealer's data — see
`Dealer\ShopOrders.aspx`, which resolves the shop through a dealer-scoped query
before it renders anything.

A **Driver** is represented as a staff assignment record (`Routes.PreferredDriverId`,
`RouteSchedules.DriverId`, both referencing `Users`), which is what the
specification permits; there is no Driver dashboard because none is needed.

---

## 6. Project layout

```
BWDMS.slnx
BWDMS/
  BWDMS.csproj              every file registered (Content + Compile)
  Web.config                connection string, session, customErrors, cookies
  Web.Release.config        release transform (requireSSL, debug=false)
  Global.asax(.cs)          Application_Error -> App_Data\AppLog.txt
  Error.aspx                friendly, information-safe error page
  Default.aspx
  Account/      3 pages   Login, CreateAdmin, ChangePassword
  Admin/       12 pages   dashboard, dealers, categories, products, variants,
                         users, orders, reports
  Dealer/      34 pages   dashboard, staff, vehicles, routes, villages,
                         schedules, schedule villages, shops, shop orders,
                         company receipts, inventory, stock ledger,
                         vehicle loading, orders, dispatches, returns,
                         reconciliations, reports
  Salesman/    10 pages   dashboard, routes, shops, collection, vehicle stock,
                         create order, orders, bill, offline queue, reports
  Master/                DashboardMaster.master (+ code-behind)
  Handlers/              OrderSync.ashx (+ code-behind) - offline sync endpoint
  Data/                  DatabaseHelper, PasswordHelper, AppAuth, CsvExport
  Content/               Dashboard.css, Login.css + per-module CSS
  Scripts/               Dashboard.js, Login.js, OfflineSync.js + per-module JS
Database/                00_CreateDatabase.sql, 01_Schema.sql,
                         02_ReceiptsAndOrderSource.sql, Deploy-Database.ps1
```

**60 `.aspx` pages**, 130 C# files.

> `Services\` and `Models\` from the suggested layout are intentionally absent:
> the specification allows refining the organisation. Business logic lives in
> the page code-behind next to the rules it enforces, with shared helpers in
> `Data\` (`DatabaseHelper`, `PasswordHelper`, `AppAuth`, `CsvExport`).

---

## 7. Implemented features

### Catalogue (Admin)
Dealers (list/search/filter, add, edit, activate/deactivate, unique account
email), product categories (add/edit/search/deactivate), products, product
variants with packet size/weight, MRP, box/patti/packet units with explicit
conversion ratios, company→dealer purchase price and dealer→shop selling price.
No barcodes, no credit limits, no hard-coded product list.

### Routes, villages, schedules (Dealer)
Permanent routes — **no Bi-Weekly concept anywhere**. A route can operate on
several weekdays, and several routes may share a weekday. A village is
permanently attached to one route via `RouteVillages`; a `RouteSchedule`
carries vehicle, salesman and driver; `RouteVillageSchedules` gives the ordered
visit sequence (`VisitSequence`) and **may contain the same village in several
schedules** — that is how repeat visits are modelled, without duplicating the
village or route masters. Duplicate active schedule-village assignments are
rejected, and a village may only be scheduled on a schedule that belongs to its
own permanent route.

### Shops
List, search, add, edit, active status, contact/phone/address, village and route
validation, and **per-shop order history** (`Dealer\ShopOrders.aspx`). The
business term **SHOP** is used in every label, heading and menu item.

### The route day

This is the heart of the application, and the flow is:

```
Admin          creates the dealer and the product catalogue
Dealer         creates salesmen, routes, villages, shops, vehicles
               assigns a vehicle to each route schedule, if that vehicle is free
     |
     v  start of day
Dealer         loads stock ONTO the vehicle FOR that route
     |
     v  on the road
Salesman       visits the shops, creates the order, the BILL is generated
               the goods come off the VEHICLE, not out of the godown
     |
     v  end of day
Dealer         counts what is left on the vehicle
               expected closing = loaded - sold - damaged + returned
               variance = goods that left without a bill
     |
     v
route complete
```

Three rules make this work, and each one is enforced by the database as well
as the UI:

* **A load belongs to a route.** `VehicleStock` is keyed on
  `(DealerId, VehicleId, **RouteScheduleId**, StockDate, ProductVariantId)`, so
  two routes running on the same day never share one bag of stock. Stock is
  loaded per route, not merely per vehicle.
* **One vehicle to one route, if free.** A filtered unique index
  `UQ_RouteSchedules_Vehicle_Weekday` makes it impossible for one vehicle to be
  on two active routes on the same weekday, and `AddRouteSchedule.aspx`
  explains it: *"That vehicle is already assigned to another route on Monday.
  Choose a free vehicle, or a different day."*
* **A village may sit on several routes** — that is allowed and intended — but
  never twice on the *same* route (`UQ_RouteVillages_Route_Village`).

Because the sale is taken off the vehicle, the godown is reduced **once**, at
load time, and never again when the salesman bills. That is what makes the
end-of-day check meaningful.

### Company stock receipts and inventory (PART 12)
`Dealer\CompanyReceipts.aspx` lists receipts (search, status filter, Post
action). `Dealer\AddCompanyReceipt.aspx` is one form for draft and post, with a
line editor. Posting happens in a **single transaction**: header, details,
`Inventory` increase and the `StockTransactions` ledger row all succeed or all
roll back. A **Posted receipt is terminal** — the `UPDATE ... WHERE Status =
'Draft'` must affect exactly one row, so a double click or a double post cannot
move stock twice. Ledger entries use the `Packet` (`PKT`) unit because stock is
counted in base packets.

### Vehicle loading (PART 12/14)
`Dealer\VehicleLoading.aspx` moves goods godown → vehicle **for a named route**.
Pick the route first and its vehicle follows. In **one transaction** it reduces
`Inventory`, upserts that route's `VehicleStock` row, and writes a `Stock Out`
ledger entry with reference `LOAD-<vehicle>-<date>`. The godown read uses
`UPDLOCK, ROWLOCK` and loading more stock than exists is rejected. A crafted
request cannot load one route's stock onto another route's vehicle.

### Orders (PART 13)
Three **order sources** — `Beat` (salesman, during a shop visit),
`Telephone` (dealer, for next day) and `Counter` (at the warehouse) — stored in
`Orders.OrderSource`, independent of `OrderType`. Server-side **status
transition table**:

| From | May become |
|---|---|
| `Pending` | `Confirmed`, `Cancelled` |
| `Confirmed` | `Dispatched`, `Cancelled` |
| `Dispatched` | terminal for status (only a return may follow) |
| `Cancelled` | terminal |

Illegal transitions are rejected by name, e.g. *"A Pending order cannot go
straight to Dispatched. Confirm it first."* Line items can only be edited while
`Pending` or `Confirmed`.

**Stock is not deducted when a draft order is created**, and a dealer's order
only moves the godown when it becomes `Dispatched`.

A **salesman's beat order is a real sale**: the goods came off the vehicle when
it was loaded, so when the bill is made they are issued from the **vehicle** for
that route and day:

```
available = loaded - sold - damaged + returned
```

The sale increments `VehicleStock.SoldPackets` under `UPDLOCK, ROWLOCK` and
writes a `Vehicle Sale` ledger row. If the vehicle does not carry enough, the
**whole bill is rejected and rolled back** — *"Not enough stock on the vehicle
for Masala Twist – 100 g Packet: only 2 packet(s) on board, this bill needs 5.
The rest of the bill was not saved."* A route with no vehicle assigned cannot
bill at all, and stock that was never loaded cannot be sold.

The godown is **not** touched again at sale time: it was reduced once, at load
time, so nothing is counted twice.

`Salesman\Bill.aspx?orderId=N` is the printable bill the shop is handed:
dealer header, shop and address, route/vehicle/salesman, the line items, and
the totals. It is read-only and scoped to `SalesmanId = Session["UserId"]`, so a
bill link cannot be re-pointed at somebody else's order.

Line items keep a **price snapshot** (`OrderDetails.UnitPrice`), so editing a
product price later never changes historical orders.

### Dispatches (PART 13)
`Dealer\AddDispatch.aspx` picks several confirmed orders, and in one transaction
creates the `Dispatches`/`DispatchDetails` rows, flips each order to
`Dispatched`, and deducts the godown **once per variant** — validating
sufficiency first. `Dispatches.aspx` and `DispatchDetails.aspx` list and show
them.

### Returns (PART 13/14)
`Dealer\AddReturn.aspx` is pre-filled from a dispatched order's lines. Each line
carries a **Good / Damaged** condition:

* **Good** → the godown is increased and a `Stock In` ledger row is written.
* **Damaged** → the godown is *not* increased (damaged goods are not sellable
  stock); the loss is recorded as a `Damage` ledger entry.

Returning more packets than were dispatched is rejected per line. The order
itself stays `Dispatched` — a return is an additive record, not a status change.

### Vehicle reconciliation (PART 14)
`AddReconciliation.aspx` closes out a route's day. Pick the route and its
vehicle follows. Expected closing is

```
loaded - sold (what the salesman actually billed) - damaged + returned
```

and the physical count is entered per variant, so **variance = goods that left
the vehicle without a bill**. Expected is recomputed inside the save
transaction; a route with no vehicle says so rather than showing a misleading
empty table. Approval records `ApprovedBy`/`ApprovedAt`. A balance is **never
silently overwritten** to make the count agree.

### Reporting (PART 15)
Role-scoped report sets for Admin, Dealer and Salesman, every figure a real
parameterized aggregate over transactional data: stock summary, low stock,
orders by status, orders by **source**, sales by product, by salesman, by route
and village, by shop, stock movement, company receipt history, vehicle
reconciliation and discrepancies, returns and damaged goods. Every report has a
**CSV export** (`Data\CsvExport.cs`) that streams exactly the rows the grid is
showing, as `text/csv` with an `attachment` filename, RFC 4180 quoting and a
UTF-8 BOM so Excel opens it correctly.

### Offline-first salesman workflow (PART 16) — implemented

A salesman working out of coverage is not blocked by the network, and nothing
is ever pretended to be accepted before the server says so.

* **Capture** — `Salesman\CreateOrder.aspx` has a **Save Offline** button. It
  never posts back; it writes the draft straight into **IndexedDB**
  (`Scripts\OfflineSync.js`) with a client-generated operation id.
* **Honest states** — every queued order is `pending`, `syncing`, `synced` or
  `failed`. The UI says explicitly *"This order has NOT reached the server
  yet."* The Save Order button warns that it will not reach the server when
  offline and points the salesman at Save Offline instead.
* **Automatic upload** — the queue is pushed as soon as `navigator.onLine`
  turns true, and on demand from `Salesman\OfflineQueue.aspx`, which shows the
  counts, the reason for every rejection, and offers **Retry** or **Delete**.
* **Idempotent** — the browser's operation id is stored in `Orders.ClientOrderId`
  behind a UNIQUE index. Uploading the same order twice returns the original
  order as a `duplicate`; it never creates a second one and never deducts
  stock twice.
* **Server-validated** — `Handlers\OrderSync.ashx` re-checks everything:
  the caller must be a logged-in **salesman**, the shop must still be active
  and still on one of *his* routes, the product variant and unit must still be
  active, the route day must not be closed, the vehicle must still carry the
  stock, and the price must still be the current one.
* **Explicit failures** — a rejected order keeps its reason and is never
  silently dropped:

  | Code | What the salesman is told |
  |---|---|
  | `PRICE_CHANGED` | "The price of *X* changed to *Y* while you were offline." |
  | `INSUFFICIENT_STOCK` | "Not enough on the vehicle for *X*: only *n* packet(s) left, this bill needs *m*." |
  | `SHOP_INACTIVE` | "That shop has been deactivated." |
  | `ROUTE_CLOSED` | "The dealer has closed this route day." |
  | `NO_VEHICLE` | "That route has no vehicle assigned." |
  | `NOT_AUTHORIZED` | "That route is no longer assigned to you." |
  | `PRODUCT_UNAVAILABLE` | "A product on this bill is no longer available." |

  Each order is processed in its own transaction, so one bad order never blocks
  the rest of the queue.

* **Stock is still the vehicle's** — a synchronised order issues from
  `VehicleStock` exactly as an online one does, so the end-of-day count stays
  meaningful.

### Price history (PART 11)

`ProductPrices` carries `EffectiveFrom` and is now unique on
`(ProductOptionId, EffectiveFrom)`, so a unit option can hold a real price
history. Changing a price in `Admin\AddVariant.aspx` starts a **new dated row**
rather than overwriting the old one (amending the same day's row is still
allowed, so a typo can be corrected). Order and bill pricing resolves the row
**in force on the order date** via `OUTER APPLY … ORDER BY EffectiveFrom DESC`,
so a bill raised today uses today's price and a bill raised earlier still
matches what was charged. Order lines additionally keep their own price
snapshot.

### UI (PART 17)
Shared `DashboardMaster.master` with a role-aware sidebar (all 28 links resolve
to real pages; there are no `NavigateUrl="#"` placeholders). Page title and
breadcrumb area, compact desktop sidebar that collapses on small screens, and a
main content area. Styles are in separate `Content\*.css` files and behaviour in
separate `Scripts\*.js` — no inline styles or scripts. List pages have search,
filters, status badges, add/edit actions and confirmation on destructive
actions; forms have labels, ASP.NET validation controls, **server-side**
validation that preserves input on failure, Save and Cancel, and meaningful
success/error feedback.

---

## 8. Database design

32 tables, 3 reporting views, 39 foreign keys, 8 check constraints, 280 indexes.

* **Security/organisation** — `Users` (with `PasswordHash`, `Role`,
  `DealerId`), `AuditLog`.
* **Routes** — `Routes`, `Villages`, `RouteSchedules`, `RouteVillages`,
  `RouteVillageSchedules`, `WeeklyRoutePlans`, `WeeklyRoutePlanVillages`.
* **Shops** — `Shops`.
* **Fleet** — `Vehicles`, `VehicleStock`.
* **Catalogue** — `ProductCategories`, `Products`, `ProductVariants`,
  `SellingUnits`, `ProductUnitOptions`, `ProductPrices`.
* **Inventory** — `Inventory` (balance, unique on `(DealerId,
  ProductVariantId)`), `StockTransactions` + `StockTransactionDetails` (the
  ledger).
* **Receipts** — `CompanyStockReceipts` + `CompanyStockReceiptDetails`.
* **Orders** — `Orders` + `OrderDetails`, `Dispatches` + `DispatchDetails`,
  `SalesReturns` + `SalesReturnDetails`.
* **Reconciliation** — `VehicleReconciliations` +
  `VehicleReconciliationDetails`.
* **Other** — `Expenses`.

Money is `DECIMAL(18,2)` end to end — never floating point. Every master has
`IsActive` and audit columns (`CreatedBy`, `CreatedAt`, `UpdatedBy`,
`UpdatedAt`). Deactivation never deletes history. Cascading deletes that would
destroy financial or operational records are avoided; the few cascades that do
exist only remove a dependent **detail** row of a header the user is deleting.

### Transactions and concurrency

Every multi-table write runs inside a single `SqlTransaction` with
`try { … tx.Commit(); } catch { tx.Rollback(); throw; }`: posting a company
receipt, loading a vehicle, confirming/fulfilling an order, dispatching,
recording a return, adjusting stock and approving a reconciliation.

Stock is read with `WITH (UPDLOCK, ROWLOCK)` and written with a **conditional
update** — `UPDATE … SET QuantityPackets = QuantityPackets + @Delta … WHERE
QuantityPackets + @Delta >= 0` — so two simultaneous requests cannot both pass a
sufficiency check and both spend the same stock. When the guard affects zero
rows the code re-reads under lock and either inserts the missing row (retrying
once if it races a unique key) or throws the "not enough stock" error.

---

## 9. Security

* PBKDF2 (salted, 100 000 iterations) password hashing via `PasswordHelper`.
* Session-based authentication; `Session_End` is not needed — an expired session
  simply leaves `Session["UserId"]` null and the existing guard redirects.
* Generic login failure message; user enumeration is not possible.
* `no-cache`, `no-store` headers on every protected page.
* Every SQL statement is parameterized; every dealer-owned query is scoped by
  `DealerId`; every salesman query by `SalesmanId`.
* Output is encoded; dynamic text outside a bound control goes through
  `Server.HtmlEncode`.
* `customErrors` + an IIS `httpErrors` safety net always answer 404/500 with
  `Error.aspx`, which never reveals an exception, stack trace, connection string
  or record data in any mode. `Web.config` comments explain that
  `mode="RemoteOnly"` intentionally leaves the detailed page for localhost
  developers.
* Unhandled exceptions are appended to `App_Data\AppLog.txt` with the UTC
  timestamp, URL, user and stack trace — and never with passwords or secrets.
* HTTPS-ready: `httpOnlyCookies` is on, `requireSSL` is off only because local
  development runs plain http, and **`Web.Release.config` flips `requireSSL` to
  `true`**, so a deployed release is HTTPS-only without breaking localhost.
* No database password or secret is hard-coded; LocalDB uses integrated
  security.

---

## 10. Known limitations

1. **`Services\` and `Models\` folders** are not used — see §6. The
   specification allows refining the organisation.
2. **Export is CSV only.** Excel and PDF export were not added because the
   requirement is conditional on correctness; one correct export is better than
   three unreliable ones.
3. **The sidebar is frozen.** `Master\DashboardMaster.master` may only receive
   URL-only edits to existing `lnk*` controls, so the modules added after the
   sidebar was built (Company Receipts, Vehicle Loading, Dispatches, Returns,
   Shop Orders) are reachable from the **Dealer dashboard quick-action bar**,
   and the Salesman pages including **Offline Queue** from the order screens.
4. **Drivers are staff records**, not a separate role or dashboard — permitted by
   the specification, but there is no driver-specific view.
5. **A route day closes per schedule, not per dated row.** `RouteSchedules` is a
   recurring schedule, so `StockDate` records the most recently closed day and
   the schedule stays closed until a reconciliation is un-approved. This suits
   the weekly-route model but does not keep a separate open/closed flag for each
   historical date.
6. **Offline sync needs a service worker to be fully unattended.** The queue
   survives a browser restart and uploads on the `online` event or on demand,
   but it does not sync while the tab is closed.
7. **No automated test for a concurrent stock race.** Locking is verified by
   code inspection and terminal-state guards — see `TESTING.md`.

---

## 11. Testing

See **[TESTING.md](TESTING.md)** for the full checklist and the actual results,
including the tests that were executed, those that were not, and why.

Summary of the automated runs that gate this build:

| Suite | Scope | Result |
|---|---|---|
| Build | `MSBuild /t:Rebuild` | exit 0 |
| Page regression | 60 pages × 3 roles = 180 requests | 174 pass / 0 fail |
| Unhandled exceptions | `App_Data\AppLog.txt` | none logged |
| Route-day flow | load → sell → bill → count | 23 / 23 |
| Offline sync + route closure + price history | 22 / 22 |
| Company receipt / dispatch / returns / cancel / CSV | 25 / 25 |
| Company receipt E2E | 9 / 9 |

---

## 12. Deployment (Windows IIS)

1. **Build** the solution in Release.
2. **Transform** `Web.config` → `Web.Release.config` (MSBuild does this for a
   publish; `debug` becomes `false` and `requireSSL` becomes `true`).
3. **Copy** the application folder to the IIS site root, keeping `App_Data\`.
4. **Database**: on the server, either
   * keep the file-attached model and make sure the app pool identity can read
     and write `App_Data\*.mdf`, or
   * point `Initial Catalog=BWDMS` at a real database, create it first by running
     `00_CreateDatabase.sql` with its `@CreateNamedDb` flag set to `1`, then run
     `01` and `02` through `Deploy-Database.ps1 -ConnectionString "<yours>"`.
5. **Application pool**: .NET CLR **v4.0**, **Integrated** pipeline mode,
   **Load User Profile = True** (LocalDB needs it).
6. **HTTPS**: bind a certificate on the site and keep `requireSSL="true"` from
   the release transform.
7. **Brand the first administrator** through `Account\CreateAdmin.aspx`, then
   delete or disable that page's reachability, and change every demo password.
8. **Logging**: `App_Data\AppLog.txt` grows with unhandled exceptions; wire it
   into your monitoring or rotate it with a scheduled task.

---

## 13. Troubleshooting

| Symptom | Cause / fix |
|---|---|
| *Cannot open database … because it does not exist* | You ran `USE BWDMS`. There is no database by that name — the MDF is attached by path. Run `Deploy-Database.ps1`. |
| You are logged out after every build | Expected: recompiling restarts the app and clears `InProc` session. Log in again. |
| `GET_LOCK` / file-in-use errors from `sqlcmd` | Stop IIS Express, or run the query while the app is not attached. |
| Port 8099 already in use | Change `/port:` in the `iisexpress.exe` argument and the URLs. |
| A blank page / detailed error locally | `customErrors mode="RemoteOnly"` shows the developer page on localhost. From another machine you get `Error.aspx`. |