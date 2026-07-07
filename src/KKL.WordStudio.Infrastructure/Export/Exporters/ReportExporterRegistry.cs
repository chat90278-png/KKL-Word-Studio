namespace KKL.WordStudio.Infrastructure.Export.Exporters;

using KKL.WordStudio.Application.Abstractions;

/// <summary>Default in-memory implementation of <see cref="IReportExporterRegistry"/>.</summary>
public sealed class ReportExporterRegistry : IReportExporterRegistry
{
    private readonly Dictionary<string, IReportExporter> _exporters = new(StringComparer.OrdinalIgnoreCase);

    public void Register(IReportExporter exporter) => _exporters[exporter.FormatKey] = exporter;

    public IReportExporter Resolve(string formatKey) =>
        _exporters.TryGetValue(formatKey, out var exporter)
            ? exporter
            : throw new KeyNotFoundException($"No exporter registered for format '{formatKey}'.");

    public IReadOnlyCollection<IReportExporter> All => _exporters.Values;
}
