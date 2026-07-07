namespace KKL.WordStudio.Domain.Tests;

using KKL.WordStudio.Domain.DataBinding;
using KKL.WordStudio.Domain.DataSources;
using KKL.WordStudio.Domain.Elements;
using KKL.WordStudio.Domain.Projects;
using KKL.WordStudio.Domain.Reports;
using Xunit;

public class ProjectAndDataSourceTests
{
    [Fact]
    public void Project_CanHoldMultipleReportsSharingOneDataSource()
    {
        var dataSource = new ExcelDataSource
        {
            Name = "Sales",
            Workbook = new Workbook { FileName = "sales.xlsx" }
        };

        var project = new Project { Name = "Q3 Reporting" };
        project.DataSources.Add(dataSource);
        project.Reports.Add(new Report { Name = "Summary" });
        project.Reports.Add(new Report { Name = "Detailed" });

        Assert.Equal(2, project.Reports.Count);
        Assert.Single(project.DataSources);
    }

    [Fact]
    public void ExcelDataSource_Fields_AreDerivedFromColumnMappings()
    {
        var dataSource = new ExcelDataSource
        {
            Name = "Sales",
            Workbook = new Workbook { FileName = "sales.xlsx" }
        };
        dataSource.ColumnMappings.Add(new ColumnMapping
        {
            SourceColumn = "CustomerName",
            TargetField = new DataField { Name = "CustomerName", DataType = "string" }
        });

        Assert.Single(dataSource.Fields);
        Assert.Equal("CustomerName", dataSource.Fields[0].Name);
    }

    [Fact]
    public void TableElement_CanBeBoundToADataSourceByName()
    {
        var table = new TableElement { Binding = new Binding { DataSourceName = "Sales" } };

        Assert.NotNull(table.Binding);
        Assert.Equal("Sales", table.Binding!.DataSourceName);
    }
}
