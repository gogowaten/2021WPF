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

using MyHSV;

////MSE / PSNR vs SSIM の比較画像紹介 - Qiita
////https://qiita.com/yoya/items/510043d836c9f2f0fe2f

////Structural similarity - Wikipedia
////https://en.wikipedia.org/wiki/Structural_similarity

////画質評価指標SSIMについて調べてみた - Visualize
////https://visualize.hatenablog.com/entry/2016/02/20/144657

//WPF、Color(RGB)とHSVを相互変換するdll作ってみた、オブジェクトブラウザ使ってみた - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/15380324



/// <summary>
/// 
/// </summary>
namespace _20211007_SSIM_HSV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double C1 = 0.01 * 255 * (0.01 * 255);//(0.01*255)^2=6.5025
        private const double C2 = 0.03 * 255 * (0.03 * 255);//(0.03*255)^2=58.5225
        private const double C3 = C2 / 2.0;                 //58.5225/2=29.26125
        private (BitmapSource bitmap, byte[] pixels) MySourceColor1;
        private (BitmapSource bitmap, byte[] pixels) MySourceColor2;

        public MainWindow()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(MyImage1, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(MyImage2, BitmapScalingMode.NearestNeighbor);
            MyComboBoxWndSize.ItemsSource = new List<int>() { 4, 8, 16 };
            MyComboBoxWndSize.SelectedIndex = 1;
            MyComboBoxStep.ItemsSource = new List<int>() { 1, 2, 4, 8, 16 };
            MyComboBoxStep.SelectedIndex = 2;

#if DEBUG
            Top = 0;
            Left = 0;
#endif
        }



        /// <summary>
        /// RGBからHSV変換、RGBの各配列からHSVの各配列
        /// </summary>
        /// <param name="B1"></param>
        /// <param name="B2"></param>
        /// <param name="G1"></param>
        /// <param name="G2"></param>
        /// <param name="R1"></param>
        /// <param name="R2"></param>
        /// <param name="h1"></param>
        /// <param name="h2"></param>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        private void RGBtoHSV(
            byte[] B1, byte[] B2,
            byte[] G1, byte[] G2,
            byte[] R1, byte[] R2,
           ref double[] h1, ref double[] h2,
           ref double[] s1, ref double[] s2,
           ref double[] v1, ref double[] v2)
        {
            for (int i = 0; i < B1.Length; i++)
            {
                HSV tempHSV1 = HSV.Color2HSV(R1[i], G1[i], B1[i]);
                h1[i] = tempHSV1.Hue;
                s1[i] = tempHSV1.Saturation;
                v1[i] = tempHSV1.Value;
                HSV tempHSV2 = HSV.Color2HSV(R2[i], G2[i], B2[i]);
                h2[i] = tempHSV2.Hue;
                s2[i] = tempHSV2.Saturation;
                v2[i] = tempHSV2.Value;
            }
        }
        private void RGBtoHSV(byte[] B1, byte[] G1, byte[] R1,
           ref double[] h1, ref double[] s1, ref double[] v1)
        {
            for (int i = 0; i < B1.Length; i++)
            {
                HSV tempHSV1 = HSV.Color2HSV(R1[i], G1[i], B1[i]);
                h1[i] = tempHSV1.Hue;
                s1[i] = tempHSV1.Saturation;
                v1[i] = tempHSV1.Value;
            }
        }


        #region SSIM

        /// <summary>
        /// HSVでSSIMを計算、ピクセルフォーマットBGRA32画像専用
        /// </summary>
        /// <param name="pixels1">順番がBGRAの画素値配列</param>
        /// <param name="pixels2"></param>
        /// <param name="width">横ピクセル数</param>
        /// <param name="height">縦ピクセル数</param>
        /// <param name="wndSize">ブロックサイズ、通常は8</param>
        /// <param name="step">次のブロックとのオフセットピクセル数、ブロックサイズが8なら2か4が適当</param>
        /// <returns></returns>
        private (double hsv, double h, double s, double v) SsimHsv(byte[] pixels1, byte[] pixels2, int width, int height, int wndSize, int step)
        {
            double hTotal = 0;
            double sTotal = 0;
            double vTotal = 0;

            int count = 0;
            int length = wndSize * wndSize;
            byte[] B1 = new byte[length];
            byte[] B2 = new byte[length];
            byte[] G1 = new byte[length];
            byte[] G2 = new byte[length];
            byte[] R1 = new byte[length];
            byte[] R2 = new byte[length];
            byte[] A1 = new byte[length];//未使用
            byte[] A2 = new byte[length];//未使用
            double[] h1 = new double[length];
            double[] s1 = new double[length];
            double[] v1 = new double[length];
            double[] h2 = new double[length];
            double[] s2 = new double[length];
            double[] v2 = new double[length];


            for (int y = 0; y < height - wndSize; y += 4 + step)
            {
                for (int x = 0; x < width - wndSize; x += 4 + step)
                {
                    //ブロックウィンドウ取得
                    GetBlockWindow(pixels1, pixels2, x, y, wndSize, width, B1, B2, G1, G2, R1, R2, A1, A2);
                    //RGBからHSVに変換
                    RGBtoHSV(B1, B2, G1, G2, R1, R2, ref h1, ref h2, ref s1, ref s2, ref v1, ref v2);
                    //0-255に変換
                    for (int i = 0; i < length; i++)
                    {
                        h1[i] = h1[i] / 360 * 255;
                        h2[i] = h2[i] / 360 * 255;
                        s1[i] *= 255;
                        s2[i] *= 255;
                        v1[i] *= 255;
                        v2[i] *= 255;
                    }

                    hTotal += SSIM(h1, h2);
                    sTotal += SSIM(s1, s2);
                    vTotal += SSIM(v1, v2);
                    count++;
                }
            }
            hTotal /= count;
            sTotal /= count;
            vTotal /= count;

            double result = hTotal + sTotal + vTotal;
            result /= 3.0;
            return (result, hTotal, sTotal, vTotal);
        }

        private void GetBlockWindow(byte[] pixels1, byte[] pixels2,
            int xBegin, int yBegin, int wndSize, int stride,
            byte[] B1, byte[] B2, byte[] G1, byte[] G2, byte[] R1, byte[] R2, byte[] A1, byte[] A2)
        {
            int count = 0;
            for (int y = yBegin; y < yBegin + wndSize; y++)
            {
                for (int x = xBegin; x < xBegin + wndSize; x++)
                {
                    int p = y * stride + 4 * x;
                    B1[count] = pixels1[p];
                    B2[count] = pixels2[p];
                    G1[count] = pixels1[p + 1];
                    G2[count] = pixels2[p + 1];
                    R1[count] = pixels1[p + 2];
                    R2[count] = pixels2[p + 2];
                    A1[count] = pixels1[p + 3];
                    A2[count] = pixels2[p + 3];
                    count++;
                }
            }
        }



        private double SSIM(double[] vs1, double[] vs2)
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
            decimal total = 0;
            for (int i = 0; i < vs1.Length; i++)
            {
                total += (decimal)((vs1[i] - ave1) * (vs2[i] - ave2));
            }
            return (double)(total / vs2.Length);
        }
        private double Covariance(double[] vs1, double ave1, double[] vs2, double ave2)
        {
            if (vs1.Length != vs2.Length)
            {
                return double.NaN;
            }
            decimal total = 0;
            for (int i = 0; i < vs1.Length; i++)
            {
                total += (decimal)((vs1[i] - ave1) * (vs2[i] - ave2));
            }
            return (double)(total / vs2.Length);
        }

        private double Covariance(byte[] vs1, byte[] vs2)
        {
            if (vs1.Length != vs2.Length)
            {
                return double.NaN;
            }

            double ave1 = Average(vs1);
            double ave2 = Average(vs2);
            decimal total = 0;
            for (int i = 0; i < vs1.Length; i++)
            {
                total += (decimal)((vs1[i] - ave1) * (vs2[i] - ave2));
            }
            return (double)(total / vs2.Length);
        }

        /// <summary>
        /// 分散
        /// </summary>
        /// <param name="vs"></param>
        /// <param name="average"></param>
        /// <returns></returns>
        private double Variance(byte[] vs, double average)
        {
            decimal total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                double temp = vs[i] - average;
                total += (decimal)(temp * temp);
            }
            return (double)(total / vs.Length);
        }
        private double Variance(double[] vs, double average)
        {
            decimal total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                decimal temp = (decimal)(vs[i] - average);
                total += temp * temp;
            }
            return (double)(total / vs.Length);
        }

        ///// <summary>
        ///// 分散、求め方その2
        ///// </summary>
        ///// <param name="vs"></param>
        ///// <param name="average"></param>
        ///// <returns></returns>
        //private double Variance分散2(byte[] vs, double average)
        //{
        //    double total = 0;
        //    for (int i = 0; i < vs.Length; i++)
        //    {
        //        total += vs[i] * vs[i];
        //    }
        //    return (total / vs.Length) - (average * average);
        //}
        //private double Variance分散2(double[] vs, double average)
        //{
        //    double total = 0;
        //    for (int i = 0; i < vs.Length; i++)
        //    {
        //        total += vs[i] * vs[i];
        //    }
        //    return (total / vs.Length) - (average * average);
        //}

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
        private double Average(double[] vs)
        {
            double total = 0;
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

        /// <summary>
        /// 画像ファイルパスからPixelFormats.Bgra32のBitmapSourceと輝度の配列を作成
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="pixelFormat"></param>
        /// <param name="dpiX"></param>
        /// <param name="dpiY"></param>
        /// <returns></returns>
        private (BitmapSource, byte[]) MakeBitmapSourceFromImageFile(string filePath, PixelFormat pixelFormat, double dpiX = 96, double dpiY = 96)
        {
            BitmapSource source = null;
            byte[] pixels = null;
            try
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    source = BitmapFrame.Create(stream);
                    if (source.Format != pixelFormat)
                    {
                        source = new FormatConvertedBitmap(source, pixelFormat, null, 0);
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


        private void DisplaySSIM()
        {
            if (MySourceColor1.bitmap != null && MySourceColor2.bitmap != null)
            {
                var result = SsimHsv(MySourceColor1.pixels, MySourceColor2.pixels,
                    MySourceColor1.bitmap.PixelWidth, MySourceColor1.bitmap.PixelHeight,
                    (int)MyComboBoxWndSize.SelectedItem, (int)MyComboBoxStep.SelectedItem);

                MyTextBlockSSIM.Text = "SSIM = " + result.hsv.ToString();
                MyTextBlockH.Text = result.h.ToString();
                MyTextBlockS.Text = result.s.ToString();
                MyTextBlockV.Text = result.v.ToString();
            }
        }

        private void ScrollViewer_Drop_1(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            //ファイルパス取得
            string[] datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            List<string> paths = datas.ToList();
            paths.Sort();
            (BitmapSource, byte[]) temp = MakeBitmapSourceFromImageFile(paths[0], PixelFormats.Bgra32);
            if (temp.Item1 == null && temp.Item2 == null)
            {
                _ = MessageBox.Show("ドロップされたファイルから画像を取得できなかった");
            }
            else
            {
                MySourceColor1 = temp;
                MyImage1.Source = MySourceColor1.bitmap;
                MyTextBlock1.Text = System.IO.Path.GetFileName(paths[0]).ToString();
                if (MyImage2.Source != null && MySourceColor1.pixels.Length == MySourceColor2.pixels.Length)
                {
                    DisplaySSIM();
                }
                else
                {
                    MyTextBlockSSIM.Text = "SSIM";
                }
            }

        }

        private void ScrollViewer_Drop_2(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            //ファイルパス取得
            string[] datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            List<string> paths = datas.ToList();
            paths.Sort();
            (BitmapSource, byte[]) temp = MakeBitmapSourceFromImageFile(paths[0], PixelFormats.Bgra32);
            if (temp.Item1 == null && temp.Item2 == null)
            {
                _ = MessageBox.Show("ドロップされたファイルから画像を取得できなかった");
            }
            else
            {
                MySourceColor2 = temp;
                MyImage2.Source = MySourceColor2.bitmap;
                MyTextBlock2.Text = System.IO.Path.GetFileName(paths[0]).ToString();
                if (MyImage1.Source != null && MySourceColor1.pixels.Length == MySourceColor2.pixels.Length)
                {
                    DisplaySSIM();
                }
                else
                {
                    MyTextBlockSSIM.Text = "SSIM";
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DisplaySSIM();
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
