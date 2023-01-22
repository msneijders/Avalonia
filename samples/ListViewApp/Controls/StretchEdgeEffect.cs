using System;
using System.Diagnostics;
using System.Numerics;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;

namespace ListViewApp.Controls;

public class StretchEdgeEffect : Decorator
{
    StretchEdgeEffectController? _controller;

    public StretchEdgeEffect()
    {
        _controller = new StretchEdgeEffectController(this);
        AddHandler(Gestures.ScrollGestureEvent, OnScrollGesture, handledEventsToo: true);
        AddHandler(Gestures.ScrollGestureEndedEvent, OnScrollGestureEnded, handledEventsToo: true);
    }

    private void OnScrollGesture(object? sender, ScrollGestureEventArgs e)
    {
        _controller?.OnScrollGesture(sender, e);
    }

    private void OnScrollGestureEnded(object? sender, ScrollGestureEndedEventArgs e)
    {
        _controller?.OnScrollGestureEnded(sender, e);
    }

    private class StretchEdgeEffectController
    {
        private readonly Visual _child;

        private double _stretch_y = 1.0;
        private bool _isStretched = false;
        private bool _isReleasing = false;
        private bool _stretchedBottom = false;
        private readonly Stopwatch _st = Stopwatch.StartNew();
        private readonly IRenderTimer _renderTimer;
        private readonly Avalonia.Animation.Easings.Easing _easing = new Avalonia.Animation.Easings.CubicEaseOut();

        private bool _needsUpdate = true;
        public bool NeedsUpdate => _needsUpdate;

        public StretchEdgeEffectController(Visual child)
        {
            _child = child;
            _renderTimer = AvaloniaLocator.Current.GetService<IRenderTimer>()!;
        }

        public void OnScrollGesture(object? sender, ScrollGestureEventArgs e)
        {
            if (e.Handled)
            {
                ReleaseStretch();
                return;
            }

            if (_isReleasing)
                return;

            if (e.Delta.Y == 0)
                return;

            var easing = new Avalonia.Animation.Easings.CubicEaseOut();
            var ease = easing.Ease((_stretch_y - 1.0) * 20.0);

            _stretch_y += Math.Abs(e.Delta.Y) / (1000.0 + (ease * 18000.0));

            if (_stretch_y >= 1.05)
                _stretch_y = 1.05; // max 5% stretching

            e.Handled = true;
            e.ShouldEndScrollGesture = true;

            _stretchedBottom = e.Delta.Y >= 0;
            StretchVertically(_stretch_y);
        }

        public void OnScrollGestureEnded(object? sender, ScrollGestureEndedEventArgs e)
        {
            ReleaseStretch();
        }

        private void ResetStretch()
        {
            _stretch_y = 1.0;
            _isStretched = false;
            _isReleasing = false;
        }

        private void StretchVertically(double value)
        {
            _isStretched = true;

            var v = (ElementComposition.GetElementVisual(_child) as CompositionContainerVisual)!;
            v.CenterPoint = new Vector3(0f, 0f, 0f);
            v.Scale = new Vector3(1f, (float)value, 1f);
            if (_stretchedBottom)
            {
                v.Offset = new Vector3(0f, _stretchedBottom ? -(float)(v.Size.Y * (value - 1.0)) : 0f, 0f);
            }
        }

        private void ReleaseStretch()
        {
            if (!_isStretched)
                return;

            if (_isReleasing)
                return;

            _isReleasing = true;

            TimeSpan duration = TimeSpan.FromMilliseconds(350);
            Avalonia.Animation.Easings.Easing easing = new Avalonia.Animation.Easings.CubicEaseOut();

            var v = (ElementComposition.GetElementVisual(_child) as CompositionContainerVisual)!;
            var releaseAnimationGroup = v.Compositor.CreateAnimationGroup();
            var scalingAnimation = v.Compositor.CreateVector3KeyFrameAnimation();
            scalingAnimation.Duration = duration;
            scalingAnimation.InsertKeyFrame(0f, v.Scale);
            scalingAnimation.InsertKeyFrame(1f, Vector3.One, easing);
            scalingAnimation.Target = nameof(v.Scale);
            releaseAnimationGroup.Add(scalingAnimation);

            if (_stretchedBottom)
            {
                var offsetAnimation = v.Compositor.CreateVector3KeyFrameAnimation();
                offsetAnimation.Duration = duration;
                offsetAnimation.InsertKeyFrame(0f, v.Offset);
                offsetAnimation.InsertKeyFrame(1f, Vector3.Zero, easing);
                offsetAnimation.Target = nameof(v.Offset);
                releaseAnimationGroup.Add(offsetAnimation);
            }
            v.StartAnimationGroup(releaseAnimationGroup);

            // question: another way to know when releaseAnimationGroup is completed?
            DispatcherTimer.RunOnce(ResetStretch, duration);
        }
    }
}
