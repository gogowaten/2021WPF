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

namespace _20210307_画像拡大縮小保存test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();

        }

        private void MyInitialize()
        {
            string path = @"D:\ブログ用\チェック用2\WP_20210228_11_25_51_Pro_2021_02_28_午後わてん.jpg";
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(path);
            bitmapImage.EndInit();
            //bitmapImage.Freeze();

            ImageDrawing imgDraw = new(bitmapImage, new Rect(0, 0, 100, 100));
            //MyCanvas.Background = new DrawingBrush( imgDraw);

            DrawingImage di = new DrawingImage();
            di.Drawing = imgDraw;


            //Image img = new();
            //img.Source = di;
            //img.Stretch = Stretch.None;
            //MyCanvas.Children.Add(img);

            //bitmapImage.DecodePixelWidth = 100;
            //bitmapImage.DecodePixelHeight = 100;

            TransformedBitmap tb = new();
            tb.Source = bitmapImage;
            tb.Transform = new ScaleTransform(0.5, 0.5);

            //DrawingVisual dv = new();
            //using (var dc = dv.RenderOpen())
            //{
            //    dc.DrawImage(bitmapImage, new Rect(0, 0, 200, 200));
            //}
            //RenderTargetBitmap rBitmap = new(200, 200, 96, 96, PixelFormats.Pbgra32);

            //rBitmap.Render(dv);
            //MyCanvas.Children.Add(new Image() { Source = rBitmap });

            MyCanvas.Children.Add(new Image() { Source = tb });
        }
    }
}
