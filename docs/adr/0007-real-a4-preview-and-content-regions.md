# ADR 0007: Structured Content Regions, Real A4 Preview, and Scoped Word Fidelity

## Status
Accepted

## Context
Sprint 5 opened with five architecture-review questions before any new
feature work, per the sprint's own instruction. Each is addressed below,
followed by the "real A4 preview" feature decisions that followed from
them.

## Review answers

### 1. How close is Preview to Word today?
Not close at all — Sprint 4's preview was a flat plain-text block list with
no page shape, margins, header/footer, or zoom. This sprint's core work
directly addresses that gap.

### 2. Is the content tree fit to become a shared model for future PDF/HTML exporters?
Not quite, as a flat list — it had no way to represent repeating
header/footer regions, a table of contents, or page geometry, all of
which any faithful PDF/HTML exporter will also need. **Decision:**
`IReportContentBuilder` now returns `ReportContentDocument`
(`HeaderNodes`/`BodyNodes`/`FooterNodes`/`TableOfContents`/`PageLayout`)
instead of a flat `IReadOnlyList<ReportContentNode>`. Element
classification logic is unchanged — only the shape of the result was
insufficient, not the interpretation itself.

### 3. Does `ReportContentBuilder` need a Strategy/Visitor split yet?
**No.** Its growth this sprint came from a structural distinction
(header/body/footer, TOC derivation), not from a proliferation of element
types — Shape/Barcode/Chart/DataRegion still aren't produced by the Report
Designer. Splitting into a Strategy-per-element-kind now would be
premature generalization. Revisit when a real increase in element-type
count (not structural regions) makes the switch statement genuinely
unwieldy.

### 4. Does `ExcelDataProvider` need streaming/lazy enumeration?
**No, not yet.** The OpenXML SDK's DOM-based reading loads a worksheet
part into memory before iterating, which is a known, real limitation for
very large sheets (the SDK does offer a streaming `OpenXmlReader` for
this). No sprint so far has hit an actual large-dataset problem. Adding
streaming now would be solving a hypothetical, not an observed, problem —
noted in the README as a deliberate, revisit-when-needed deferral.

### 5. Do Preview and Export share resolution without redundant repetition?
**They shared the logic (Sprint 4 was correct about that) but Preview was
re-running it unnecessarily.** `PreviewViewModel` listened to
`Workspace.WorkspaceChanged`, which fires on every selection change too —
so clicking a different node in the Report Designer's tree re-opened and
re-read every bound table's Excel file, with no relation to what was
actually clicked. **Fix:** added a narrower `IWorkspace.ReportContentChanged`
event, raised only by actions that mutate report content or switch the
active project/report — not by selection-only setters. `PreviewViewModel`
now subscribes to this event instead of the broad one.

## "Real A4 Preview" feature decisions

- **Page geometry needed no Domain change.** `Page.WidthMillimeters` /
  `HeightMillimeters` / `MarginsMillimeters` / `Orientation` already
  existed (Sprint 1) and were simply never consumed by anything. Preview
  now converts them to WPF device-independent pixels (96 DPI) for a
  correctly-proportioned on-screen A4 page; `WordExporter` converts them
  to twips for the real `.docx` page size/margins. Same source values,
  two unit conversions — not two independently-authored page models.

- **Header/Footer content is real, user-authored `TextElement`s** in
  `PageHeader`/`PageFocus` sections (existing `SectionKind` values from
  Sprint 1 — never used until now). The Report Designer gained "Add Header
  Text"/"Add Footer Text" commands that reuse the existing
  `InsertElement` method (now section-kind-aware), auto-creating the
  section on first use exactly like `Body` already does.

- **Table of Contents is derived, not authored.** `Report.IncludeTableOfContents`
  (new bool) toggles whether `ReportContentBuilder` scans `BodyNodes` for
  Heading/AltHeading text and emits `TocEntry` records — the same
  classification Preview already renders headings with. In Word, this
  meant headings needed *real* named paragraph styles with an
  `w:outlineLvl`, since Word's native `TOC` field only collects
  outline-leveled paragraphs — this is why `WordExporter` now builds a
  `StyleDefinitionsPart` with `Heading1`/`Heading2` styles, something
  ADR 0006 had explicitly deferred. The TOC and page-number fields are
  emitted as real, updatable Word fields (`SimpleField` with `TOC`/`PAGE`
  instructions) — opened in Word, a user can "Update Field" to populate
  them, exactly as with any Word-authored document.

- **Page numbers**: `Page.ShowPageNumbers` (new bool, default true) drives
  a `PAGE` field in the exported footer. Preview cannot know a real page
  count without real pagination (deliberately out of scope, see below), so
  it shows a static "Page 1" — labeled as an approximation, not simulated
  as if accurate.

- **Preview renders exactly one simulated page, not true pagination.**
  Splitting body content across multiple physical pages based on measured
  height is real layout/pagination work that belongs to the future Engine
  (ADR 0002/0005), not to this preview. The page shape, margins,
  header/footer, and TOC are real and match what Word will produce; where
  content would actually overflow onto a second page is not simulated.
  This keeps the Rendering-boundary decision from ADR 0002 intact —
  nothing new was added to `KKL.WordStudio.Rendering` this sprint either.

## Consequences
- `WordExporter` and the WPF Preview now derive page size, margins,
  header/footer content, page numbers, and TOC from the exact same
  `ReportContentDocument` — the visual-parity requirement this sprint
  asked for is satisfied structurally, not just by both "happening" to
  look similar.
- `IReportPreviewRenderer`/`PreviewSnapshot` gained the same
  Header/Body/Footer/TOC/PageLayout shape as `ReportContentDocument` —
  intentionally mirrored rather than reusing Application DTOs directly
  in UI, keeping the UI layer free to add presentation-only fields later
  without touching Application.
- PDF export remains out of scope this sprint, as instructed — but the
  content model it will eventually consume is now considerably closer to
  what a real PDF/HTML exporter needs (regions + TOC + page layout),
  answering review question 2 going forward.
