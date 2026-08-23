using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Messaging;
using TodoAvalonia.Messages;

namespace TodoAvalonia.Controls;

public class ReorderableMasonryGrid : MasonryGrid
{
    private object? _draggedData;
    private Point _grabOffset;
    private bool _dragging;

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
        e.Pointer.Capture(this);
    }

    private void OnChildPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_draggedData is null || e.Pointer.Captured != this) return;

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
        }

        draggedChild.RenderTransform = new TranslateTransform(delta.X, delta.Y);
        draggedChild.ZIndex = 1;

        foreach (var child in Children)
        {
            if (child == draggedChild) continue;
            if (child.DataContext is not IReorderable) continue;
            if (!child.Bounds.Contains(pointerInPanel)) continue;

            WeakReferenceMessenger.Default.Send(new TaskListReorderMessage(_draggedData, child.DataContext!));

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
                draggedChild.RenderTransform = null;
                draggedChild.ZIndex = 0;
            }

            WeakReferenceMessenger.Default.Send(new TaskListsReorderedMessage());
        }

        _draggedData = null;
        _dragging = false;
        InvalidateArrange();
    }

    protected override bool CanAnimate(Control element)
    {
        return !_dragging || element.DataContext != _draggedData;
    }
}