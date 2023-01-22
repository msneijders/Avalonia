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

// 
// https://github.com/npolyak/NP.Avalonia.Demos/blob/main/CustomBehaviors/NP.Demos.DragBehaviorSample/MainWindow.axaml
//

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

        public void Update(TimeSpan time)
        {
            var progress = Math.Max(0.0, Math.Min(1.0, _st.ElapsedMilliseconds / 350.0));
            var e = _easing.Ease(progress);
            var value = (_stretch_y - 1.0) * e;

            if (progress >= 1.0)
            {
                _needsUpdate = false;
                _renderTimer.Tick -= Update; // zo loopt android niet vast
                Dispatcher.UIThread.Post(ResetStretch, DispatcherPriority.Render);
            }
            else
            {
                //StretchVertically(_stretch_y - value);
                Dispatcher.UIThread.Post(() => StretchVertically(_stretch_y - value), DispatcherPriority.Render);
            }
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

            //v.TransformMatrix = new System.Numerics.Matrix4x4(
            //    1f, 0f, 0f, 0f,
            //    0f, (float)value, 0f, 0f,
            //    0f, 0f, 1f, 0f,
            //    0f, _stretchedBottom ? -(float)(v.Size.Y * (value - 1.0)) : 0f, 0f, 1f
            //    );

            //_child.RenderTransformOrigin = new RelativePoint(0.5, _stretchedBottom ? 1.0 : 0.0, RelativeUnit.Relative);
            //_child.RenderTransform = new MatrixTransform(new Matrix(
            //1.0, 0.0, 0.0,
            //0.0, value, 0.0,
            //0.0, 0.0, 1.0));
        }

        //class AnimationTest : CompositionObject
        //{
        //    Stopwatch _sw = Stopwatch.StartNew();

        //    public AnimationTest()
        //    {
        //    }
        //}


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
            //v.StartAnimation(nameof(v.Scale), re);

            DispatcherTimer.RunOnce(ResetStretch, duration);

            ////a.SetReferenceParameter("stretch_y", v);

            //a.SetMatrix4x4Parameter("scaling", Matrix4x4.Identity);
            //a.Expression = "scaling";

            ////a.InsertKeyFrame(0f, v.Scale, new Avalonia.Animation.Easings.CubicEaseOut());
            ////a.InsertKeyFrame(1f, Vector3.One, new Avalonia.Animation.Easings.CubicEaseOut());
            //v.StartAnimation("TransformMatrix", a);
            //a.Dispose();0

            //v.TransformMatrix = new System.Numerics.Matrix4x4(
            //    1f, 0f, 0f, 0f,
            //    0f, (float)value, 0f, 0f,
            //    0f, 0f, 1f, 0f,
            //    0f, _stretchedBottom ? -(float)(v.Size.Y * (value - 1.0)) : 0f, 0f, 1f
            //    );

            //_st.Restart();
            //_renderTimer.Tick += Update; // dit loopt vast op android
        }
    }
}
