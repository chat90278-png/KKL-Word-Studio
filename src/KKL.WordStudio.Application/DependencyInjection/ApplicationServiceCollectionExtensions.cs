namespace KKL.WordStudio.Application.DependencyInjection;

using KKL.WordStudio.Application.Content;
using KKL.WordStudio.Application.Plugins;
using KKL.WordStudio.Application.Workspace;
using Microsoft.Extensions.DependencyInjection;

/// <summary>Composition entry point for the Application layer's own services (as opposed to Infrastructure's).</summary>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddWordStudioApplication(this IServiceCollection services)
    {
        services.AddSingleton<PluginCatalog>();
        services.AddSingleton<IWorkspace, Workspace>();
        services.AddSingleton<IReportContentBuilder, ReportContentBuilder>();
        return services;
    }
}
