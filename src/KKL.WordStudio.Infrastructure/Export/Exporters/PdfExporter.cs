namespace KKL.WordStudio.Infrastructure.Export.Exporters;

using KKL.WordStudio.Application.Abstractions;
using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;
using KKL.WordStudio.Shared.Results;

public sealed class PdfExporter : IReportExporter
{
    public string FormatKey => "pdf";
    public string DisplayName => "PDF Document (.pdf)";

    public Task<Result<Stream>> ExportAsync(Project project, Report report, ExportOptions options, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure<Stream>("PDF export is not yet implemented."));
}
