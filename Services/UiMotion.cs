using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SupaTweaker.Services;

public static class UiMotion
{
    private static readonly Dictionary<ScrollViewer, double> _target = [];

    public static void EnableSmoothScroll()
    {
        EventManager.RegisterClassHandler(
            typeof(ScrollViewer),
            UIElement.PreviewMouseWheelEvent,
            new MouseWheelEventHandler(OnWheel),
            true);
    }

    public static Task FadeSwap(FrameworkElement el, Action swap)
    {
        var tcs = new TaskCompletionSource();
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(120))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };
        fadeOut.Completed += (_, _) =>
        {
            swap();
            if (el.RenderTransform is not TranslateTransform tr)
            {
                tr = new TranslateTransform();
                el.RenderTransform = tr;
            }
            tr.Y = 12;
            el.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                });
            tr.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(12, 0, TimeSpan.FromMilliseconds(180))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                });
            tcs.TrySetResult();
        };
        el.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        return tcs.Task;
    }

    private static void OnWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer sv) return;
        if (sv.ComputedVerticalScrollBarVisibility != Visibility.Visible &&
            sv.ScrollableHeight <= 0) return;

        e.Handled = true;
        if (!_target.TryGetValue(sv, out var dest))
            dest = sv.VerticalOffset;
        dest = Math.Clamp(dest - e.Delta * 0.85, 0, sv.ScrollableHeight);
        _target[sv] = dest;

        var anim = new DoubleAnimation(sv.VerticalOffset, dest, TimeSpan.FromMilliseconds(220))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        anim.Completed += (_, _) =>
        {
            if (_target.TryGetValue(sv, out var t) && Math.Abs(sv.VerticalOffset - t) < 0.5)
                _target.Remove(sv);
        };

        var helper = GetHelper(sv);
        helper.Offset = sv.VerticalOffset;
        helper.BeginAnimation(ScrollOffsetHelper.OffsetProperty, anim);
    }

    private static ScrollOffsetHelper GetHelper(ScrollViewer sv)
    {
        if (sv.Tag is ScrollOffsetHelper h) return h;
        h = new ScrollOffsetHelper(sv);
        sv.Tag = h;
        return h;
    }
}

public sealed class ScrollOffsetHelper : UIElement
{
    private readonly ScrollViewer _sv;

    public ScrollOffsetHelper(ScrollViewer sv) => _sv = sv;

    public static readonly DependencyProperty OffsetProperty =
        DependencyProperty.Register(nameof(Offset), typeof(double), typeof(ScrollOffsetHelper),
            new PropertyMetadata(0d, (d, e) => ((ScrollOffsetHelper)d)._sv.ScrollToVerticalOffset((double)e.NewValue)));

    public double Offset
    {
        get => (double)GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }
}
