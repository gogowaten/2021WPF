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

namespace _20210410_画像縮小Bilinear
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapSource MyBitmapSource;
        private byte[] pixels;
        public MainWindow()
        {
            InitializeComponent();
        }

        private BitmapSource Bilinear(BitmapSource source,int yoko,int tate)
        {
            int w = source.PixelWidth;
            int h = source.PixelHeight;
            int stride = (w * source.Format.BitsPerPixel + 7) / 8;
            pixels = new byte[h * stride];
            source.CopyPixels(pixels, stride, 0);

            double scaleW = w / yoko;
            double scaleH = h / tate;
            for (int y = 0; y < tate; y++)
            {
                for (int x = 0; x < yoko; x++)
                {
                    double xx = (x + 0.5) * scaleW;



                    double yy = (y + 0.5) * scaleH;
                    int p = y * stride + x;
                }
            }
        }

        /// <summary>
        /// PixelFormatをGray8固定、dpiは指定してファイルから画像読み込み
        /// </summary>
        /// <param name="filePath">フルパス</param>
        /// <param name="dpiX"></param>
        /// <param name="dpiY"></param>
        /// <returns></returns>
        private (BitmapSource bitmapSource, byte[] pixels) MakeBitmapSourceGray8FromFile(string filePath, double dpiX = 96, double dpiY = 96)
        {
            BitmapSource source = null;
            byte[] pixels = Array.Empty<byte>();
            try
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    source = BitmapFrame.Create(stream);
                    if (source.Format != PixelFormats.Gray8)
                    {
                        source = new FormatConvertedBitmap(source, PixelFormats.Gray8, null, 0);
                    }
                    int w = source.PixelWidth;
                    int h = source.PixelHeight;
                    int stride = (w * source.Format.BitsPerPixel + 7) / 8;
                    pixels = new byte[h * stride];
                    source.CopyPixels(pixels, stride, 0);
                    source = BitmapSource.Create(w, h, dpiX, dpiY, source.Format, source.Palette, pixels, stride);
                };
            }
            catch (Exception)
            { }
            return (source, pixels);
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            //ファイルパス取得
            var datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            var paths = datas.ToList();
            paths.Sort();

            (MyBitmapSource, pixels) = MakeBitmapSourceGray8FromFile(paths[0]);
            MyImage.Source = MyBitmapSource;

        }

        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
