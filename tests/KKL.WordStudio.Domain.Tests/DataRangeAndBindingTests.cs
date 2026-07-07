namespace KKL.WordStudio.Domain.Tests;

using KKL.WordStudio.Domain.DataBinding;
using KKL.WordStudio.Domain.DataSources;
using KKL.WordStudio.Domain.Expressions;
using Xunit;

public class DataRangeAndBindingTests
{
    [Fact]
    public void RangeReference_IsComputed_NotStored()
    {
        var range = new DataRange
        {
            DataStartRow = 2,
            DataEndRow = 150,
            StartColumn = 1,
            EndColumn = 4
        };

        Assert.Equal("A2:D150", range.RangeReference);
    }

    [Fact]
    public void RangeReference_FallsBackToRowOnly_WhenColumnsUnknown()
    {
        var range = new DataRange { DataStartRow = 5, DataEndRow = 90 };
        Assert.Equal("Row 5:90", range.RangeReference);
    }

    [Fact]
    public void WasAutoDetected_DistinguishesAutoFromManualOverride()
    {
        var range = new DataRange { DataStartRow = 1, DataEndRow = 100, WasAutoDetected = true };
        Assert.True(range.WasAutoDetected);

        range.DataEndRow = 95; // user manually corrects
        range.WasAutoDetected = false;

        Assert.False(range.WasAutoDetected);
        Assert.Equal(95, range.DataEndRow);
    }

    [Fact]
    public void Binding_CanCarryFilterAndSort_WithoutDuplicatingDataSourceLevelConcerns()
    {
        var binding = new Binding
        {
            DataSourceName = "Sales",
            Filter = new Expression { Text = "=Fields.Region = 'North'" }
        };
        binding.SortFields.Add(new SortField { FieldName = "Amount", Direction = SortDirection.Descending });

        Assert.Equal("Sales", binding.DataSourceName);
        Assert.NotNull(binding.Filter);
        Assert.Single(binding.SortFields);
        Assert.Equal(SortDirection.Descending, binding.SortFields[0].Direction);
    }
}
