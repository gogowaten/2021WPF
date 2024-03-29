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

using MyHSV;

////MSE / PSNR vs SSIM の比較画像紹介 - Qiita
////https://qiita.com/yoya/items/510043d836c9f2f0fe2f

////Structural similarity - Wikipedia
////https://en.wikipedia.org/wiki/Structural_similarity

////画質評価指標SSIMについて調べてみた - Visualize
////https://visualize.hatenablog.com/entry/2016/02/20/144657

//WPF、Color(RGB)とHSVを相互変換するdll作ってみた、オブジェクトブラウザ使ってみた - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/15380324

//HSV.dll
//https://github.com/gogowaten/20180226forMyBlog/releases/download/HSVdll1.2.2/HSVdll1.2.2.zip
//Release HSVdll · gogowaten/20180226forMyBlog
//https://github.com/gogowaten/20180226forMyBlog/releases/tag/HSVdll1.2.2



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
        private const double CF1 = 0.01 * 179 * 0.01 * 179;//(0.01*179)^2=3.2041、色相のSSIMで使う定数

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
            double[] temp = new double[length];
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
                        s1[i] *= 255;
                        s2[i] *= 255;
                        v1[i] *= 255;
                        v2[i] *= 255;
                    }

                    //hTotal += SSIMHue(h1, h2, temp);
                    //hTotal += SSIMHue2(h1, h2, temp);
                    hTotal += SSIMHue3(h1, h2, temp);
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

        /// <summary>
        /// Hue(色相)専用SSIM
        /// </summary>
        /// <param name="h1">画像1のHue(0-360の値)</param>
        /// <param name="h2"></param>
        /// <param name="temp">計算用</param>
        /// <returns></returns>
        private double SSIMHue(double[] h1, double[] h2, double[] temp)
        {
            //色相が10と330の類似度は
            //このときの差は、330-10=320ではなく、360-330+10=40で計算
            for (int i = 0; i < h1.Length; i++)
            {
                double diff = Math.Abs(h1[i] - h2[i]);
                if (diff > 180) { diff = Math.Abs(diff - 360); }
                temp[i] = 180 - diff;
            }
            double ave1 = Average(h1);//平均
            double ave2 = Average(h2);
            double fixAve = Average(temp);
            double covar = Covariance(h1, ave1, h2, ave2);//共分散
            double vari1 = Variance(h1, ave1);//分散
            double vari2 = Variance(h2, ave2);
            double bunsi = (2 * 180.0 * fixAve + CF1) * (2 * covar + C2);//分子
            double bunbo = ((180 * 180) + (fixAve * fixAve) + CF1) * (vari1 + vari2 + C2);//分母
            double ssim = bunsi / bunbo;

            return ssim;
        }


        //共分散の計算がわからんので、
        /// <summary>
        /// Hue(色相)専用SSIM、分散と共分散も専用のもので計算
        /// </summary>
        /// <param name="h1"></param>
        /// <param name="h2"></param>
        /// <param name="temp"></param>
        /// <returns></returns>
        private double SSIMHue2(double[] h1, double[] h2, double[] temp)
        {
            double ave1 = Average(h1);//平均
            double ave2 = Average(h2);
            double vari1 = VarianceHue(h1, ave1);//分散
            double vari2 = VarianceHue(h2, ave2);
            double stdev1 = Math.Sqrt(vari1);//標準偏差
            double stdev2 = Math.Sqrt(vari2);//標準偏差
            double CF2 = 0.03 * 359 * (0.03 * 359);
            double bunsi = 2 * stdev1 * stdev2 + CF2;//分子
            double bunbo = vari1 + vari2 + CF2;//分母
            double ssim = bunsi / bunbo;


            //色相の類似度を0-1で表す、1のとき完全一致
            //色相が10と330の類似度は
            //このときの差は、330-10=320ではなく、360-330+10=40で計算
            //差の最大値は180なので、1-(40/180)=0.77777778
            for (int i = 0; i < h1.Length; i++)
            {
                double diff = Math.Abs(h1[i] - h2[i]);
                if (diff > 180) { diff = Math.Abs(diff - 360); }
                temp[i] = 1 - (diff / 180.0);
            }
            double fixAve = Average(temp);
            ssim *= fixAve;
            return ssim;
        }
        private double SSIMHue3(double[] h1, double[] h2, double[] temp)
        {
            double ave1 = AverageHue(h1);//色相の平均
            double ave2 = AverageHue(h2);
            double covar = CovarianceHue(h1, ave1, h2, ave2);//共分散
            double vari1 = VarianceHue(h1, ave1);//分散
            double vari2 = VarianceHue(h2, ave2);
            double CF2 = 0.03 * 359 * (0.03 * 359);
            double bunsi = 2 * covar + CF2;//分子            
            double bunbo = vari1 + vari2 + CF2;//分母
            double ssim = bunsi / bunbo;


            //色相の類似度を0-1で表す、1のとき完全一致
            //色相が10と330の類似度は
            //このときの差は、330-10=320ではなく、360-330+10=40で計算
            //差の最大値は180なので、1-(40/180)=0.77777778
            for (int i = 0; i < h1.Length; i++)
            {
                double diff = Math.Abs(h1[i] - h2[i]);
                if (diff > 180) { diff = Math.Abs(diff - 360); }
                temp[i] = 1 - (diff / 180.0);
            }
            double fixAve = Average(temp);
            ssim *= fixAve;
            return ssim;
        }
        private double SsimHue4(double[] h1, double[] h2)
        {
            int length = h1.Length;
            double sin1 = 0, cos1 = 0;
            double sin2 = 0, cos2 = 0;
            for (int i = 0; i < length; i++)
            {
                sin1 += Math.Sin(Radian(h1[i]));
                sin2 += Math.Sin(Radian(h2[i]));
                cos1 += Math.Cos(Radian(h1[i]));
                cos2 += Math.Cos(Radian(h2[i]));
            }
            //平均
            double ave1 = Degree(Math.Atan2(sin1, cos1));
            if (ave1 < 0) { ave1 += 360; }
            double ave2 = Degree(Math.Atan2(sin2, cos2));
            if (ave2 < 0) { ave2 += 360; }

            //分散
            double varp1 = 1 - Math.Sqrt((sin1 / length) * (sin1 / length) + (cos1 / length) * (cos1 / length));
            double varp2 = 1 - Math.Sqrt((sin2 / length) * (sin2 / length) + (cos2 / length) * (cos2 / length));

            //共分散
            double total = 0;
            for (int i = 0; i < length; i++)
            {
                total += Fix180(h1[i]) * Fix180(h2[i]);
            }
            double covar = total / length;

            double Fix180(double v)
            {
                double t = Math.Abs(v - ave1);
                if (t > 180) { return 360 - t; }
                else { return t; }
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

        private double HueFix(double[] vs1, double[] vs2)
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
        /// <summary>
        /// Hue(色相)専用共分散
        /// </summary>
        /// <param name="vs1">0-360</param>
        /// <param name="ave1"></param>
        /// <param name="vs2"></param>
        /// <param name="ave2"></param>
        /// <returns></returns>
        private double CovarianceHue(double[] vs1, double ave1, double[] vs2, double ave2)
        {
            if (vs1.Length != vs2.Length)
            {
                return double.NaN;
            }
            decimal total = 0;

            //for (int i = 0; i < vs1.Length; i++)
            //{
            //    decimal d1 = (decimal)(vs1[i] - ave1);
            //    if (d1 > 180) { d1 -= 180; }
            //    if (d1 < -180) { d1 += 180; }
            //    decimal d2 = (decimal)(vs2[i] - ave2);
            //    if (d2 > 180) { d2 -= 180; }
            //    if (d2 < -180) { d2 += 180; }

            //    decimal d1 = Math.Abs((decimal)(vs1[i] - ave1));
            //    if (d1 > 180) { d1 = 360 - d1; }
            //    decimal d2 = Math.Abs((decimal)(vs2[i] - ave2));
            //    if (d2 > 180) { d2 = 360 - d2; }

            //    total += d1 * d2;
            //}

            //double[] ds1 = new double[vs1.Length];
            //double[] ds2 = new double[vs1.Length];
            //for (int i = 0; i < vs1.Length; i++)
            //{
            //    double d1 = vs1[i] - ave1;
            //    double d2 = vs2[i] - ave2;
            //    double diff = Math.Abs(d1 - d2);
            //    if (diff > 180)
            //    {
            //        if (d1 > d2) { d1 -= 360; }
            //        else { d2 -= 360; }
            //    }
            //    ds1[i] = d1;
            //    ds2[i] = d2;
            //}
            //var d1Ave = Average(ds1);
            //var d2Ave = Average(ds2);

            //for (int i = 0; i < ds1.Length; i++)
            //{
            //    decimal d1 = (decimal)(ds1[i] - d1Ave);
            //    decimal d2 = (decimal)(ds2[i] - d2Ave);
            //    total += d1 * d2;
            //}

            //return (double)(total / vs2.Length);


            double[] ds1 = new double[vs1.Length];
            double[] ds2 = new double[vs1.Length];
            for (int i = 0; i < vs1.Length; i++)
            {
                ds1[i] = Math.Abs(vs1[i] - ave1) > 180 ? (vs1[i] - 360) : vs1[i];
                ds2[i] = Math.Abs(vs2[i] - ave2) > 180 ? (vs2[i] - 360) : vs2[i];
            }
            for (int i = 0; i < vs1.Length; i++)
            {
                total += (decimal)((ds1[i] - ave1) * (ds2[i] - ave2));
            }
            return (double)(total / vs1.Length);

        }
        //private double CovarianceHue(double[] vs1, double[] vs2)
        //{
        //    if (vs1.Length != vs2.Length)
        //    {
        //        return double.NaN;
        //    }
        //    decimal total = 0;
        //    //色相は上下じゃなくて相対的なのものだから負の相関になる場合でも
        //    //差が180以上だった場合はvs2を-360して計算
        //    double[] temp = new double[vs1.Length];
        //    for (int i = 0; i < vs1.Length; i++)
        //    {
        //        if(vs1)
        //    }
        //    for (int i = 0; i < vs1.Length; i++)
        //    {
        //        //decimal d1 = (decimal)(vs1[i] - ave1);
        //        decimal d1 = Math.Abs((decimal)(vs1[i] - ave1));
        //        if (d1 > 180) { d1 -= 180; }
        //        if (d1 < -180) { d1 += 180; }
        //        //decimal d2 = (decimal)(vs2[i] - ave2);
        //        decimal d2 = Math.Abs((decimal)(vs2[i] - ave2));
        //        if (d2 > 180) { d2 -= 180; }
        //        if (d2 < -180) { d2 += 180; }
        //        total += d1 * d2;
        //    }
        //    return (double)(total / vs2.Length);
        //}

        private double Covariance(byte[] vs1, byte[] vs2)
        {
            if (vs1.Length != vs2.Length)
            {
                return double.NaN;
            }
            //偏差が180以上だった場合は-180して計算
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
        /// <summary>
        /// Hue(色相)や角度の分散
        /// </summary>
        /// <param name="vs"></param>
        /// <param name="average"></param>
        /// <returns>0.0-1.0</returns>
        private double VarianceHue(double[] vs, double average)
        {
            decimal total = 0;
            for (int i = 0; i < vs.Length; i++)
            {
                decimal temp = Math.Abs((decimal)(vs[i] - average));
                if (temp > 180) { temp = 360 - temp; }
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

        /// <summary>
        /// 角度やHueの平均
        /// </summary>
        /// <param name="hue">0から360度</param>
        /// <returns>0-360</returns>
        private double AverageHue(double[] hue)
        {
            double sin = 0, cos = 0;
            TotalSinAndCos(hue, ref sin, ref cos);
            double result = (double)Degree(Math.Atan2(sin, cos));
            if (result < 0) { result += 360; }
            return result;
        }
        private void TotalSinAndCos(double[] hue, ref double sin, ref double cos)
        {
            for (int i = 0; i < hue.Length; i++)
            {
                sin += Math.Sin(Radian(hue[i]));
                cos += Math.Cos(Radian(hue[i]));
            }
        }
        private static double Radian(double degree)
        {
            return degree / 180.0 * Math.PI;
        }
        private static double Degree(double radian)
        {
            return radian / Math.PI * 180;
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
