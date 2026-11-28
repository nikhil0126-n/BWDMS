# Admin responsive code — one file per commit, every 30 minutes

The working project already contains the **final** responsive code. This folder
contains one file per commit so you can commit **one file every half hour** in
this order.

> **Order matters:** commit `02-AdminResponsive.css` before
> `03-DashboardMaster.master`, because the master page links to that new CSS
> file.

## Schedule

| Time | Commit | File in project | What this file adds | Commit message |
| --- | --- | --- | --- | --- |
| 00:00 | 1 | `Content/Dashboard.css` | Shared foundation: wide tables scroll sideways, flex/grid children can shrink, tablet table min-width | `fix(admin): make shared tables and layout responsive` |
| 00:30 | 2 | `Content/AdminResponsive.css` *(new)* | Phone/tablet rules: wrapping buttons, full-width search, stacked report dates, stacked headers, text wrapping, pager and detail-card fixes | `feat(admin): add phone and tablet responsive layout rules` |
| 01:00 | 3 | `Master/DashboardMaster.master` | Loads `AdminResponsive.css` after `Dashboard.css` for every Admin page | `chore(admin): load responsive stylesheet in master page` |
| 01:30 | 4 | `Scripts/Dashboard.js` | Admin Dashboard top-bar hamburger opens the mobile sidebar drawer | `fix(admin): wire dashboard topbar toggle to mobile drawer` |
| 02:00 | 5 | `Admin/AddVariant.aspx` | Unit-conversion table gets the `variant-grid` class so it scrolls on phones | `fix(admin): make variant matrix scroll on phones` |
| 02:30 | 6 | `Account/CreateAdmin.aspx` | Adds viewport meta tag so the admin setup page scales on phones | `fix(admin): add viewport meta to create-admin page` |

Each numbered folder holds the exact file for that commit. Copy it to the
matching project path (it is already there), stage **that one file only**, and
commit.

## Commit commands (git example)

```bash
# 00:00
git add Content/Dashboard.css
git commit -m "fix(admin): make shared tables and layout responsive"

# 00:30
git add Content/AdminResponsive.css
git commit -m "feat(admin): add phone and tablet responsive layout rules"

# 01:00
git add Master/DashboardMaster.master
git commit -m "chore(admin): load responsive stylesheet in master page"

# 01:30
git add Scripts/Dashboard.js
git commit -m "fix(admin): wire dashboard topbar toggle to mobile drawer"

# 02:00
git add Admin/AddVariant.aspx
git commit -m "fix(admin): make variant matrix scroll on phones"

# 02:30
git add Account/CreateAdmin.aspx
git commit -m "fix(admin): add viewport meta to create-admin page"
```

This project folder is not currently a git repo, so if you use another version
control system, just stage the same **single file** listed in that row.

## Quick check after each commit

Open an Admin page at about 375px wide:

- No horizontal page scrollbar; wide tables scroll inside the card.
- Header buttons and filters wrap onto new lines.
- Search fields use the full row.
- The hamburger opens the sidebar; backdrop / Esc closes it.

Desktop layout is unchanged.
