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

using System.Diagnostics;
//using System.Drawing;


namespace _20210429_Lanczosっぽいなにかできれいな縮小
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapSource MyBitmapOrigin;
        private BitmapSource MyBitmapOrigin32bit;
        private ImageBrush MyImageBrush;
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.Top = 0;
            this.Left = 0;
#endif
            MyImageBrush = MakeTileBrush(MakeCheckeredPattern(16, Colors.WhiteSmoke, Colors.LightGray));
            //this.Background = MakeTileBrush(MakeCheckeredPattern(16, Colors.WhiteSmoke, Colors.LightGray));

            RenderOptions.SetBitmapScalingMode(MyImage, BitmapScalingMode.NearestNeighbor);
        }



        ////処理時間計測
        //private void MyExe(
        //    Func<BitmapSource, int, int, int, BitmapSource> func,
        //    BitmapSource source, int width, int height, int a)
        //{
        //    var sw = new Stopwatch();
        //    sw.Start();
        //    var bitmap = func(source, width, height, a);
        //    sw.Stop();
        //    MyStatusItem.Content = $"処理時間：{sw.Elapsed.TotalSeconds:000.000}秒, {func.Method.Name}";
        //    MyImage.Source = bitmap;
        //}

        //処理時間計測
        private void MyExe2(
            Func<BitmapSource, int, int, int, Func<double, int, double>, BitmapSource> func,
            Func<double, int, double> weightFunc,
            BitmapSource source, int width, int height, int n)

        {
            var sw = new Stopwatch();
            sw.Start();
            BitmapSource bitmap = func(source, width, height, n, weightFunc);
            sw.Stop();
            MyStatusItem.Content = $"処理時間：{sw.Elapsed.TotalSeconds:000.000}秒, {func.Method.Name}, {weightFunc.Method.Name}";
            MyImage.Source = bitmap;
        }
        //処理時間計測
        private void MyExe2(
            Func<BitmapSource, int, int, int, double, Func<double, int, double, double>, BitmapSource> func,
            Func<double, int, double, double> weightFunc,
            BitmapSource source, int width, int height, int n, double scale)

        {
            var sw = new Stopwatch();
            sw.Start();
            BitmapSource bitmap = func(source, width, height, n, scale, weightFunc);
            sw.Stop();
            MyStatusItem.Content = $"処理時間：{sw.Elapsed.TotalSeconds:000.000}秒, {func.Method.Name}, {weightFunc.Method.Name}";
            MyImage.Source = bitmap;
        }


        //窓関数
        private double Sinc(double d)
        {
            return Math.Sin(Math.PI * d) / (Math.PI * d);
        }
        /// <summary>
        /// ランチョス補完法での重み計算
        /// </summary>
        /// <param name="d">距離</param>
        /// <param name="n">最大参照距離</param>
        /// <returns></returns>
        private double GetLanczosWeight(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else return Sinc(d) * Sinc(d / n);
        }

        //ランチョス改変、ボカシが入る
        //(距離/最大参照距離)の窓関数をそのまま重みにする
        private double GetLanczosWeightA(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else return Sinc(d / n);
        }

        private double GetLanczosWeightB(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                double w = Sinc(d / n);
                return w * w;
            }
        }

        private double GetLanczosWeightC(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                double w = Sinc(d / n);
                return Math.Pow(w, 1.0 / n);
            }
        }

        private double GetLanczosWeightD(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                double w = Sinc(d / (3.0 / 4 * n));
                return w * w;
            }
        }

        private double GetLanczosWeightE(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                double w1 = Sinc(d / n);
                double w2 = Sinc(d / (n * 2.0));
                return w1 * w2;
            }
        }

        private double GetLanczosWeightF(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                double nn = (1 + n) / 2.0;
                return Sinc(d / nn);
            }
        }

        //かなりシャープ
        private double GetLanczosWeightG(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                double nn = n / 2.0;
                return Sinc(d / nn);
            }
        }

        //
        private double SincA(double x)
        {
            return Math.Exp(-x) * Math.Cos(Math.PI * x);
        }
        private double GetLanczosWeightH(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return SincA(d / (n * 2.0));
            }
        }
        private double GetLanczosWeightI(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / n) * Sinc(d / (n * 2));
            }
        }
        private double GetLanczosWeightJ(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / n) * Sinc(d / (n * 1.5));
            }
        }
        private double GetLanczosWeightK(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / 1.5) * Sinc(d / n);
            }
        }
        private double GetLanczosWeightL(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / 2.0) * Sinc(d / n);
            }
        }
        private double GetLanczosWeightM(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / 3.0) * Sinc(d / n);
            }
        }
        private double GetLanczosWeightN(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / (n - 1)) * Sinc(d / n);
            }
        }
        private double GetLanczosWeightO(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / (n - 1)) * Sinc(d / (n * 2));
            }
        }
        private double GetLanczosWeightP(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / (n - 0.5)) * Sinc(d / (n * 2));
            }
        }
        private double GetLanczosWeightQ(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / (n - 0.5)) * Sinc(d / n);
            }
        }
        private double GetLanczosWeightR(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d) * Sinc(d / (n));
            }
        }
        private double GetLanczosWeightS(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / 2.0) * Sinc(d / (n * 2));
            }
        }
        private double GetLanczosWeightT(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / (n * 0.75)) * Sinc(d / n);
            }
        }
        private double GetLanczosWeightU(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / (n * 0.75)) * Sinc(d / (n * n));
            }
        }
        private double GetLanczosWeightV(double d, int n)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / (n * 0.5)) * Sinc(d / n);
            }
        }
        private double GetLanczosWeightW(double d, int n, double scale)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / scale) * Sinc(d / n);
            }
        }
        private double GetLanczosWeightX(double d, int n, double scale)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / scale) * Sinc(d / (n * scale));
            }
        }
        private double GetLanczosWeightY(double d, int n, double scale)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / (scale - 1)) * Sinc(d / (n * scale));
            }
        }
        //Xの0.75版
        private double GetLanczosWeightZ(double d, int n, double scale)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / (scale * 0.75)) * Sinc(d / (n * scale));
            }
        }
        private double GetLanczosWeightAA(double d, int n, double scale)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                //return Sinc(d / (scale * 0.75)) * Sinc(d / n);
                //return Sinc(d / (scale * 0.75)) * Sinc(d / n / (scale * 0.75));
                return Sinc(d / (scale * 0.75)) * Sinc(d / n * (scale * 0.75));
            }
        }
        //Xの0.9版
        private double GetLanczosWeightAB(double d, int n, double scale)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / (scale * 0.9)) * Sinc(d / (n * scale));
            }
        }
        private double GetLanczosWeightAC(double d, int n, double scale)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / scale) * Sinc(d / (n * (scale * 0.5)));
            }
        }
        private double GetLanczosWeightAD(double d, int n, double scale)
        {
            if (d == 0) return 1.0;
            else if (d > n) return 0.0;
            else
            {
                return Sinc(d / scale * 1);
            }
        }

        ////ランチョス改変、ボカシが入る
        //private double GetLanczosWeightB(double d, int n)
        //{
        //    if (d == 0) return 1.0;
        //    else if (d > n) return 0.0;
        //    else return Sinc(d / n * 1.4);//n=1.0~1.5程度、1.0ソフト～1.5シャープ
        //}



        /// <summary>
        /// 画像の拡大縮小、ランチョス法で補完、PixelFormats.Bgra32専用)
        /// 通常版をセパラブルとParallelで高速化
        /// </summary>
        /// <param name="source">PixelFormats.Bgra32のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="n">最大参照距離、3か4がいい</param>
        /// <returns></returns>
        private BitmapSource LanczosBgra32Ex(BitmapSource source, int width, int height, int n)
        {
            //1ピクセルあたりのバイト数、Byte / Pixel
            int pByte = (source.Format.BitsPerPixel + 7) / 8;

            //元画像の画素値の配列作成
            int sourceWidth = source.PixelWidth;
            int sourceHeight = source.PixelHeight;
            int sourceStride = sourceWidth * pByte;//1行あたりのbyte数
            byte[] sourcePixels = new byte[sourceHeight * sourceStride];
            source.CopyPixels(sourcePixels, sourceStride, 0);

            //変換後の画像の画素値の配列用
            double widthScale = (double)sourceWidth / width;//横倍率
            double heightScale = (double)sourceHeight / height;
            int stride = width * pByte;
            byte[] pixels = new byte[height * stride];

            //横処理用配列
            double[] xResult = new double[sourceHeight * stride];

            //横処理
            _ = Parallel.For(0, sourceHeight, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    //参照点
                    double rx = (x + 0.5) * widthScale;
                    //参照点四捨五入で基準
                    int xKijun = (int)(rx + 0.5);
                    //修正した重み取得
                    double[] ws = GetFixWeihgts(rx, n);

                    double bSum = 0, gSum = 0, rSum = 0, aSum = 0;
                    double alphaFix = 0;
                    int pp;
                    for (int xx = -n; xx < n; xx++)
                    {
                        int xc = xKijun + xx;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                        pp = (y * sourceStride) + (xc * pByte);
                        double weight = ws[xx + n];
                        //完全透明ピクセル(a=0)だった場合はRGBは計算しないで
                        //重みだけ足し算して後で使う
                        if (sourcePixels[pp + 3] == 0)
                        {
                            alphaFix += weight;
                            continue;
                        }
                        bSum += sourcePixels[pp] * weight;
                        gSum += sourcePixels[pp + 1] * weight;
                        rSum += sourcePixels[pp + 2] * weight;
                        aSum += sourcePixels[pp + 3] * weight;
                    }
                    //                    C#、WPF、バイリニア法での画像の拡大縮小変換、半透明画像(32bit画像)対応版 - 午後わてんのブログ
                    //https://gogowaten.hatenablog.com/entry/2021/04/17/151803#32bit%E3%81%A824bit%E3%81%AF%E9%81%95%E3%81%A3%E3%81%9F

                    //完全透明ピクセルによるRGB値の修正
                    //参照範囲がすべて完全透明だった場合は0のままでいいので計算しない
                    if (alphaFix == 1) continue;
                    //完全透明ピクセルが混じっていた場合は、その分を差し引いてRGB修正する
                    double rgbFix = 1 / (1 - alphaFix);
                    bSum *= rgbFix;
                    gSum *= rgbFix;
                    rSum *= rgbFix;

                    pp = y * stride + x * pByte;
                    xResult[pp] = bSum;
                    xResult[pp + 1] = gSum;
                    xResult[pp + 2] = rSum;
                    xResult[pp + 3] = aSum;
                }
            });

            //縦処理
            _ = Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    double ry = (y + 0.5) * heightScale;
                    int yKijun = (int)(ry + 0.5);

                    double[] ws = GetFixWeihgts(ry, n);
                    double bSum = 0, gSum = 0, rSum = 0, aSum = 0;
                    double alphaFix = 0;
                    int pp;
                    for (int yy = -n; yy < n; yy++)
                    {
                        int yc = yKijun + yy;
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                        pp = (yc * stride) + (x * pByte);
                        double weight = ws[yy + n];

                        if (xResult[pp + 3] == 0)
                        {
                            alphaFix += weight;
                            continue;
                        }
                        bSum += xResult[pp] * weight;
                        gSum += xResult[pp + 1] * weight;
                        rSum += xResult[pp + 2] * weight;
                        aSum += xResult[pp + 3] * weight;
                    }

                    if (alphaFix == 1) continue;
                    //完全透明ピクセルが混じっていた場合は、その分を差し引いてRGB修正する
                    double rgbFix = 1 / (1 - alphaFix);
                    bSum *= rgbFix;
                    gSum *= rgbFix;
                    rSum *= rgbFix;

                    //0～255の範囲を超えることがあるので、修正
                    bSum = bSum < 0 ? 0 : bSum > 255 ? 255 : bSum;
                    gSum = gSum < 0 ? 0 : gSum > 255 ? 255 : gSum;
                    rSum = rSum < 0 ? 0 : rSum > 255 ? 255 : rSum;
                    aSum = aSum < 0 ? 0 : aSum > 255 ? 255 : aSum;
                    int ap = (y * stride) + (x * pByte);
                    pixels[ap] = (byte)(bSum + 0.5);
                    pixels[ap + 1] = (byte)(gSum + 0.5);
                    pixels[ap + 2] = (byte)(rSum + 0.5);
                    pixels[ap + 3] = (byte)(aSum + 0.5);
                }
            });


            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;

            //修正した重み取得
            double[] GetFixWeihgts(double r, int n)
            {
                int nn = n * 2;//全体の参照距離
                //基準距離
                double s = r - (int)r;
                double d = (s < 0.5) ? 0.5 - s : 0.5 - s + 1;

                //各重みと重み合計
                double[] ws = new double[nn];
                double sum = 0;
                for (int i = -n; i < n; i++)
                {
                    double w = GetLanczosWeight(Math.Abs(d + i), n);
                    sum += w;
                    ws[i + n] = w;
                }

                //重み合計で割り算して修正、全体で100%(1.0)にする
                for (int i = 0; i < nn; i++)
                {
                    ws[i] /= sum;
                }
                return ws;
            }
        }


        /// <summary>
        /// 画像の拡大縮小、ランチョス法で補完、PixelFormats.Bgr24専用)
        /// 通常版をセパラブルとParallelで高速化
        /// </summary>
        /// <param name="source">PixelFormats.Bgr24のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="n">最大参照距離、3か4がいい</param>
        /// <returns></returns>
        private BitmapSource LanczosBgr24Ex(BitmapSource source, int width, int height, int n)
        {
            //1ピクセルあたりのバイト数、Byte / Pixel
            int pByte = (source.Format.BitsPerPixel + 7) / 8;

            //元画像の画素値の配列作成
            int sourceWidth = source.PixelWidth;
            int sourceHeight = source.PixelHeight;
            int sourceStride = sourceWidth * pByte;//1行あたりのbyte数
            byte[] sourcePixels = new byte[sourceHeight * sourceStride];
            source.CopyPixels(sourcePixels, sourceStride, 0);

            //変換後の画像の画素値の配列用
            double widthScale = (double)sourceWidth / width;//横倍率
            double heightScale = (double)sourceHeight / height;
            int stride = width * pByte;
            byte[] pixels = new byte[height * stride];

            //横処理用配列
            double[] xResult = new double[sourceHeight * stride];

            //横処理
            _ = Parallel.For(0, sourceHeight, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    //参照点
                    double rx = (x + 0.5) * widthScale;
                    //参照点四捨五入で基準
                    int xKijun = (int)(rx + 0.5);
                    //修正した重み取得
                    double[] ws = GetFixWeihgts(rx, n);

                    double bSum = 0, gSum = 0, rSum = 0;
                    int pp;
                    for (int xx = -n; xx < n; xx++)
                    {
                        int xc = xKijun + xx;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                        pp = (y * sourceStride) + (xc * pByte);
                        double weight = ws[xx + n];
                        bSum += sourcePixels[pp] * weight;
                        gSum += sourcePixels[pp + 1] * weight;
                        rSum += sourcePixels[pp + 2] * weight;
                    }
                    pp = y * stride + x * pByte;
                    xResult[pp] = bSum;
                    xResult[pp + 1] = gSum;
                    xResult[pp + 2] = rSum;
                }
            });

            //縦処理
            _ = Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    double ry = (y + 0.5) * heightScale;
                    int yKijun = (int)(ry + 0.5);

                    double[] ws = GetFixWeihgts(ry, n);
                    double bSum = 0, gSum = 0, rSum = 0;
                    int pp;
                    for (int yy = -n; yy < n; yy++)
                    {
                        int yc = yKijun + yy;
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                        pp = (yc * stride) + (x * pByte);
                        double weight = ws[yy + n];
                        bSum += xResult[pp] * weight;
                        gSum += xResult[pp + 1] * weight;
                        rSum += xResult[pp + 2] * weight;
                    }
                    //0～255の範囲を超えることがあるので、修正
                    bSum = bSum < 0 ? 0 : bSum > 255 ? 255 : bSum;
                    gSum = gSum < 0 ? 0 : gSum > 255 ? 255 : gSum;
                    rSum = rSum < 0 ? 0 : rSum > 255 ? 255 : rSum;
                    int ap = (y * stride) + (x * pByte);
                    pixels[ap] = (byte)(bSum + 0.5);
                    pixels[ap + 1] = (byte)(gSum + 0.5);
                    pixels[ap + 2] = (byte)(rSum + 0.5);
                }
            });


            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;

            //修正した重み取得
            double[] GetFixWeihgts(double r, int n)
            {
                int nn = n * 2;//全体の参照距離
                //基準距離
                double s = r - (int)r;
                double d = (s < 0.5) ? 0.5 - s : 0.5 - s + 1;

                //各重みと重み合計
                double[] ws = new double[nn];
                double sum = 0;
                for (int i = -n; i < n; i++)
                {
                    double w = GetLanczosWeight(Math.Abs(d + i), n);
                    sum += w;
                    ws[i + n] = w;
                }

                //重み合計で割り算して修正、全体で100%(1.0)にする
                for (int i = 0; i < nn; i++)
                {
                    ws[i] /= sum;
                }
                return ws;
            }
        }





        /// <summary>
        /// 画像の拡大縮小、ランチョス法で補完、PixelFormats.Bgra32専用)
        /// 
        /// </summary>
        /// <param name="source">PixelFormats.Bgra32のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="n">最大参照距離、3か4がいい</param>
        /// <returns></returns>
        private BitmapSource LanczosBgra32(BitmapSource source, int width, int height, int n, Func<double, int, double> myFunc)
        {
            //1ピクセルあたりのバイト数、Byte / Pixel
            int pByte = (source.Format.BitsPerPixel + 7) / 8;

            //元画像の画素値の配列作成
            int sourceWidth = source.PixelWidth;
            int sourceHeight = source.PixelHeight;
            int sourceStride = sourceWidth * pByte;//1行あたりのbyte数
            byte[] sourcePixels = new byte[sourceHeight * sourceStride];
            source.CopyPixels(sourcePixels, sourceStride, 0);

            //変換後の画像の画素値の配列用
            double widthScale = (double)sourceWidth / width;//横倍率
            double heightScale = (double)sourceHeight / height;
            int stride = width * pByte;
            byte[] pixels = new byte[height * stride];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //参照点
                    double rx = (x + 0.5) * widthScale;
                    double ry = (y + 0.5) * heightScale;
                    //参照点四捨五入で基準
                    int xKijun = (int)(rx + 0.5);
                    int yKijun = (int)(ry + 0.5);
                    //修正した重み取得
                    var ws = GetFixWeights(rx, ry, n);

                    double bSum = 0, gSum = 0, rSum = 0, aSum = 0;
                    double alphaFix = 0;
                    //参照範囲は基準から上(xは左)へn、下(xは右)へn-1の範囲
                    for (int yy = -n; yy < n; yy++)
                    {
                        int yc = yKijun + yy;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                        for (int xx = -n; xx < n; xx++)
                        {
                            int xc = xKijun + xx;
                            xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                            int pp = (yc * sourceStride) + (xc * pByte);
                            double weight = ws[xx + n, yy + n];
                            //完全透明ピクセル(a=0)だった場合はRGBは計算しないで
                            //重みだけ足し算して後で使う
                            if (sourcePixels[pp + 3] == 0)
                            {
                                alphaFix += weight;
                                continue;
                            }
                            bSum += sourcePixels[pp] * weight;
                            gSum += sourcePixels[pp + 1] * weight;
                            rSum += sourcePixels[pp + 2] * weight;
                            aSum += sourcePixels[pp + 3] * weight;
                        }
                    }

                    //                    C#、WPF、バイリニア法での画像の拡大縮小変換、半透明画像(32bit画像)対応版 - 午後わてんのブログ
                    //https://gogowaten.hatenablog.com/entry/2021/04/17/151803#32bit%E3%81%A824bit%E3%81%AF%E9%81%95%E3%81%A3%E3%81%9F
                    //完全透明ピクセルによるRGB値の修正
                    //参照範囲がすべて完全透明だった場合は0のままでいいので計算しない
                    if (alphaFix == 1) continue;
                    //完全透明ピクセルが混じっていた場合は、その分を差し引いてRGB修正する
                    double rgbFix = 1 / (1 - alphaFix);
                    bSum *= rgbFix;
                    gSum *= rgbFix;
                    rSum *= rgbFix;

                    //0～255の範囲を超えることがあるので、修正
                    bSum = bSum < 0 ? 0 : bSum > 255 ? 255 : bSum;
                    gSum = gSum < 0 ? 0 : gSum > 255 ? 255 : gSum;
                    rSum = rSum < 0 ? 0 : rSum > 255 ? 255 : rSum;
                    aSum = aSum < 0 ? 0 : aSum > 255 ? 255 : aSum;

                    int ap = (y * stride) + (x * pByte);
                    pixels[ap] = (byte)(bSum + 0.5);
                    pixels[ap + 1] = (byte)(gSum + 0.5);
                    pixels[ap + 2] = (byte)(rSum + 0.5);
                    pixels[ap + 3] = (byte)(aSum + 0.5);
                }
            };

            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;

            //修正した重み取得
            double[,] GetFixWeights(double rx, double ry, int n)
            {
                int nn = n * 2;//全体の参照距離
                //基準になる距離計算
                double sx = rx - (int)rx;
                double sy = ry - (int)ry;
                double dx = (sx < 0.5) ? 0.5 - sx : 0.5 - sx + 1;
                double dy = (sy < 0.5) ? 0.5 - sy : 0.5 - sy + 1;

                //各ピクセルの重みと、重み合計を計算
                double[] xw = new double[nn];
                double[] yw = new double[nn];
                double xSum = 0, ySum = 0;
                for (int i = -n; i < n; i++)
                {
                    double x = myFunc(Math.Abs(dx + i), n);
                    xSum += x;
                    xw[i + n] = x;
                    double y = myFunc(Math.Abs(dy + i), n);
                    ySum += y;
                    yw[i + n] = y;
                }

                //重み合計で割り算して修正、全体で100%(1.0)にする
                for (int i = 0; i < nn; i++)
                {
                    xw[i] /= xSum;
                    yw[i] /= ySum;
                }

                // x * y
                double[,] ws = new double[nn, nn];
                for (int y = 0; y < nn; y++)
                {
                    for (int x = 0; x < nn; x++)
                    {
                        ws[x, y] = xw[x] * yw[y];
                    }
                }
                return ws;
            }
        }


        /// <summary>
        /// 画像の拡大縮小、ランチョス法で補完、PixelFormats.Bgra32専用)
        /// 
        /// </summary>
        /// <param name="source">PixelFormats.Bgra32のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="n">最大参照距離、3か4がいい</param>
        /// <returns></returns>
        private BitmapSource LanczosBgra32(BitmapSource source, int width, int height, int n, double scale, Func<double, int, double, double> myFunc)
        {
            //1ピクセルあたりのバイト数、Byte / Pixel
            int pByte = (source.Format.BitsPerPixel + 7) / 8;

            //元画像の画素値の配列作成
            int sourceWidth = source.PixelWidth;
            int sourceHeight = source.PixelHeight;
            int sourceStride = sourceWidth * pByte;//1行あたりのbyte数
            byte[] sourcePixels = new byte[sourceHeight * sourceStride];
            source.CopyPixels(sourcePixels, sourceStride, 0);

            //変換後の画像の画素値の配列用
            double widthScale = (double)sourceWidth / width;//横倍率
            double heightScale = (double)sourceHeight / height;
            int stride = width * pByte;
            byte[] pixels = new byte[height * stride];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //参照点
                    double rx = (x + 0.5) * widthScale;
                    double ry = (y + 0.5) * heightScale;
                    //参照点四捨五入で基準
                    int xKijun = (int)(rx + 0.5);
                    int yKijun = (int)(ry + 0.5);
                    //修正した重み取得
                    var ws = GetFixWeights(rx, ry, n);

                    double bSum = 0, gSum = 0, rSum = 0, aSum = 0;
                    double alphaFix = 0;
                    //参照範囲は基準から上(xは左)へn、下(xは右)へn-1の範囲
                    for (int yy = -n; yy < n; yy++)
                    {
                        int yc = yKijun + yy;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                        for (int xx = -n; xx < n; xx++)
                        {
                            int xc = xKijun + xx;
                            xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                            int pp = (yc * sourceStride) + (xc * pByte);
                            double weight = ws[xx + n, yy + n];
                            //完全透明ピクセル(a=0)だった場合はRGBは計算しないで
                            //重みだけ足し算して後で使う
                            if (sourcePixels[pp + 3] == 0)
                            {
                                alphaFix += weight;
                                continue;
                            }
                            bSum += sourcePixels[pp] * weight;
                            gSum += sourcePixels[pp + 1] * weight;
                            rSum += sourcePixels[pp + 2] * weight;
                            aSum += sourcePixels[pp + 3] * weight;
                        }
                    }

                    //                    C#、WPF、バイリニア法での画像の拡大縮小変換、半透明画像(32bit画像)対応版 - 午後わてんのブログ
                    //https://gogowaten.hatenablog.com/entry/2021/04/17/151803#32bit%E3%81%A824bit%E3%81%AF%E9%81%95%E3%81%A3%E3%81%9F
                    //完全透明ピクセルによるRGB値の修正
                    //参照範囲がすべて完全透明だった場合は0のままでいいので計算しない
                    if (alphaFix == 1) continue;
                    //完全透明ピクセルが混じっていた場合は、その分を差し引いてRGB修正する
                    double rgbFix = 1 / (1 - alphaFix);
                    bSum *= rgbFix;
                    gSum *= rgbFix;
                    rSum *= rgbFix;

                    //0～255の範囲を超えることがあるので、修正
                    bSum = bSum < 0 ? 0 : bSum > 255 ? 255 : bSum;
                    gSum = gSum < 0 ? 0 : gSum > 255 ? 255 : gSum;
                    rSum = rSum < 0 ? 0 : rSum > 255 ? 255 : rSum;
                    aSum = aSum < 0 ? 0 : aSum > 255 ? 255 : aSum;

                    int ap = (y * stride) + (x * pByte);
                    pixels[ap] = (byte)(bSum + 0.5);
                    pixels[ap + 1] = (byte)(gSum + 0.5);
                    pixels[ap + 2] = (byte)(rSum + 0.5);
                    pixels[ap + 3] = (byte)(aSum + 0.5);
                }
            };

            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;

            //修正した重み取得
            double[,] GetFixWeights(double rx, double ry, int n)
            {
                int nn = n * 2;//全体の参照距離
                //基準になる距離計算
                double sx = rx - (int)rx;
                double sy = ry - (int)ry;
                double dx = (sx < 0.5) ? 0.5 - sx : 0.5 - sx + 1;
                double dy = (sy < 0.5) ? 0.5 - sy : 0.5 - sy + 1;

                //各ピクセルの重みと、重み合計を計算
                double[] xw = new double[nn];
                double[] yw = new double[nn];
                double xSum = 0, ySum = 0;
                for (int i = -n; i < n; i++)
                {
                    double x = myFunc(Math.Abs(dx + i), n, scale);
                    xSum += x;
                    xw[i + n] = x;
                    double y = myFunc(Math.Abs(dy + i), n, scale);
                    ySum += y;
                    yw[i + n] = y;
                }

                //重み合計で割り算して修正、全体で100%(1.0)にする
                for (int i = 0; i < nn; i++)
                {
                    xw[i] /= xSum;
                    yw[i] /= ySum;
                }

                // x * y
                double[,] ws = new double[nn, nn];
                for (int y = 0; y < nn; y++)
                {
                    for (int x = 0; x < nn; x++)
                    {
                        ws[x, y] = xw[x] * yw[y];
                    }
                }
                return ws;
            }
        }


        ////未使用
        ///// <summary>
        ///// 画像の拡大縮小、ランチョス法で補完、PixelFormats.Bgra32専用)
        ///// 通常版
        ///// </summary>
        ///// <param name="source">PixelFormats.Bgra32のBitmap</param>
        ///// <param name="width">変換後の横ピクセル数を指定</param>
        ///// <param name="height">変換後の縦ピクセル数を指定</param>
        ///// <param name="n">最大参照距離、3か4がいい</param>
        ///// <returns></returns>
        //private BitmapSource LanczosBgra32(BitmapSource source, int width, int height, int n)
        //{
        //    //1ピクセルあたりのバイト数、Byte / Pixel
        //    int pByte = (source.Format.BitsPerPixel + 7) / 8;

        //    //元画像の画素値の配列作成
        //    int sourceWidth = source.PixelWidth;
        //    int sourceHeight = source.PixelHeight;
        //    int sourceStride = sourceWidth * pByte;//1行あたりのbyte数
        //    byte[] sourcePixels = new byte[sourceHeight * sourceStride];
        //    source.CopyPixels(sourcePixels, sourceStride, 0);

        //    //変換後の画像の画素値の配列用
        //    double widthScale = (double)sourceWidth / width;//横倍率
        //    double heightScale = (double)sourceHeight / height;
        //    int stride = width * pByte;
        //    byte[] pixels = new byte[height * stride];

        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            //参照点
        //            double rx = (x + 0.5) * widthScale;
        //            double ry = (y + 0.5) * heightScale;
        //            //参照点四捨五入で基準
        //            int xKijun = (int)(rx + 0.5);
        //            int yKijun = (int)(ry + 0.5);
        //            //修正した重み取得
        //            var ws = GetFixWeights(rx, ry, n);

        //            double bSum = 0, gSum = 0, rSum = 0, aSum = 0;
        //            double alphaFix = 0;
        //            //参照範囲は基準から上(xは左)へn、下(xは右)へn-1の範囲
        //            for (int yy = -n; yy < n; yy++)
        //            {
        //                int yc = yKijun + yy;
        //                //マイナス座標や画像サイズを超えていたら、収まるように修正
        //                yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
        //                for (int xx = -n; xx < n; xx++)
        //                {
        //                    int xc = xKijun + xx;
        //                    xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
        //                    int pp = (yc * sourceStride) + (xc * pByte);
        //                    double weight = ws[xx + n, yy + n];
        //                    //完全透明ピクセル(a=0)だった場合はRGBは計算しないで
        //                    //重みだけ足し算して後で使う
        //                    if (sourcePixels[pp + 3] == 0)
        //                    {
        //                        alphaFix += weight;
        //                        continue;
        //                    }
        //                    bSum += sourcePixels[pp] * weight;
        //                    gSum += sourcePixels[pp + 1] * weight;
        //                    rSum += sourcePixels[pp + 2] * weight;
        //                    aSum += sourcePixels[pp + 3] * weight;
        //                }
        //            }

        //            //                    C#、WPF、バイリニア法での画像の拡大縮小変換、半透明画像(32bit画像)対応版 - 午後わてんのブログ
        //            //https://gogowaten.hatenablog.com/entry/2021/04/17/151803#32bit%E3%81%A824bit%E3%81%AF%E9%81%95%E3%81%A3%E3%81%9F
        //            //完全透明ピクセルによるRGB値の修正
        //            //参照範囲がすべて完全透明だった場合は0のままでいいので計算しない
        //            if (alphaFix == 1) continue;
        //            //完全透明ピクセルが混じっていた場合は、その分を差し引いてRGB修正する
        //            double rgbFix = 1 / (1 - alphaFix);
        //            bSum *= rgbFix;
        //            gSum *= rgbFix;
        //            rSum *= rgbFix;

        //            //0～255の範囲を超えることがあるので、修正
        //            bSum = bSum < 0 ? 0 : bSum > 255 ? 255 : bSum;
        //            gSum = gSum < 0 ? 0 : gSum > 255 ? 255 : gSum;
        //            rSum = rSum < 0 ? 0 : rSum > 255 ? 255 : rSum;
        //            aSum = aSum < 0 ? 0 : aSum > 255 ? 255 : aSum;

        //            int ap = (y * stride) + (x * pByte);
        //            pixels[ap] = (byte)(bSum + 0.5);
        //            pixels[ap + 1] = (byte)(gSum + 0.5);
        //            pixels[ap + 2] = (byte)(rSum + 0.5);
        //            pixels[ap + 3] = (byte)(aSum + 0.5);
        //        }
        //    };

        //    BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
        //    return bitmap;

        //    //修正した重み取得
        //    double[,] GetFixWeights(double rx, double ry, int n)
        //    {
        //        int nn = n * 2;//全体の参照距離
        //        //基準になる距離計算
        //        double sx = rx - (int)rx;
        //        double sy = ry - (int)ry;
        //        double dx = (sx < 0.5) ? 0.5 - sx : 0.5 - sx + 1;
        //        double dy = (sy < 0.5) ? 0.5 - sy : 0.5 - sy + 1;

        //        //各ピクセルの重みと、重み合計を計算
        //        double[] xw = new double[nn];
        //        double[] yw = new double[nn];
        //        double xSum = 0, ySum = 0;
        //        for (int i = -n; i < n; i++)
        //        {
        //            double x = GetLanczosWeight(Math.Abs(dx + i), n);
        //            xSum += x;
        //            xw[i + n] = x;
        //            double y = GetLanczosWeight(Math.Abs(dy + i), n);
        //            ySum += y;
        //            yw[i + n] = y;
        //        }

        //        //重み合計で割り算して修正、全体で100%(1.0)にする
        //        for (int i = 0; i < nn; i++)
        //        {
        //            xw[i] /= xSum;
        //            yw[i] /= ySum;
        //        }

        //        // x * y
        //        double[,] ws = new double[nn, nn];
        //        for (int y = 0; y < nn; y++)
        //        {
        //            for (int x = 0; x < nn; x++)
        //            {
        //                ws[x, y] = xw[x] * yw[y];
        //            }
        //        }
        //        return ws;
        //    }
        //}



        //未使用
        /// <summary>
        /// 画像の拡大縮小、ランチョス法で補完、PixelFormats.Bgr24専用)
        /// 通常版
        /// </summary>
        /// <param name="source">PixelFormats.Bgr24のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="n">最大参照距離、3か4がいい</param>
        /// <returns></returns>
        private BitmapSource LanczosBgr24(BitmapSource source, int width, int height, int n)
        {
            //1ピクセルあたりのバイト数、Byte / Pixel
            int pByte = (source.Format.BitsPerPixel + 7) / 8;

            //元画像の画素値の配列作成
            int sourceWidth = source.PixelWidth;
            int sourceHeight = source.PixelHeight;
            int sourceStride = sourceWidth * pByte;//1行あたりのbyte数
            byte[] sourcePixels = new byte[sourceHeight * sourceStride];
            source.CopyPixels(sourcePixels, sourceStride, 0);

            //変換後の画像の画素値の配列用
            double widthScale = (double)sourceWidth / width;//横倍率
            double heightScale = (double)sourceHeight / height;
            int stride = width * pByte;
            byte[] pixels = new byte[height * stride];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //参照点
                    double rx = (x + 0.5) * widthScale;
                    double ry = (y + 0.5) * heightScale;
                    //参照点四捨五入で基準
                    int xKijun = (int)(rx + 0.5);
                    int yKijun = (int)(ry + 0.5);
                    //修正した重み取得
                    var ws = GetFixWeights(rx, ry, n);

                    double bSum = 0, gSum = 0, rSum = 0;
                    //参照範囲は基準から上(xは左)へn、下(xは右)へn-1の範囲
                    for (int yy = -n; yy < n; yy++)
                    {
                        int yc = yKijun + yy;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                        for (int xx = -n; xx < n; xx++)
                        {
                            int xc = xKijun + xx;
                            xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                            int pp = (yc * sourceStride) + (xc * pByte);
                            double weight = ws[xx + n, yy + n];
                            bSum += sourcePixels[pp] * weight;
                            gSum += sourcePixels[pp + 1] * weight;
                            rSum += sourcePixels[pp + 2] * weight;
                        }
                    }
                    //0～255の範囲を超えることがあるので、修正
                    bSum = bSum < 0 ? 0 : bSum > 255 ? 255 : bSum;
                    gSum = gSum < 0 ? 0 : gSum > 255 ? 255 : gSum;
                    rSum = rSum < 0 ? 0 : rSum > 255 ? 255 : rSum;
                    int ap = (y * stride) + (x * pByte);
                    pixels[ap] = (byte)(bSum + 0.5);
                    pixels[ap + 1] = (byte)(gSum + 0.5);
                    pixels[ap + 2] = (byte)(rSum + 0.5);
                }
            };

            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;

            //修正した重み取得
            double[,] GetFixWeights(double rx, double ry, int n)
            {
                int nn = n * 2;//全体の参照距離
                //基準になる距離計算
                double sx = rx - (int)rx;
                double sy = ry - (int)ry;
                double dx = (sx < 0.5) ? 0.5 - sx : 0.5 - sx + 1;
                double dy = (sy < 0.5) ? 0.5 - sy : 0.5 - sy + 1;

                //各ピクセルの重みと、重み合計を計算
                double[] xw = new double[nn];
                double[] yw = new double[nn];
                double xSum = 0, ySum = 0;
                for (int i = -n; i < n; i++)
                {
                    double x = GetLanczosWeight(Math.Abs(dx + i), n);
                    xSum += x;
                    xw[i + n] = x;
                    double y = GetLanczosWeight(Math.Abs(dy + i), n);
                    ySum += y;
                    yw[i + n] = y;
                }

                //重み合計で割り算して修正、全体で100%(1.0)にする
                for (int i = 0; i < nn; i++)
                {
                    xw[i] /= xSum;
                    yw[i] /= ySum;
                }

                // x * y
                double[,] ws = new double[nn, nn];
                for (int y = 0; y < nn; y++)
                {
                    for (int x = 0; x < nn; x++)
                    {
                        ws[x, y] = xw[x] * yw[y];
                    }
                }
                return ws;
            }
        }




        /// <summary>
        /// 画像ファイルパスからPixelFormats.Bgra32のBitmapSource作成
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dpiX"></param>
        /// <param name="dpiY"></param>
        /// <returns></returns>
        private BitmapSource MakeBitmapSourceBgra32FromFile(string filePath, double dpiX = 96, double dpiY = 96)
        {
            BitmapSource source = null;
            try
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    source = BitmapFrame.Create(stream);
                    if (source.Format != PixelFormats.Bgra32)
                    {
                        source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
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

        /// <summary>
        /// 画像ファイルパスからPixelFormats.Bgr24のBitmapSource作成
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dpiX"></param>
        /// <param name="dpiY"></param>
        /// <returns></returns>
        private BitmapSource MakeBitmapSourceBgr24FromFile(string filePath, double dpiX = 96, double dpiY = 96)
        {
            BitmapSource source = null;
            try
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    source = BitmapFrame.Create(stream);
                    if (source.Format != PixelFormats.Bgr24)
                    {
                        source = new FormatConvertedBitmap(source, PixelFormats.Bgr24, null, 0);
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



        #region コピペ

        //        クリップボードに複数の形式のデータをコピーする - .NET Tips(VB.NET, C#...)
        //https://dobon.net/vb/dotnet/system/clipboardmultidata.html
        //        アルファ値を失わずに画像のコピペできた、.NET WPFのClipboard - 午後わてんのブログ
        //https://gogowaten.hatenablog.com/entry/2021/02/10/134406
        /// <summary>
        /// BitmapSourceをPNG形式に変換したものと、そのままの形式の両方をクリップボードにコピーする
        /// </summary>
        /// <param name="source"></param>
        private void ClipboardSetImageWithPng(BitmapSource source)
        {
            //DataObjectに入れたいデータを入れて、それをクリップボードにセットする
            DataObject data = new();

            //BitmapSource形式そのままでセット
            data.SetData(typeof(BitmapSource), source);

            //PNG形式にエンコードしたものをMemoryStreamして、それをセット
            //画像をPNGにエンコード
            PngBitmapEncoder pngEnc = new();
            pngEnc.Frames.Add(BitmapFrame.Create(source));
            //エンコードした画像をMemoryStreamにSava
            using var ms = new System.IO.MemoryStream();
            pngEnc.Save(ms);
            data.SetData("PNG", ms);

            //クリップボードにセット
            Clipboard.SetDataObject(data, true);

        }


        /// <summary>
        /// クリップボードからBitmapSourceを取り出して返す、PNG(アルファ値保持)形式に対応
        /// </summary>
        /// <returns></returns>
        private BitmapSource GetImageFromClipboardWithPNG()
        {
            BitmapSource source = null;
            //クリップボードにPNG形式のデータがあったら、それを使ってBitmapFrame作成して返す
            //なければ普通にClipboardのGetImage、それでもなければnullを返す
            using var ms = (System.IO.MemoryStream)Clipboard.GetData("PNG");
            if (ms != null)
            {
                //source = BitmapFrame.Create(ms);//これだと取得できない
                source = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
            else if (Clipboard.ContainsImage())
            {
                //そのままだとAlphaがおかしいのでBgr32にコンバート
                source = new FormatConvertedBitmap(Clipboard.GetImage(), PixelFormats.Bgr32, null, 0);
            }
            return source;
        }

        /// <summary>
        /// クリップボードからBitmapSourceを取り出して返す、bmp優先、次にpng(アルファ値保持)形式に対応
        /// </summary>
        /// <returns></returns>
        private BitmapSource GetImageFromClipboardBmpNextPNG()
        {
            BitmapSource source = null;
            BitmapSource img = Clipboard.GetImage();
            //BMP
            if (img != null)
            {
                //そのままだとAlphaがおかしいのでBgr32にコンバート
                source = new FormatConvertedBitmap(img, PixelFormats.Bgr32, null, 0);
            }
            //PNG
            else
            {
                //クリップボードにPNG形式のデータがあったら、それを使ってBitmapFrame作成
                using var ms = (System.IO.MemoryStream)Clipboard.GetData("PNG");
                if (ms != null)
                {
                    source = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
            }
            return source;
        }



        /// <summary>
        /// 市松模様の元になる画像作成、2色を2マスずつ合計4マス交互に並べた画像、
        /// □■
        /// ■□
        /// </summary>
        /// <param name="cellSize">1マスの1辺の長さ、作成される画像はこれの2倍の1辺になる</param>
        /// <param name="c1">色1</param>
        /// <param name="c2">色2</param>
        /// <returns>画像のピクセルフォーマットはBgra32</returns>
        private WriteableBitmap MakeCheckeredPattern(int cellSize, Color c1, Color c2)
        {
            int width = cellSize * 2;
            int height = cellSize * 2;
            var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            int stride = 4 * width;// wb.Format.BitsPerPixel / 8 * width;
            byte[] pixels = new byte[stride * height];
            //すべてを1色目で塗る
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = c1.B;
                pixels[i + 1] = c1.G;
                pixels[i + 2] = c1.R;
                pixels[i + 3] = c1.A;
            }

            //2色目で市松模様にする
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //左上と右下に塗る
                    if ((y < cellSize & x < cellSize) | (y >= cellSize & x >= cellSize))
                    {
                        int p = y * stride + x * 4;
                        pixels[p] = c2.B;
                        pixels[p + 1] = c2.G;
                        pixels[p + 2] = c2.R;
                        pixels[p + 3] = c2.A;
                    }
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
            return wb;
        }

        /// <summary>
        /// BitmapからImageBrush作成
        /// 引き伸ばし無しでタイル状に敷き詰め
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private ImageBrush MakeTileBrush(BitmapSource bitmap)
        {
            var imgBrush = new ImageBrush(bitmap);
            imgBrush.Stretch = Stretch.None;//これは必要ないかも
            //タイルモード、タイル
            imgBrush.TileMode = TileMode.Tile;
            //タイルサイズは元画像のサイズ
            imgBrush.Viewport = new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
            //タイルサイズ指定方法は絶対値、これで引き伸ばされない
            imgBrush.ViewportUnits = BrushMappingMode.Absolute;
            return imgBrush;
        }



        //ファイルドロップ時
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            //ファイルパス取得
            var datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            var paths = datas.ToList();
            paths.Sort();
            MyBitmapOrigin = MakeBitmapSourceBgr24FromFile(paths[0]);
            MyImage.Source = MyBitmapOrigin;
            MyBitmapOrigin32bit = MakeBitmapSourceBgra32FromFile(paths[0]);
            MyImage.Source = MyBitmapOrigin32bit;
        }

        //ボタンクリック
        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            //MyExe(LanczosBgr24, MyBitmapOrigin, yoko, tate, (int)MySlider.Value);
            MyExe2(LanczosBgra32, GetLanczosWeight, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }


        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightA, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton3_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightB, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton4_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightC, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        //画像をクリップボードにコピー
        private void MyButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            ClipboardSetImageWithPng((BitmapSource)MyImage.Source);
        }


        //クリップボードから画像追加
        private void MyButtonPaste_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource img = GetImageFromClipboardWithPNG();
            if (img != null)
            {
                //FormatConvertedBitmap bitmap = new(img, PixelFormats.Gray8, null, 0);
                //MyBitmapOrigin = bitmap;
                //MyImage.Source = bitmap;

                FormatConvertedBitmap bitmap = new(img, PixelFormats.Bgr24, null, 0);
                MyBitmapOrigin = bitmap;
                FormatConvertedBitmap bitmap32 = new(img, PixelFormats.Bgra32, null, 0);
                MyImage.Source = bitmap32;
                MyBitmapOrigin32bit = bitmap32;
            }
        }

        private void MySlider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Slider slider = sender as Slider;
            if (e.Delta > 0) slider.Value += slider.SmallChange;
            else slider.Value -= slider.SmallChange;
        }


        private void MyButtonToOrigin_Click(object sender, RoutedEventArgs e)
        {
            //MyImage.Source = MyBitmapOrigin;
            MyImage.Source = MyBitmapOrigin32bit;
        }


        #endregion コピペ

        private void MyButton5_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightD, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton6_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightE, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton7_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightF, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton8_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightG, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton9_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightH, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton10_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightI, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton11_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightJ, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton12_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightK, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton13_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightL, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButtonItimatu模様_Click(object sender, RoutedEventArgs e)
        {
            if (this.Background == MyImageBrush)
            {
                this.Background = Brushes.White;
            }
            else
            {
                this.Background = MyImageBrush;
            }
        }

        private void MyButton14_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightM, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton15_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightN, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton16_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightO, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton17_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightP, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton18_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightQ, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButtonPasteBmp_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource img = GetImageFromClipboardBmpNextPNG();
            if (img != null)
            {
                FormatConvertedBitmap bitmap = new(img, PixelFormats.Bgr24, null, 0);
                MyBitmapOrigin = bitmap;
                FormatConvertedBitmap bitmap32 = new(img, PixelFormats.Bgra32, null, 0);
                MyImage.Source = bitmap32;
                MyBitmapOrigin32bit = bitmap32;
            }
        }

        private void MyButton19_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightR, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton20_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightS, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton21_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightT, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton22_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightU, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton23_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightV, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton24_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightW, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value, MySliderScale.Value);
        }

        private void MyButton25_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightX, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value, MySliderScale.Value);
        }

        private void MyButton26_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightY, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value, MySliderScale.Value);
        }

        private void MyButton27_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightZ, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value, MySliderScale.Value);
        }

        private void MyButton28_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightAA, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value, MySliderScale.Value);
        }

        private void MyButton29_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightAB, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value, MySliderScale.Value);
        }

        private void MyButton30_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightAC, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value, MySliderScale.Value);
        }

        private void MyButton31_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe2(LanczosBgra32, GetLanczosWeightAD, MyBitmapOrigin32bit, yoko, tate, (int)MySlider.Value, MySliderScale.Value);
        }
    }
}