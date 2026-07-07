# ADR 0001: Layered Solution Structure and Native .kws Project Format

## Status
Accepted

## Context
KKL Word Studio is planned as a multi-year, commercial-grade report designer.
It needs clear module boundaries from day one and a project file format that
is not tied to any single export format.

## Decision

### Layering
```
KKL.WordStudio.Shared          (zero dependencies; used by everyone)
KKL.WordStudio.Domain          -> Shared
KKL.WordStudio.Application     -> Domain, Shared
KKL.WordStudio.Rendering       -> Domain, Shared            (interaction only, see ADR 0002)
KKL.WordStudio.Infrastructure  -> Application, Domain, Shared
KKL.WordStudio.UI              -> everything (composition root)
```
Reference direction is enforced by project references, not just convention:
Domain never references Application/Infrastructure/UI; Rendering never
references Application/Infrastructure.

### Native project format: .kws
Report projects are saved as `.kws`, a zip container (the same technique
DOCX/XLSX use internally) holding:
- `manifest.json` — format/product version, timestamps
- `report.json` — the serialized Domain `Report` aggregate
- `datasources.json` — data source definitions (added when data-source
  editing ships)
- `/resources/images/*` — embedded binary resources (added when the Image
  element ships)

Word/PDF/etc. output is never part of the persisted project — it is always
produced on demand via `IReportExporter` (see ADR 0002 for why that
interface lives in Application, not Domain).

## Consequences
- A `.kws` file is portable and forward-compatible: adding a new element
  type only requires the JSON schema to gain a new discriminated case, not
  a persisted-format rewrite.
- Any exporter can be added later purely in Infrastructure without ever
  touching how projects are saved/opened.
