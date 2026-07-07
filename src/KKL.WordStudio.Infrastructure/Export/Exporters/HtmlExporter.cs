namespace KKL.WordStudio.Infrastructure.Export.Exporters;

using KKL.WordStudio.Application.Abstractions;
using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;
using KKL.WordStudio.Shared.Results;

public sealed class HtmlExporter : IReportExporter
{
    public string FormatKey => "html";
    public string DisplayName => "HTML Document (.html)";

    public Task<Result<Stream>> ExportAsync(Project project, Report report, ExportOptions options, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure<Stream>("HTML export is not yet implemented."));
}
