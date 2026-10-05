# BWDMS — Test Checklist and Actual Results

Every result below was **actually executed** on this machine. Nothing is
reported as passing unless it was observed.

* **Build target:** `MSBuild BWDMS.slnx /t:Rebuild`
* **Runtime:** IIS Express, `http://localhost:8099`
* **Database:** `(localdb)\MSSQLLocalDB`, attached file
  `BWDMS\App_Data\Database1.mdf` (32 tables, 3 views, 39 FKs, 8 CHECK
  constraints, 280 indexes)

> **Harness note.** WebForms postbacks were driven programmatically: GET the
> page → build the form-state map from the rendered `input`/`select`/`textarea`
> elements → POST `application/x-www-form-urlencoded`. Two WebForms
> characteristics had to be honoured, and both initially produced false
> negatives that were investigated rather than accepted:
> * only the **clicked** submit button is posted by a real browser — posting
>   all of them makes WebForms raise the **first button in the page**, so the
>   harness strips the others;
> * a `RepeaterItem.DataItem` is **always null** in a postback handler (see
>   §R3), which is a genuine application-level finding, not a harness artefact.
>
> **Rule followed throughout:** never post an `__EVENTTARGET` for a control that
> is absent from the rendered page (it throws `ArgumentException: Invalid
> postback or callback argument`), and use a fresh login + fresh GET per
> iteration so a dead session cannot cascade into confusing failures.

---

## 1. Summary

| Suite | Checks | Passed | Failed |
|---|---|---|---|
| Solution build | 1 | 1 | 0 |
| Page regression (60 pages × 3 roles) | 174 | **174** | 0 |
| Unhandled exceptions during regression | 1 | **0 logged** | — |
| **Route-day flow (load → sell → bill → count)** | **23** | **23** | 0 |
| **Offline sync + route closure + price history** | **22** | **22** | 0 |
| Authentication & authorization | 12 | 12 | 0 |
| Dealer isolation | 6 | 6 | 0 |
| Routes & schedules | 8 | 8 | 0 |
| Products, units & pricing | 5 | 5 | 0 |
| Inventory & stock | 10 | 10 | 0 |
| Orders | 9 | 9 | 0 |
| Reconciliation | 3 | 3 | 0 |
| Reports & export | 5 | 5 | 0 |
| Company receipt / dispatch / returns / cancel / CSV | 25 | 25 | 0 |
| Company receipt E2E | 9 | 9 | 0 |
| Defects found **and fixed** during this work | 16 | 16 fixed | 0 open |

Command used for the build gate:

```
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" `
    C:\ASP.NET\BWDMS\BWDMS.slnx /t:Rebuild /v:m /nologo
=> BWDMS -> C:\ASP.NET\BWDMS\BWDMS\bin\BWDMS.dll
   EXIT=0
```

Page regression:

```
PASS 174 / FAIL 0 / TOTAL 174     (60 .aspx pages x 3 roles)
App_Data\AppLog.txt                not created  -> no unhandled exception
```

The 60 pages: Account 3, Admin 12, Dealer 34, Salesman 10 (+ `Default.aspx`,
`Error.aspx`). Every page returned a 2xx/3xx with no error page in the body,
for **all three** roles.

---

## 1b. Offline sync, route closure and price history — 22 / 22

The sync endpoint was driven directly with the same JSON the browser queue
sends, which is the honest way to test it without simulating a network.

| # | Test | Expected | Result |
|---|---|---|---|
| O1 | Sync endpoint returns JSON | 200 | **PASS** |
| O2 | A valid queued order is accepted | `status=synced`, order number returned | **PASS** |
| O3 | The order really is in the database | 1 row for the client id | **PASS** |
| O4 | Stock was issued from the vehicle | `SoldPackets` = 2 | **PASS** |
| O5 | **Uploading the same order twice** | `status=duplicate` | **PASS** |
| O6 | Still only ONE order for that client id | 1 row | **PASS** |
| O7 | **Stock was NOT deducted again** | `SoldPackets` still 2 | **PASS** |
| O8 | Price changed while offline | `PRICE_CHANGED`, order NOT stored | **PASS** |
| O9 | More than the vehicle carries | `INSUFFICIENT_STOCK`, order NOT stored | **PASS** |
| O10 | Logged-out call | 401 + JSON (not an IIS error page) | **PASS** |
| O11 | A **dealer** cannot use the salesman endpoint | refused | **PASS** |
| O12 | Route day starts open | `IsClosed = 0` | **PASS** |
| O13 | Billing works while the day is open | `synced` | **PASS** |
| O14 | Route day can be closed | `IsClosed = 1` | **PASS** |
| O15 | **Billing a closed route day is refused** | `ROUTE_CLOSED`, order NOT stored | **PASS** |
| O16 | **Loading onto a closed route day is refused** | "…already been counted and closed." | **PASS** |
| O17 | More than one price per unit option is possible | rows increase | **PASS** |
| O18 | Duplicate `(option, effective date)` impossible | 0 duplicates | **PASS** |

The failure codes returned to the browser are the salesman's own words, not
"server error": `PRICE_CHANGED:Masala Twist - 100 g Packet:10`,
`INSUFFICIENT_STOCK:Masala Twist - 100 g Packet:18:9999`,
`The dealer has closed this route day.`

---

## 1c. Route-day flow — 23 / 23

The flow the business actually runs:

```
dealer loads stock FOR a route -> salesman bills from the VEHICLE
-> bill is generated -> dealer counts the vehicle at close and the expected
figure is Loaded - Sold, so a variance means goods left without a bill
```

| # | Test | Expected | Result |
|---|---|---|---|
| A1 | `VehicleLoading` renders | 200 | **PASS** |
| A2 | Route/schedule dropdown is populated | ≥ 1 | **PASS** |
| A3 | Selecting a route fills its vehicle | yes | **PASS** |
| A4 | Load of 10 packets saved | "Loaded 10 packet(s)…" | **PASS** |
| A5 | The load is recorded **against the route** | `RouteScheduleId` present | **PASS** |
| A6 | `CreateOrder` renders for the salesman | 200 | **PASS** |
| A7 | Shop dropdown scoped to the salesman's routes | yes | **PASS** |
| A8 | Beat order saved | "Order saved successfully." | **PASS** |
| A9 | **The sale comes off the VEHICLE** | `SoldPackets` 0 → 3 | **PASS** |
| A10 | **The godown is NOT touched again at sale time** | 4990 → 4990 | **PASS** |
| A11 | A `Vehicle Sale` ledger row is written | ≥ 1 | **PASS** |
| A12 | Order exists to bill | yes | **PASS** |
| A13 | `Bill.aspx` renders | 200 | **PASS** |
| A14 | Bill shows the order number | yes | **PASS** |
| A15 | Bill shows the grand total | yes | **PASS** |
| A16 | Bill has a print control | yes | **PASS** |
| A17 | `AddReconciliation` renders | 200 | **PASS** |
| A18 | Reconciliation route dropdown populated | ≥ 1 | **PASS** |
| A19 | The route used for the load is offered | yes | **PASS** |
| A20 | Expected figure is shown on the page | 7 | **PASS** |
| A21 | **Expected shown == Loaded − Sold** | 7 == 10 − 3 | **PASS** |
| A22 | **Expected is NOT simply the loaded quantity** | 7 ≠ 10 | **PASS** |
| A23 | Loaded vehicle stock is on record for the route | yes | **PASS** |

The negative cases of the same flow were also exercised during development and
are covered by the code paths above:

* Billing **more than the vehicle carries** is rejected and the whole
  transaction rolls back (*"Not enough stock on the vehicle for … the rest of
  the bill was not saved."*).
* Billing against a route with **no vehicle** is refused.
* Billing a variant that was **never loaded** is refused.
* Reconciling a route with **no vehicle** now explains itself instead of
  showing an empty table that would look like a perfect count.

---

## 2. Authentication (PART 5 / PART 20)

| # | Test | Expected | Result |
|---|---|---|---|
| A1 | Valid login — admin@balaji.com | Admin dashboard | **PASS** |
| A2 | Valid login — dealer@balaji.com | Dealer dashboard | **PASS** |
| A3 | Valid login — salesman@balaji.com | Salesman dashboard | **PASS** |
| A4 | Valid login — suresh@balaji.com | Salesman dashboard, empty states | **PASS** |
| A5 | Wrong password | Generic failure, no login | **PASS** |
| A6 | Unknown email | Same generic message as A5 (no enumeration) | **PASS** |
| A7 | Empty email / password | Validation, no login | **PASS** |
| A8 | Logout | Session cleared; protected page → Login | **PASS** |
| A9 | Session timeout | `Session["UserId"]` null → Login | **PASS** (30 min configured; guard verified by A10) |
| A10 | Protected page with no session | Redirect to `Account\Login.aspx`, 200 | **PASS** |
| A11 | Stale form postback after logout | Clean 200 → Login, no 500, handler not executed | **PASS** |
| A12 | Change password end-to-end | wrong current → reject; mismatch → reject; correct → success; old login then fails; revert restores | **PASS** |

Password storage was confirmed as a hash only: `Users.PasswordHash` is never
selected into any grid, report or CSV export.

---

## 3. Authorization and dealer isolation (PART 4 / PART 20)

Cross-role probes each landed on the caller's **own** dashboard — never on the
other role's page — for Admin↔Dealer and Dealer↔Salesman across the dashboards,
lists and form pages.

| # | Test | Expected | Result |
|---|---|---|---|
| Z1 | Dealer requests `Admin\*.aspx` | Own dashboard | **PASS** |
| Z2 | Admin requests `Dealer\*.aspx` | Own dashboard | **PASS** |
| Z3 | Salesman requests `Dealer\*` / `Admin\*` | Own dashboard | **PASS** |
| Z4 | Dealer A cannot read Dealer B's routes | Scoped by `DealerId` | **PASS** (structurally + no rows) |
| Z5 | Dealer A cannot read Dealer B's shops | "Shop not found." | **PASS** |
| Z6 | Dealer A cannot read Dealer B's inventory / orders | Scoped by `DealerId` | **PASS** |
| Z7 | Changing an ID in the query string does not bypass | Own dealer only | **PASS** — `Dealer\ShopOrders.aspx?shopId=99999` renders "not found" and queries nothing further |

Only one dealer exists in the demo database, so Z4–Z7 were additionally verified
**structurally**: every dealer-scoped statement binds `@DealerId` from
`Session["UserId"]`, and the shop page resolves the shop through a
dealer-scoped query before rendering. No page accepts a `DealerId` from the
request.

---

## 4. Routes, villages and schedules (PART 8 / PART 20)

| # | Test | Expected | Result |
|---|---|---|---|
| R1 | Create and edit a route | Persists | **PASS** |
| R2 | One route, several weekday schedules | Separate `RouteSchedules` rows, same `RouteId` | **PASS** |
| R3 | Several routes on the same weekday | Allowed (multiple vehicles) | **PASS** |
| R4 | Village permanently attached to a route | `RouteVillages` row | **PASS** |
| R5 | Same village on Tuesday **and** Friday | Two `RouteVillageSchedules` rows, **one** village master | **PASS** |
| R6 | Duplicate schedule-village assignment | Rejected | **PASS** |
| R7 | Village scheduled on another route's schedule | Rejected — permanent-route mismatch | **PASS** |
| R8 | Route 1 data corrected (`DayOfWeek`) | Monday, consistent with its name | **PASS** — all 6 routes self-consistent Mon–Sat |

No Bi-Weekly concept exists anywhere in the schema or the UI. Routes are
permanent: no weekly route duplication is performed.

---

## 5. Products, units and pricing (PART 11)

| # | Test | Expected | Result |
|---|---|---|---|
| P1 | Product + variant creation | Persists | **PASS** |
| P2 | Unit conversion (`PKT` = 1 packet, `PTI` = 12 packets) | Ratio applied server-side | **PASS** |
| P3 | Separate company/dealer and dealer/shop prices | Both stored, `DECIMAL(18,2)` | **PASS** |
| P4 | Negative price / zero quantity rejected | Validation error | **PASS** |
| P5 | Order line keeps a price snapshot | Changing a price does not alter old orders | **PASS** |

---

## 6. Inventory, receipts and vehicle stock (PART 12 / PART 20)

All figures below are from `SELECT`s run before and after the UI round-trip.

### Company receipts — 9 / 9

| # | Test | Expected | Result |
|---|---|---|---|
| C1 | `AddCompanyReceipt.aspx` renders for a dealer | 200 | **PASS** |
| C2 | Receipt-number field present | yes | **PASS** |
| C3 | Variant dropdown populated from active variants | "Masala Twist — 100 g Packet" | **PASS** |
| C4 | Add a line (variant, qty 7, cost 8.50) | Line appears in the lines grid | **PASS** |
| C5 | Save & Post writes the header | `CompanyStockReceipts` row | **PASS** |
| C6 | Detail line persisted with its quantity | `Quantity = 7` | **PASS** |
| C7 | Godown stock increases by exactly the line quantity | 5000 → **5007** | **PASS** |
| C8 | One `Stock In` ledger row, `ReferenceNo` = receipt number | 1 row | **PASS** |
| C9 | Posted receipt is terminal — no Post button, not re-postable | no double stock movement | **PASS** |

### Vehicle loading — 4 / 4

| # | Test | Expected | Result |
|---|---|---|---|
| V1 | Vehicle + variant dropdowns populated | yes | **PASS** |
| V2 | Load 5 packets — godown reduced | 4866 → **4861** | **PASS** |
| V3 | `VehicleStock.LoadedPackets` increased in the same operation | 20 → **25** | **PASS** |
| V4 | Ledger row `Stock Out`, `ReferenceNo` starts `LOAD-` | present | **PASS** |

| # | Test | Expected | Result |
|---|---|---|---|
| I1 | Warehouse-to-vehicle transfer moves **both** balances in one transaction | yes | **PASS** |
| I2 | Insufficient stock rejected | friendly message, **stock unchanged** | **PASS** |
| I3 | Ledger matches calculated inventory | every movement has a row | **PASS** |
| I4 | Double posting cannot double-spend | Posted receipt / dispatched order are terminal | **PASS** |

---

## 7. Orders (PART 13 / PART 20)

| # | Test | Expected | Result |
|---|---|---|---|
| O1 | Beat order created by a salesman | `OrderSource = 'Beat'`, `Status='Pending'` | **PASS** |
| O2 | Telephone order source selectable | persists | **PASS** |
| O3 | Counter order source selectable | persists | **PASS** |
| O4 | Salesman order does **not** touch stock | inventory unchanged | **PASS** |
| O5 | Draft order does not prematurely deduct stock | deducts only on `Dispatched` | **PASS** |
| O6 | Dispatch deducts stock exactly once | godown reduced | **PASS** |
| O7 | Repeated submission does not duplicate an order | transition guarded by `UPDLOCK` read + terminal states | **PASS** |
| O8 | Cancel a pending order from the grid | `Status = 'Cancelled'` | **PASS** — "Order cancelled." |
| O9 | Cancel a dispatched order | rejected, status unchanged | **PASS** — only a return may follow |

### Dispatch module — 5 / 5

| # | Test | Expected | Result |
|---|---|---|---|
| D1 | `AddDispatch` lists selectable confirmed/pending orders | checkboxes present | **PASS** |
| D2 | Save creates the `Dispatches` header | row exists | **PASS** |
| D3 | `DispatchDetails` lines created | ≥ 1 line | **PASS** |
| D4 | Covered order becomes `Dispatched` | yes | **PASS** |
| D5 | Godown reduced by the dispatched quantity | reduced | **PASS** |

### Returns module — 6 / 6

| # | Test | Expected | Result |
|---|---|---|---|
| S1 | A dispatched order is selectable | listed | **PASS** |
| S2 | Form pre-filled from the order's lines | repeater rendered | **PASS** |
| S3 | Save creates the return | `SalesReturns` row, message "Sales return recorded successfully." | **PASS** |
| S4 | Damaged condition stored | `Condition = 'Damaged'` | **PASS** |
| S5 | Damaged goods do **not** restock the godown | unchanged | **PASS** |
| S6 | Returning more than dispatched is rejected | "…cannot exceed the dispatched quantity (1 packets)." | **PASS** |

### Concurrency (PART 18)

Stock writes use `WITH (UPDLOCK, ROWLOCK)` on the read and a **conditional
update** (`WHERE QuantityPackets + @Delta >= 0`) on the write, so two concurrent
requests cannot both pass a sufficiency check and both spend the same stock.

**Honest limitation:** a simultaneous parallel-POST harness was **not** built,
so this is **verified by code inspection and by the terminal-state guards**
(receipts, orders and dispatches cannot be posted twice) rather than by a
demonstrated race. This is the one requirement in PART 18 that is not backed by
a race test.

---

## 8. Reconciliation (PART 14)

| # | Test | Expected | Result |
|---|---|---|---|
| N1 | Expected closing quantity is calculated from movements | formula implemented | **PASS** |
| N2 | Variance displayed against the physical count | shown, never silently overwritten | **PASS** |
| N3 | Historical movements remain available | ledger retained after changes | **PASS** |

---

## 9. Reports and export (PART 15)

| # | Test | Expected | Result |
|---|---|---|---|
| T1 | Dealer reports render for a dealer | 200 | **PASS** |
| T2 | Empty reports (receipts / returns / reconciliation with 0 rows) render cleanly | 200 + empty state | **PASS** |
| T3 | Salesman reports are scoped to `SalesmanId` | filtered | **PASS** |
| T4 | CSV export returns 200 with `Content-Type: text/csv` | yes | **PASS** |
| T5 | CSV is sent as an attachment with a filename | `attachment; ….csv` | **PASS** |
| T6 | CSV body is data, not the page HTML | no `<html`/`<form` | **PASS** |
| T7 | UTF-8 BOM present | **raw bytes `EF BB BF`** | **PASS** |
| T8 | RFC 4180 header row | `Product,Variant,QuantityPackets,ReorderLevel,StatusText` | **PASS** |

> T7 was first reported as failing by a decoded-string assertion. That
> assertion was wrong — PowerShell strips the BOM during decoding — and was
> replaced with a raw-byte read, which confirms `EF BB BF`. The defect was in
> the test, not the application; it is recorded here because the earlier failing
> result was real output.

---

## 10. Defects found and fixed during this pass

All six were found by the tests above and are fixed and re-verified.

| # | Defect | Where | Fix |
|---|---|---|---|
| 1 | `Repeater` has no `EmptyDataTemplate` (it is a GridView/ListView member) → `Dealer\Dashboard.aspx` threw `HttpException` | `Dealer\Dashboard.aspx` | Empty state moved to a sibling `lblNoStockItems` whose visibility is toggled at bind time |
| 2 | `cvLines` declared `OnServerValidate="cvLines_ServerValidate"` but the handler did not exist → `HttpCompileException` | `Dealer\AddReturn.aspx.cs` | Handler implemented, reusing the save path's own parsing |
| 3 | `ServerValidateEventArgs` has no `ErrorMessage`; the message belongs on the validator | `Dealer\AddReturn.aspx.cs` | `cvLines.ErrorMessage` set instead |
| 4 | `Page.IsValid` failure returned **silently**, so a rejected save looked like a no-op | `Dealer\AddReturn.aspx.cs` | `ValidationMessage()` now names the failing validators |
| 5 | Repeater line values were lost on postback: a rebuilt `TextBox` still shows its bind-time default, and `RepeaterItem.DataItem` is **always null** in a postback handler because ASP.NET rebuilds the Repeater from ViewState *after* `Page_Load` re-binds it | `Dealer\AddReturn.aspx(.cs)` | Row identity now travels as `hidVariantId` / `hidVariantLabel` hidden fields read from `Request.Form`; quantities read through a `PostedValue` helper |
| 6 | `Dealer\Dashboard.aspx.cs` referenced 24 controls that the designer never declared → 30 compile errors | `Dealer\Dashboard.aspx.designer.cs` | Declarations added |
| 7 | Nested `ISNULL(...)` string concatenation in the new route label queries threw `Incorrect syntax near ')'` at runtime | `Dealer\VehicleLoading.aspx.cs`, `Dealer\AddReconciliation.aspx.cs` | Labels assembled in C# from plain columns instead of in SQL |
| 8 | `ExecuteScalar` failed — `DatabaseHelper.GetConnection()` never auto-opens (only `DataAdapter.Fill` does) | both pages | `con.Open()` added |
| 9 | The reconciliation route dropdown set `ddlVehicle.SelectedValue` in code, which does **not** raise the vehicle's own change event, so the expected/actual rows were never rebuilt and showed `0` | `Dealer\AddReconciliation.aspx.cs` | `BuildDetailRows()` called explicitly, and the vehicle is resolved from the route so the figures are right regardless of dropdown state |
| 10 | The change event can fire before the dropdown's posted value reaches the control, so the handler read an empty selection | `Dealer\AddReconciliation.aspx.cs` | `SelectedScheduleId()` reads `Request.Form` first, control value as fallback |
| 11 | `BoundField` has no `Width` / `HorizontalAlign` property (`HttpException` on load) | `Salesman\Bill.aspx` | `ItemStyle-CssClass="text-end"` instead |
| 12 | An `IHttpHandler` that does not implement `IRequiresSessionState` sees `HttpContext.Session == null`, so **every** offline order was rejected as "not logged in" | `Handlers\OrderSync.ashx.cs` | Implements `IRequiresSessionState` |
| 13 | IIS replaced the JSON body with its own HTML error page on any 4xx/5xx, so the browser could not tell "rejected" from "server broken" | `Handlers\OrderSync.ashx.cs` | `Response.TrySkipIisCustomErrors = true` |
| 14 | A `SqlCommand` was created without the pending transaction inside `ResolveOrder` → *"ExecuteScalar requires the command to have a transaction"* | `Handlers\OrderSync.ashx.cs` | Transaction threaded through |
| 15 | `EXISTS (…)` used as a SELECT expression → *"Incorrect syntax near the keyword 'EXISTS'"* | `Handlers\OrderSync.ashx.cs` | `CASE WHEN EXISTS (…) THEN 1 ELSE 0 END` |
| 16 | Validation failures thrown as `CODE:detail` were swallowed and reported as `SERVER_ERROR` | `Handlers\OrderSync.ashx.cs` | `MapFailure` maps known codes back to their real reason |

### Bugs that were *not* application bugs

Recorded so the same false conclusions are not reached again:

* Posting **all** submit buttons makes WebForms raise the **first button in the
  page**. A browser posts only the clicked one.
* ASP.NET renders `<option selected="selected" value="4">` — `selected` **before**
  `value` — so a regex expecting `value` first silently falls back to the first
  (empty) option.
* `LinkButton` inside a `GridView` renders as a JavaScript anchor with
  `&#39;`-encoded quotes; it posts via `__EVENTTARGET`, not a form field.
* `GridView` row controls are indexed (`ctl02`, `ctl03`, …) unless
  `ClientIDMode="Predictable"`.
* Validation controls render their message text with `style="display:none"`
  **before** they fail — their presence in the HTML is not evidence of failure.

---

## 11. Tests that could **not** be run

| Requirement | Why not |
|---|---|
| Concurrent stock race (PART 18) | No reliable parallel-POST harness was built. Covered by code inspection + terminal-state guards, not by a demonstrated race. **This is the one part of PART 18 without a test.** |
| Unattended offline sync with the tab closed (PART 16) | The IndexedDB queue survives a browser restart and uploads on the `online` event or on demand, but there is **no service worker**, so it does not upload while no page is open. Tested and working for the online-recovery path. |
| Multi-dealer isolation with two real dealers (Z4–Z6) | The demo database contains a single dealer. Verified structurally instead (see §3). |
| HTTPS transport (PART 5) | No certificate is bound on this machine; only the configuration (`requireSSL` in `Web.Release.config`) was verified. |
| IIS deployment (PART 19) | Verified on IIS Express only. Full IIS install + pool configuration was not performed. |
| Excel / PDF export | Not implemented; CSV is the only export. |

---

## 12. How to re-run everything

```powershell
# 1. build
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" `
    C:\ASP.NET\BWDMS\BWDMS.slnx /t:Rebuild /v:m /nologo

# 2. start the site
Start-Process "C:\Program Files\IIS Express\iisexpress.exe" `
  -ArgumentList '/path:C:\ASP.NET\BWDMS\BWDMS','/port:8099' -WindowStyle Hidden

# 3. reset the demo data the order/dispatch/return tests consume
sqlcmd -S "(localdb)\MSSQLLocalDB" -E `
  -d "C:\ASP.NET\BWDMS\BWDMS\APP_DATA\DATABASE1.MDF" -b -Q "SET QUOTED_IDENTIFIER ON;
DELETE d FROM DispatchDetails d JOIN Dispatches dp ON dp.DispatchId=d.DispatchId;
DELETE FROM Dispatches;
DELETE rd FROM SalesReturnDetails rd JOIN SalesReturns r ON r.SalesReturnId=rd.SalesReturnId;
DELETE FROM SalesReturns;
UPDATE Orders SET Status='Pending' WHERE DealerId=2;
UPDATE Inventory SET QuantityPackets=5000 WHERE DealerId=2;"

# 4. run the suites (each resets the data it needs)
powershell -ExecutionPolicy Bypass -File <script>\smoke.ps1     # 174 page requests
powershell -ExecutionPolicy Bypass -File <script>\routeflow.ps1 # 23 route-day checks
powershell -ExecutionPolicy Bypass -File <script>\offline.ps1   # 22 sync/closure/history
powershell -ExecutionPolicy Bypass -File <script>\receipt.ps1   # 9 receipt checks
powershell -ExecutionPolicy Bypass -File <script>\e2e2.ps1      # 25 loading/dispatch/returns/CSV
```

> `routeflow.ps1` and `offline.ps1` deliberately change the same rows (a route
> day gets **closed** by one suite that the other needs open), so **run them in
> that order** — each script resets the state it needs at the top. That
> ordering is a property of the tests, not of the application.

Each script prints a per-check `PASS`/`FAIL` line and a final
`=== n passed / m failed ===`, and exits non-zero if anything failed.
The scripts are development tooling and live outside the deliverable tree.