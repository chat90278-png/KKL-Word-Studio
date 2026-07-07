# KKL Word Studio

Enterprise-grade WPF report designer (.NET 8, C#, MVVM, Clean Architecture).

## Solution layout

| Project | Depends on | Responsibility |
|---|---|---|
| `KKL.WordStudio.Shared` | — | Result type, guards, extensions, framework-independent geometry, theme key constants |
| `KKL.WordStudio.Domain` | Shared | Abstract report model: `Report`, `Page`, `Section`, element hierarchy, visitor pattern, styling. No I/O, no export, no UI. |
| `KKL.WordStudio.Application` | Domain, Shared | Use cases and extensibility contracts: `IReportExporter`, `IDataProvider`, plugin module system |
| `KKL.WordStudio.Infrastructure` | Application, Domain, Shared | `.kws` persistence, concrete exporters (Word/PDF/HTML/Image/Excel), concrete data providers |
| `KKL.WordStudio.Rendering` | Domain, Shared | Design-surface interaction only: hit-testing, selection, snapping, rulers, zoom. No layout/pagination/execution logic (that is a future `Engine` project — see ADR 0002). |
| `KKL.WordStudio.UI` | everything | WPF shell, ViewModels, composition root |

See `docs/adr/` for the reasoning behind these boundaries.

## Sprint 5 scope (this drop)
- **Real A4 preview**: correctly-proportioned page (from the Report's actual `Page` dimensions), zoom in/out/reset, distinct Header/Body/Footer regions, an optional Table of Contents — all sharing the exact same `ReportContentDocument` `WordExporter` consumes.
- **Real Word page layout**: `.docx` output now has real page size/margins (from Domain `Page`, converted to twips), real `HeaderPart`/`FooterPart` content, a `PAGE` field for page numbers, and a native, updatable `TOC` field.
- **Real heading styles**: `WordExporter` now builds a `StyleDefinitionsPart` with `Heading1`/`Heading2` (with `outlineLvl`) — required for Word's native TOC field to find headings; this was explicitly deferred in ADR 0006 and is now done because the TOC requirement forced it.
- `Report.IncludeTableOfContents` and `Page.ShowPageNumbers` (new, Domain) — TOC entries are *derived* from existing Heading/AltHeading content, never separately authored.
- Report Designer: "Add Header Text" / "Add Footer Text" commands (reusing the existing section-aware `InsertElement`), and a Table-of-Contents toggle.
- `IReportContentBuilder` restructured from a flat node list to `ReportContentDocument` (Header/Body/Footer/TOC/PageLayout regions) — a real gap found when evaluating whether the shared model could serve future PDF/HTML exporters.
- Fixed a real Preview/Export redundancy: `Workspace.ReportContentChanged` (new, narrower than `WorkspaceChanged`) means selecting a tree node no longer forces every bound table's Excel file to be re-read.
- Evaluated and explicitly deferred: `ReportContentBuilder` Strategy/Visitor split (not enough element-type growth yet) and `ExcelDataProvider` streaming (no observed large-dataset problem yet) — see ADR 0007.

## Sprint 4 scope
- **First real Word export.** `WordExporter` (Infrastructure) uses the OpenXML SDK to produce an actual `.docx` — headings, alt headings, paragraphs, and tables (bound or static).
- **Preview and Word Export now share one interpretation.** `Application.Content.IReportContentBuilder` walks the Report exactly once and both `PreviewRenderer` and `WordExporter` consume its output — neither re-decides what counts as a heading or what a bound table's rows are.
- **First real (non-in-memory) data provider.** `ExcelDataProvider` reads actual configured Excel ranges via OpenXML; `IDataProviderRegistry` added (mirrors `IReportExporterRegistry`) since a second provider now exists.
- `Binding.SortFields` is now genuinely applied (structured, no evaluator needed); `Binding.Filter` is intentionally not yet applied (needs an expression evaluator — future Engine work) and is surfaced via `TableContentNode.FilterWasIgnored` rather than silently dropped.
- `Workbook.SourcePath` added — exporting/reading real data requires the actual file location, which wasn't being stored before.
- `Export to Word` wired into the UI (File menu + toolbar), completing New → design → bind data → export end-to-end.

## Sprint 3 scope
- Project Explorer (Data Sources → Excel Files → Worksheets, Reports, Templates placeholder, Settings) — pure projection of the existing Project model, no Domain change
- First working Report Designer: tree view of the active Report, "Add Heading / Add Alt Heading / Add Table" commands funneled through one DnD-ready insertion method
- Table Properties panel: Name, Description, Style (Bold/FontSize), Show Header
- Binding UI: pick a DataSource, see its resolved Worksheet/DataRange live (no new Domain state — a pure lookup, validating ADR 0004)
- Preview foundation: `IReportPreviewRenderer` abstraction (Application) + placeholder implementation (UI), reacts live to `Workspace.WorkspaceChanged`
- `Section.AutoHeight` (default true) — sections flow with content instead of assuming fixed banded-report heights
- `TableElement.Description`
- `ReportElementFlattener` — shared visitor-based tree lookup used by the Designer, Table Properties, and Preview
- MainWindow redesigned: five panels open simultaneously (Project Explorer, Excel Workspace, Report Designer, Table Properties, Preview)
- `Save Project` wired into the UI

## Sprint 2 scope
- Working Excel Workspace: open multiple .xlsx files, list/switch sheets, preview grid with row/column headers, pick a start row, auto-detect data end with manual override, generate column mappings, save the resulting DataSource into the active Project
- `IExcelWorkbookReader` (Application) / `OpenXmlExcelWorkbookReader` (Infrastructure), built on the OpenXML SDK (no new dependency — reused the package already planned for Word export)
- `DataRange` restructured to explicit start/end row, header row, columns, and an auto-vs-manual provenance flag; `RangeReference` is now computed, never stored
- `Binding` extended with `Filter` (Expression) and structured `SortFields`
- `Workspace` refined: generalized `SelectedReportElementId`, added `ActiveDataSourceName`, added a lightweight `IsPreviewActive` flag
- Round-trip test that writes a real .xlsx with the OpenXML SDK and reads it back through the new reader

## Sprint 1.5 scope
- `Project` established as the aggregate root (was `Report`)
- `DataSource` hierarchy: `ExcelDataSource`, `Workbook`, `Worksheet`, `DataRange`, `ColumnMapping`
- First-class `Binding` type on `TableElement`/`DataRegion`
- `Workspace` (Application layer) for cross-panel session state
- `.kws` persistence updated to serialize `Project`

## Sprint 1 scope
- Full solution/project skeleton with correct reference directions
- Abstract report model (Domain) with visitor pattern
- Plugin-oriented extensibility contracts (Application)
- Native `.kws` persistence (zip + JSON), round-trip tested
- Strategy-based exporter registry with stub exporters (Word/PDF/HTML/Image/Excel)
- DI composition root using `Microsoft.Extensions.Hosting` + Serilog

## Not yet implemented
- PDF/HTML/Image/Excel export (still stubs — only Word is real; PDF explicitly out of scope for Sprint 5)
- True multi-page pagination (Preview renders one accurately-shaped, margined, headed/footed A4 page; splitting body content across multiple physical pages is future Engine work — see ADR 0002/0005/0007)
- `Binding.Filter` execution (needs an expression evaluator — future Engine work; surfaced via `FilterWasIgnored` rather than silently dropped)
- Embedded images in Word output (needs the Asset catalog — ADR 0004)
- `ExcelDataProvider` streaming/lazy enumeration for very large sheets (evaluated in Sprint 5, deliberately deferred — no observed problem yet, see ADR 0007)
- Drag-and-drop in the Report Designer (architected for it via `InsertElement`, not implemented)
- Filter/Sort editing UI for Binding (Sort now executes; UI to configure it is still deferred)
- Expression/formula engine, project-level Asset catalog (deliberately deferred, see ADR 0004)
- AvalonDock-based docking (current layout is Grid + GridSplitter, chosen so it can be swapped later without ViewModel changes)

## Restoring and running
```
dotnet restore
dotnet build
dotnet run --project src/KKL.WordStudio.UI/KKL.WordStudio.UI.csproj
```
(Requires a Windows target for the UI project, since it uses WPF.)
