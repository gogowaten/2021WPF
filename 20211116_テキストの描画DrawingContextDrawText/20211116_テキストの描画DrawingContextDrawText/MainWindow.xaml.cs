using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;

namespace _20211116_テキストの描画DrawingContextDrawText
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string text = "WPFの文字列描画\n" +
                "ゆっくり霊夢とゆっくり魔理沙の\nゆっくりしていってね！！！";
            CultureInfo cultureInfo = CultureInfo.CurrentUICulture;
            FlowDirection flowDirection = FlowDirection.LeftToRight;
            Typeface typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
            //FormattedText fText = new(text, cultureInfo, flowDirection, typeface, 50, new SolidColorBrush(), 96);
            FormattedText fText = new(text, cultureInfo, flowDirection, new Typeface("Meiryo UI"), 50, Brushes.Red, 96);

            var textWidth = fText.GetMaxTextWidths();
            var geo = fText.BuildGeometry(new Point(0, 0));

            MyPath.Data = geo;
            MyPath.Fill = Brushes.Transparent;
            MyPath.Stroke = Brushes.MediumAquamarine;
            MyPath.StrokeThickness = 2;

            DrawingVisual dv = new();
            //ro.DrawGlyphRun
            using (DrawingContext context = dv.RenderOpen())
            {
                context.DrawText(fText, new Point());
                //context.DrawText(fText, new Point());
            }
            RenderTargetBitmap render = new((int)dv.ContentBounds.Width, (int)dv.ContentBounds.Height, 96, 96, PixelFormats.Pbgra32);
            render.Render(dv);

            Pen MyPen = new(Brushes.MediumAquamarine, 2);
            Rect renderRect = geo.GetRenderBounds(MyPen);
            Geometry geo2 = fText.BuildHighlightGeometry(new Point());

            TranslateTransform tt = new(-renderRect.X, -renderRect.Y);
            geo.Transform = tt;
            

            using (DrawingContext context = dv.RenderOpen())
            {
                context.DrawGeometry(Brushes.Transparent, MyPen, geo);
            }

            Rect dvRect = dv.ContentBounds;
            RenderTargetBitmap render2 = new((int)dvRect.Width + 1, (int)dvRect.Height + 1, 96, 96, PixelFormats.Pbgra32);
            render2.Render(dv);

            //RenderTargetBitmap render2 = new((int)fText.Width, (int)fText.Height, 96, 96, PixelFormats.Pbgra32);
            //render2.Render(dv);

            //RenderTargetBitmap render2 = new((int)geo.Bounds.Right, (int)geo.Bounds.Bottom, 96, 96, PixelFormats.Pbgra32);
            //render2.Render(dv);

            //RenderTargetBitmap render2 = new((int)dv.ContentBounds.Width, (int)dv.ContentBounds.Height, 96, 96, PixelFormats.Pbgra32);
            //render2.Render(dv);
            

            Rect georect = geo.Bounds;
            var twidth = fText.Width;
            var tHeight = fText.Height;
            var tExtent = fText.Extent;
            var tBaseLine = fText.Baseline;
        }
    }

}
