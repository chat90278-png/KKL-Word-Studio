namespace KKL.WordStudio.Infrastructure.Persistence;

using System.IO.Compression;
using System.Text.Json;
using KKL.WordStudio.Application.Abstractions;
using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;
using KKL.WordStudio.Shared.Constants;
using KKL.WordStudio.Shared.Results;
using Microsoft.Extensions.Logging;

/// <summary>
/// Reads/writes the native .kws project container. A .kws file is a zip
/// archive (the same technique DOCX/XLSX themselves use) containing:
///   /manifest.json   - format/product version, timestamps
///   /project.json    - the serialized Domain Project aggregate (data
///                      sources + reports + settings)
///   /resources/images/* - embedded binary resources (added in a later sprint)
///
/// As of ADR 0003, the persisted root is Project, not Report — the on-disk
/// container format (zip + JSON) itself is unchanged from Sprint 1, only
/// the entry name and the serialized type changed (report.json -> project.json).
///
/// Word/PDF/etc. are never written here — those are IReportExporter
/// outputs, produced on demand from a Report within the Project, not
/// persisted project state.
/// </summary>
public sealed class KwsProjectRepository : IProjectService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly ILogger<KwsProjectRepository> _logger;

    public KwsProjectRepository(ILogger<KwsProjectRepository> logger) => _logger = logger;

    public Project CreateNew()
    {
        var project = new Project { Name = "Untitled Project" };
        var report = new Report { Name = "Report1" };
        var page = new Domain.Reports.Page();
        page.Sections.Add(new Domain.Reports.Section { Name = "Body", Kind = Domain.Reports.SectionKind.Body });
        report.Pages.Add(page);
        project.Reports.Add(report);
        return project;
    }

    public async Task<Result> SaveAsync(Project project, string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!filePath.EndsWith(AppConstants.ProjectFileExtension, StringComparison.OrdinalIgnoreCase))
                filePath += AppConstants.ProjectFileExtension;

            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create);

            var manifest = new KwsProjectManifest
            {
                FormatVersion = AppConstants.ProjectFileFormatVersion,
                ProductVersion = AppConstants.ApplicationName
            };

            await WriteJsonEntryAsync(archive, "manifest.json", manifest, cancellationToken);
            await WriteJsonEntryAsync(archive, "project.json", project, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save project to {FilePath}", filePath);
            return Result.Failure($"Could not save project: {ex.Message}");
        }
    }

    public async Task<Result<Project>> OpenAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(filePath))
                return Result.Failure<Project>($"Project file not found: {filePath}");

            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);

            var entry = archive.GetEntry("project.json")
                ?? throw new InvalidDataException("The .kws package is missing project.json.");

            await using var entryStream = entry.Open();
            var project = await JsonSerializer.DeserializeAsync<Project>(entryStream, JsonOptions, cancellationToken)
                ?? throw new InvalidDataException("project.json could not be deserialized.");

            return Result.Success(project);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open project from {FilePath}", filePath);
            return Result.Failure<Project>($"Could not open project: {ex.Message}");
        }
    }

    private static async Task WriteJsonEntryAsync<T>(ZipArchive archive, string entryName, T value, CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        await using var stream = entry.Open();
        await JsonSerializer.SerializeAsync(stream, value, JsonOptions, cancellationToken);
    }
}
