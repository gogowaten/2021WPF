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
            this.Height = 400;


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

        //縮小専用
        private BitmapSource Bilinear2冗長(BitmapSource source, int yoko, int tate)
        {
            //元画像の画素値の配列作成
            int sourceWidth = source.PixelWidth;
            int sourceHeight = source.PixelHeight;
            int stride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[sourceHeight * stride];
            source.CopyPixels(pixels, stride, 0);

            //縮小後の画像の画素値の配列用
            double yokoScale = (double)sourceWidth / yoko;//横倍率
            double tateScale = (double)sourceHeight / tate;
            int scaledStride = (yoko * source.Format.BitsPerPixel + 7) / 8;
            byte[] resultPixels = new byte[tate * scaledStride];

            for (int y = 0; y < tate; y++)
            {
                for (int x = 0; x < yoko; x++)
                {
                    double rX = (x + 0.5) * yokoScale;//参照x
                    double rY = (y + 0.5) * tateScale;

                    //ピクセルの境目
                    int p1X = (int)(rX + 0.5);
                    int p1Y = (int)(rY + 0.5);

                    //参照範囲の四隅
                    double p2X = rX - 0.5;
                    double p2Y = rY - 0.5;
                    double p3X = rX + 0.5;
                    double p3Y = rY - 0.5;
                    double p4X = rX - 0.5;
                    double p4Y = rY + 0.5;
                    double p5X = rX + 0.5;
                    double p5Y = rY + 0.5;

                    //4区の辺の長さ
                    double aW = p1X - p2X;
                    double aH = p1Y - p2Y;
                    double bW = p3X - p1X;
                    double bH = p1Y - p3Y;
                    double cW = p1X - p4X;
                    double cH = p4Y - p1Y;
                    double dW = p5X - p1X;
                    double dH = p5Y - p1Y;

                    //4区の面積
                    double aArea = aW * aH;
                    double bArea = bW * bH;
                    double cArea = cW * cH;
                    double dArea = dW * dH;

                    //4区が属するピクセル座標
                    int aX = (int)p2X;
                    int aY = (int)p2Y;
                    int bX = (int)p3X;
                    int bY = (int)p3Y;
                    int cX = (int)p4X;
                    int cY = (int)p4Y;
                    int dX = (int)p5X;
                    int dY = (int)p5Y;

                    //4区のピクセルの値
                    byte a = pixels[aY * stride + aX];
                    byte b = pixels[bY * stride + bX];
                    byte c = pixels[cY * stride + cX];
                    byte d = pixels[dY * stride + dX];

                    //値(色)*面積(割合)
                    double aR = a * aArea;
                    double bR = b * bArea;
                    double cR = c * cArea;
                    double dR = d * dArea;

                    //4区を合計して四捨五入で完成
                    double ABCD = aR + bR + cR + dR;
                    byte result = (byte)(ABCD + 0.5);
                    resultPixels[y * scaledStride + x] = result;

                }
            }
            BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
            return bitmap;
        }

        //縮小専用
        private BitmapSource Bilinear2(BitmapSource source, int yoko, int tate)
        {
            //元画像の画素値の配列作成
            int sourceWidth = source.PixelWidth;
            int sourceHeight = source.PixelHeight;
            int stride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[sourceHeight * stride];
            source.CopyPixels(pixels, stride, 0);

            //縮小後の画像の画素値の配列用
            double yokoScale = (double)sourceWidth / yoko;//横倍率
            double tateScale = (double)sourceHeight / tate;
            int scaledStride = (yoko * source.Format.BitsPerPixel + 7) / 8;
            byte[] resultPixels = new byte[tate * scaledStride];

            for (int y = 0; y < tate; y++)
            {
                for (int x = 0; x < yoko; x++)
                {
                    //参照Point
                    Point rP = new((x + 0.5) * yokoScale, (y + 0.5) * tateScale);

                    //ピクセルの境目
                    int p1X = (int)(rP.X + 0.5);
                    int p1Y = (int)(rP.Y + 0.5);

                    //参照範囲の四隅座標
                    Point p2 = new(rP.X - 0.5, rP.Y - 0.5);
                    Point p3 = new(rP.X + 0.5, rP.Y - 0.5);
                    Point p4 = new(rP.X - 0.5, rP.Y + 0.5);
                    Point p5 = new(rP.X + 0.5, rP.Y + 0.5);

                    //4区の面積
                    double aArea = (p1X - p2.X) * (p1Y - p2.Y);
                    double bArea = (p3.X - p1X) * (p1Y - p3.Y);
                    double cArea = (p1X - p4.X) * (p4.Y - p1Y);
                    double dArea = (p5.X - p1X) * (p5.Y - p1Y);

                    //4区のピクセルの値
                    byte aValue = pixels[(int)p2.Y * stride + (int)p2.X];
                    byte bValue = pixels[(int)p3.Y * stride + (int)p3.X];
                    byte cValue = pixels[(int)p4.Y * stride + (int)p4.X];
                    byte dValue = pixels[(int)p5.Y * stride + (int)p5.X];

                    //値(色)*面積(割合)
                    double aR = aValue * aArea;
                    double bR = bValue * bArea;
                    double cR = cValue * cArea;
                    double dR = dValue * dArea;

                    //4区を合計して四捨五入で完成
                    resultPixels[y * scaledStride + x] = (byte)(aR + bR + cR + dR + 0.5);

                }
            }
            BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
            return bitmap;
        }

        //拡大縮小両対応
        private BitmapSource Bilinear3(BitmapSource source, int yoko, int tate)
        {
            //元画像の画素値の配列作成
            int sourceWidth = source.PixelWidth;
            int sourceHeight = source.PixelHeight;
            int stride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[sourceHeight * stride];
            source.CopyPixels(pixels, stride, 0);

            //縮小後の画像の画素値の配列用
            double yokoScale = (double)sourceWidth / yoko;//横倍率
            double tateScale = (double)sourceHeight / tate;
            int scaledStride = (yoko * source.Format.BitsPerPixel + 7) / 8;
            byte[] resultPixels = new byte[tate * scaledStride];

            for (int y = 0; y < tate; y++)
            {
                for (int x = 0; x < yoko; x++)
                {
                    //参照点座標
                    Point rP = new((x + 0.5) * yokoScale, (y + 0.5) * tateScale);

                    //ピクセルの境目座標
                    //参照点を四捨五入、0.5足して小数部分切り捨てで四捨五入になる
                    int p1X = (int)(rP.X + 0.5);
                    int p1Y = (int)(rP.Y + 0.5);

                    //参照範囲の四隅座標
                    //参照範囲は1x1なので上下左右に0.5ずつ足したり引いたり
                    Point p2 = new(rP.X - 0.5, rP.Y - 0.5);
                    Point p3 = new(rP.X + 0.5, rP.Y - 0.5);
                    Point p4 = new(rP.X - 0.5, rP.Y + 0.5);
                    Point p5 = new(rP.X + 0.5, rP.Y + 0.5);

                    //4区の面積
                    double aArea = (p1X - p2.X) * (p1Y - p2.Y);
                    double bArea = (p3.X - p1X) * (p1Y - p3.Y);
                    double cArea = (p1X - p4.X) * (p4.Y - p1Y);
                    double dArea = (p5.X - p1X) * (p5.Y - p1Y);

                    //拡大時は範囲を超えることがあるので範囲内に修正
                    FixPoint(ref p2, sourceWidth, sourceHeight);
                    FixPoint(ref p3, sourceWidth, sourceHeight);
                    FixPoint(ref p4, sourceWidth, sourceHeight);
                    FixPoint(ref p5, sourceWidth, sourceHeight);

                    //4区のピクセルの値取得、座標は各座標の小数部分切り捨て
                    byte aValue = pixels[(int)p2.Y * stride + (int)p2.X];
                    byte bValue = pixels[(int)p3.Y * stride + (int)p3.X];
                    byte cValue = pixels[(int)p4.Y * stride + (int)p4.X];
                    byte dValue = pixels[(int)p5.Y * stride + (int)p5.X];

                    //値(色)*面積(割合)
                    double aR = aValue * aArea;
                    double bR = bValue * bArea;
                    double cR = cValue * cArea;
                    double dR = dValue * dArea;

                    //4区を合計して四捨五入で完成
                    resultPixels[y * scaledStride + x] = (byte)(aR + bR + cR + dR + 0.5);

                }
            }
            BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
            return bitmap;

            //拡大時は範囲を超えることがあるので範囲内に修正
            void FixPoint(ref Point p, double sourcePixelWidth, double sourcePixelHeight)
            {                
                if (p.X < 0) p.X = 0;
                if (p.X >= sourcePixelWidth) p.X = sourcePixelWidth - 1;
                if (p.Y < 0) p.Y = 0;
                if (p.Y >= sourcePixelHeight) p.Y = sourcePixelHeight - 1;
            }
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

        private void ChangeBitmapSource(BitmapSource source)
        {
            MyScaledBitmap = source;
            MyScaledImage.Source = source;
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
            MyScaledImage.Source = MyBitmapSource;
        }

        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource source = Bilinear2(MyBitmapSource, 2, 2);
            //BitmapSource source = Bilinear2冗長(MyBitmapSource, 2, 2);
            //BitmapSource source = Bilinear(MyBitmapSource, 2, 2);
            //BitmapSource source = BilinearTestOnlyHeightOne(MyBitmapSource, 3, 1);
            ChangeBitmapSource(source);
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

        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapSource.PixelWidth / 2.0);
            int tate = (int)Math.Ceiling(MyBitmapSource.PixelHeight / 2.0);
            ChangeBitmapSource(Bilinear3(MyBitmapSource, yoko, tate));
        }
        private void MyButton3_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapSource.PixelWidth / 3.0);
            int tate = (int)Math.Ceiling(MyBitmapSource.PixelHeight / 3.0);
            ChangeBitmapSource(Bilinear3(MyBitmapSource, yoko, tate));
        }

        private void MyButton2bai_Click(object sender, RoutedEventArgs e)
        {
            int yoko = MyBitmapSource.PixelWidth * 2;
            int tate = MyBitmapSource.PixelHeight * 2;
            ChangeBitmapSource(Bilinear3(MyBitmapSource, yoko, tate));
        }

        private void MyButton3bai_Click(object sender, RoutedEventArgs e)
        {
            int yoko = MyBitmapSource.PixelWidth * 3;
            int tate = MyBitmapSource.PixelHeight * 3;
            ChangeBitmapSource(Bilinear3(MyBitmapSource, yoko, tate));
        }
    }
}
