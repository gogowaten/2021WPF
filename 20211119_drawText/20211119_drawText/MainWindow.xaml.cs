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
using System.Windows.Media.Effects;


namespace _20211119_drawText
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string MyText = "WPFで改行付きの\n文字列描画\nゆっくりしていってね！！！";

        public MainWindow()
        {
            InitializeComponent();

            
            
            FormattedText formattedText = new(
                MyText,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    FontFamily,
                    FontStyle,
                    FontWeight,
                    FontStretch),
                40,
                Brushes.Red,
                96);

            //formattedText.SetTextDecorations(TextDecorations.Strikethrough, 1, 5);
            //formattedText.SetTextDecorations(TextDecorations.Strikethrough, 10, 5);
            TextDecoration textDecoration = new();
            textDecoration.Pen = new(Brushes.Black, 1);
            textDecoration.Location = TextDecorationLocation.Strikethrough;
            TextDecorationCollection textDecorations = new();
            textDecorations.Add(textDecoration);

            formattedText.SetTextDecorations(textDecorations);

            TextEffect textEffect = new();
            textEffect.PositionStart = 2;
            MyTextBlock.TextDecorations = textDecorations;
            MyTextBox.TextDecorations = textDecorations;

            DrawingVisual dv = new();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawText(formattedText, new Point());
            }
            Rect drawRect = dv.ContentBounds;
            RenderTargetBitmap targetBitmap = new((int)drawRect.Width, (int)drawRect.Height, 96, 96, PixelFormats.Pbgra32);
            targetBitmap.Render(dv);


            var geo = formattedText.BuildGeometry(new Point());
            Pen MyPen = new(Brushes.DarkMagenta, 1);
            MyPen.LineJoin = PenLineJoin.Round;

            GeometryDrawing geometryDrawing = new(Brushes.Aqua, MyPen, geo);
            DrawingBrush MyBrush = new(geometryDrawing);
            MyBrush.Stretch = Stretch.None;

            Rect geoRect = geo.Bounds;
            Rect renderRect = geo.GetRenderBounds(MyPen);
            AlignmentX alignmentx = MyBrush.AlignmentX;
            AlignmentY alignmentY = MyBrush.AlignmentY;
            
            Grid grid = new();
            grid.Background = MyBrush;
            _ = MyStackPanel.Children.Add(grid);
            grid.Width = drawRect.Width;
            grid.Height = drawRect.Height;


            Path MyPath = new();
            MyStackPanel.Children.Add(MyPath);
            MyPath.Data = geo;            
            MyPath.Fill = Brushes.Aqua;
            MyPath.Stroke = Brushes.DarkMagenta;
            MyPath.StrokeThickness = 1;

            var renderGeo = MyPath.RenderedGeometry;
            var renderW1 = renderGeo.Bounds.Width;
            var renderW2 = renderGeo.GetRenderBounds(MyPen).Width;

            ////ぼかし
            //BlurEffect blurEffect = new();
            //blurEffect.Radius = 3.0;
            //MyPath.Effect = blurEffect;

            //ドロップシャドウ
            DropShadowEffect dropShadowEffect = new();
            dropShadowEffect.BlurRadius = 10.0;//ぼかし半径
            dropShadowEffect.Color = Colors.DarkMagenta;//色
            dropShadowEffect.Direction = 315;//方向
            dropShadowEffect.Opacity = 1.0;//不透明度
            dropShadowEffect.ShadowDepth = 10;//距離
            MyPath.Effect = dropShadowEffect;


            //ぼかし＆ドロップシャドウはできない？
            System.Windows.Media.Effects.Effect effect;
            
            

        }


    }
}
