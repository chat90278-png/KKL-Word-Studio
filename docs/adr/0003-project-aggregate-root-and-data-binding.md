# ADR 0003: Project as Aggregate Root, DataSource Hierarchy, and First-Class Binding

## Status
Accepted

## Context
Sprint 1.5 was a review pass (not a rewrite) prompted by five questions:
aggregate root correctness, domain-model completeness for the
Excel→Word workflow, whether ReportTable↔DataSource binding was actually
modeled, whether a Workspace concept was needed, and whether Persistence
needed to follow any Domain changes. Each is addressed independently below;
several raised alternatives were deliberately rejected.

## Decisions

### 1. Aggregate root: Project, not Report
`Report` previously owned `DataSources` directly. This was a boundary
error: a data source is imported independently of any particular report
design, and a single project routinely needs multiple report designs over
the same data (e.g. a "Summary" and a "Detailed" report from one workbook).
`Project` is now the aggregate root, owning `DataSources`, `Reports`, and
`Settings`. `Report`'s internal structure (Pages/Sections/Elements/visitor)
is unchanged — only what sits above it changed.

**Consequence:** `.kws` now serializes `Project` (`project.json`), and
`IReportProjectService` was renamed `IProjectService` returning `Project`.

### 2. DataSource hierarchy added to Domain
Added `DataSource` (abstract, implements existing `IDataSourceDefinition`),
`ExcelDataSource`, `Workbook`, `Worksheet`, `DataRange`, `ColumnMapping`,
and `ProjectSettings`. `ColumnMapping` lives at the DataSource level
(Excel column ↔ logical `DataField`) rather than being duplicated on every
report element — report elements bind to logical field names via the
existing `Expression` mechanism, so this mapping only needs to exist once.

**Rejected additions:**
- **Heading** — indistinguishable from a `TextElement` placed in a
  `ReportHeader`/`PageHeader` Section; a dedicated class would duplicate
  `ReportElement` without adding an invariant.
- **Template** — `Report` is already a reusable, serializable unit, and a
  `Project` can already hold several. A separate `Template` type today
  would just be a copy of `Report` with no new behavior; revisit only if
  a real need (parameterized template gallery) appears.
- **Renaming TableElement to ReportTable** — same concept, existing type;
  renaming would only churn code/tests without architectural benefit. The
  actual gap (binding capability) is addressed below instead.

### 3. First-class `Binding` type
`TableElement` had no way to declare "this table reads from DataSource X"
at all; `DataRegion` had only a bare `string? DataSourceName`. Added
`DataBinding.Binding` (currently just `DataSourceName`, structured so
filter/sort can be added later without touching every bindable element)
and applied it to both `TableElement` and `DataRegion`, replacing
`DataRegion`'s bare string. Per-cell data population continues to use the
existing `Expression` type — unchanged, no duplicate mechanism introduced.

### 4. Workspace added to Application (not Domain, not a new project)
Active project/report, selected worksheet, selected table, and similar
runtime state carry no business invariant and must not live in Domain.
They are also cross-cutting across several planned panels (Data Sources,
Outline Explorer, Property Inspector, Design Surface), so they don't belong
solely inside one ViewModel either. Added `Application.Workspace.IWorkspace`
— plain `.NET` events only, no UI-framework dependency, so Application
stays framework-agnostic. `Rendering.ISelectionService` (canvas-level
selection) is unaffected and remains separate; the UI composition layer is
responsible for keeping the two in sync, so Rendering still never
references Application (ADR 0002 boundary holds).

### 5. Persistence updated to match
`KwsProjectRepository` now implements `IProjectService` and writes
`project.json` instead of `report.json`. This is a direct, unavoidable
consequence of Decision 1, not an independent design change — the zip
container technique itself is unchanged.

## Consequences
- Domain and Rendering purity re-verified after the change: no reference
  to `IReportExporter`/`IPluginModule`/`IWorkspace`/`IProjectService`
  anywhere under Domain; no reference to pagination/execution concepts
  anywhere under Rendering (see Sprint 1.5 verification sweep).
- All existing Sprint 1 element types, the visitor pattern, and the
  exporter/plugin abstractions from ADR 0002 are unchanged — this was a
  targeted correction, not a rewrite.
