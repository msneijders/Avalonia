using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using Avalonia.Platform;
using Avalonia.Svg.Skia;

namespace ListViewApp.Controls
{
    internal static class Extensions
    {
        public static T GetService<T>(this IServiceProvider sp) => (T)sp?.GetService(typeof(T));
        public static Uri GetContextBaseUri(this IServiceProvider ctx) => ctx.GetService<IUriContext>().BaseUri;
    }

    [TypeConverter(typeof(IconTypeConverter))]
    public class IconImage
    {
        public IImage? Image { get; set; }
    }

    public class IconTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            var s = (string)value;
            if (s.EndsWith(".svg"))
            {
                var baseUri = context?.GetContextBaseUri();
                var source = SvgSource.Load<SvgSource>(s, baseUri);
                return new IconImage() { Image = new SvgImage() { Source = source } };
            }
            else
            {
                var uri = s.StartsWith("/")
                    ? new Uri(s, UriKind.Relative)
                    : new Uri(s, UriKind.RelativeOrAbsolute);

                if (uri.IsAbsoluteUri && uri.IsFile)
                    return new Bitmap(uri.LocalPath);

                var assets = AvaloniaLocator.Current.GetRequiredService<IAssetLoader>();
                var baseuri = context.GetContextBaseUri();
                return new IconImage() { Image = new Bitmap(assets.Open(uri, baseuri)) };
            }
        }
    }

    public class Icon : Control
    {
        public static readonly StyledProperty<IconImage?> SourceProperty =
            AvaloniaProperty.Register<Image, IconImage?>(nameof(Source));

        /// <summary>
        /// Defines the <see cref="Stretch"/> property.
        /// </summary>
        public static readonly StyledProperty<Stretch> StretchProperty =
            AvaloniaProperty.Register<Image, Stretch>(nameof(Stretch), Stretch.Uniform);

        /// <summary>
        /// Defines the <see cref="StretchDirection"/> property.
        /// </summary>
        public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
            AvaloniaProperty.Register<Image, StretchDirection>(
                nameof(StretchDirection),
                StretchDirection.Both);

        [Content]
        public IconImage? Source
        {
            get { return GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value controlling how the image will be stretched.
        /// </summary>
        public Stretch Stretch
        {
            get { return GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value controlling in what direction the image will be stretched.
        /// </summary>
        public StretchDirection StretchDirection
        {
            get { return GetValue(StretchDirectionProperty); }
            set { SetValue(StretchDirectionProperty, value); }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = Math.Min(finalSize.Width, finalSize.Height);
            return new Size(size, size);
        }

        /// <summary>
        /// Renders the control.
        /// </summary>
        /// <param name="context">The drawing context.</param>
        public sealed override void Render(DrawingContext context)
        {
            var source = Source;

            if (source != null && Bounds.Width > 0 && Bounds.Height > 0)
            {
                Rect viewPort = new Rect(Bounds.Size);
                Size sourceSize = source.Image.Size;

                Vector scale = Stretch.CalculateScaling(Bounds.Size, sourceSize, StretchDirection);
                Size scaledSize = sourceSize * scale;
                Rect destRect = viewPort
                    .CenterRect(new Rect(scaledSize))
                    .Intersect(viewPort);
                Rect sourceRect = new Rect(sourceSize)
                    .CenterRect(new Rect(destRect.Size / scale));

                var interpolationMode = RenderOptions.GetBitmapInterpolationMode(this);

                context.DrawImage(source.Image, sourceRect, destRect, interpolationMode);
            }
        }
    }
}
