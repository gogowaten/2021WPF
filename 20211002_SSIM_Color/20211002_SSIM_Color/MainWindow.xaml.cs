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

////MSE / PSNR vs SSIM の比較画像紹介 - Qiita
////https://qiita.com/yoya/items/510043d836c9f2f0fe2f

////Structural similarity - Wikipedia
////https://en.wikipedia.org/wiki/Structural_similarity

////画質評価指標SSIMについて調べてみた - Visualize
////https://visualize.hatenablog.com/entry/2016/02/20/144657

namespace _20211002_SSIM_Color
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double C1 = 0.01 * 255 * (0.01 * 255);//(0.01*255)^2=6.5025
        private const double C2 = 0.03 * 255 * (0.03 * 255);//(0.03*255)^2=58.5225
        private const double C3 = C2 / 2.0;//58.5225/2=29.26125
        //private (BitmapSource bitmap, byte[] pixels) MySourceGray1;
        //private (BitmapSource bitmap, byte[] pixels) MySourceGray2;
        private (BitmapSource bitmap, byte[] pixels) MySourceColor1;
        private (BitmapSource bitmap, byte[] pixels) MySourceColor2;


        public MainWindow()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(MyImage1, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(MyImage2, BitmapScalingMode.NearestNeighbor);
            MyComboBoxWndSize.ItemsSource = new List<int>() { 4, 8, 16 };
            MyComboBoxWndSize.SelectedIndex = 1;
            MyComboBoxStep.ItemsSource = new List<int>() { 1, 2, 4, 8 };
            MyComboBoxStep.SelectedIndex = 1;

#if DEBUG
            Top = 0;
            Left = 0;
#endif
        }



        #region SSIM
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pixels1"></param>
        /// <param name="pixels2"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="wndSize">ブロックサイズ、通常は8</param>
        /// <param name="step">ブロック間のピクセル数</param>
        /// <returns></returns>
        private double SSIMNxN(byte[] pixels1, byte[] pixels2, int width, int height, int wndSize, int step)
        {
            double total = 0;
            int count = 0;
            for (int y = 0; y < height - wndSize; y += step)
            {
                for (int x = 0; x < width - wndSize; x += step)
                {
                    (byte[] vs1, byte[] vs2) subWnd = GetNxNWindw(pixels1, pixels2, x, y, wndSize, width);
                    total += SSIM(subWnd.vs1, subWnd.vs2);
                    count++;
                }
            }
            double result = total / count;
            return result;
        }
        private (byte[], byte[]) GetNxNWindw(byte[] pixels1, byte[] pixels2, int xBegin, int yBegin, int wndSize, int stride)
        {
            byte[] wind1 = new byte[wndSize * wndSize];
            byte[] wind2 = new byte[wndSize * wndSize];
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




        private void ScrollViewer_Drop_1(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            //ファイルパス取得
            string[] datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            List<string> paths = datas.ToList();
            paths.Sort();
            (BitmapSource, byte[]) temp = MakeBitmapSourceFromImageFile(paths[0],PixelFormats.Bgra32);
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
                    double result = SSIMNxN(MySourceColor1.pixels, MySourceColor2.pixels,
                        MySourceColor1.bitmap.PixelWidth, MySourceColor1.bitmap.PixelHeight,
                        (int)MyComboBoxWndSize.SelectedItem, (int)MyComboBoxStep.SelectedItem);
                    MyTextBlockSSIM.Text = "SSIM = " + result.ToString();
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
            (BitmapSource, byte[]) temp = MakeBitmapSourceFromImageFile(paths[0],PixelFormats.Bgra32);
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
                    double result = SSIMNxN(MySourceColor1.pixels, MySourceColor2.pixels,
                        MySourceColor1.bitmap.PixelWidth, MySourceColor1.bitmap.PixelHeight,
                        (int)MyComboBoxWndSize.SelectedItem, (int)MyComboBoxStep.SelectedItem);
                    MyTextBlockSSIM.Text = "SSIM = " + result.ToString();
                }
                else
                {
                    MyTextBlockSSIM.Text = "SSIM";
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MySourceColor1.bitmap != null && MySourceColor2.bitmap != null)
            {
                double result = SSIMNxN(MySourceColor1.pixels, MySourceColor2.pixels,
                    MySourceColor1.bitmap.PixelWidth, MySourceColor1.bitmap.PixelHeight,
                    (int)MyComboBoxWndSize.SelectedItem, (int)MyComboBoxStep.SelectedItem);
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
