namespace KKL.WordStudio.Application.Abstractions;

/// <summary>
/// Resolves an IDataProvider by provider key at runtime — the same
/// registry pattern already used for IReportExporterRegistry (ADR 0002).
/// Added in Sprint 4 because a second real provider (ExcelDataProvider)
/// now exists alongside InMemoryDataProvider; without a registry, adding
/// each new source type (SQL, REST, ...) would require a growing
/// type-check chain somewhere instead of a simple lookup by
/// DataSource.ProviderKey.
/// </summary>
public interface IDataProviderRegistry
{
    void Register(IDataProvider provider);
    IDataProvider Resolve(string providerKey);
}
