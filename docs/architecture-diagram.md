# KKL Word Studio — Architecture Diagram (post Sprint 2)

## Layer dependency graph

```mermaid
graph TD
    Shared["KKL.WordStudio.Shared<br/>(Result, Guards, Extensions, Geometry, ThemeKeys)"]
    Domain["KKL.WordStudio.Domain<br/>(Project, Report, DataSource hierarchy, Elements, Binding)"]
    Application["KKL.WordStudio.Application<br/>(IReportExporter, IDataProvider, PluginCatalog, Workspace)"]
    Infrastructure["KKL.WordStudio.Infrastructure<br/>(.kws persistence, Word/PDF/HTML/Image/Excel exporters)"]
    Rendering["KKL.WordStudio.Rendering<br/>(Hit-testing, Selection, Snap, Zoom, Rulers)"]
    UI["KKL.WordStudio.UI<br/>(Composition root, ViewModels, Views)"]

    Domain --> Shared
    Application --> Domain
    Application --> Shared
    Infrastructure --> Application
    Infrastructure --> Domain
    Infrastructure --> Shared
    Rendering --> Domain
    Rendering --> Shared
    UI --> Application
    UI --> Infrastructure
    UI --> Rendering
    UI --> Domain
    UI --> Shared

    style Domain fill:#e8f4ea,stroke:#3c8c4a
    style Rendering fill:#eaeef7,stroke:#3c5c8c
    style Application fill:#f7f0e0,stroke:#a67c1e
```

Note: `Rendering` never references `Application` — enforced by ADR 0002.
`Domain` never references `Application`/`Infrastructure` — enforced by ADR 0001/0003.

## Domain model (post ADR 0004 / Sprint 2)

```mermaid
graph TD
    Project["Project (aggregate root)"]
    Project --> DataSources["List&lt;DataSource&gt;"]
    Project --> Reports["List&lt;Report&gt;"]
    Project --> Settings["ProjectSettings"]

    DataSources --> ExcelDataSource
    ExcelDataSource --> Workbook
    Workbook --> Worksheet
    Worksheet --> DataRange["DataRange (structured: start/end row, header row, columns, WasAutoDetected)"]
    ExcelDataSource --> ColumnMapping

    Reports --> Report
    Report --> Page
    Page --> Section
    Section --> Container
    Container --> ReportElement
    ReportElement --> TextElement
    ReportElement --> ImageElement
    ReportElement --> TableElement
    ReportElement --> ShapeElement
    ReportElement --> BarcodeElement
    ReportElement --> ChartElement
    ReportElement --> DataRegion

    TableElement -. "Binding (DataSourceName + Filter + SortFields)" .-> DataSources
    DataRegion -. "Binding" .-> DataSources

    style Project fill:#e8f4ea,stroke:#3c8c4a
    style TableElement fill:#fdeeee,stroke:#a83c3c
    style DataRegion fill:#fdeeee,stroke:#a83c3c
    style DataRange fill:#fef6e0,stroke:#a67c1e
```

`Binding` (Sprint 2) now carries `Filter` (an `Expression`, reused from the
element-binding mechanism already used for cell content) and `SortFields`
(structured `SortField` list) — but deliberately not Worksheet/DataRange/
ColumnMapping, which stay resolved once at the DataSource level (ADR 0004).

## Excel import flow (Sprint 2 end-to-end path)

```mermaid
graph LR
    A["Open Excel file"] --> B["List sheets"]
    B --> C["Preview grid"]
    C --> D["Pick start row"]
    D --> E["Detect data end"]
    E --> F["Map columns"]
    F --> G["Add DataSource to Project"]
```

`IExcelWorkbookReader` (Application) / `OpenXmlExcelWorkbookReader`
(Infrastructure) implement steps A–E using the OpenXML SDK — the same
package already planned for Word export, so no new dependency was added.

