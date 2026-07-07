# ADR 0004: Column Mapping Ownership, Binding Scope, Structured DataRange, and Workspace Refinement

## Status
Accepted

## Context
Sprint 2 built the first working Excel Workspace and, per the sprint's own
requirement, re-validated the domain model against real usage before
extending it. Five questions were evaluated independently.

## Decisions

### 1. ColumnMapping stays at DataSource level (not moved to Binding)
Considered: letting the same worksheet be mapped differently per report
table (e.g. column B → `CustomerName` in one table, → `ClientTitle` in
another). Rejected: this breaks single-source-of-truth for what a column
*means* — the same underlying data would carry two names depending on
which table happened to render it, which is inconsistent with how
established report designers (SSRS, Crystal, Power BI) model
datasets/fields, and would make debugging "why does this field show wrong
data" much harder. The actual need behind the suggestion — using the same
sheet differently per table — is a filtering/sorting need, not a
renaming need, and is addressed by Decision 2 instead.

### 2. Binding gains Filter and SortFields; explicitly does NOT gain Worksheet/DataRange/HeaderRow/ColumnMapping
Those four are already resolved once at the DataSource level
(`ExcelDataSource.ActiveWorksheetName`, `Worksheet.SelectedRange`,
`DataSource.ColumnMappings`). Adding them to `Binding` as well would create
two sources of truth that could silently disagree (e.g. Binding says
"Sheet1" while the DataSource it points to says "Sheet2"). Filter and Sort
are different in kind: they describe how *this particular element*
consumes the data source's rows, which legitimately varies per element.
Sort uses a structured `SortField` (field + direction) rather than a
generic `Expression`, since sorting is always resolvable to that concrete
shape and a structured type is safer for an exporter/engine to consume
than a string to parse.

### 3. DataRange became structured; RangeReference is now computed, not stored
The Sprint 1.5 version stored a single opaque A1-string. That cannot
represent "start row picked by user, end row auto-detected, then
manually corrected" without repeatedly re-parsing the string, and offers
no way to distinguish an auto-detected value from a manual override.
Replaced with explicit fields (`DataStartRow`, `DataEndRow`,
`HeaderRowIndex`, `StartColumn`/`EndColumn`, `WasAutoDetected`);
`RangeReference` is now a derived, read-only display string computed from
those fields, so it can never drift out of sync with them.

### 4. Report Explorer's Project → Reports → Heading/Table shape needs no Domain change
Headings and Tables in the pictured Explorer tree map directly to existing
`TextElement`/`TableElement` instances reachable via the existing
Section/Container tree and `IReportElementVisitor` — this is a UI
presentation concern (projecting the existing model), not a missing
Domain type. `Templates` remains rejected per ADR 0003. `Assets` (image/
resource catalog) is a genuine, currently-unmodeled gap, but it is out of
this sprint's scope (Excel/data-side hardening) — implementing it now
would be speculative ahead of the image/resource-management work that
actually needs it; it is deliberately deferred, not silently dropped.

### 5. Workspace: generalized selection, added data-source disambiguation, kept preview data out
- `SelectedTableElementId` → `SelectedReportElementId` (Guid?): the
  Property Inspector needs to react to selection of any element type, not
  only tables — Sprint 1.5's scope was narrower than real usage requires.
- Added `ActiveDataSourceName` alongside `SelectedWorksheetName`: with
  multiple Excel files now genuinely open at once, a worksheet name alone
  is ambiguous (two open files can both have a "Sheet1").
- Added `IsPreviewActive` as a lightweight flag only. The actual preview
  grid (potentially large) stays in `ExcelWorkspaceViewModel` (UI layer) —
  putting bulk, transient rendering data into the cross-cutting `Workspace`
  singleton would turn it into a god object other panels have no reason to
  carry the weight of.

## Consequences
- `IExcelWorkbookReader` (Application) / `OpenXmlExcelWorkbookReader`
  (Infrastructure) reuse the OpenXML SDK package already planned for Word
  export — no new dependency was introduced to read Excel files.
- `ColumnLetterConverter` moved to Shared so Domain's computed
  `RangeReference` and Infrastructure's sheet reader use the exact same
  column-letter conversion, rather than two independent implementations
  that could disagree.
- Domain/Rendering purity re-verified after these changes (see Sprint 2
  verification sweep) — no coupling introduced in either direction.
