namespace KKL.WordStudio.Infrastructure.Tests;

using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;
using KKL.WordStudio.Infrastructure.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class KwsProjectRepositoryTests
{
    [Fact]
    public async Task SaveThenOpen_RoundTripsProjectAndReportNames()
    {
        var repository = new KwsProjectRepository(NullLogger<KwsProjectRepository>.Instance);
        var project = new Project { Name = "Quarterly Sales Project" };
        var report = new Report { Name = "Quarterly Sales Report" };
        report.Pages.Add(new Page());
        project.Reports.Add(report);

        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        var saveResult = await repository.SaveAsync(project, tempFile);
        Assert.True(saveResult.IsSuccess);

        var savedPath = tempFile + ".kws";
        var openResult = await repository.OpenAsync(savedPath);

        Assert.True(openResult.IsSuccess);
        Assert.Equal("Quarterly Sales Project", openResult.Value.Name);
        Assert.Single(openResult.Value.Reports);
        Assert.Equal("Quarterly Sales Report", openResult.Value.Reports[0].Name);

        File.Delete(savedPath);
    }
}
