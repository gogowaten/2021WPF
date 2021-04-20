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
//Bicubic（バイキュービック法）～1.基本編～ | Rain or Shine
//https://www.rainorshine.asia/2013/04/03/post2351.html
//    Bicubic interpolation - Wikipedia
//https://en.wikipedia.org/wiki/Bicubic_interpolation
//    画像の拡大「Bicubic法」: koujinz blog
//http://koujinz.cocolog-nifty.com/blog/2009/05/bicubic-a97c.html

namespace _20210417_バイキュービック
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapSource MyBitmapOrigin;
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.Top = 0;
            this.Left = 0;
#endif
            this.Background = MakeTileBrush(MakeCheckeredPattern(10, Colors.WhiteSmoke, Colors.LightGray));
        }




        private static double GetWeightCubic(double d, double a = -1.0)
        {
            return d switch
            {
                2 => 0,
                <= 1 => ((a + 2) * (d * d * d)) - ((a + 3) * (d * d)) + 1,
                < 2 => (a * (d * d * d)) - (5 * a * (d * d)) + (8 * a * d) - (4 * a),
                _ => 0
            };
        }
        //private static double GetWeightCubic(double d)
        //{
        //    return d switch
        //    {
        //        2 => 0,
        //        <= 1 => 1 - 2 * (d * d) + d * d * d,
        //        < 2 => 4 - 8 * d + 5 * (d * d) - (d * d * d),
        //        _ => 0
        //    };
        //}
        /// <summary>
        /// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        /// </summary>
        /// <param name="source">PixelFormats.Gray8のBitmap</param>
        /// <param name="yoko">変換後の横ピクセル数を指定</param>
        /// <param name="tate">変換後の縦ピクセル数を指定</param>
        /// <returns></returns>
        private BitmapSource BicubicGray8(BitmapSource source, int yoko, int tate)
        {
            //元画像の画素値の配列作成
            int sourceWidth = source.PixelWidth;
            int sourceHeight = source.PixelHeight;
            int stride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[sourceHeight * stride];
            source.CopyPixels(pixels, stride, 0);

            //変換後の画像の画素値の配列用
            double yokoScale = (double)sourceWidth / yoko;//横倍率
            double tateScale = (double)sourceHeight / tate;
            int scaledStride = (yoko * source.Format.BitsPerPixel + 7) / 8;
            byte[] resultPixels = new byte[tate * scaledStride];
            int pByte = (source.Format.BitsPerPixel + 7) / 8;//1ピクセルあたりのバイト数、Byte / Pixel

            for (int y = 0; y < tate; y++)
            {
                for (int x = 0; x < yoko; x++)
                {

                    //参照点
                    double rx = (x + 0.5) * yokoScale;
                    double ry = (y + 0.5) * tateScale;
                    //基準左上
                    int kLeft = (int)(rx - 0.5) - 1;
                    int kTop = (int)(ry - 0.5) - 1;
                    //右下限界
                    int migi = sourceWidth - 1;
                    int sita = sourceHeight - 1;
                    if (kLeft < 0 || kTop < 0 || kLeft + 3 > migi || kTop + 3 > sita)
                    {
                        resultPixels[y * scaledStride + x] = 0;
                        continue;
                    }


                    //参照点の-0.5，-0.5
                    double bpX = rx - 0.5;
                    double bpY = ry - 0.5;
                    //小数部分s
                    double sx = bpX % 1;
                    double sy = bpY % 1;
                    //参照範囲の左上ピクセル座標
                    int ixc = (int)bpX - 1;
                    int iyc = (int)bpY - 1;


                    double[] xw = new double[4]
                    {
                        GetWeightCubic(sx+1),GetWeightCubic(sx),GetWeightCubic(1-sx),GetWeightCubic(2-sx)
                    };
                    double[] yw = new double[4]
                    {
                        GetWeightCubic(sy+1),GetWeightCubic(sy),GetWeightCubic(1-sy),GetWeightCubic(2-sy)
                    };

                    int p = (int)(ry - 0.5) * stride + (int)(rx - 0.5);

                    double RV = 0.0;
                    RV += pixels[p - stride - pByte] * (xw[0] * yw[0]);
                    RV += pixels[p - stride] * (xw[1] * yw[0]);
                    RV += pixels[p - stride + pByte] * (xw[2] * yw[0]);
                    RV += pixels[p - stride + pByte + pByte] * (xw[3] * yw[0]);

                    RV += pixels[p - pByte] * (xw[0] * yw[1]);
                    RV += pixels[p] * (xw[1] * yw[1]);
                    RV += pixels[p + pByte] * (xw[2] * yw[1]);
                    RV += pixels[p + pByte + pByte] * (xw[3] * yw[1]);

                    RV += pixels[p + stride - pByte] * (xw[0] * yw[2]);
                    RV += pixels[p + stride] * (xw[1] * yw[2]);
                    RV += pixels[p + stride + pByte] * (xw[2] * yw[2]);
                    RV += pixels[p + stride + pByte + pByte] * (xw[3] * yw[2]);

                    RV += pixels[p + stride + stride - pByte] * (xw[0] * yw[3]);
                    RV += pixels[p + stride + stride] * (xw[1] * yw[3]);
                    RV += pixels[p + stride + stride + pByte] * (xw[2] * yw[3]);
                    RV += pixels[p + stride + stride + pByte + pByte] * (xw[3] * yw[3]);



                    RV = RV > 255 ? 255 : RV < 0 ? 0 : RV;
                    resultPixels[y * scaledStride + x] = (byte)(RV + 0.5);
                }
            };


            BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
            return bitmap;
        }


        //a指定版を高速化、2倍以上の高速化、さらに高速化？の仮想範囲作成を少しスッキリ＋仮想部分をミラーリング配置
        /// <summary>
        /// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        /// </summary>
        /// <param name="source">PixelFormats.Gray8のBitmap</param>
        /// <param name="yoko">変換後の横ピクセル数を指定</param>
        /// <param name="tate">変換後の縦ピクセル数を指定</param>
        /// <returns></returns>
        private BitmapSource BicubicGray8Test(BitmapSource source, int yoko, int tate)
        {
            //元画像の画素値の配列作成
            int sourceWidth = source.PixelWidth;
            int sourceHeight = source.PixelHeight;
            int stride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[sourceHeight * stride];
            source.CopyPixels(pixels, stride, 0);

            //変換後の画像の画素値の配列用
            double yokoScale = (double)sourceWidth / yoko;//横倍率
            double tateScale = (double)sourceHeight / tate;
            int scaledStride = (yoko * source.Format.BitsPerPixel + 7) / 8;
            byte[] resultPixels = new byte[tate * scaledStride];

            double a = MySlider.Value;

            //仮想範囲
            byte[] vrPixels = new byte[(sourceHeight + 3) * (stride + 3)];
            int vrStride = stride + 3;
            int vrWidth = sourceWidth + 3;
            int vrHeight = sourceHeight + 3;
            
            //中段
            int count = 0;
            for (int i = vrWidth + 1; i < vrPixels.Length - vrStride - vrStride; i += vrWidth)
            {
                vrPixels[i - 1] = pixels[count];//左端
                for (int j = 0; j < sourceWidth; j++)
                {
                    vrPixels[i + j] = pixels[count];//中間
                    count++;
                }
                vrPixels[i + sourceWidth] = pixels[count - 1];//右端1個前
                vrPixels[i + sourceWidth + 1] = pixels[count - 1];//右端
            }
            //最上段と最下段とその1段上の行
            int endRow = (vrStride * vrHeight) - vrStride;
            for (int i = 0; i < vrStride; i++)
            {
                vrPixels[i] = vrPixels[i + vrStride];//最上段
                vrPixels[endRow + i] = vrPixels[i + endRow - vrStride - vrStride];//最下段
                vrPixels[endRow + i - vrStride] = vrPixels[i + endRow - vrStride - vrStride];
            }


            for (int y = 0; y < tate; y++)
            {
                for (int x = 0; x < yoko; x++)
                {
                    //参照点
                    double rx = (x + 0.5) * yokoScale;
                    double ry = (y + 0.5) * tateScale;

                    //参照点の-0.5，-0.5
                    double bpX = rx - 0.5;
                    double bpY = ry - 0.5;
                    //小数部分、これが基本の距離になる
                    double dx = Math.Abs(bpX % 1);
                    double dy = Math.Abs(bpY % 1);
                    //ここで4x4個すべての重み取得
                    var ws = GetWs(dx, dy);
                    //整数部分
                    int xInt = (int)bpX;
                    int yInt = (int)bpY;

                    double vv = 0;

                    for (int yy = -1; yy < 3; yy++)
                    {
                        int vrFix = (yInt + yy) * vrStride + vrWidth + 1;
                        for (int xx = -1; xx < 3; xx++)
                        {
                            byte value = vrPixels[vrFix + xInt + xx];
                            //byte value = vrPixels[(yc * stride) + ixc + xx];
                            vv += value * ws[xx + 1, yy + 1];
                        }
                    }
                    vv = vv > 255 ? 255 : vv < 0 ? 0 : vv;
                    resultPixels[y * scaledStride + x] = (byte)(vv + 0.5);
                }
            };

            BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
            return bitmap;


            //4x4個すべての重み計算
            double[,] GetWs(double dx, double dy)
            {
                double[] xw = new double[] { GetWeightCubic(dx + 1, a), GetWeightCubic(dx, a), GetWeightCubic(1 - dx, a), GetWeightCubic(2 - dx, a) };
                double[] yw = new double[] { GetWeightCubic(dy + 1, a), GetWeightCubic(dy, a), GetWeightCubic(1 - dy, a), GetWeightCubic(2 - dy, a) };
                double[,] ws = new double[4, 4];
                for (int yy = 0; yy < 4; yy++)
                {
                    ws[0, yy] = xw[0] * yw[yy];
                    ws[1, yy] = xw[1] * yw[yy];
                    ws[2, yy] = xw[2] * yw[yy];
                    ws[3, yy] = xw[3] * yw[yy];
                }
                return ws;
            }
        }
        
        ////a指定版を高速化、2倍以上の高速化、さらに高速化？の仮想範囲作成を少しスッキリ
        ///// <summary>
        ///// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        ///// </summary>
        ///// <param name="source">PixelFormats.Gray8のBitmap</param>
        ///// <param name="yoko">変換後の横ピクセル数を指定</param>
        ///// <param name="tate">変換後の縦ピクセル数を指定</param>
        ///// <returns></returns>
        //private BitmapSource BicubicGray8Test(BitmapSource source, int yoko, int tate)
        //{
        //    //元画像の画素値の配列作成
        //    int sourceWidth = source.PixelWidth;
        //    int sourceHeight = source.PixelHeight;
        //    int stride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
        //    byte[] pixels = new byte[sourceHeight * stride];
        //    source.CopyPixels(pixels, stride, 0);

        //    //変換後の画像の画素値の配列用
        //    double yokoScale = (double)sourceWidth / yoko;//横倍率
        //    double tateScale = (double)sourceHeight / tate;
        //    int scaledStride = (yoko * source.Format.BitsPerPixel + 7) / 8;
        //    byte[] resultPixels = new byte[tate * scaledStride];

        //    double a = MySlider.Value;

        //    //仮想範囲
        //    byte[] vrPixels = new byte[(sourceHeight + 3) * (stride + 3)];
        //    int vrStride = stride + 3;
        //    int vrWidth = sourceWidth + 3;
        //    int vrHeight = sourceHeight + 3;
            
        //    //中段
        //    int count = 0;
        //    for (int i = vrWidth + 1; i < vrPixels.Length - vrStride - vrStride; i += vrWidth)
        //    {
        //        vrPixels[i - 1] = pixels[count];//左端
        //        for (int j = 0; j < sourceWidth; j++)
        //        {
        //            vrPixels[i + j] = pixels[count];//中間
        //            count++;
        //        }
        //        vrPixels[i + sourceWidth] = pixels[count - 1];//右端1個前
        //        vrPixels[i + sourceWidth + 1] = pixels[count - 1];//右端
        //    }
        //    //最上段と最下段とその1段上の行
        //    int endRow = (vrStride * vrHeight) - vrStride;
        //    for (int i = 0; i < vrStride; i++)
        //    {
        //        vrPixels[i] = vrPixels[i + vrStride];//最上段
        //        vrPixels[endRow + i] = vrPixels[i + endRow - vrStride - vrStride];//最下段
        //        vrPixels[endRow + i - vrStride] = vrPixels[i + endRow - vrStride - vrStride];
        //    }


        //    for (int y = 0; y < tate; y++)
        //    {
        //        for (int x = 0; x < yoko; x++)
        //        {
        //            //参照点
        //            double rx = (x + 0.5) * yokoScale;
        //            double ry = (y + 0.5) * tateScale;

        //            //参照点の-0.5，-0.5
        //            double bpX = rx - 0.5;
        //            double bpY = ry - 0.5;
        //            //小数部分、これが基本の距離になる
        //            double dx = Math.Abs(bpX % 1);
        //            double dy = Math.Abs(bpY % 1);
        //            //ここで4x4個すべての重み取得
        //            var ws = GetWs(dx, dy);
        //            //整数部分
        //            int xInt = (int)bpX;
        //            int yInt = (int)bpY;

        //            double vv = 0;

        //            for (int yy = -1; yy < 3; yy++)
        //            {
        //                int vrFix = (yInt + yy) * vrStride + vrWidth + 1;
        //                for (int xx = -1; xx < 3; xx++)
        //                {
        //                    byte value = vrPixels[vrFix + xInt + xx];
        //                    //byte value = vrPixels[(yc * stride) + ixc + xx];
        //                    vv += value * ws[xx + 1, yy + 1];
        //                }
        //            }
        //            vv = vv > 255 ? 255 : vv < 0 ? 0 : vv;
        //            resultPixels[y * scaledStride + x] = (byte)(vv + 0.5);
        //        }
        //    };

        //    BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
        //    return bitmap;


        //    //4x4個すべての重み計算
        //    double[,] GetWs(double dx, double dy)
        //    {
        //        double[] xw = new double[] { GetWeightCubic(dx + 1, a), GetWeightCubic(dx, a), GetWeightCubic(1 - dx, a), GetWeightCubic(2 - dx, a) };
        //        double[] yw = new double[] { GetWeightCubic(dy + 1, a), GetWeightCubic(dy, a), GetWeightCubic(1 - dy, a), GetWeightCubic(2 - dy, a) };
        //        double[,] ws = new double[4, 4];
        //        for (int yy = 0; yy < 4; yy++)
        //        {
        //            ws[0, yy] = xw[0] * yw[yy];
        //            ws[1, yy] = xw[1] * yw[yy];
        //            ws[2, yy] = xw[2] * yw[yy];
        //            ws[3, yy] = xw[3] * yw[yy];
        //        }
        //        return ws;
        //    }
        //}

        ////a指定版を高速化、2倍以上の高速化、さらに高速化？
        ///// <summary>
        ///// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        ///// </summary>
        ///// <param name="source">PixelFormats.Gray8のBitmap</param>
        ///// <param name="yoko">変換後の横ピクセル数を指定</param>
        ///// <param name="tate">変換後の縦ピクセル数を指定</param>
        ///// <returns></returns>
        //private BitmapSource BicubicGray8Test(BitmapSource source, int yoko, int tate)
        //{
        //    //元画像の画素値の配列作成
        //    int sourceWidth = source.PixelWidth;
        //    int sourceHeight = source.PixelHeight;
        //    int stride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
        //    byte[] pixels = new byte[sourceHeight * stride];
        //    source.CopyPixels(pixels, stride, 0);

        //    //変換後の画像の画素値の配列用
        //    double yokoScale = (double)sourceWidth / yoko;//横倍率
        //    double tateScale = (double)sourceHeight / tate;
        //    int scaledStride = (yoko * source.Format.BitsPerPixel + 7) / 8;
        //    byte[] resultPixels = new byte[tate * scaledStride];

        //    double a = MySlider.Value;

        //    //仮想範囲
        //    byte[] vrPixels = new byte[(sourceHeight + 3) * (stride + 3)];
        //    //byte[] vrPixels = new byte[pixels.Length + sourceHeight * 3 + sourceWidth * 3 + 4];
        //    int vrStride = stride + 3;
        //    int vrWidth = sourceWidth + 3;
        //    int vrHeight = sourceHeight + 3;
        //    //1行目
        //    vrPixels[0] = pixels[0];//先頭
        //    for (int i = 1; i < vrStride - 1; i++)
        //    {
        //        vrPixels[i] = pixels[i - 1];
        //    }
        //    vrPixels[vrStride - 1] = pixels[stride - 1];//右端
        //    vrPixels[vrStride - 2] = pixels[stride - 1];//右端の1個前

        //    //下段の2行
        //    vrPixels[vrPixels.Length - vrStride - vrStride] = pixels[pixels.Length - stride];//1行前の先頭
        //    vrPixels[vrPixels.Length - vrStride] = pixels[pixels.Length - stride];//最下段の先頭
        //    int count = pixels.Length - stride;
        //    for (int i = vrPixels.Length - vrStride - vrStride + 1; i < vrPixels.Length - vrStride - vrStride + 1 + stride; i++)
        //    {
        //        vrPixels[i] = pixels[count];
        //        vrPixels[i + vrStride] = pixels[count];
        //        count++;
        //    }
        //    vrPixels[vrPixels.Length - 1] = pixels[pixels.Length - 1];//最下段右端
        //    vrPixels[vrPixels.Length - 2] = pixels[pixels.Length - 1];//最下段右端1個前
        //    vrPixels[vrPixels.Length - vrStride - 1] = pixels[pixels.Length - 1];//1行前の右端
        //    vrPixels[vrPixels.Length - vrStride - 2] = pixels[pixels.Length - 1];//1行前の右端1個前

        //    //中段
        //    count = 0;
        //    for (int i = vrWidth + 1; i < vrPixels.Length - vrStride - vrStride; i += vrWidth)
        //    {
        //        vrPixels[i - 1] = pixels[count];//左端
        //        for (int j = 0; j < sourceWidth; j++)
        //        {
        //            vrPixels[i + j] = pixels[count];//中間
        //            count++;
        //        }
        //        vrPixels[i + sourceWidth] = pixels[count - 1];//右端1個前
        //        vrPixels[i + sourceWidth + 1] = pixels[count - 1];//右端
        //    }


        //    for (int y = 0; y < tate; y++)
        //    {
        //        for (int x = 0; x < yoko; x++)
        //        {
        //            //参照点
        //            double rx = (x + 0.5) * yokoScale;
        //            double ry = (y + 0.5) * tateScale;

        //            //参照点の-0.5，-0.5
        //            double bpX = rx - 0.5;
        //            double bpY = ry - 0.5;
        //            //小数部分、これが基本の距離になる
        //            double dx = Math.Abs(bpX % 1);
        //            double dy = Math.Abs(bpY % 1);
        //            //ここで4x4個すべての重み取得
        //            var ws = GetWs(dx, dy);
        //            //整数部分
        //            int xInt = (int)bpX;
        //            int yInt = (int)bpY;

        //            double vv = 0;

        //            for (int yy = -1; yy < 3; yy++)
        //            {
        //                int vrFix = (yInt + yy) * vrStride + vrWidth + 1;
        //                for (int xx = -1; xx < 3; xx++)
        //                {
        //                    byte value = vrPixels[vrFix + xInt + xx];
        //                    //byte value = vrPixels[(yc * stride) + ixc + xx];
        //                    vv += value * ws[xx + 1, yy + 1];
        //                }
        //            }
        //            vv = vv > 255 ? 255 : vv < 0 ? 0 : vv;
        //            resultPixels[y * scaledStride + x] = (byte)(vv + 0.5);
        //        }
        //    };

        //    BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
        //    return bitmap;


        //    //4x4個すべての重み計算
        //    double[,] GetWs(double dx, double dy)
        //    {
        //        double[] xw = new double[] { GetWeightCubic(dx + 1, a), GetWeightCubic(dx, a), GetWeightCubic(1 - dx, a), GetWeightCubic(2 - dx, a) };
        //        double[] yw = new double[] { GetWeightCubic(dy + 1, a), GetWeightCubic(dy, a), GetWeightCubic(1 - dy, a), GetWeightCubic(2 - dy, a) };
        //        double[,] ws = new double[4, 4];
        //        for (int yy = 0; yy < 4; yy++)
        //        {
        //            ws[0, yy] = xw[0] * yw[yy];
        //            ws[1, yy] = xw[1] * yw[yy];
        //            ws[2, yy] = xw[2] * yw[yy];
        //            ws[3, yy] = xw[3] * yw[yy];
        //        }
        //        return ws;
        //    }
        //}


        ////a指定版を高速化、2倍以上の高速化
        ///// <summary>
        ///// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        ///// </summary>
        ///// <param name="source">PixelFormats.Gray8のBitmap</param>
        ///// <param name="yoko">変換後の横ピクセル数を指定</param>
        ///// <param name="tate">変換後の縦ピクセル数を指定</param>
        ///// <returns></returns>
        //private BitmapSource BicubicGray8Test(BitmapSource source, int yoko, int tate)
        //{
        //    //元画像の画素値の配列作成
        //    int sourceWidth = source.PixelWidth;
        //    int sourceHeight = source.PixelHeight;
        //    int stride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
        //    byte[] pixels = new byte[sourceHeight * stride];
        //    source.CopyPixels(pixels, stride, 0);

        //    //変換後の画像の画素値の配列用
        //    double yokoScale = (double)sourceWidth / yoko;//横倍率
        //    double tateScale = (double)sourceHeight / tate;
        //    int scaledStride = (yoko * source.Format.BitsPerPixel + 7) / 8;
        //    byte[] resultPixels = new byte[tate * scaledStride];

        //    double a = MySlider.Value;

        //    for (int y = 0; y < tate; y++)
        //    {
        //        for (int x = 0; x < yoko; x++)
        //        {
        //            //参照点
        //            double rx = (x + 0.5) * yokoScale;
        //            double ry = (y + 0.5) * tateScale;

        //            //参照点の-0.5，-0.5
        //            double bpX = rx - 0.5;
        //            double bpY = ry - 0.5;
        //            //小数部分、これが基本の距離になる
        //            double dx = Math.Abs(bpX % 1);
        //            double dy = Math.Abs(bpY % 1);
        //            //ここで4x4個すべての重み取得
        //            var ws = GetWs(dx, dy);
        //            //整数部分
        //            int ixc = (int)bpX;
        //            int iyc = (int)bpY;

        //            double vv = 0;

        //            for (int yy = -1; yy < 3; yy++)
        //            {
        //                int yc = iyc + yy;
        //                yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
        //                for (int xx = -1; xx < 3; xx++)
        //                {
        //                    int xc = ixc + xx;
        //                    xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
        //                    byte value = pixels[yc * stride + xc];
        //                    vv += value * ws[xx + 1, yy + 1];
        //                }
        //            }
        //            vv = vv > 255 ? 255 : vv < 0 ? 0 : vv;
        //            resultPixels[y * scaledStride + x] = (byte)(vv + 0.5);
        //        }
        //    };

        //    BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
        //    return bitmap;


        //    //4x4個すべての重み計算
        //    double[,] GetWs(double dx, double dy)
        //    {
        //        double[] xw = new double[] { GetWeightCubic(dx + 1, a), GetWeightCubic(dx, a), GetWeightCubic(1 - dx, a), GetWeightCubic(2 - dx, a) };
        //        double[] yw = new double[] { GetWeightCubic(dy + 1, a), GetWeightCubic(dy, a), GetWeightCubic(1 - dy, a), GetWeightCubic(2 - dy, a) };
        //        double[,] ws = new double[4, 4];
        //        for (int yy = 0; yy < 4; yy++)
        //        {
        //            ws[0, yy] = xw[0] * yw[yy];
        //            ws[1, yy] = xw[1] * yw[yy];
        //            ws[2, yy] = xw[2] * yw[yy];
        //            ws[3, yy] = xw[3] * yw[yy];
        //        }
        //        return ws;
        //    }
        //}


        ////a指定版
        ///// <summary>
        ///// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        ///// </summary>
        ///// <param name="source">PixelFormats.Gray8のBitmap</param>
        ///// <param name="yoko">変換後の横ピクセル数を指定</param>
        ///// <param name="tate">変換後の縦ピクセル数を指定</param>
        ///// <returns></returns>
        //private BitmapSource BicubicGray8Test(BitmapSource source, int yoko, int tate)
        //{
        //    //元画像の画素値の配列作成
        //    int sourceWidth = source.PixelWidth;
        //    int sourceHeight = source.PixelHeight;
        //    int stride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
        //    byte[] pixels = new byte[sourceHeight * stride];
        //    source.CopyPixels(pixels, stride, 0);

        //    //変換後の画像の画素値の配列用
        //    double yokoScale = (double)sourceWidth / yoko;//横倍率
        //    double tateScale = (double)sourceHeight / tate;
        //    int scaledStride = (yoko * source.Format.BitsPerPixel + 7) / 8;
        //    byte[] resultPixels = new byte[tate * scaledStride];

        //    for (int y = 0; y < tate; y++)
        //    {
        //        for (int x = 0; x < yoko; x++)
        //        {
        //            //参照点
        //            double rx = (x + 0.5) * yokoScale;
        //            double ry = (y + 0.5) * tateScale;

        //            //参照点の-0.5，-0.5
        //            double bpX = rx - 0.5;
        //            double bpY = ry - 0.5;
        //            //小数部分s
        //            double sx = Math.Abs(bpX % 1);
        //            double sy = Math.Abs(bpY % 1);
        //            //整数部分
        //            int ixc = (int)bpX;
        //            int iyc = (int)bpY;

        //            double vv = 0;
        //            for (int yy = -1; yy < 3; yy++)
        //            {
        //                double dy = Math.Abs(sy - yy);
        //                double yw = GetWeightCubic(dy, MySlider.Value);
        //                int yc = iyc + yy;
        //                yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
        //                for (int xx = -1; xx < 3; xx++)
        //                {
        //                    double dx = Math.Abs(sx - xx);
        //                    double xw = GetWeightCubic(dx, MySlider.Value);
        //                    int xc = ixc + xx;
        //                    xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
        //                    byte value = pixels[yc * stride + xc];
        //                    vv += value * yw * xw;
        //                }
        //            }
        //            vv = vv > 255 ? 255 : vv < 0 ? 0 : vv;
        //            resultPixels[y * scaledStride + x] = (byte)(vv + 0.5);
        //        }
        //    };

        //    BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
        //    return bitmap;
        //}


        /// <summary>
        /// 画像ファイルパスからPixelFormats.Gray8のBitmapSource作成
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dpiX"></param>
        /// <param name="dpiY"></param>
        /// <returns></returns>
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
                source = Clipboard.GetImage();
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
            MyBitmapOrigin = MakeBitmapSourceGray8FromFile(paths[0]);
            MyImage.Source = MyBitmapOrigin;
        }

        //ボタンクリック
        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / 2.0);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / 2.0);
            MyImage.Source = BicubicGray8Test(MyBitmapOrigin, yoko, tate);
        }


        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / 3.0);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / 3.0);
            MyImage.Source = BicubicGray8Test(MyBitmapOrigin, yoko, tate);
        }

        private void MyButton3_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = BicubicGray8Test(MyBitmapOrigin,
                MyBitmapOrigin.PixelWidth * 2,
                MyBitmapOrigin.PixelHeight * 2);
        }

        private void MyButton4_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = BicubicGray8Test(MyBitmapOrigin,
                MyBitmapOrigin.PixelWidth * 3,
                MyBitmapOrigin.PixelHeight * 3);
        }

        //画像をクリップボードにコピー
        private void MyButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            ClipboardSetImageWithPng((BitmapSource)MyImage.Source);
        }


        //クリップボードから画像追加
        private void MyButtonPaste_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource bitmap = GetImageFromClipboardWithPNG();
            if (bitmap != null)
            {
                MyBitmapOrigin = bitmap;
                MyImage.Source = bitmap;
            }
        }

        #endregion コピペ
    }
}