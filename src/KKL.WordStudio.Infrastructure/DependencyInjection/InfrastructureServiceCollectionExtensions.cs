namespace KKL.WordStudio.Infrastructure.DependencyInjection;

using KKL.WordStudio.Application.Abstractions;
using KKL.WordStudio.Application.Content;
using KKL.WordStudio.Application.Excel;
using KKL.WordStudio.Infrastructure.DataProviders;
using KKL.WordStudio.Infrastructure.Excel;
using KKL.WordStudio.Infrastructure.Export.Exporters;
using KKL.WordStudio.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Composition root helper for Infrastructure. Registers the built-in
/// exporters/providers as the "core" plugin set — additional third-party
/// plugin modules are appended by the UI composition root via
/// PluginCatalog, without touching this method.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddWordStudioInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IProjectService, KwsProjectRepository>();
        services.AddSingleton<IExcelWorkbookReader, OpenXmlExcelWorkbookReader>();

        // Data providers: registry-based (Sprint 4) rather than a single DI
        // registration, since more than one provider kind now exists —
        // adding a new source type (SQL, REST, ...) only means Register()-ing
        // it here, never touching ReportContentBuilder's resolution logic.
        services.AddSingleton<IDataProviderRegistry>(sp =>
        {
            var registry = new DataProviderRegistry();
            registry.Register(new InMemoryDataProvider());
            registry.Register(new ExcelDataProvider(sp.GetRequiredService<ILogger<ExcelDataProvider>>()));
            return registry;
        });

        services.AddSingleton<IReportExporterRegistry>(sp =>
        {
            var registry = new ReportExporterRegistry();
            registry.Register(new WordExporter(
                sp.GetRequiredService<IReportContentBuilder>(),
                sp.GetRequiredService<ILogger<WordExporter>>()));
            registry.Register(new PdfExporter());
            registry.Register(new HtmlExporter());
            registry.Register(new ImageExporter());
            registry.Register(new ExcelExporter());
            return registry;
        });

        return services;
    }
}
