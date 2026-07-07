namespace KKL.WordStudio.Domain.DataBinding;

/// <summary>
/// Describes a data source's *shape* (name + fields) as stored in the
/// report/project model. This is metadata only — the Domain never connects
/// to an actual data source; that's an Infrastructure/Application concern
/// (IDataProvider, in the Application layer).
/// </summary>
public interface IDataSourceDefinition
{
    string Name { get; }
    IReadOnlyList<DataField> Fields { get; }
}
