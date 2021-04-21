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


        ////a指定版を高速化、2倍以上の高速化、さらに高速化？の仮想範囲作成を少しスッキリ＋仮想部分をミラーリング配置これから




        //a指定版、＋xy別計算
        /// <summary>
        /// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        /// </summary>
        /// <param name="source">PixelFormats.Gray8のBitmap</param>
        /// <param name="yoko">変換後の横ピクセル数を指定</param>
        /// <param name="tate">変換後の縦ピクセル数を指定</param>
        /// <param name="a">-2.0～2.0くらいを指定する、基準は-1.0、小さくするとシャープ、大きくするとぼかし</param>
        /// <returns></returns>
        private BitmapSource BicubicGray8Test(BitmapSource source, int yoko, int tate, double a = -1.0)
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

            for (int y = 0; y < tate; y++)
            {
                for (int x = 0; x < yoko; x++)
                {
                    //参照点
                    double rx = (x + 0.5) * yokoScale;
                    double ry = (y + 0.5) * tateScale;
                    //参照点四捨五入で基準
                    int xKijun = (int)(rx + 0.5);
                    int yKijun = (int)(ry + 0.5);

                    double vv = 0;
                    //参照範囲は基準から左へ2、右へ1の範囲
                    double[] yw = new double[4];
                    double sx = rx % 1;
                    double dx = (sx < 0.5) ? 0.5 - sx : 0.5 - sx + 1;

                    yw[0] = GetWeightCubic(2 - dx, a);
                    yw[1] = GetWeightCubic(1 - dx, a);
                    yw[2] = GetWeightCubic(dx, a);
                    yw[3] = GetWeightCubic(1 + dx, a);
                    double sy = ry % 1;
                    double dy = (sy < 0.5) ? 0.5 - sy : 0.5 - sy + 1;

                    double[] yw = new double[] {
                                    GetWeightCubic(2 - dy, a),
                                    GetWeightCubic(1 - dy, a),
                                    GetWeightCubic(dy, a),
                                    GetWeightCubic(1 + dy, a) };

                    
                    for (int yy = -2; yy <= 1; yy++)//
                    {
                        //+0.5しているのは中心座標で計算するため
                        double dy = Math.Abs(ry - (yy + yKijun + 0.5));//距離
                        double yw = GetWeightCubic(dy, a);//重み
                        int yc = yKijun + yy;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                    
                    }

                    for (int xx = -2; xx <= 1; xx++)
                    {
                        double dx = Math.Abs(rx - (xx + xKijun + 0.5));
                        double xw = GetWeightCubic(dx, a);
                        int xc = xKijun + xx;
                        xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                        
                    }

                    byte value = pixels[yc * stride + xc];
                    vv += value * yw * xw;

                    //0～255の範囲を超えることがあるので、修正
                    vv = vv < 0 ? 0 : vv > 255 ? 255 : vv;
                    resultPixels[y * scaledStride + x] = (byte)(vv + 0.5);
                }
            };

            BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
            return bitmap;
        }






        ////a指定版、仮想範囲で高速化＋参照範囲の重み計算を別にして高速化
        ///// <summary>
        ///// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        ///// </summary>
        ///// <param name="source">PixelFormats.Gray8のBitmap</param>
        ///// <param name="yoko">変換後の横ピクセル数を指定</param>
        ///// <param name="tate">変換後の縦ピクセル数を指定</param>
        ///// <param name="a">-2.0～2.0くらいを指定する、基準は-1.0、小さくするとシャープ、大きくするとぼかし</param>
        ///// <returns></returns>
        //private BitmapSource BicubicGray8Test(BitmapSource source, int yoko, int tate, double a = -1.0)
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

        //    //仮想範囲もとのサイズから左右上下に2ピクセル拡張
        //    byte[] vrPixels = new byte[(sourceHeight + 4) * (stride + 4)];
        //    int vrStride = stride + 4;
        //    int vrWidth = sourceWidth + 4;
        //    int vrHeight = sourceHeight + 4;

        //    //中段
        //    int count = 0;
        //    for (int i = vrWidth + vrWidth + 2; i < vrPixels.Length - vrStride - vrStride; i += vrWidth)
        //    {
        //        vrPixels[i - 2] = pixels[count];//左端
        //        vrPixels[i - 1] = pixels[count];//左端の1個右
        //        for (int j = 0; j < sourceWidth; j++)
        //        {
        //            vrPixels[i + j] = pixels[count];//中間
        //            count++;
        //        }
        //        vrPixels[i + sourceWidth] = pixels[count - 1];//右端1個左
        //        vrPixels[i + sourceWidth + 1] = pixels[count - 1];//右端
        //    }
        //    //最上段とその1段下と最下段とその1段上の行
        //    int endRow = (vrStride * vrHeight) - vrStride;
        //    for (int i = 0; i < vrStride; i++)
        //    {
        //        vrPixels[i] = vrPixels[i + vrStride + vrStride];//最上段
        //        vrPixels[i + vrStride] = vrPixels[i + vrStride + vrStride];//最上段の1段下
        //        vrPixels[endRow + i - vrStride] = vrPixels[i + endRow - vrStride - vrStride];
        //        vrPixels[endRow + i] = vrPixels[i + endRow - vrStride - vrStride];//最下段
        //    }

        //    for (int y = 0; y < tate; y++)
        //    {
        //        for (int x = 0; x < yoko; x++)
        //        {
        //            //参照点
        //            double rx = (x + 0.5) * yokoScale;
        //            double ry = (y + 0.5) * tateScale;
        //            //参照点四捨五入で基準
        //            int xKijun = (int)(rx + 0.5);
        //            int yKijun = (int)(ry + 0.5);

        //            //4x4の重み取得
        //            var ws = Get4x4Weight(rx, ry);
        //            double vv = 0;
        //            //参照範囲は基準から左へ2、右へ1の範囲
        //            for (int yy = -2; yy <= 1; yy++)//
        //            {
        //                int vrFix = ((yKijun + yy) * vrStride) + vrStride + vrStride + 2;
        //                for (int xx = -2; xx <= 1; xx++)
        //                {
        //                    byte value = vrPixels[vrFix + xKijun + xx];
        //                    vv += value * ws[xx + 2, yy + 2];
        //                }
        //            }
        //            //0～255の範囲を超えることがあるので、修正
        //            vv = vv < 0 ? 0 : vv > 255 ? 255 : vv;
        //            resultPixels[y * scaledStride + x] = (byte)(vv + 0.5);
        //        }
        //    };

        //    BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
        //    return bitmap;

        //    //4x4個すべての重み計算
        //    double[,] Get4x4Weight(double rx, double ry)
        //    {
        //        double sx = rx % 1;
        //        double sy = ry % 1;
        //        double dx = (sx < 0.5) ? 0.5 - sx : 0.5 - sx + 1;
        //        double dy = (sy < 0.5) ? 0.5 - sy : 0.5 - sy + 1;

        //        double[] xw = new double[] {
        //                GetWeightCubic(2 - dx, a),
        //                GetWeightCubic(1 - dx, a),
        //                GetWeightCubic(dx, a),
        //                GetWeightCubic(1 + dx, a) };
        //        double[] yw = new double[] {
        //                GetWeightCubic(2 - dy, a),
        //                GetWeightCubic(1 - dy, a),
        //                GetWeightCubic(dy, a),
        //                GetWeightCubic(1 + dy, a) };

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


        ////a指定版、仮想範囲で高速化
        ///// <summary>
        ///// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        ///// </summary>
        ///// <param name="source">PixelFormats.Gray8のBitmap</param>
        ///// <param name="yoko">変換後の横ピクセル数を指定</param>
        ///// <param name="tate">変換後の縦ピクセル数を指定</param>
        ///// <param name="a">-2.0～2.0くらいを指定する、基準は-1.0、小さくするとシャープ、大きくするとぼかし</param>
        ///// <returns></returns>
        //private BitmapSource BicubicGray8Test(BitmapSource source, int yoko, int tate, double a = -1.0)
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

        //    //仮想範囲もとのサイズから左右上下に2ピクセル拡張
        //    byte[] vrPixels = new byte[(sourceHeight + 4) * (stride + 4)];
        //    int vrStride = stride + 4;
        //    int vrWidth = sourceWidth + 4;
        //    int vrHeight = sourceHeight + 4;

        //    //中段
        //    int count = 0;
        //    for (int i = vrWidth + vrWidth + 2; i < vrPixels.Length - vrStride - vrStride; i += vrWidth)
        //    {
        //        vrPixels[i - 2] = pixels[count];//左端
        //        vrPixels[i - 1] = pixels[count];//左端の1個右
        //        for (int j = 0; j < sourceWidth; j++)
        //        {
        //            vrPixels[i + j] = pixels[count];//中間
        //            count++;
        //        }
        //        vrPixels[i + sourceWidth] = pixels[count - 1];//右端1個左
        //        vrPixels[i + sourceWidth + 1] = pixels[count - 1];//右端
        //    }
        //    //最上段とその1段下と最下段とその1段上の行
        //    int endRow = (vrStride * vrHeight) - vrStride ;
        //    for (int i = 0; i < vrStride; i++)
        //    {
        //        vrPixels[i] = vrPixels[i + vrStride + vrStride];//最上段
        //        vrPixels[i + vrStride] = vrPixels[i + vrStride + vrStride];//最上段の1段下
        //        vrPixels[endRow + i - vrStride] = vrPixels[i + endRow - vrStride - vrStride];
        //        vrPixels[endRow + i] = vrPixels[i + endRow - vrStride - vrStride];//最下段
        //    }

        //    for (int y = 0; y < tate; y++)
        //    {
        //        for (int x = 0; x < yoko; x++)
        //        {
        //            //参照点
        //            double rx = (x + 0.5) * yokoScale;
        //            double ry = (y + 0.5) * tateScale;
        //            //参照点四捨五入で基準
        //            int xKijun = (int)(rx + 0.5);
        //            int yKijun = (int)(ry + 0.5);

        //            double vv = 0;
        //            //参照範囲は基準から左へ2、右へ1の範囲
        //            for (int yy = -2; yy <= 1; yy++)//
        //            {
        //                int vrFix = ((yKijun + yy) * vrStride) + vrStride + vrStride + 2;
        //                //+0.5しているのは中心座標で計算するため
        //                double dy = Math.Abs(ry - (yy + yKijun + 0.5));//距離
        //                double yw = GetWeightCubic(dy, a);//重み
        //                for (int xx = -2; xx <= 1; xx++)
        //                {
        //                    double dx = Math.Abs(rx - (xx + xKijun + 0.5));
        //                    double xw = GetWeightCubic(dx, a);
        //                    byte value = vrPixels[vrFix + xKijun + xx];
        //                    vv += value * yw * xw;
        //                }
        //            }
        //            //0～255の範囲を超えることがあるので、修正
        //            vv = vv < 0 ? 0 : vv > 255 ? 255 : vv;
        //            resultPixels[y * scaledStride + x] = (byte)(vv + 0.5);
        //        }
        //    };

        //    BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
        //    return bitmap;
        //}



        ////a指定版、参照範囲の重み計算を別にして高速化
        ///// <summary>
        ///// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        ///// </summary>
        ///// <param name="source">PixelFormats.Gray8のBitmap</param>
        ///// <param name="yoko">変換後の横ピクセル数を指定</param>
        ///// <param name="tate">変換後の縦ピクセル数を指定</param>
        ///// <param name="a">-2.0～2.0くらいを指定する、基準は-1.0、小さくするとシャープ、大きくするとぼかし</param>
        ///// <returns></returns>
        //private BitmapSource BicubicGray8Test(BitmapSource source, int yoko, int tate, double a = -1.0)
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
        //            //参照点四捨五入で基準
        //            int xKijun = (int)(rx + 0.5);
        //            int yKijun = (int)(ry + 0.5);
        //            //4x4の重み取得
        //            var ws = Get4x4Weight(rx, ry);
        //            double vv = 0;
        //            //参照範囲は基準から左へ2、右へ1の範囲
        //            for (int yy = -2; yy <= 1; yy++)//
        //            {
        //                int yc = yKijun + yy;
        //                //マイナス座標や画像サイズを超えていたら、収まるように修正
        //                yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
        //                for (int xx = -2; xx <= 1; xx++)
        //                {
        //                    int xc = xKijun + xx;
        //                    xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
        //                    byte value = pixels[yc * stride + xc];
        //                    vv += value * ws[xx + 2, yy + 2];
        //                }
        //            }
        //            //0～255の範囲を超えることがあるので、修正
        //            vv = vv < 0 ? 0 : vv > 255 ? 255 : vv;
        //            resultPixels[y * scaledStride + x] = (byte)(vv + 0.5);
        //        }
        //    };

        //    BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
        //    return bitmap;

        //    //4x4個すべての重み計算
        //    double[,] Get4x4Weight(double rx, double ry)
        //    {
        //        double sx = rx % 1;
        //        double sy = ry % 1;
        //        double dx = (sx < 0.5) ? 0.5 - sx : 0.5 - sx + 1;
        //        double dy = (sy < 0.5) ? 0.5 - sy : 0.5 - sy + 1;

        //        double[] xw = new double[] {
        //            GetWeightCubic(2 - dx, a),
        //            GetWeightCubic(1 - dx, a),
        //            GetWeightCubic(dx, a),
        //            GetWeightCubic(1 + dx, a) };
        //        double[] yw = new double[] {
        //            GetWeightCubic(2 - dy, a),
        //            GetWeightCubic(1 - dy, a),
        //            GetWeightCubic(dy, a), 
        //            GetWeightCubic(1 + dy, a) };

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
        ///// <param name="a">-2.0～2.0くらいを指定する、基準は-1.0、小さくするとシャープ、大きくするとぼかし</param>
        ///// <returns></returns>
        //private BitmapSource BicubicGray8Test(BitmapSource source, int yoko, int tate,double a=-1.0)
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
        //            //参照点四捨五入で基準
        //            int xKijun = (int)(rx + 0.5);
        //            int yKijun = (int)(ry + 0.5);

        //            double vv = 0;
        //            //参照範囲は基準から左へ2、右へ1の範囲
        //            for (int yy = -2; yy <= 1; yy++)//
        //            {
        //                //+0.5しているのは中心座標で計算するため
        //                double dy = Math.Abs(ry - (yy + yKijun + 0.5));//距離
        //                double yw = GetWeightCubic(dy, a);//重み
        //                int yc = yKijun + yy;
        //                //マイナス座標や画像サイズを超えていたら、収まるように修正
        //                yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
        //                for (int xx = -2; xx <= 1; xx++)
        //                {
        //                    double dx = Math.Abs(rx - (xx + xKijun + 0.5));
        //                    double xw = GetWeightCubic(dx, a);
        //                    int xc = xKijun + xx;
        //                    xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
        //                    byte value = pixels[yc * stride + xc];
        //                    vv += value * yw * xw;
        //                }
        //            }
        //            //0～255の範囲を超えることがあるので、修正
        //            vv = vv < 0 ? 0 : vv > 255 ? 255 : vv;
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