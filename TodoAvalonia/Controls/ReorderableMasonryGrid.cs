using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace TodoAvalonia.Controls;

public class ReorderableMasonryGrid : MasonryGrid
{
    private object? _draggedData;
    private Point _grabOffset;
    private bool _dragging;
    private List<Control>? _previewOrder;

    public ReorderableMasonryGrid()
    {
        AddHandler(PointerPressedEvent, OnChildPointerPressed, RoutingStrategies.Bubble);
        AddHandler(PointerMovedEvent, OnChildPointerMoved, RoutingStrategies.Bubble);
        AddHandler(PointerReleasedEvent, OnChildPointerReleased, RoutingStrategies.Bubble);
    }

    private Control? FindDirectChild(Visual source)
    {
        Visual? current = source as Control;

        while (current != null && current.GetVisualParent() != this)
            current = current.GetVisualParent();

        return current as Control;
    }

    private void OnChildPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Visual source) return;

        var child = FindDirectChild(source);
        if (child?.DataContext is not IReorderable) return;

        _grabOffset = e.GetPosition(source);
        _draggedData = child.DataContext;
    }

    private void OnChildPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_draggedData is null) return;

        var draggedChild = Children.FirstOrDefault(c => c.DataContext == _draggedData);
        if (draggedChild is null) return;

        var pointerInPanel = e.GetPosition(this);
        var grabVector = new Vector(_grabOffset.X, _grabOffset.Y);
        var desiredTopLeft = pointerInPanel - grabVector;
        var delta = desiredTopLeft - draggedChild.Bounds.Position;
        var distance = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

        if (!_dragging)
        {
            if (distance <= 5) return;
            _dragging = true;
            e.Pointer.Capture(this);
            _previewOrder = Children.Where(c => c.DataContext is IReorderable).ToList();
            draggedChild.Transitions = null;
        }

        draggedChild.RenderTransform = new TranslateTransform(delta.X, delta.Y);
        draggedChild.ZIndex = 1;

        foreach (var child in _previewOrder!)
        {
            if (child == draggedChild) continue;
            if (!child.Bounds.Contains(pointerInPanel)) continue;

            var draggedIndex = _previewOrder.IndexOf(draggedChild);
            var targetIndex = _previewOrder.IndexOf(child);
            _previewOrder.RemoveAt(draggedIndex);
            _previewOrder.Insert(targetIndex, draggedChild);
            InvalidateArrange();

            break;
        }
    }

    private void OnChildPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_draggedData is null) return;

        e.Pointer.Capture(null);

        if (_dragging)
        {
            var draggedChild = Children.FirstOrDefault(c => c.DataContext == _draggedData);
            if (draggedChild is not null)
            {
                if (draggedChild.RenderTransform is TranslateTransform t)
                    SetTrackedPosition(draggedChild, draggedChild.Bounds.Position + new Vector(t.X, t.Y));

                draggedChild.RenderTransform = null;
                draggedChild.ZIndex = 0;
            }

            var newIndex = draggedChild is not null ? _previewOrder!.IndexOf(draggedChild) : -1;
            if (newIndex >= 0 && _draggedData is IReorderable reorderable)
                reorderable.NewIndex(newIndex);
        }

        _previewOrder = null;
        _draggedData = null;
        _dragging = false;
        InvalidateArrange();
    }

    protected override IReadOnlyList<Control> GetArrangeOrder()
    {
        if (!_dragging || _previewOrder is null) return base.GetArrangeOrder();

        var fixedChildren = Children.Where(c => c.DataContext is not IReorderable);
        return fixedChildren.Concat(_previewOrder).ToList();
    }

    protected override bool ShouldArrangeChild(Control child)
    {
        return !(_dragging && child.DataContext == _draggedData);
    }
}