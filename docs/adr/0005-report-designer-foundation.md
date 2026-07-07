# ADR 0005: Report Designer Foundation, Section Flow Behavior, and Preview Abstraction

## Status
Accepted

## Context
Sprint 3 built the first working Report Designer, Project Explorer, Table
Properties/Binding panel, and a Preview foundation. Per the sprint's own
requirement, each proposal was evaluated against the existing model before
any change was made.

## Decisions

### 1. Project Explorer needed no Domain change
`Project.DataSources` → `ExcelDataSource.Workbook.Worksheets`,
`Project.Reports`, and `Project.Settings` already shape exactly the tree
the Explorer needs. Implemented as a pure UI projection
(`ProjectExplorerViewModel`) over the existing model. `Templates` is shown
as an empty placeholder node — visible in the tree's shape without
fabricating data for a feature that (per ADR 0003) doesn't exist yet.

### 2. Heading / Alt Heading remain TextElement — no new Domain types
Reaffirms ADR 0003. Both are created as `TextElement` with a different
`Style`, governed by a single shared convention
(`Application.Styling.HeadingStylePresets`) so the Report Designer (which
creates them) and the Preview renderer (which classifies them for display)
can never disagree about what counts as a heading. Placed in Application
rather than UI because a future `WordExporter` will need the exact same
convention to map a heading onto a Word "Heading 1" style.

### 3. Section gained `AutoHeight` (default true)
Classic banded report engines assume fixed-height sections; KKL Word
Studio's actual usage — authoring a flowing document skeleton — needs
sections that grow with their content by default. `Height` remains for the
rare fixed-size case (e.g. a footer that must be exactly 2cm). This was the
one genuine gap found when validating the Report Tree against real usage;
everything else (Pages → Sections → Container → Elements) already modeled
a document skeleton adequately.

### 4. Table Designer: Description added to TableElement only; "Show Header" needed no new field
`TableElement.Description` was added (not promoted to the `ReportElement`
base — YAGNI; promote only once a second element type needs it). "Show
Header" reuses the existing `TableRowKind.Header` row instead of adding a
redundant boolean: the UI checkbox adds/removes the header-kind row
directly, so there is exactly one place recording whether a header exists,
not two that could disagree.

### 5. Binding UI needed no Domain change — validates ADR 0004
Resolving "which Worksheet / which DataRange" for a bound table is a pure
lookup: `Binding.DataSourceName` → matching entry in `Project.DataSources`
→ (if `ExcelDataSource`) `ActiveWorksheetName` → matching `Worksheet` →
`SelectedRange.RangeReference`. Nothing needed to be added because ADR
0004 deliberately kept Worksheet/DataRange off `Binding` — this sprint is
the proof that decision was correct.

### 6. Preview: `IReportPreviewRenderer` introduced in Application
The Preview panel needed to be wired to Workspace changes today without
committing to the real Rendering/Engine pipeline's shape. An abstraction
(`Application.Preview.IReportPreviewRenderer`, returning a
`PreviewSnapshot`/`PreviewBlock` DTO) lets a trivial
`PlaceholderPreviewRenderer` (UI) satisfy it now; a future layout-aware
renderer replaces only that implementation, with zero change to the
Preview panel's binding code. Deliberately placed in Application, not
Rendering — Rendering's ADR 0002 scope (hit-test/selection/snap/zoom) was
not touched this sprint, confirmed by the same grep sweep used in prior
sprints.

### 7. Rendering boundary re-verified, untouched
No file under `KKL.WordStudio.Rendering` was modified this sprint. The
ADR 0002 grep sweep (no pagination/expression-evaluation/execution
references) was re-run and still passes.

### 8/9. Workspace: sufficient as-is; only a lookup helper was missing
The field set from ADR 0004 (ActiveProject/ActiveReport/
ActiveDataSourceName/SelectedWorksheetName/SelectedReportElementId/
IsPreviewActive) covers every panel added this sprint. What was missing
was not a Workspace field but a way to turn `SelectedReportElementId`
(a Guid) into an actual element — now that three consumers (Report
Designer tree, Table Properties, Preview) need it. Added
`Domain.Visitors.ReportElementFlattener`, built on the existing visitor
pattern, rather than three independent ad-hoc tree walks.

## Consequences
- `KwsProjectRepository.CreateNew()` now seeds a default Body `Section` so
  the Report Designer always has a valid insertion target — a small
  correctness fix surfaced by actually wiring up element insertion.
- Every "Add Heading/Alt Heading/Table" command funnels through one method
  (`ReportDesignerViewModel.InsertElement`), so a future drag-drop handler
  reuses it rather than requiring a second insertion path.
- MainWindow now hosts five panels simultaneously (Project Explorer, Excel
  Workspace, Report Designer, Table Properties, Preview) via a
  GridSplitter-based layout — deliberately not a docking framework yet,
  consistent with the Sprint 1 decision to defer that choice.
