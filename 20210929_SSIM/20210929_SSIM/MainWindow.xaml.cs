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

namespace _20210929_SSIM
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
            RenderOptions.SetBitmapScalingMode(MyImage1, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(MyImage2, BitmapScalingMode.NearestNeighbor);
        }



        #region SSIM

        private double SSIM8x8(byte[] pixels1, byte[] pixels2, int width, int height)
        {
            double total = 0;
            int count = 0;
            for (int y = 0; y < height - 8; y++)
            {
                for (int x = 0; x < width - 8; x++)
                {
                    (byte[] vs1, byte[] vs2) subWnd = Get8x8Windw(pixels1, pixels2, x, y, width);
                    total += SSIM(subWnd.vs1, subWnd.vs2);
                    count++;
                }
            }
            double result = total / count;
            return result;
        }
        private (byte[], byte[]) Get8x8Windw(byte[] vs1, byte[] vs2, int xBegin, int yBegin, int stride)
        {
            byte[] wind1 = new byte[8 * 8];
            byte[] wind2 = new byte[8 * 8];
            int count = 0;
            for (int y = yBegin; y < yBegin + 8; y++)
            {
                for (int x = xBegin; x < xBegin + 8; x++)
                {
                    int p = y * stride + x;
                    wind1[count] = vs1[p];
                    wind2[count] = vs2[p];
                    count++;
                }
            }
            return (wind1, wind2);
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

        #endregion SSIM

        #region 基本計算
        /// <summary>
        /// 共分散
        /// </summary>
        /// <param name="vs1"></param>
        /// <param name="ave1"></param>
        /// <param name="vs2"></param>
        /// <param name="ave2"></param>
        /// <returns></returns>
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
        private double Covariance(byte[] vs1, byte[] vs2)
        {
            if (vs1.Length != vs2.Length)
            {
                return double.NaN;
            }

            double ave1 = Average(vs1);
            double ave2 = Average(vs2);
            double total = 0;
            for (int i = 0; i < vs1.Length; i++)
            {
                total += (vs1[i] - ave1) * (vs2[i] - ave2);
            }
            return total / vs2.Length;
        }

        /// <summary>
        /// 分散
        /// </summary>
        /// <param name="vs"></param>
        /// <param name="average"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 分散、求め方その2
        /// </summary>
        /// <param name="vs"></param>
        /// <param name="average"></param>
        /// <returns></returns>
        private double Variance分散2(byte[] vs, double average)
        {
            double total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                total += vs[i] * vs[i];
            }
            return (total / vs.Length) - (average * average);
        }

        /// <summary>
        /// 平均
        /// </summary>
        /// <param name="vs"></param>
        /// <returns></returns>
        private double Average(byte[] vs)
        {
            ulong total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                total += vs[i];
            }
            return total / (double)vs.Length;
        }
        #endregion 基本計算


        /// <summary>
        /// 画像ファイルパスからPixelFormats.Gray8(グレースケール)のBitmapSourceと輝度の配列を作成
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




        private void ScrollViewer_Drop_1(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            //ファイルパス取得
            var datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            var paths = datas.ToList();
            paths.Sort();
            var temp = MakeBitmapSourceGray8(paths[0]);
            if (temp.Item1 == null && temp.Item2 == null)
            {
                MessageBox.Show("ドロップされたファイルから画像を取得できなかった");
            }
            else
            {
                MySource1st = temp;
                MyImage1.Source = MySource1st.bitmap;
                MyTextBlock1.Text = System.IO.Path.GetFileName(paths[0]).ToString();
            }

        }

        private void ScrollViewer_Drop_2(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            //ファイルパス取得
            var datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            var paths = datas.ToList();
            paths.Sort();
            var temp = MakeBitmapSourceGray8(paths[0]);
            if (temp.Item1 == null && temp.Item2 == null)
            {
                MessageBox.Show("ドロップされたファイルから画像を取得できなかった");
            }
            else
            {
                MySource2nd = temp;
                MyImage2.Source = MySource2nd.bitmap;
                MyTextBlock2.Text = System.IO.Path.GetFileName(paths[0]).ToString();
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MySource1st.bitmap != null && MySource2nd.bitmap != null)
            {
                double result = SSIM8x8(MySource1st.pixels, MySource2nd.pixels, MySource1st.bitmap.PixelWidth, MySource1st.bitmap.PixelHeight);
                MyTextBlockSSIM.Text = "SSIM = " + result.ToString();

            }

        }

        private void MyScrollViewer1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double vOffset = MyScrollViewer1.VerticalOffset;
            double hOffset = MyScrollViewer1.HorizontalOffset;
            if (vOffset != MyScrollViewer2.VerticalOffset)
            {
                MyScrollViewer2.ScrollToVerticalOffset(vOffset);
            }
            if (hOffset != MyScrollViewer2.HorizontalOffset)
            {
                MyScrollViewer2.ScrollToHorizontalOffset(hOffset);
            }

        }

        private void MyScrollViewer2_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double vOffset = MyScrollViewer2.VerticalOffset;
            double hOffset = MyScrollViewer2.HorizontalOffset;
            if (vOffset != MyScrollViewer1.VerticalOffset)
            {
                MyScrollViewer1.ScrollToVerticalOffset(vOffset);
            }
            if (hOffset != MyScrollViewer1.HorizontalOffset)
            {
                MyScrollViewer1.ScrollToHorizontalOffset(hOffset);
            }

        }

        private void MySliderScale_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                MySliderScale.Value += MySliderScale.SmallChange;
            }
            else
            {
                MySliderScale.Value -= MySliderScale.SmallChange;
            }
        }
    }
}
