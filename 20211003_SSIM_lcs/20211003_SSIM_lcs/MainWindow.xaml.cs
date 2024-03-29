﻿using System;
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

namespace _20211003_SSIM_lcs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double C1 = 0.01 * 255 * (0.01 * 255);//(0.01*255)^2=6.5025
        private const double C2 = 0.03 * 255 * (0.03 * 255);//(0.03*255)^2=58.5225
        private const double C3 = C2 / 2.0;//58.5225/2=29.26125
        private (BitmapSource bitmap, byte[] pixels) MySource1;
        private (BitmapSource bitmap, byte[] pixels) MySource2;


        public MainWindow()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(MyImage1, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(MyImage2, BitmapScalingMode.NearestNeighbor);
            MyComboBoxWndSize.ItemsSource = new List<int>() { 4, 8, 16 };
            MyComboBoxWndSize.SelectedIndex = 1;
            MyComboBoxStep.ItemsSource = new List<int>() { 1, 2, 4, 8 };
            MyComboBoxStep.SelectedIndex = 0;

#if DEBUG
            Top = 0;
            Left = 0;
#endif
        }


        #region SSIM

        /// <summary>
        /// グレースケール画像専用、SSIM計算
        /// </summary>
        /// <param name="pixels1"></param>
        /// <param name="pixels2"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="wndSize">ブロックウィンドウサイズ、通常は8</param>
        /// <param name="step">ブロックウィンドウ間のピクセル数</param>
        /// <returns></returns>
        private double SSIMNxN(byte[] pixels1, byte[] pixels2, int width, int height, int wndSize, int step)
        {
            double total = 0;
            int count = 0;
            byte[] wind1 = new byte[wndSize * wndSize];
            byte[] wind2 = new byte[wndSize * wndSize];
            for (int y = 0; y < height - wndSize; y += step)
            {
                for (int x = 0; x < width - wndSize; x += step)
                {
                    GetBlockWindw(wind1, wind2, pixels1, pixels2, x, y, wndSize, width);
                    total += SSIM(wind1, wind2);
                    count++;
                }
            }
            double result = total / count;
            return result;
        }
        /// <summary>
        /// グレースケール画像専用、LCSでSSIM
        /// </summary>
        /// <param name="pixels1">画素値配列</param>
        /// <param name="pixels2"></param>
        /// <param name="width">横ピクセル数</param>
        /// <param name="height">縦ピクセル数</param>
        /// <param name="wndSize">ブロックウィンドウサイズ、8が適当</param>
        /// <param name="step">ブロックウィンドウのずらし幅ピクセル、2か4が適当</param>
        /// <returns></returns>
        private (double, double, double, double) SSIMWithLCS(byte[] pixels1, byte[] pixels2, int width, int height, int wndSize, int step)
        {
            double tSsim = 0;
            double tL = 0, tC = 0, tS = 0;
            int count = 0;
            byte[] wind1 = new byte[wndSize * wndSize];
            byte[] wind2 = new byte[wndSize * wndSize];
            for (int y = 0; y < height - wndSize; y += step)
            {
                for (int x = 0; x < width - wndSize; x += step)
                {
                    GetBlockWindw(wind1, wind2, pixels1, pixels2, x, y, wndSize, width);
                    (double, double, double) lcs = GetLCS(wind1, wind2);
                    //tSsim += lcs.Item1* lcs.Item1 * lcs.Item2 * lcs.Item3;
                    tSsim += lcs.Item1 * lcs.Item2 * lcs.Item3;
                    tL += lcs.Item1;
                    tC += lcs.Item2;
                    tS += lcs.Item3;
                    count++;
                }
            }
            double ssim = tSsim / count;
            double l = tL / count;
            double c = tC / count;
            double s = tS / count;
            return (ssim, l, c, s);
        }

        /// <summary>
        /// 指定されたブロックウィンドウの配列を入れる、グレースケール画像専用
        /// </summary>
        /// <param name="wind1">受け取り用配列</param>
        /// <param name="wind2"></param>
        /// <param name="pixels1">画素値配列</param>
        /// <param name="pixels2"></param>
        /// <param name="xBegin">ブロックウィンドウの左上座標に当たる配列のインデックス</param>
        /// <param name="yBegin"></param>
        /// <param name="wndSize">ブロックウィンドウサイズ</param>
        /// <param name="stride">1ピクセルあたりのbyte数 * 横ピクセル数</param>
        private void GetBlockWindw(byte[] wind1, byte[] wind2,
            byte[] pixels1, byte[] pixels2, int xBegin, int yBegin, int wndSize, int stride)
        {
            int count = 0;
            for (int y = yBegin; y < yBegin + wndSize; y++)
            {
                for (int x = xBegin; x < xBegin + wndSize; x++)
                {
                    int p = y * stride + x;
                    wind1[count] = pixels1[p];
                    wind2[count] = pixels2[p];
                    count++;
                }
            }
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


        private (double, double, double) GetLCS(byte[] vsX, byte[] vsY)
        {
            double aveX = Average(vsX);//平均
            double aveY = Average(vsY);
            double covar = Covariance(vsX, aveX, vsY, aveY);//共分散
            double variX = Variance(vsX, aveX);//分散
            double variY = Variance(vsY, aveY);
            double stdevX = Math.Sqrt(variX);//標準偏差
            double stdevY = Math.Sqrt(variY);

            double l = (2 * aveX * aveY + C1) / (aveX * aveX + aveY * aveY + C1);
            double c = (2 * stdevX * stdevY + C2) / (variX + variY + C2);
            double s = (covar + C3) / (stdevX * stdevY + C3);

            return (l, c, s);
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


        private void MyDisplaySSIM()
        {
            (double, double, double, double) result = SSIMWithLCS(MySource1.pixels, MySource2.pixels,
                        MySource1.bitmap.PixelWidth, MySource1.bitmap.PixelHeight,
                        (int)MyComboBoxWndSize.SelectedItem, (int)MyComboBoxStep.SelectedItem);
            MyTextBlockLCS.Text = result.Item1.ToString();
            MyTextBlockL.Text = result.Item2.ToString();
            MyTextBlockC.Text = result.Item3.ToString();
            MyTextBlockS.Text = result.Item4.ToString();

            MyTextBlockSSIM.Text = "SSIM = " +
                SSIMNxN(MySource1.pixels, MySource2.pixels,
                MySource2.bitmap.PixelWidth, MySource2.bitmap.PixelHeight,
                (int)MyComboBoxWndSize.SelectedItem, (int)MyComboBoxStep.SelectedItem).ToString();

        }

        private void ScrollViewer_Drop_1(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            //ファイルパス取得
            string[] datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            List<string> paths = datas.ToList();
            paths.Sort();
            (BitmapSource, byte[]) temp = MakeBitmapSourceFromImageFile(paths[0], PixelFormats.Gray8);
            if (temp.Item1 == null && temp.Item2 == null)
            {
                _ = MessageBox.Show("ドロップされたファイルから画像を取得できなかった");
            }
            else
            {
                MySource1 = temp;
                MyImage1.Source = MySource1.bitmap;
                MyTextBlock1.Text = System.IO.Path.GetFileName(paths[0]).ToString();
                if (MyImage2.Source != null && MySource1.pixels.Length == MySource2.pixels.Length)
                {
                    MyDisplaySSIM();
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
            (BitmapSource, byte[]) temp = MakeBitmapSourceFromImageFile(paths[0], PixelFormats.Gray8);
            if (temp.Item1 == null && temp.Item2 == null)
            {
                _ = MessageBox.Show("ドロップされたファイルから画像を取得できなかった");
            }
            else
            {
                MySource2 = temp;
                MyImage2.Source = MySource2.bitmap;
                MyTextBlock2.Text = System.IO.Path.GetFileName(paths[0]).ToString();
                if (MyImage1.Source != null && MySource1.pixels.Length == MySource2.pixels.Length)
                {
                    MyDisplaySSIM();
                }
                else
                {
                    MyTextBlockSSIM.Text = "SSIM";
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MySource1.bitmap != null && MySource2.bitmap != null)
            {
                MyDisplaySSIM();
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
