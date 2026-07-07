namespace KKL.WordStudio.Infrastructure.DataProviders;

using KKL.WordStudio.Application.Abstractions;

/// <summary>Default in-memory implementation of IDataProviderRegistry — mirrors ReportExporterRegistry's shape exactly.</summary>
public sealed class DataProviderRegistry : IDataProviderRegistry
{
    private readonly Dictionary<string, IDataProvider> _providers = new(StringComparer.OrdinalIgnoreCase);

    public void Register(IDataProvider provider) => _providers[provider.ProviderKey] = provider;

    public IDataProvider Resolve(string providerKey) =>
        _providers.TryGetValue(providerKey, out var provider)
            ? provider
            : throw new KeyNotFoundException($"No data provider registered for key '{providerKey}'.");
}
