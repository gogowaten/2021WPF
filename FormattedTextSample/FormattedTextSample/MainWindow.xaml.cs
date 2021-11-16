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

//WPFのFormattedTextクラスのプロパティを表示するサンプルアプリケーション | 山本隆の開発日誌
//https://www.gesource.jp/weblog/?p=8052

//GitHub - gesource / WPF - Sample: WPFのサンプル
//    https://github.com/gesource/WPF-Sample
//↑からのコピペ改変

namespace FormattedTextSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        public FontFamily SelectedFontFamily
        {
            get => new FontFamily(myView.FontName);
            set => myView.FontName = value.Source;
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void CheckBoxWidth_Checked(object sender, RoutedEventArgs e) => myView.DrawWidth = true;
        private void CheckBoxWidth_Unchecked(object sender, RoutedEventArgs e) => myView.DrawWidth = false;

        private void CheckBoxHeight_Checked(object sender, RoutedEventArgs e) => myView.DrawHeight = true;
        private void CheckBoxHeight_Unchecked(object sender, RoutedEventArgs e) => myView.DrawHeight = false;

        private void CheckBoxBaseline_Checked(object sender, RoutedEventArgs e) => myView.DrawBaseline = true;
        private void CheckBoxBaseline_Unchecked(object sender, RoutedEventArgs e) => myView.DrawBaseline = false;

        private void CheckBoxOverhangAfter_Checked(object sender, RoutedEventArgs e) => myView.DrawOverhangAfter = true;
        private void CheckBoxOverhangAfter_Unchecked(object sender, RoutedEventArgs e) => myView.DrawOverhangAfter = false;

        private void CheckBoxOverhangLeading_Checked(object sender, RoutedEventArgs e) => myView.DrawOverhangLeading = true;
        private void CheckBoxOverhangLeading_Unchecked(object sender, RoutedEventArgs e) => myView.DrawOverhangLeading = false;

        private void CheckBoxOverhangTrailing_Checked(object sender, RoutedEventArgs e) => myView.DrawOverhangTrailing = true;
        private void CheckBoxOverhangTrailing_Unchecked(object sender, RoutedEventArgs e) => myView.DrawOverhangTrailing = false;

        private void CheckBoxExtent_Checked(object sender, RoutedEventArgs e) => myView.DrawExtent = true;
        private void CheckBoxExtent_Unchecked(object sender, RoutedEventArgs e) => myView.DrawExtent = false;
    }
    public class MyView : UserControl
    {
        public string FontName { get => fontName; set { fontName = value; InvalidateVisual(); } }
        public bool DrawWidth { set { drawWidth = value; InvalidateVisual(); } }
        public bool DrawHeight { set { drawHeight = value; InvalidateVisual(); } }
        public bool DrawBaseline { set { drawBaseline = value; InvalidateVisual(); } }
        public bool DrawOverhangAfter { set { drawOverhangAfter = value; InvalidateVisual(); } }
        public bool DrawOverhangLeading { set { drawOverhangLeading = value; InvalidateVisual(); } }
        public bool DrawOverhangTrailing { set { drawOverhangTrailing = value; InvalidateVisual(); } }
        public bool DrawExtent { set { drawExtent = value; InvalidateVisual(); } }

        private string fontName = "Verdana";
        private bool drawWidth = false;
        private bool drawHeight = false;
        private bool drawBaseline = false;
        private bool drawOverhangAfter = false;
        private bool drawOverhangLeading = false;
        private bool drawOverhangTrailing = false;
        private bool drawExtent = false;

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            DrawText(drawingContext, "x", new Point(100, 50));
            DrawText(drawingContext, "j", new Point(350, 50));
            DrawText(drawingContext, "M", new Point(600, 50));
        }

        private void DrawText(DrawingContext drawingContext, string text, Point startPoint)
        {
            FormattedText formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface(fontName),
                196,
                Brushes.Black,
                96);

            var baseline = formattedText.Baseline;
            var overhangAfter = formattedText.OverhangAfter;
            var overhangLeading = formattedText.OverhangLeading;
            var overhangTrailing = formattedText.OverhangTrailing;
            var extent = formattedText.Extent;

            Pen pen1 = new Pen(Brushes.Red, 1);
            Pen pen2 = new Pen(Brushes.LightSlateGray, 1)
            {
                DashStyle = DashStyles.DashDot
            };

            if (drawWidth)
            {
                // Width
                // 行の先頭から末尾までの距離
                var left = startPoint.X;
                var right = startPoint.X + formattedText.Width;
                var top = startPoint.Y;
                var bottom = startPoint.Y + formattedText.Height; ;
                drawingContext.DrawLine(pen1, new Point(left, top), new Point(right, top));
                drawingContext.DrawLine(pen2, new Point(left, top), new Point(left, bottom));
                drawingContext.DrawLine(pen2, new Point(right, top), new Point(right, bottom));

                DrawCaption(drawingContext, "Width", left, top - 20);
            }

            if (drawHeight)
            {
                // Height
                // 行の上から下までの距離
                var left = startPoint.X;
                var right = startPoint.X + formattedText.Width;
                var top = startPoint.Y;
                var bottom = startPoint.Y + formattedText.Height;
                drawingContext.DrawLine(pen1, new Point(left, top), new Point(left, bottom));
                drawingContext.DrawLine(pen2, new Point(left, top), new Point(right, top));
                drawingContext.DrawLine(pen2, new Point(left, bottom), new Point(right, bottom));

                DrawCaption(drawingContext, "Height", left - 60, top);
            }

            if (drawBaseline)
            {
                // Baseline
                var left = startPoint.X;
                var right = startPoint.X + formattedText.Width;
                var top = startPoint.Y + baseline;
                drawingContext.DrawLine(pen1, new Point(left, top), new Point(right, top));

                DrawCaption(drawingContext, "Baseline", left - 78, top - 18);
            }

            if (drawOverhangAfter)
            {
                // OverhangAfter
                var left = startPoint.X;
                var right = startPoint.X + formattedText.Width;
                var top = startPoint.Y + formattedText.Height + overhangAfter;
                var bottom = startPoint.Y + formattedText.Height;
                drawingContext.DrawLine(pen1, new Point(left, top), new Point(left, bottom));
                drawingContext.DrawLine(pen2, new Point(left, top), new Point(right, top));
                drawingContext.DrawLine(pen2, new Point(left, bottom), new Point(right, bottom));

                DrawCaption(drawingContext, "OverhangAfter", left - 70, top);
            }

            if (drawOverhangLeading)
            {
                // OverhangLeading
                var left = startPoint.X;
                var right = startPoint.X + overhangLeading;
                var top = startPoint.Y;
                var bottom = startPoint.Y + formattedText.Height;
                drawingContext.DrawLine(pen1, new Point(left, bottom), new Point(right, bottom));
                drawingContext.DrawLine(pen2, new Point(left, top), new Point(left, bottom));
                drawingContext.DrawLine(pen2, new Point(right, top), new Point(right, bottom));

                DrawCaption(drawingContext, "OverhangLeading", left, bottom);
            }

            if (drawOverhangTrailing)
            {
                // OverhangTrailing
                var left = startPoint.X + formattedText.Width - overhangTrailing;
                var right = startPoint.X + formattedText.Width;
                var top = startPoint.Y;
                var bottom = startPoint.Y + formattedText.Height;
                drawingContext.DrawLine(pen1, new Point(left, bottom), new Point(right, bottom));
                drawingContext.DrawLine(pen2, new Point(left, top), new Point(left, bottom));
                drawingContext.DrawLine(pen2, new Point(right, top), new Point(right, bottom));

                DrawCaption(drawingContext, "OverhangTrailing", left, bottom - 18);
            }

            if (drawExtent)
            {
                // Extent
                // 描画するテキストの高さ
                var left = startPoint.X;
                var right = startPoint.X + formattedText.Width;
                var top = startPoint.Y + formattedText.Height + overhangAfter - extent;
                var bottom = startPoint.Y + formattedText.Height + overhangAfter;
                drawingContext.DrawLine(pen1, new Point(left, top), new Point(left, bottom));
                drawingContext.DrawLine(pen2, new Point(left, top), new Point(right, top));
                drawingContext.DrawLine(pen2, new Point(left, bottom), new Point(right, bottom));

                DrawCaption(drawingContext, "Extent", left - 60, top);
            }

            drawingContext.DrawText(formattedText, startPoint);
        }

        private void DrawCaption(DrawingContext drawingContext, string text, double originX, double originY)
        {
            FormattedText formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("Consolas"),
                18,
                Brushes.Red,
                96);
            drawingContext.DrawText(formattedText, new Point(originX, originY));
        }
    }
}
