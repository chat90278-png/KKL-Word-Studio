namespace KKL.WordStudio.Domain.DataSources;

using System.Text.Json.Serialization;
using KKL.WordStudio.Domain.DataBinding;

/// <summary>
/// Abstract base for anything a report can bind to. Concrete today:
/// <see cref="ExcelDataSource"/>. The abstraction exists so future
/// providers (SQL, REST, CSV) can be added as new DataSource subtypes
/// without touching Report/Table/DataRegion binding code, which only ever
/// depends on IDataSourceDefinition.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ExcelDataSource), "excel")]
public abstract class DataSource : IDataSourceDefinition
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }

    public List<ColumnMapping> ColumnMappings { get; } = new();

    [JsonIgnore]
    public IReadOnlyList<DataField> Fields => ColumnMappings.Select(m => m.TargetField).ToList();

    /// <summary>
    /// Identifies which registered IDataProvider actually knows how to read
    /// this data source's rows (see IDataProviderRegistry, Sprint 4). An
    /// abstract property rather than a type-check chain (`is
    /// ExcelDataSource` / `is SqlDataSource` / ...) so adding a new
    /// DataSource subtype never requires touching the resolution logic.
    /// </summary>
    [JsonIgnore]
    public abstract string ProviderKey { get; }
}
