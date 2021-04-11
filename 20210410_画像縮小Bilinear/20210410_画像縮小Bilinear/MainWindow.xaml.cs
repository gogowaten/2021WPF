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
        private BitmapSource MyScaledBitmap;

        public MainWindow()
        {
            InitializeComponent();

            this.Top = 0;
            this.Left = 0;
            this.Height = 100;


            MyBitmapSource = MakeBitmapSourceGray8FromFile(@"D:\ブログ用\テスト用画像\縮小拡大用\3x3.bmp");
            //MyBitmapSource = MakeBitmapSourceGray8FromFile(@"D:\ブログ用\テスト用画像\縮小拡大用\4x1_250_200_150_100.bmp");
            MyImage.Source = MyBitmapSource;
            MyScaledImage.Source = MyBitmapSource;
            //RenderOptions.SetBitmapScalingMode(MyImage, BitmapScalingMode.NearestNeighbor);
        }

        //縦1ピクセルの画像専用
        private BitmapSource Bilinear(BitmapSource source, int yoko, int tate)
        {
            int sourceWidth = source.PixelWidth;
            int sourceHeight = source.PixelHeight;
            int stride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[sourceHeight * stride];
            source.CopyPixels(pixels, stride, 0);

            double scaleW = (double)sourceWidth / yoko;
            double scaleH = (double)sourceHeight / tate;
            int scaledStride = (yoko * source.Format.BitsPerPixel + 7) / 8;
            byte[] resultPixels = new byte[tate * scaledStride];
            for (int y = 0; y < tate; y++)
            {
                int yPoint = y * scaledStride;
                for (int x = 0; x < yoko; x++)
                {
                    double referenceX = (x + 0.5) * scaleW;//参照座標
                    int leftPoint = (int)(referenceX - 0.5);//参照座標の左ピクセル座標
                    double leftRatio = referenceX - (int)referenceX;//左ピクセルの割合
                    //参照先の左右ピクセルの値にそれぞれの割合をかけて足し算で完成
                    int lp = leftPoint + stride * y;
                    double left = pixels[lp] * leftRatio;
                    double right = pixels[lp + 1] * (1 - leftRatio);
                    double resultHorizontal = left + right;

                    double referenceY = (y + 0.5) * scaleH;//参照座標
                    int topPoint = (int)(referenceY - 0.5);//参照座標の上ピクセル座標
                    double topRatio = referenceY - (int)referenceY;//上ピクセルの割合
                    //参照先の上下ピクセルの値にそれぞれの割合をかけて足し算で完成
                    int tp = topPoint * stride + x;
                    double top = pixels[tp] * topRatio;
                    double bottom = pixels[tp + stride] * (1 - topRatio);
                    double resultVertical = top + bottom;
                    //縦横合計して半分にして四捨五入
                    byte result = (byte)(((resultHorizontal + resultVertical) / 2) + 0.5);
                    resultPixels[yPoint + x] = result;

                }
            }
            BitmapSource rBitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);

            return rBitmap;
        }

        //縦1ピクセルの画像専用
        private BitmapSource BilinearTestOnlyHeightOne(BitmapSource source, int yoko, int tate)
        {
            int sourceWidth = source.PixelWidth;
            int sourceHeight = source.PixelHeight;
            int stride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[sourceHeight * stride];
            source.CopyPixels(pixels, stride, 0);

            double scaleW = (double)sourceWidth / yoko;
            double scaleH = (double)sourceHeight / tate;
            byte[] resultPixels = new byte[tate * (yoko * source.Format.BitsPerPixel + 7) / 8];
            for (int y = 0; y < tate; y++)
            {
                for (int x = 0; x < yoko; x++)
                {
                    double xx = x + 0.5;//      ピクセルの中心座標
                    double a = xx * scaleW;//   参照先中心座標
                    double b = a - 0.5;//       aの左端座標
                    double c = a - (int)a;//    aの小数部分(これが割合になる)
                    int d = (int)b;//           小数切り捨てで左ピクセル座標
                    int e = d + 1;//            右ピクセル座標
                    byte f = pixels[d];//       左ピクセルの色
                    byte g = pixels[e];//       右ピクセルの色
                    double h = f * c;//         左ピクセルの色*割合
                    double i = g * (1 - c);//   右ピクセルの色*割合
                    double j = h + i;//         左右合計
                    //四捨五入で完成                    
                    resultPixels[x] = (byte)(j + 0.5);

                    ////↑をまとめた
                    //double referenceX = (x + 0.5) * scaleW;//参照座標
                    //int leftPixel = (int)(referenceX - 0.5);//参照座標の左ピクセル座標
                    //double leftRatio = referenceX - (int)referenceX;//左ピクセルの割合
                    ////参照先の左右ピクセルの値にそれぞれの割合をかけて足し算で完成
                    //double left = pixels[leftPixel] * leftRatio;
                    //double right = pixels[leftPixel + 1] * (1 - leftRatio);
                    //byte result = (byte)(left + right + 0.5);
                    //resultPixels[x] = result;

                }
            }
            BitmapSource rBitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, stride);

            return rBitmap;
        }

        /// <summary>
        /// PixelFormatをGray8固定、dpiは指定してファイルから画像読み込み
        /// </summary>
        /// <param name="filePath">フルパス</param>
        /// <param name="dpiX"></param>
        /// <param name="dpiY"></param>
        /// <returns></returns>
        private (BitmapSource bitmapSource, byte[] pixels) MakeBitmapSourceWithPixelsGray8FromFile(string filePath, double dpiX = 96, double dpiY = 96)
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
        private BitmapSource MakeBitmapSourceGray8FromFile(string filePath, double dpiX = 96, double dpiY = 96)
        {
            BitmapSource source = null;
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
                    byte[] pixels = new byte[h * stride];
                    source.CopyPixels(pixels, stride, 0);
                    source = BitmapSource.Create(w, h, dpiX, dpiY, source.Format, source.Palette, pixels, stride);
                };
            }
            catch (Exception)
            { }
            return source;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            //ファイルパス取得
            var datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            var paths = datas.ToList();
            paths.Sort();
            MyBitmapSource = MakeBitmapSourceGray8FromFile(paths[0]);
            MyImage.Source = MyBitmapSource;

        }

        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource source = Bilinear(MyBitmapSource, 2, 2);
            //BitmapSource source = BilinearTestOnlyHeightOne(MyBitmapSource, 3, 1);
            MyScaledBitmap = source;
            MyScaledImage.Source = source;
        }

        private void MyGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            MyScaledImage.Visibility = Visibility.Hidden;
            MyImage.Visibility = Visibility.Visible;
        }

        private void MyGrid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            MyScaledImage.Visibility = Visibility.Visible;
            MyImage.Visibility = Visibility.Hidden;
        }

        private void MyButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage(MyScaledBitmap);
        }
    }
}
