using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace ListViewApp.Controls;

// 
// https://github.com/npolyak/NP.Avalonia.Demos/blob/main/CustomBehaviors/NP.Demos.DragBehaviorSample/MainWindow.axaml
//

public class StretchEdgeEffect : Decorator
{
    public StretchEdgeEffect()
    {
        AddHandler(Gestures.ScrollGestureEvent, OnScrollGesture, handledEventsToo: true);
        AddHandler(Gestures.ScrollGestureEndedEvent, OnScrollGestureEnded, handledEventsToo: true);
    }

    double stretch_y = 1.0;
    private void OnScrollGesture(object? sender, ScrollGestureEventArgs e)
    {
        if (e.Handled)
        {
            ReleaseStretchRepeater();
            return;
        }

        if (isReleasing)
            return;

        if (e.Delta.Y == 0)
            return;

        //if (true)//e.Delta.Y > 5)
        //{
        //    System.Diagnostics.Debug.WriteLine($"ListView.OnScrollGesture ({e.Id}): {e.Delta.Y}");
        //}

        var easing = new Avalonia.Animation.Easings.CubicEaseOut();
        var ease = easing.Ease((stretch_y - 1.0) * 20.0);

        stretch_y += Math.Abs(e.Delta.Y) / (1000.0 + (ease * 18000.0));

        if (stretch_y >= 1.05)
            stretch_y = 1.05; // max 5% stretching

        e.Handled = true;
        e.ShouldEndScrollGesture = true;

        if (e.Delta.Y < 0)
        {
            stretchedBottom = false;
            StretchRepeaterVertically(stretch_y);
        }
        else
        {
            stretchedBottom = true;
            StretchRepeaterVerticallyBottom(stretch_y);
        }
    }

    void StretchRepeaterVertically(double value)
    {
        isStretched = true;
        Child!.RenderTransformOrigin = new RelativePoint(0.5, 0.0, RelativeUnit.Relative);
        Child.RenderTransform = new MatrixTransform(new Matrix(
        1.0, 0.0, 0.0,
        0.0, value, 0.0,
        0.0, 0.0, 1.0));

        //Debug.WriteLine($"stretch.y = {value}");
    }

    void StretchRepeaterVerticallyBottom(double value)
    {
        isStretched = true;

        Child!.RenderTransformOrigin = new RelativePoint(0.5, 1.0, RelativeUnit.Relative);
        Child!.RenderTransform = new MatrixTransform(new Matrix(
        1.0, 0.0, 0.0,
        0.0, value, 0.0,
        0.0, 0.0, 1.0));
    }

    bool isStretched = false;
    bool isReleasing = false;
    bool stretchedBottom = false;
    void ReleaseStretchRepeater()
    {
        if (!isStretched)
            return;

        if (isReleasing)
            return;

        isReleasing = true;
        Debug.WriteLine($"Releasing ({stretch_y})");

        var easing = new Avalonia.Animation.Easings.CubicEaseOut();
        var st = Stopwatch.StartNew();

        DispatcherTimer.Run(
            () =>
            {
                //stretch_y -= steps;
                var progress = Math.Max(0.0, Math.Min(1.0, st.ElapsedMilliseconds / 350.0));

                var e = easing.Ease(progress);
                var value = (stretch_y - 1.0) * e;

                if (stretchedBottom)
                {
                    StretchRepeaterVerticallyBottom(stretch_y - value);
                }
                else
                {
                    StretchRepeaterVertically(stretch_y - value);
                }

                if (progress >= 1.0)// stretch_y <= 1.0)
                {
                    stretch_y = 1.0;

                    if (stretchedBottom)
                    {
                        StretchRepeaterVerticallyBottom(stretch_y);
                    }
                    else
                    {
                        StretchRepeaterVertically(stretch_y);
                    }

                    isStretched = false;
                    isReleasing = false;
                    Debug.WriteLine($"Released");
                    return false;
                }

                return true;
            }, TimeSpan.FromMilliseconds(4), priority: DispatcherPriority.Render);
    }

    private void OnScrollGestureEnded(object? sender, ScrollGestureEndedEventArgs e)
    {
        //System.Diagnostics.Debug.WriteLine($"OnScrollGestureEnded ({e.Id})");
        ReleaseStretchRepeater();
    }
}
