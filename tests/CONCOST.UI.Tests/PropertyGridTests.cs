using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Controls;

namespace CONCOST.UI.Tests;

public class PropertyGridTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    [Fact]
    public void BuildProperties_SkipsHiddenAndIndexerProperties_AndMarksReadOnlyProperties()
    {
        RunOnSTA(() =>
        {
            var grid = new PropertyGrid { SelectedObject = new SampleModel() };
            var items = grid.Categories.SelectMany(category => category.Properties).ToArray();

            Assert.DoesNotContain(items, item => item.Name == nameof(SampleModel.Hidden));
            Assert.DoesNotContain(items, item => item.Name == "Item");
            Assert.Contains(items, item => item.Name == nameof(SampleModel.Computed) && item.IsReadOnly);
        });
    }

    [Fact]
    public void PropertyGrid_ConvertsEditedValuesAndTracksExternalModelChanges()
    {
        RunOnSTA(() =>
        {
            var model = new SampleModel();
            var grid = new PropertyGrid { SelectedObject = model };
            var count = grid.Categories.SelectMany(category => category.Properties)
                .Single(item => item.Name == nameof(SampleModel.Count));

            count.Value = "42";
            Assert.Equal(42, model.Count);
            Assert.Null(count.ValidationError);

            model.Count = 53;
            Assert.Equal(53, count.Value);
        });
    }

    [Fact]
    public void PropertyGrid_ReportsInvalidEditsAndSupportsNullableNumbers()
    {
        RunOnSTA(() =>
        {
            var model = new SampleModel();
            var grid = new PropertyGrid { SelectedObject = model };
            var count = grid.Categories.SelectMany(category => category.Properties)
                .Single(item => item.Name == nameof(SampleModel.Count));
            var optional = grid.Categories.SelectMany(category => category.Properties)
                .Single(item => item.Name == nameof(SampleModel.OptionalCount));

            count.Value = "not a number";
            Assert.NotNull(count.ValidationError);
            Assert.Equal(7, model.Count);

            optional.Value = "18";
            Assert.Equal(18, model.OptionalCount);
            Assert.Equal(PropertyEditorType.Number, optional.EditorType);
            optional.Value = null;
            Assert.Null(model.OptionalCount);
        });
    }

    [Fact]
    public void PropertyGrid_KeepsHostSuppliedCategoriesWhenItHasNoSelectedObject()
    {
        RunOnSTA(() =>
        {
            var grid = new PropertyGrid();
            var category = new PropertyCategoryGroup { CategoryName = "Constraints" };

            category.Properties.Add(new PropertyItem { Name = "Height", DisplayName = "Height", Value = "3000" });
            grid.Categories.Add(category);

            // The first layout pass is what applies the template. With no SelectedObject to reflect
            // over, that pass used to clear the collection the host had just filled — which is how a
            // properties palette lost every row until the selection was made a second time.
            var host = new Border { Child = grid };

            host.Measure(new Size(300, 400));
            host.Arrange(new Rect(0, 0, 300, 400));
            host.UpdateLayout();

            Assert.Single(grid.Categories);
            Assert.Single(grid.Categories[0].Properties);
        });
    }

    private sealed class SampleModel : INotifyPropertyChanged
    {
        private int _count = 7;
        public event PropertyChangedEventHandler? PropertyChanged;

        public int Count
        {
            get => _count;
            set { _count = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count))); }
        }

        public int? OptionalCount { get; set; } = 12;
        public string Computed => "read only";

        [Browsable(false)]
        public string Hidden { get; set; } = "hidden";

        public string this[int index] => index.ToString();
    }
}
