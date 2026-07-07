namespace KKL.WordStudio.Application.Plugins;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A self-contained unit of extensibility. Each plugin (a new exporter, a
/// new data provider, a bundle of toolbox items, ...) implements this once
/// and registers its own services — the host application never needs to
/// know what a given plugin contains, only that it can call
/// <see cref="ConfigureServices"/> on startup. This is what satisfies
/// "future modules addable without modifying existing architecture".
/// </summary>
public interface IPluginModule
{
    string Name { get; }
    void ConfigureServices(IServiceCollection services);
}
