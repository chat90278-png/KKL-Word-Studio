namespace KKL.WordStudio.Domain.Tests;

using KKL.WordStudio.Domain.Reports;
using Xunit;

public class Sprint5DomainTests
{
    [Fact]
    public void Report_IncludeTableOfContents_DefaultsToFalse()
    {
        var report = new Report();
        Assert.False(report.IncludeTableOfContents);
    }

    [Fact]
    public void Page_ShowPageNumbers_DefaultsToTrue()
    {
        var page = new Page();
        Assert.True(page.ShowPageNumbers);
    }
}
