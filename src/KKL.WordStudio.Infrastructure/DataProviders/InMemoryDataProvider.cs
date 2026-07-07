namespace KKL.WordStudio.Infrastructure.DataProviders;

using KKL.WordStudio.Application.Abstractions;
using KKL.WordStudio.Domain.DataBinding;
using KKL.WordStudio.Shared.Results;

/// <summary>
/// Simplest possible IDataProvider — serves rows supplied directly in
/// code/tests. Real providers (SQL, CSV, REST) follow the same contract
/// and are added as plugins without touching this one.
/// </summary>
public sealed class InMemoryDataProvider : IDataProvider
{
    private readonly Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, object?>>> _rowsBySource = new();

    public string ProviderKey => "in-memory";

    public void Seed(string dataSourceName, IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
        => _rowsBySource[dataSourceName] = rows;

    public Task<Result<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> GetRowsAsync(
        IDataSourceDefinition definition, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_rowsBySource.TryGetValue(definition.Name, out var rows)
            ? Result.Success(rows)
            : Result.Failure<IReadOnlyList<IReadOnlyDictionary<string, object?>>>(
                $"No seeded rows for data source '{definition.Name}'."));
    }
}
