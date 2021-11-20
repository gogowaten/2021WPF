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
            TextDecorationCollection textDecorations = new();
            textDecorations.Add(textDecoration);

            formattedText.SetTextDecorations(textDecorations);

            TextBox textBox = new();
            textBox.Text = formattedText;
            textBox.

            var geo = formattedText.BuildGeometry(new Point());
            Pen MyPen = new(Brushes.Magenta, 1);
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
            grid.Width = renderRect.Width;
            grid.Height = renderRect.Height;


            Path MyPath = new();
            MyStackPanel.Children.Add(MyPath);
            MyPath.Data = geo;            
            MyPath.Fill = Brushes.Aqua;
            MyPath.Stroke = Brushes.Magenta;
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
            dropShadowEffect.Color = Colors.Magenta;//色
            dropShadowEffect.Direction = 315;//方向
            dropShadowEffect.Opacity = 1.0;//不透明度
            dropShadowEffect.ShadowDepth = 10;//距離
            MyPath.Effect = dropShadowEffect;


            //ぼかし＆ドロップシャドウはできない？
            System.Windows.Media.Effects.Effect effect;
            TextEffect textEffect = new();
            

        }


    }
}
