namespace KKL.WordStudio.Application.Abstractions;

/// <summary>
/// Resolves an <see cref="IReportExporter"/> by format key at runtime.
/// Implemented in Infrastructure and populated at startup (directly, or via
/// plugin modules through <see cref="Plugins.IPluginModule"/>) so that new
/// exporters never require changes to Application or Domain code.
/// </summary>
public interface IReportExporterRegistry
{
    void Register(IReportExporter exporter);
    IReportExporter Resolve(string formatKey);
    IReadOnlyCollection<IReportExporter> All { get; }
}
