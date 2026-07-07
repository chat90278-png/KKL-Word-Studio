namespace KKL.WordStudio.Application.Plugins;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Holds the set of known <see cref="IPluginModule"/> instances and applies
/// them to the DI container at startup. Today this is a simple in-process
/// list populated by the composition root (see
/// InfrastructureServiceCollectionExtensions); nothing prevents swapping
/// the discovery mechanism for MEF, assembly-scanning, or a NuGet-based
/// plugin loader later, since consumers only ever see this catalog, never
/// the discovery mechanism itself.
/// </summary>
public sealed class PluginCatalog
{
    private readonly List<IPluginModule> _modules = new();

    public IReadOnlyList<IPluginModule> Modules => _modules;

    public PluginCatalog Register(IPluginModule module)
    {
        _modules.Add(module);
        return this;
    }

    public void ApplyTo(IServiceCollection services)
    {
        foreach (var module in _modules)
            module.ConfigureServices(services);
    }
}
