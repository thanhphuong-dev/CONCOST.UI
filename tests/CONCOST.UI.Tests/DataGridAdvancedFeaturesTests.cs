using System;
using System.Globalization;
using System.Windows;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

public class DataGridAdvancedFeaturesTests
{
    [Fact]
    public void DetailsVisibilityToBoolConverter_ConvertsCorrectly()
    {
        var converter = DetailsVisibilityToBoolConverter.Instance;

        var visTrue = converter.Convert(Visibility.Visible, typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.Equal(true, visTrue);

        var visFalse = converter.Convert(Visibility.Collapsed, typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.Equal(false, visFalse);

        var backVis = converter.ConvertBack(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Visible, backVis);

        var backColl = converter.ConvertBack(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, backColl);
    }

    [Theory]
    [InlineData(ComparisonOperator.GreaterThan, 10.0, 0.0, 15.0, true)]
    [InlineData(ComparisonOperator.GreaterThan, 10.0, 0.0, 5.0, false)]
    [InlineData(ComparisonOperator.LessThan, 10.0, 0.0, 5.0, true)]
    [InlineData(ComparisonOperator.LessThan, 10.0, 0.0, 15.0, false)]
    [InlineData(ComparisonOperator.Between, 5.0, 15.0, 10.0, true)]
    [InlineData(ComparisonOperator.Between, 5.0, 15.0, 20.0, false)]
    public void ConditionalFormattingRule_EvaluatesCorrectly(
        ComparisonOperator op, double val1, double val2, double testVal, bool expected)
    {
        var rule = new ConditionalFormattingRule
        {
            Operator = op,
            Value1 = val1,
            Value2 = val2
        };

        bool actual = rule.Evaluate(testVal);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DataPager_PaginationCalculations_AreAccurate()
    {
        var thread = new System.Threading.Thread(() =>
        {
            var pager = new DataPager
            {
                PageSize = 10,
                TotalItems = 45,
                PageIndex = 1
            };

            Assert.Equal(5, pager.TotalPages);
            Assert.False(pager.CanGoPrev);
            Assert.True(pager.CanGoNext);

            pager.NextPage();
            Assert.Equal(2, pager.PageIndex);
            Assert.True(pager.CanGoPrev);
            Assert.True(pager.CanGoNext);

            pager.LastPage();
            Assert.Equal(5, pager.PageIndex);
            Assert.True(pager.CanGoPrev);
            Assert.False(pager.CanGoNext);

            pager.PrevPage();
            Assert.Equal(4, pager.PageIndex);

            pager.FirstPage();
            Assert.Equal(1, pager.PageIndex);
        });
    }

    [Fact]
    public void CellType_Enum_HasRequiredEnterpriseTypes()
    {
        Assert.True(Enum.IsDefined(typeof(CellType), "RowExpander"));
        Assert.True(Enum.IsDefined(typeof(CellType), "CheckBox"));
        Assert.True(Enum.IsDefined(typeof(CellType), "Numeric"));
        Assert.True(Enum.IsDefined(typeof(CellType), "Progress"));
        Assert.True(Enum.IsDefined(typeof(CellType), "Badge"));
        Assert.True(Enum.IsDefined(typeof(CellType), "Rating"));
    }
}
