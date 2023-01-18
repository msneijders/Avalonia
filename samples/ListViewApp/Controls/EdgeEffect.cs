using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;

namespace ListViewApp.Controls;

public class EdgeEffect : ContentControl
{
    public EdgeEffect()
    {
        AddHandler(Gestures.ScrollGestureEvent, OnScrollGesture);
        AddHandler(Gestures.ScrollGestureEndedEvent, OnScrollGestureEnded);
    }

    private void OnScrollGestureEnded(object? sender, ScrollGestureEndedEventArgs e)
    {
    }

    private void OnScrollGesture(object? sender, ScrollGestureEventArgs e)
    {
    }
}
