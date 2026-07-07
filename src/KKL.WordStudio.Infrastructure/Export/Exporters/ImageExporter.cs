namespace KKL.WordStudio.Infrastructure.Export.Exporters;

using KKL.WordStudio.Application.Abstractions;
using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;
using KKL.WordStudio.Shared.Results;

public sealed class ImageExporter : IReportExporter
{
    public string FormatKey => "image";
    public string DisplayName => "Image (.png)";

    public Task<Result<Stream>> ExportAsync(Project project, Report report, ExportOptions options, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure<Stream>("Image export is not yet implemented."));
}
