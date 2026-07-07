# ADR 0006: Shared Report Interpretation for Preview and Word Export

## Status
Accepted

## Context
Sprint 4's explicit priority: Preview and Word Export must use the same
Report model and must not work from separate logic. Building a real
WordExporter without addressing this would have meant two independent
implementations each deciding "what is a heading", "what does this bound
table actually contain" — with no structural guarantee they'd ever agree.

## Decision

### `IReportContentBuilder` is the single shared interpretation
Added `Application.Content.IReportContentBuilder`
(`BuildAsync(Project, Report) -> IReadOnlyList<ReportContentNode>`). It
walks the Report tree exactly once, classifies every element via the
existing `HeadingStylePresets` convention, and — for bound tables —
resolves real rows through `IDataProviderRegistry`. Both
`PreviewRenderer` (UI) and `WordExporter` (Infrastructure) call this and
only this; neither re-walks the tree or re-decides an element's kind.
`ReportContentKind` (renamed from the Sprint 3 `PreviewBlockKind`) is now
the one enum both consumers share, rather than two enums that happened to
look similar.

### Two real gaps surfaced while wiring this up, both fixed
1. **`Workbook` never stored the file's actual path** — only a display
   `FileName`. Exporting a bound table requires re-reading the real file,
   which was impossible without it. Added `Workbook.SourcePath`.
2. **No real (non-in-memory) `IDataProvider` existed.** `IExcelWorkbookReader`
   (Sprint 2) only supports design-time browsing/preview, not row
   extraction for execution. Added `ExcelDataProvider`, and — since a
   second real provider now exists — `IDataProviderRegistry`
   (`DataSource.ProviderKey` → provider), mirroring the
   `IReportExporterRegistry` pattern already established. This is the
   first time the "Data Providers" plugin extension point promised in
   ADR 0001 is actually populated with more than one implementation.

### Binding.SortFields is now actually applied; Binding.Filter is not
Sort is structured (field + direction) and needs no expression evaluator
— implemented as ordinary LINQ ordering over the resolved rows. Filter
needs to evaluate a boolean `Expression` against each row, which requires
an expression evaluator that doesn't exist yet (Engine — ADR 0002/0004).
Rather than silently dropping a configured filter, `TableContentNode`
carries `FilterWasIgnored` so both consumers can surface that the shown
rows are unfiltered, instead of pretending they're correct.

### WordExporter emits direct formatting, not named Word styles
Headings become bold/enlarged runs via `RunProperties`, not a "Heading 1"
paragraph style — no `StyleDefinitionsPart` has been built. This produces
a correct, readable .docx today; producing output a user could reformat
via Word's built-in Styles pane is deferred until that's actually needed.

### Images and DataRegion remain unhandled, deliberately
`ImageContentNode` degrades to a `[Image: Name]` placeholder in both
Preview and Word output — real embedding needs the Asset/resource catalog
deferred since ADR 0004. `DataRegion` is skipped entirely by
`ReportContentBuilder` — the Sprint 3 Report Designer doesn't create it
yet, so implementing its repeating-template semantics now would be
speculative.

## Consequences
- `IReportExporter.ExportAsync` and `IReportPreviewRenderer.RenderAsync`
  both now take `Project` alongside `Report` (a necessary, mechanical
  signature change across all five exporter stubs) — resolving bound data
  always requires the owning Project's DataSources.
- A future real Rendering/Engine-backed preview, or a richer WordExporter
  (real Word styles, images, filters), replaces exactly one piece each
  time — `IReportContentBuilder`'s output shape is what both consumers
  were always built against, so neither needs to change when the other
  improves.
