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

using System.Numerics;

//MSE / PSNR vs SSIM の比較画像紹介 - Qiita
//https://qiita.com/yoya/items/510043d836c9f2f0fe2f

//Structural similarity - Wikipedia
//https://en.wikipedia.org/wiki/Structural_similarity

//画質評価指標SSIMについて調べてみた - Visualize
//https://visualize.hatenablog.com/entry/2016/02/20/144657


namespace _20210928_SSIM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double C1 = 0.01 * 255 * (0.01 * 255);//(0.01*255)^2=6.5025
        private const double C2 = 0.03 * 255 * (0.03 * 255);//(0.03*255)^2=58.5225
        private const double C3 = C2 / 2.0;//58.5225/2=29.26125
        private (BitmapSource bitmap, byte[] pixels) MySource1st;
        private (BitmapSource bitmap, byte[] pixels) MySource2nd;

        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();

        }
        private void MyInitialize()
        {
            string str = "1 2 3";
            string[] ss = str.Split(' ');
            var sss = ss.Select(x => byte.Parse(x)).ToArray();
            var vs = str.Split(' ').Select(x => byte.Parse(x)).ToArray();

            MySource1st = MakeBitmapSourceGray8(@"D:\動画D\真・三國無双3エンコード検証\origin.png");
            //MySource1st = MakeBitmapSourceGray8(@"E:\オレ\エクセル\ゲーム履歴\真・三國無双3\20210810_223344_458_.png");
            MyImageOrigin.Source = MySource1st.bitmap;
            MySource2nd = MakeBitmapSourceGray8(@"D:\動画D\真・三國無双3エンコード検証\x265_default_q21_i420.png");
            //MySource2nd = MakeBitmapSourceGray8(@"E:\オレ\エクセル\ゲーム履歴\真・三國無双3\20210811_143435_675_.png");
            MyImage2nd.Source = MySource2nd.bitmap;
        }
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            //ファイルパス取得
            var datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            var paths = datas.ToList();
            paths.Sort();
            //MyBitmapOrigin = MakeBitmapSourceGray8(paths[0]);
        }


        /// <summary>
        /// 画像ファイルパスからPixelFormats.Gray8(グレースケール)のBitmapSource作成
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dpiX"></param>
        /// <param name="dpiY"></param>
        /// <returns></returns>
        private (BitmapSource, byte[]) MakeBitmapSourceGray8(string filePath, double dpiX = 96, double dpiY = 96)
        {
            BitmapSource source = null;
            byte[] pixels = null;
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

        private void MyButtonTest_Click(object sender, RoutedEventArgs e)
        {
            var neko = MySource1st;
        }

        private void MyButtonSIMM_Click(object sender, RoutedEventArgs e)
        {
            byte[] vs1 = new byte[] { 1, 2, 3, 4, 5 };
            byte[] vs2 = new byte[] { 1, 2, 3, 4, 1 };
            double result = SSIM(vs1, vs2);
            result = SSIM(MySource1st.pixels, MySource2nd.pixels);
            double result2 = SSIM2(MySource1st.pixels, MySource2nd.pixels);
            double result3 = SSIM8x8(MySource1st.pixels, MySource2nd.pixels, MySource1st.bitmap.PixelWidth, MySource1st.bitmap.PixelHeight);
            double result4 = SSIMNxN(MySource1st.pixels, MySource2nd.pixels, MySource1st.bitmap.PixelWidth, MySource1st.bitmap.PixelHeight, 16);


        }
        private double SSIM(byte[] vs1, byte[] vs2)
        {
            double ave1 = Average(vs1);//平均
            double ave2 = Average(vs2);
            double covar = Covariance(vs1, ave1, vs2, ave2);//共分散
            double vari1 = Variance(vs1, ave1);//分散
            double vari2 = Variance(vs2, ave2);
            double bunsi = (2 * ave1 * ave2 + C1) * (2 * covar + C2);//分子
            double bunbo = (ave1 * ave1 + ave2 * ave2 + C1) * (vari1 + vari2 + C2);//分母
            double ssim = bunsi / bunbo;
            return ssim;
        }
        private double SSIM2(byte[] vs1, byte[] vs2)
        {
            double ave1 = Average(vs1);//平均
            double ave2 = Average(vs2);
            double covar = Covariance(vs1, ave1, vs2, ave2);//共分散
            double vari1 = Variance(vs1, ave1);//分散
            double vari2 = Variance(vs2, ave2);
            double stdev1 = Math.Sqrt(vari1);//標準偏差
            double stdev2 = Math.Sqrt(vari2);

            double l = (2 * ave1 * ave2 + C1) / (ave1 * ave1 + ave2 * ave2 + C1);
            double c = (2 * stdev1 * stdev2 + C2) / (vari1 + vari2 + C2);
            double s = (covar + C3) / (stdev1 * stdev2 + C3);

            double ssim = l * c * s;
            return ssim;
        }
        private double SSIMNxN(byte[] pixels1, byte[] pixels2, int width, int height, int wndSize)
        {
            double total = 0;
            int count = 0;
            for (int y = 0; y < height - wndSize; y++)
            {
                for (int x = 0; x < width - wndSize; x++)
                {
                    (byte[] vs1, byte[] vs2) subWnd = GetNxNWindw(pixels1, pixels2, x, y, wndSize);
                    total += SSIM(subWnd.vs1, subWnd.vs2);
                    count++;
                }
            }
            double result = total / count;
            return result;
        }
        private (byte[], byte[]) GetNxNWindw(byte[] pixels1, byte[] pixels2, int xBegin, int yBegin, int wndSize)
        {
            byte[] wind1 = new byte[wndSize * wndSize];
            byte[] wind2 = new byte[wndSize * wndSize];
            int count = 0;
            for (int y = yBegin; y < yBegin + wndSize; y++)
            {
                for (int x = xBegin; x < xBegin + wndSize; x++)
                {
                    int p = y * wndSize + x;
                    wind1[count] = pixels1[p];
                    wind2[count] = pixels2[p];
                    count++;
                }
            }
            return (wind1, wind2);
        }


        private double SSIM8x8(byte[] pixels1, byte[] pixels2, int width, int height)
        {
            double total = 0;
            int count = 0;
            for (int y = 0; y < height - 8; y++)
            {
                for (int x = 0; x < width - 8; x++)
                {
                    (byte[] vs1, byte[] vs2) subWnd = Get8x8Windw(pixels1, pixels2, x, y);
                    total += SSIM(subWnd.vs1, subWnd.vs2);
                    count++;
                }
            }
            double result = total / count;
            return result;
        }
        private (byte[], byte[]) Get8x8Windw(byte[] vs1, byte[] vs2, int xBegin, int yBegin)
        {
            byte[] wind1 = new byte[8 * 8];
            byte[] wind2 = new byte[8 * 8];
            int count = 0;
            for (int y = yBegin; y < yBegin + 8; y++)
            {
                for (int x = xBegin; x < xBegin + 8; x++)
                {
                    int p = y * 8 + x;
                    wind1[count] = vs1[p];
                    wind2[count] = vs2[p];
                    count++;
                }
            }
            return (wind1, wind2);
        }





        private double Covariance(byte[] vs1, double ave1, byte[] vs2, double ave2)
        {
            if (vs1.Length != vs2.Length)
            {
                return double.NaN;
            }
            double total = 0;
            for (int i = 0; i < vs1.Length; i++)
            {
                total += (vs1[i] - ave1) * (vs2[i] - ave2);
            }
            return total / vs2.Length;
        }
        private double Variance(byte[] vs, double average)
        {
            double total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                double temp = vs[i] - average;
                total += temp * temp;
            }
            return total / vs.Length;
        }
        
        
        private double Variance分散2(byte[] vs, double average)
        {
            double total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                total += vs[i] * vs[i];
            }
            return (total / vs.Length) - (average * average);
        }

        private double Average(byte[] vs)
        {
            ulong total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                total += vs[i];
            }
            return total / (double)vs.Length;
        }
    }
}
