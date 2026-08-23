using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media.Transformation;

namespace TodoAvalonia.Controls;

public class MasonryGrid : Panel
{
    public static readonly StyledProperty<double> MinItemWidthProperty =
        AvaloniaProperty.Register<MasonryGrid, double>(nameof(MinItemWidth), 100);

    public static readonly StyledProperty<double> ColumnSpacingProperty =
        AvaloniaProperty.Register<MasonryGrid, double>(nameof(ColumnSpacing));

    public static readonly StyledProperty<double> RowSpacingProperty =
        AvaloniaProperty.Register<MasonryGrid, double>(nameof(RowSpacing));

    private Dictionary<object, Point> _itemsPositions = new Dictionary<object, Point>();

    private Transitions _baseTransform = [
        new TransformOperationsTransition
        {
            Property = Visual.RenderTransformProperty,
            Duration = TimeSpan.FromMilliseconds(200),
        }
        ];
    
    static MasonryGrid()
    {
        AffectsMeasure<MasonryGrid>(MinItemWidthProperty, ColumnSpacingProperty, RowSpacingProperty);
    }

    public double MinItemWidth
    {
        get => GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    public double ColumnSpacing
    {
        get => GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    public double RowSpacing
    {
        get => GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    private int GetColumnCount(double availableWidth)
    {
        if (double.IsInfinity(availableWidth) || MinItemWidth <= 0)
        {
            return Math.Max(1, Children.Count);
        }

        var columns = (int)Math.Floor((availableWidth + ColumnSpacing) / (MinItemWidth + ColumnSpacing));
        return Math.Max(1, columns);
    }

    private static int GetShortestColumnIndex(double[] columnHeights)
    {
        var index = 0;
        for (var i = 1; i < columnHeights.Length; i++)
        {
            if (columnHeights[i] < columnHeights[index])
            {
                index = i;
            }
        }

        return index;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var columns = GetColumnCount(availableSize.Width);
        var itemWidth = double.IsInfinity(availableSize.Width)
            ? MinItemWidth
            : Math.Max(0, (availableSize.Width - (columns - 1) * ColumnSpacing) / columns);

        var columnHeights = new double[columns];

        foreach (var child in Children)
        {
            child.Measure(new Size(itemWidth, double.PositiveInfinity));

            var columnIndex = GetShortestColumnIndex(columnHeights);
            columnHeights[columnIndex] += child.DesiredSize.Height + RowSpacing;
        }

        var totalHeight = Max(columnHeights);
        if (totalHeight > 0)
        {
            totalHeight -= RowSpacing;
        }

        var width = double.IsInfinity(availableSize.Width)
            ? columns * itemWidth + (columns - 1) * ColumnSpacing
            : availableSize.Width;

        return new Size(width, Math.Max(0, totalHeight));
    }

    protected virtual bool CanAnimate(Control element)
    {
        return true;
    }

    protected virtual IReadOnlyList<Control> GetArrangeOrder() => Children;

    protected virtual bool ShouldArrangeChild(Control child) => true;

    protected void SetTrackedPosition(Control child, Point position) => _itemsPositions[GetPositionKey(child)] = position;

    private static object GetPositionKey(Control child) => child.DataContext ?? child;

    protected override Size ArrangeOverride(Size finalSize)
    {
        var columns = GetColumnCount(finalSize.Width);
        var itemWidth = Math.Max(0, (finalSize.Width - (columns - 1) * ColumnSpacing) / columns);

        var columnHeights = new double[columns];

        foreach (var child in GetArrangeOrder())
        {
            var columnIndex = GetShortestColumnIndex(columnHeights);
            var x = columnIndex * (itemWidth + ColumnSpacing);
            var y = columnHeights[columnIndex];

            if (ShouldArrangeChild(child))
            {
                var key = GetPositionKey(child);

                if (!_itemsPositions.TryGetValue(key, out var position))
                {
                    position = new Point(x, y);
                    _itemsPositions[key] = position;
                }

                var destination = new Rect(x, y, itemWidth, child.DesiredSize.Height);

                child.Arrange(destination);

                if (position != destination.Position && CanAnimate(child))
                {
                    child.Transitions = null;

                    Vector direction = position - destination.Position;

                    child.RenderTransform = TransformOperations.Parse(
                        $"translate({direction.X.ToString(CultureInfo.InvariantCulture)}px,{direction.Y.ToString(CultureInfo.InvariantCulture)}px)"
                        );

                    child.Transitions = _baseTransform;

                    child.RenderTransform = TransformOperations.Parse("translate(0px,0px)");
                }

                _itemsPositions[key] = destination.Position;
            }

            columnHeights[columnIndex] += child.DesiredSize.Height + RowSpacing;
        }

        return finalSize;
    }

    private static double Max(double[] values)
    {
        return values.Prepend(0.0).Max();
    }
}
