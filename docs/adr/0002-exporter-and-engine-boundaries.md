# ADR 0002: Exporter Placement and the Future Rendering/Engine Split

## Status
Accepted

## Context
Two boundary questions arose during Sprint 1 planning:

1. Where should `IReportExporter` (and sibling extensibility contracts) live —
   Domain or Application?
2. Should the `Rendering` project ever contain report *execution* concerns
   (pagination, layout calculation, expression evaluation)?

## Decision

### 1. Exporter abstractions live in Application, not Domain
Exporting a report to DOCX/PDF/HTML/Image is an **application use case** — it
describes what the software *does* with a report, not what a report *is*.
The Domain layer (`Report`, `Page`, `Section`, `ReportElement` hierarchy,
`IReportElementVisitor`) must remain ignorant of every output format.

Therefore:
- `IReportExporter`, `IDataProvider`, `IPropertyEditorProvider`,
  `IToolboxItemProvider`, `IExpressionFunctionProvider` live in
  `KKL.WordStudio.Application.Abstractions`.
- Concrete implementations (`WordExporter`, `PdfExporter`, ...) live in
  `KKL.WordStudio.Infrastructure`.
- Domain has zero knowledge that exporters, or any I/O, exist.

### 2. Rendering is interaction-only; execution concerns are deferred to a future Engine project
`KKL.WordStudio.Rendering` is scoped strictly to: drawing, hit-testing,
selection, snapping, rulers, and zoom/viewport interaction.

Pagination, layout calculation, expression evaluation, data processing,
measurement, and report execution are **explicitly out of scope** for
Rendering. These will eventually form `KKL.WordStudio.Engine`, consumed by
Rendering through a future `ILayoutResult`-style contract (not yet defined —
premature to abstract before a real implementation exists).

No code in Rendering may reference pagination, expression evaluation, or
data execution concepts. This is a hard boundary, not a convention.

## Consequences
- Domain stays a pure, dependency-free model — reusable even outside this
  application (e.g., a future CLI export tool).
- Adding a new exporter (e.g., MarkdownExporter) never touches Domain.
- When `Engine` is introduced, it will sit between `Domain` and `Rendering`
  in the dependency graph, and Rendering will be refactored to consume its
  output rather than compute layout itself — this ADR exists so that
  refactor is a clean extraction, not an untangling.
