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

namespace _20210121_画像切り抜き_複数Rect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Rect MyR1;
        private Rect MyR2;
        private Rect MyR3;
        private List<Rect> MyRects;
        private Geometry MyGeometry1;
        private Geometry MyGeometry2;
        private Geometry MyGeometry3;
        private List<Geometry> MyGeometryList123;
        private List<Geometry> MyGeometryList132;
        private BitmapSource MyBitmapSource;

        public MainWindow()
        {
            InitializeComponent();


            MyInitialize();

            BB(MyBitmapSource, MyRects);

        }

        private void MyInitialize()
        {
            MyR1 = new Rect(5, 110, 85, 60);
            MyR2 = new Rect(65, 135, 130, 130);
            MyR3 = new Rect(270, 50, 135, 130);
            MyRects = new() { MyR1, MyR2, MyR3 };

            MyGeometry1 = new RectangleGeometry(MyR1);
            MyGeometry2 = new RectangleGeometry(MyR2);
            MyGeometry3 = new RectangleGeometry(MyR3);
            MyGeometryList123 = new List<Geometry>
            {
                MyGeometry1,
                MyGeometry2,
                MyGeometry3
            };
            MyGeometryList132 = new List<Geometry>
            {
                MyGeometry1,
                MyGeometry3,
                MyGeometry2
            };






            var img = new BitmapImage(new Uri(@"D:\ブログ用\チェック用2\WP_20201222_10_21_40_Pro_2020_12_22_午後わてん_ラーメン.jpg"));
            MyOriginImage.Source = img;
            MyBitmapSource = img;
        }

        private BitmapSource CroppedBitmapEx1(BitmapSource source, Geometry geometry)
        {
            var dv = new DrawingVisual();
            dv.Clip = geometry;//ここだけ違う
            dv.Offset = new Vector(-geometry.Bounds.X, -geometry.Bounds.Y);

            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawImage(source, new Rect(0, 0, source.PixelWidth, source.PixelHeight));
            }

            //GeometryのBoundsはdouble型で、中途半端な値になっていることが在るので
            //小数点以下を四捨五入で整えた値をbitmapの幅と高さに使う
            int w = (int)Math.Round(geometry.Bounds.Width, MidpointRounding.AwayFromZero);
            int h = (int)Math.Round(geometry.Bounds.Height, MidpointRounding.AwayFromZero);
            var bmp = new RenderTargetBitmap(w, h, source.DpiX, source.DpiY, PixelFormats.Pbgra32);
            bmp.Render(dv);

            return bmp;
        }


        private BitmapSource CroppedBitmapEx2(BitmapSource source, Geometry geometry)
        {
            var dv = new DrawingVisual();
            dv.Offset = new Vector(-geometry.Bounds.X, -geometry.Bounds.Y);

            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.PushClip(geometry);//ここだけ違う
                dc.DrawImage(source, new Rect(0, 0, source.PixelWidth, source.PixelHeight));
            }

            //GeometryのBoundsはdouble型で、中途半端な値になっていることが在るので
            //小数点以下を四捨五入で整えた値をbitmapの幅と高さに使う
            int w = (int)Math.Round(geometry.Bounds.Width, MidpointRounding.AwayFromZero);
            int h = (int)Math.Round(geometry.Bounds.Height, MidpointRounding.AwayFromZero);
            var bmp = new RenderTargetBitmap(w, h, source.DpiX, source.DpiY, PixelFormats.Pbgra32);
            bmp.Render(dv);

            return bmp;
        }

        private void BB(BitmapSource source, List<Rect> rectList)
        {
            var dv = new DrawingVisual();

            using (DrawingContext dc = dv.RenderOpen())
            {
                foreach (var rect in rectList)
                {
                    dc.DrawImage(MyCroppe(source, rect), rect);
                }
            }

            dv.Offset = new Vector(-dv.ContentBounds.X, -dv.ContentBounds.Y);

            var bmp = new RenderTargetBitmap(
                (int)Math.Ceiling(dv.ContentBounds.Width),
                (int)Math.Ceiling(dv.ContentBounds.Height),
                96, 96, PixelFormats.Pbgra32);

            bmp.Render(dv);
        }


        private BitmapSource MyCroppe(BitmapSource source, Rect rect)
        {
            return new CroppedBitmap(source, new Int32Rect(
                Sisyagonyuu(rect.X), Sisyagonyuu(rect.Y), Sisyagonyuu(rect.Width), Sisyagonyuu(rect.Height)));
        }
        private int Sisyagonyuu(double value)
        {
            return (int)(value + 0.5);
        }
        private int Ceiling(double value)
        {
            return (int)Math.Ceiling(value);
        }

        //Geometryの連結方法1～3
        //方法1
        //PathGeometryにAddGeometryで連結、
        //領域の重なった部分の処理ルールはFillRuleを使う、Nonzeroでor、EvenOddでxorになると思う
        private Geometry CombineGeometry1(List<Geometry> geometries, FillRule fillRule = FillRule.Nonzero)
        {
            PathGeometry pg = new() { FillRule = fillRule };
            foreach (var item in geometries)
            {
                pg.AddGeometry(item);
            }
            return pg;
        }
        //方法2
        //GeometryGroupを使う、方法1同様重なった領域はFillRuleで指定
        private Geometry CombineGeometry2(List<Geometry> geometries, FillRule fillRule = FillRule.Nonzero)
        {
            var gg = new GeometryGroup() { FillRule = fillRule };
            gg.Children = new GeometryCollection(geometries);
            return gg;
        }
        //方法3
        //GeometryのCombineを使う方法
        //整数のRectでも組み合わせる順番によって、小数点以下(誤差)が発生したりしなかったりする、という欠点があるけど
        //領域の重なった部分の処理は柔軟なGeometryCombineModeが使える
        private Geometry CombineGeometry3(List<Geometry> geometries, GeometryCombineMode mode = GeometryCombineMode.Union)
        {
            PathGeometry pg = Geometry.Combine(geometries[0], geometries[1], mode, null);
            for (int i = 2; i < geometries.Count; i++)
            {
                pg = Geometry.Combine(pg, geometries[i], mode, null);
            }
            return pg;
        }


        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = CroppedBitmapEx1(
                (BitmapSource)MyOriginImage.Source,
                Geometry.Combine(MyGeometry1, MyGeometry2, GeometryCombineMode.Union, null));
        }

        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            var geo = Geometry.Combine(MyGeometry1, MyGeometry3, GeometryCombineMode.Union, null);
            MyImage.Source = CroppedBitmapEx2((BitmapSource)MyOriginImage.Source, geo);
            //MyImage.Source = CroppedBitmapEx2((BitmapSource)MyOriginImage.Source, Geometry.Combine(MyGeometry1, MyGeometry3, GeometryCombineMode.Union, null));
        }

        private void MyButton3_Click(object sender, RoutedEventArgs e)
        {
            var pg = CombineGeometry1(MyGeometryList132);//おk
            //var pg = CombineGeometry2(MyGeometryList132);//おk
            //var pg = CombineGeometry3(MyGeometryList132);//ずれる

            MyImage.Source = CroppedBitmapEx1((BitmapSource)MyOriginImage.Source, pg);

        }
    }
}
