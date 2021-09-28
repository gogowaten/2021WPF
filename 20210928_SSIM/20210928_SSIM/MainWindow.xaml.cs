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


namespace _20210928_SSIM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            
            MySource1st = MakeBitmapSourceGray8(@"E:\オレ\エクセル\ゲーム履歴\真・三國無双3\20210810_223542_398_.png");
            MyImageOrigin.Source = MySource1st.bitmap;
            MySource2nd = MakeBitmapSourceGray8(@"E:\オレ\エクセル\ゲーム履歴\真・三國無双3\20210810_223542_398_.png");
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

        }
        private double SSIM(byte[] vs1, byte[] vs2)
        {
            const double c1 = (0.01 * 255) * (0.01 * 255);
            const double c2 = (0.03 * 255) * (0.03 * 255);
            double ave1 = Average(vs1);
            double ave2 = Average(vs2);
            double covar共分散 = Covariance共分散(vs1, ave1, vs2, ave2);
            double vari1 = Variance分散(vs1, ave1);
            double vari2 = Variance分散(vs2, ave2);
            double bunsi = (2 * ave1 * ave2 + c1) * (2 * covar共分散 + c2);
            double bunbo = (ave1 * ave1 + ave2 * ave2 + c1) * (vari1 + vari2 + c2);
            double ssim = bunsi / bunbo;
            return ssim;
        }
        private double Covariance共分散(byte[] vs1, double ave1, byte[] vs2, double ave2)
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
        private double Variance分散(byte[] vs, double average)
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
