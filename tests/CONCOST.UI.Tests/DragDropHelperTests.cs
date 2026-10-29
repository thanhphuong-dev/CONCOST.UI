using System;
using System.Threading;
using System.Windows.Controls;
using CONCOST.UI.AttachedProperties;
using Xunit;

namespace CONCOST.UI.Tests;

public class DragDropHelperTests
{
    private static void RunOnSTA(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
        {
            throw new AggregateException("STA test failed", exception);
        }
    }

    [Fact]
    public void DragDropHelper_AttachedProperties_WorkCorrectly()
    {
        RunOnSTA(() =>
        {
            var lb = new ListBox();

            Assert.False(DragDropHelper.GetIsDragSource(lb));
            Assert.False(DragDropHelper.GetIsDropTarget(lb));
            Assert.Equal(string.Empty, DragDropHelper.GetDragDropGroup(lb));

            DragDropHelper.SetIsDragSource(lb, true);
            DragDropHelper.SetIsDropTarget(lb, true);
            DragDropHelper.SetDragDropGroup(lb, "BimKanban");

            Assert.True(DragDropHelper.GetIsDragSource(lb));
            Assert.True(DragDropHelper.GetIsDropTarget(lb));
            Assert.True(lb.AllowDrop);
            Assert.Equal("BimKanban", DragDropHelper.GetDragDropGroup(lb));

            // Turning off drop target sets AllowDrop = false
            DragDropHelper.SetIsDropTarget(lb, false);
            Assert.False(DragDropHelper.GetIsDropTarget(lb));
            Assert.False(lb.AllowDrop);
        });
    }

    [Fact]
    public void InsertionAdorner_InitializesProperly()
    {
        RunOnSTA(() =>
        {
            var border = new Border { Width = 100, Height = 40 };
            var adorner = new InsertionAdorner(border, isBottom: true);
            Assert.False(adorner.IsHitTestVisible);
            Assert.Same(border, adorner.AdornedElement);
            Assert.True(adorner.IsBottom);

            adorner.IsBottom = false;
            Assert.False(adorner.IsBottom);
        });
    }

    [Fact]
    public void DragPreviewAdorner_InitializesProperly()
    {
        RunOnSTA(() =>
        {
            var root = new Border { Width = 400, Height = 300 };
            var item = new Border { Width = 100, Height = 40 };
            var preview = new DragPreviewAdorner(root, item, new System.Windows.Point(10, 10));
            Assert.False(preview.IsHitTestVisible);
            Assert.Same(root, preview.AdornedElement);
        });
    }
}

