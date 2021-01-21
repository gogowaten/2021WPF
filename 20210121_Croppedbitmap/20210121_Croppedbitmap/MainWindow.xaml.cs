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

//イメージ、描画、およびビジュアルによる塗りつぶし - WPF.NET Framework | Microsoft Docs
//https://docs.microsoft.com/ja-jp/dotnet/desktop/wpf/graphics-multimedia/painting-with-images-drawings-and-visuals?view=netframeworkdesktop-4.8

//WPF、Canvasの中に画像として保存したい要素が回転や拡大など変形されていてもOKな方法 - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/14977600


namespace _20210121_Croppedbitmap
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
            var aa = new RectangleGeometry(new Rect(20, 20, 100, 100));
            var bb = new RectangleGeometry(new Rect(160, 40, 100, 100));
            var cc = Geometry.Combine(aa, bb, GeometryCombineMode.Union, null);

            var img = new BitmapImage(new Uri(@"D:\ブログ用\テスト用画像\赤と青のグラデーション.png"));            
            var ib = new ImageBrush(img);
            
            var dv = new DrawingVisual();
            dv.Offset = new Vector(-cc.Bounds.X, -cc.Bounds.Y);
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawGeometry(ib, null, cc);
            }
            //pixelformatsはPbgra32が無難、他のBgr32やBgra32ではエラーになる
            var rb = new RenderTargetBitmap((int)dv.ContentBounds.Width, (int)dv.ContentBounds.Height, img.DpiX, img.DpiY, PixelFormats.Pbgra32);
            //var rb = new RenderTargetBitmap(256, 256, 96, 96, PixelFormats.Pbgra32);
            rb.Render(dv);            
            var source = new Image() { Stretch = Stretch.None };
            source.Source = rb;
            MyStackPanel.Children.Add(source);
            
        }
    }
}
