namespace KKL.WordStudio.Application.Abstractions;

using KKL.WordStudio.Domain.DataBinding;
using KKL.WordStudio.Shared.Results;

/// <summary>
/// Application-facing contract for retrieving actual rows from a concrete
/// data source (SQL, CSV, REST, in-memory). The Domain only knows the
/// *shape* of a data source (<see cref="IDataSourceDefinition"/>); actually
/// connecting to and querying one is an Infrastructure/plugin concern.
/// </summary>
public interface IDataProvider
{
    string ProviderKey { get; }

    Task<Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> GetRowsAsync(
        IDataSourceDefinition definition, CancellationToken cancellationToken = default);
}
