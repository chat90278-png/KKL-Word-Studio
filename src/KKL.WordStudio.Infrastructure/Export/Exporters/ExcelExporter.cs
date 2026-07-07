namespace KKL.WordStudio.Infrastructure.Export.Exporters;

using KKL.WordStudio.Application.Abstractions;
using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;
using KKL.WordStudio.Shared.Results;

public sealed class ExcelExporter : IReportExporter
{
    public string FormatKey => "xlsx";
    public string DisplayName => "Excel Workbook (.xlsx)";

    public Task<Result<Stream>> ExportAsync(Project project, Report report, ExportOptions options, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure<Stream>("Excel export is not yet implemented."));
}
