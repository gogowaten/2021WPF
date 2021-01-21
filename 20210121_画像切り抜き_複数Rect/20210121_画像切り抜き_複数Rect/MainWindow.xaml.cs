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
        private Geometry MyGeometry1;
        private Geometry MyGeometry2;
        private Geometry MyGeometry3;

        public MainWindow()
        {
            InitializeComponent();

            MyInitialize();

        }

        private void MyInitialize()
        {

            MyGeometry1 = new RectangleGeometry(new Rect(5, 110, 85, 60));
            MyGeometry2 = new RectangleGeometry(new Rect(65, 135, 130, 130));
            MyGeometry3 = new RectangleGeometry(new Rect(270, 50, 135, 130));



            GeometryGroup g12 = new GeometryGroup();
            g12.Children.Add(MyGeometry1);
            g12.Children.Add(MyGeometry2);
            g12.Children.Add(MyGeometry3);





            CombinedGeometry ff1 = new CombinedGeometry(GeometryCombineMode.Union, MyGeometry1, MyGeometry2);
            ff1 = new CombinedGeometry(GeometryCombineMode.Union, ff1, MyGeometry3);
            CombinedGeometry ff2 = new CombinedGeometry(GeometryCombineMode.Union, MyGeometry1, MyGeometry3);
            ff2 = new CombinedGeometry(GeometryCombineMode.Union, ff2, MyGeometry2);

            CombinedGeometry hh1 = new CombinedGeometry(GeometryCombineMode.Union, MyGeometry1, MyGeometry2);
            PathGeometry hh2 = CombinedGeometry.Combine(hh1, MyGeometry3, GeometryCombineMode.Union, null);


            var img = new BitmapImage(new Uri(@"D:\ブログ用\チェック用2\WP_20201222_10_21_40_Pro_2020_12_22_午後わてん_ラーメン.jpg"));
            MyOriginImage.Source = img;
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
            ////GeometryのCombineを使う方法
            ////整数のRectでも組み合わせる順番によって、小数点以下が発生したりしなかったりする、という欠点がある
            //PathGeometry geo = Geometry.Combine(MyGeometry1, MyGeometry3, GeometryCombineMode.Union, null);
            //geo = Geometry.Combine(geo, MyGeometry2, GeometryCombineMode.Union, null);
            //MyImage.Source = CroppedBitmapEx1((BitmapSource)MyOriginImage.Source, geo);

            ////GeometryGroupを使う方法
            ////GeometryCombineModeは指定できないけど
            ////FillRuleのNonzeroでUnionと同じ効果、EvenOddでXorと同じ効果
            //GeometryGroup gg = new() { FillRule = FillRule.Nonzero };
            //gg.Children.Add(MyGeometry1);
            //gg.Children.Add(MyGeometry3);
            //gg.Children.Add(MyGeometry2);
            //MyImage.Source = CroppedBitmapEx1((BitmapSource)MyOriginImage.Source, gg);

            ////GetFlattenedPathGeometryを使う方法、これもGeometryCombineModeは指定できないのでFillRuleを使う
            //PathGeometry pg = MyGeometry1.GetFlattenedPathGeometry();
            //pg.AddGeometry(MyGeometry3);
            //pg.AddGeometry(MyGeometry2);
            //pg.FillRule = FillRule.Nonzero;
            //MyImage.Source = CroppedBitmapEx1((BitmapSource)MyOriginImage.Source, pg);

            //PathGeometryを直接使う方法、これもGeometryCombineModeは指定できないのでFillRuleを使う
            PathGeometry pg = new() { FillRule = FillRule.Nonzero };
            pg.AddGeometry(MyGeometry1);
            pg.AddGeometry(MyGeometry3);
            pg.AddGeometry(MyGeometry2);
            MyImage.Source = CroppedBitmapEx1((BitmapSource)MyOriginImage.Source, pg);

            
        }
    }
}
