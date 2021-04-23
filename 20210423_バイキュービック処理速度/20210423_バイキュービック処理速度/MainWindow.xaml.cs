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

using System.Diagnostics;



//Bicubic（バイキュービック法）～1.基本編～ | Rain or Shine
//https://www.rainorshine.asia/2013/04/03/post2351.html
//Bicubic（バイキュービック法）～3.さらに高速化編～ | Rain or Shine
//https://www.rainorshine.asia/2013/12/13/post2497.html
//    Bicubic interpolation - Wikipedia
//https://en.wikipedia.org/wiki/Bicubic_interpolation
//    画像の拡大「Bicubic法」: koujinz blog
//http://koujinz.cocolog-nifty.com/blog/2009/05/bicubic-a97c.html
//画像リサイズ処理のうんちく - Qiita
//https://qiita.com/yoya/items/95c37e02762431b1abf0#%E3%82%BB%E3%83%91%E3%83%A9%E3%83%96%E3%83%AB%E3%83%95%E3%82%A3%E3%83%AB%E3%82%BF



namespace _20210423_バイキュービック処理速度
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

        }


        private void MyExe(Func<BitmapSource, int, int, double, BitmapSource> func,
            BitmapSource source, int width, int height, double a = -1.0)
        {
            var sw = new Stopwatch();
            sw.Start();
            var bitmap = func(source, width, height, a);
            sw.Stop();
            MyTextBlockTime.Text = $"処理時間：{sw.Elapsed.TotalSeconds:000.000}秒, {func.Method.Name}";
            MyImage.Source = bitmap;

        }

        /// <summary>
        /// バイキュービックで重み計算
        /// </summary>
        /// <param name="d">距離</param>
        /// <param name="a">定数、-1.0 ～ -0.5 が丁度いい</param>
        /// <returns></returns>
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




        //a指定版、重みをまとめて別計算＋仮想範囲
        /// <summary>
        /// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        /// </summary>
        /// <param name="source">PixelFormats.Gray8のBitmap</param>
        /// <param name="yoko">変換後の横ピクセル数を指定</param>
        /// <param name="tate">変換後の縦ピクセル数を指定</param>
        /// <param name="a">-0.5～-1.0くらいを指定する、小さくするとシャープ、大きくするとぼかし</param>
        /// <returns></returns>
        private BitmapSource BicubicGray8Test5(BitmapSource source, int yoko, int tate, double a = -1.0)
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

            //仮想範囲もとのサイズから左右上下に2ピクセル拡張
            byte[] vrPixels = new byte[(sourceHeight + 4) * (stride + 4)];
            int vrStride = stride + 4;
            int vrWidth = sourceWidth + 4;
            int vrHeight = sourceHeight + 4;

            //中段
            int count = 0;
            for (int i = vrWidth + vrWidth + 2; i < vrPixels.Length - vrStride - vrStride; i += vrWidth)
            {
                vrPixels[i - 2] = pixels[count];//左端
                vrPixels[i - 1] = pixels[count];//左端の1個右
                for (int j = 0; j < sourceWidth; j++)
                {
                    vrPixels[i + j] = pixels[count];//中間
                    count++;
                }
                vrPixels[i + sourceWidth] = pixels[count - 1];//右端1個左
                vrPixels[i + sourceWidth + 1] = pixels[count - 1];//右端
            }
            //最上段とその1段下と最下段とその1段上の行
            int endRow = (vrStride * vrHeight) - vrStride;
            for (int i = 0; i < vrStride; i++)
            {
                vrPixels[i] = vrPixels[i + vrStride + vrStride];//最上段
                vrPixels[i + vrStride] = vrPixels[i + vrStride + vrStride];//最上段の1段下
                vrPixels[endRow + i - vrStride] = vrPixels[i + endRow - vrStride - vrStride];
                vrPixels[endRow + i] = vrPixels[i + endRow - vrStride - vrStride];//最下段
            }

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
                    //4x4の重み取得
                    double[,] ws = Get4x4Weight(rx, ry);
                    double vv = 0;
                    //参照範囲は基準から左へ2、右へ1の範囲
                    int topLeft = ((yKijun - 2) * vrStride) + vrStride + vrStride + 2 + xKijun - 2;
                    vv += vrPixels[topLeft] * ws[0, 0];
                    vv += vrPixels[topLeft + 1] * ws[1, 0];
                    vv += vrPixels[topLeft + 2] * ws[2, 0];
                    vv += vrPixels[topLeft + 3] * ws[3, 0];
                    topLeft += vrStride;//改行
                    vv += vrPixels[topLeft] * ws[0, 1];
                    vv += vrPixels[topLeft + 1] * ws[1, 1];
                    vv += vrPixels[topLeft + 2] * ws[2, 1];
                    vv += vrPixels[topLeft + 3] * ws[3, 1];
                    topLeft += vrStride;
                    vv += vrPixels[topLeft] * ws[0, 2];
                    vv += vrPixels[topLeft + 1] * ws[1, 2];
                    vv += vrPixels[topLeft + 2] * ws[2, 2];
                    vv += vrPixels[topLeft + 3] * ws[3, 2];
                    topLeft += vrStride;
                    vv += vrPixels[topLeft] * ws[0, 3];
                    vv += vrPixels[topLeft + 1] * ws[1, 3];
                    vv += vrPixels[topLeft + 2] * ws[2, 3];
                    vv += vrPixels[topLeft + 3] * ws[3, 3];

                    //0～255の範囲を超えることがあるので、修正
                    vv = vv < 0 ? 0 : vv > 255 ? 255 : vv;
                    resultPixels[y * scaledStride + x] = (byte)(vv + 0.5);
                }
            };

            BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
            return bitmap;

            //4x4個すべての重み計算
            double[,] Get4x4Weight(double rx, double ry)
            {
                double sx = rx - (int)rx;
                double sy = ry - (int)ry;
                //double sx = rx % 1;
                //double sy = ry % 1;
                double dx = (sx < 0.5) ? 0.5 - sx : 0.5 - sx + 1;
                double dy = (sy < 0.5) ? 0.5 - sy : 0.5 - sy + 1;

                double[] xw = new double[] {
                    GetWeightCubic(2 - dx, a),
                    GetWeightCubic(1 - dx, a),
                    GetWeightCubic(dx, a),
                    GetWeightCubic(1 + dx, a) };
                double[] yw = new double[] {
                    GetWeightCubic(2 - dy, a),
                    GetWeightCubic(1 - dy, a),
                    GetWeightCubic(dy, a),
                    GetWeightCubic(1 + dy, a) };

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



        /// <summary>
        /// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        /// a指定版+セパラブル
        /// </summary>
        /// <param name="source">PixelFormats.Gray8のBitmap</param>
        /// <param name="yoko">変換後の横ピクセル数を指定</param>
        /// <param name="tate">変換後の縦ピクセル数を指定</param>
        /// <param name="a">-0.5～-1.0くらいを指定する、基準は-1.0、小さくするとシャープ、大きくするとぼかし</param>
        /// <returns></returns>
        private BitmapSource BicubicGray8Test4(BitmapSource source, int yoko, int tate, double a = -1.0)
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
            double[] xResult = new double[sourceHeight * scaledStride];

            //横方だけを処理
            for (int y = 0; y < sourceHeight; y++)
            {
                for (int x = 0; x < yoko; x++)
                {
                    //参照点
                    double rx = (x + 0.5) * yokoScale;
                    //参照点四捨五入で基準
                    int xKijun = (int)(rx + 0.5);

                    double vv = 0;
                    //参照範囲は基準から左へ2、右へ1の範囲
                    for (int xx = -2; xx <= 1; xx++)
                    {
                        //+0.5しているのは中心座標で計算するため
                        double dx = Math.Abs(rx - (xx + xKijun + 0.5));//距離
                        double xw = GetWeightCubic(dx, a);//重み
                        int xc = xKijun + xx;
                        xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                        byte value = pixels[y * stride + xc];
                        vv += value * xw;
                    }
                    xResult[y * scaledStride + x] = vv;
                }
            };
            //縦方向も処理
            for (int y = 0; y < tate; y++)
            {
                for (int x = 0; x < yoko; x++)
                {
                    //参照点
                    double ry = (y + 0.5) * tateScale;
                    //参照点四捨五入で基準
                    int yKijun = (int)(ry + 0.5);

                    double vv = 0;
                    //参照範囲は基準から左へ2、右へ1の範囲
                    for (int yy = -2; yy <= 1; yy++)
                    {
                        double dy = Math.Abs(ry - (yy + yKijun + 0.5));
                        double yw = GetWeightCubic(dy, a);
                        int yc = yKijun + yy;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                        double value = xResult[yc * scaledStride + x];
                        vv += value * yw;
                    }
                    //0～255の範囲を超えることがあるので、修正
                    vv = vv < 0 ? 0 : vv > 255 ? 255 : vv;
                    resultPixels[y * scaledStride + x] = (byte)(vv + 0.5);
                }
            }

            BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
            return bitmap;
        }




        //a指定版、仮想範囲で高速化
        /// <summary>
        /// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        /// </summary>
        /// <param name="source">PixelFormats.Gray8のBitmap</param>
        /// <param name="yoko">変換後の横ピクセル数を指定</param>
        /// <param name="tate">変換後の縦ピクセル数を指定</param>
        /// <param name="a">-0.5～-1.0くらいを指定する、小さくするとシャープ、大きくするとぼかし</param>
        /// <returns></returns>
        private BitmapSource BicubicGray8Test3(BitmapSource source, int yoko, int tate, double a = -1.0)
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

            //仮想範囲もとのサイズから左右上下に2ピクセル拡張
            byte[] vrPixels = new byte[(sourceHeight + 4) * (stride + 4)];
            int vrStride = stride + 4;
            int vrWidth = sourceWidth + 4;
            int vrHeight = sourceHeight + 4;

            //中段
            int count = 0;
            for (int i = vrWidth + vrWidth + 2; i < vrPixels.Length - vrStride - vrStride; i += vrWidth)
            {
                vrPixels[i - 2] = pixels[count];//左端
                vrPixels[i - 1] = pixels[count];//左端の1個右
                for (int j = 0; j < sourceWidth; j++)
                {
                    vrPixels[i + j] = pixels[count];//中間
                    count++;
                }
                vrPixels[i + sourceWidth] = pixels[count - 1];//右端1個左
                vrPixels[i + sourceWidth + 1] = pixels[count - 1];//右端
            }
            //最上段とその1段下と最下段とその1段上の行
            int endRow = (vrStride * vrHeight) - vrStride;
            for (int i = 0; i < vrStride; i++)
            {
                vrPixels[i] = vrPixels[i + vrStride + vrStride];//最上段
                vrPixels[i + vrStride] = vrPixels[i + vrStride + vrStride];//最上段の1段下
                vrPixels[endRow + i - vrStride] = vrPixels[i + endRow - vrStride - vrStride];
                vrPixels[endRow + i] = vrPixels[i + endRow - vrStride - vrStride];//最下段
            }

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
                    for (int yy = -2; yy <= 1; yy++)//
                    {
                        int vrFix = ((yKijun + yy) * vrStride) + vrStride + vrStride + 2;
                        //+0.5しているのは中心座標で計算するため
                        double dy = Math.Abs(ry - (yy + yKijun + 0.5));//距離
                        double yw = GetWeightCubic(dy, a);//重み
                        for (int xx = -2; xx <= 1; xx++)
                        {
                            double dx = Math.Abs(rx - (xx + xKijun + 0.5));
                            double xw = GetWeightCubic(dx, a);
                            byte value = vrPixels[vrFix + xKijun + xx];
                            vv += value * yw * xw;
                        }
                    }
                    //0～255の範囲を超えることがあるので、修正
                    vv = vv < 0 ? 0 : vv > 255 ? 255 : vv;
                    resultPixels[y * scaledStride + x] = (byte)(vv + 0.5);
                }
            };

            BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
            return bitmap;
        }



        /// <summary>
        /// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        /// a指定版、参照範囲の重み計算を別にして高速化
        /// </summary>
        /// <param name="source">PixelFormats.Gray8のBitmap</param>
        /// <param name="yoko">変換後の横ピクセル数を指定</param>
        /// <param name="tate">変換後の縦ピクセル数を指定</param>
        /// <param name="a">-0.5～-1.0くらいを指定する、小さくするとシャープ、大きくするとぼかし</param>
        /// <returns></returns>
        private BitmapSource BicubicGray8Test2(BitmapSource source, int yoko, int tate, double a = -1.0)
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
                    //4x4の重み取得
                    double[,] ws = Get4x4Weight(rx, ry);
                    double vv = 0;
                    //参照範囲は基準から左へ2、右へ1の範囲
                    for (int yy = -2; yy <= 1; yy++)//
                    {
                        int yc = yKijun + yy;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                        for (int xx = -2; xx <= 1; xx++)
                        {
                            int xc = xKijun + xx;
                            xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                            byte value = pixels[yc * stride + xc];
                            vv += value * ws[xx + 2, yy + 2];
                        }
                    }
                    //0～255の範囲を超えることがあるので、修正
                    vv = vv < 0 ? 0 : vv > 255 ? 255 : vv;
                    resultPixels[y * scaledStride + x] = (byte)(vv + 0.5);
                }
            };

            BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
            return bitmap;

            //4x4個すべての重み計算
            double[,] Get4x4Weight(double rx, double ry)
            {
                double sx = rx - (int)rx;
                double sy = ry - (int)ry;
                //double sx = rx % 1;
                //double sy = ry % 1;
                double dx = (sx < 0.5) ? 0.5 - sx : 0.5 - sx + 1;
                double dy = (sy < 0.5) ? 0.5 - sy : 0.5 - sy + 1;

                double[] xw = new double[] {
                    GetWeightCubic(2 - dx, a),
                    GetWeightCubic(1 - dx, a),
                    GetWeightCubic(dx, a),
                    GetWeightCubic(1 + dx, a) };
                double[] yw = new double[] {
                    GetWeightCubic(2 - dy, a),
                    GetWeightCubic(1 - dy, a),
                    GetWeightCubic(dy, a),
                    GetWeightCubic(1 + dy, a) };

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





        /// <summary>
        /// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        /// a指定版
        /// </summary>
        /// <param name="source">PixelFormats.Gray8のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="a">-0.5～-1.0くらいを指定するのがいい、小さくするとシャープ、大きくするとぼかし</param>
        /// <returns></returns>
        private static BitmapSource BicubicGray8Test1(BitmapSource source, int width, int height, double a = -1.0)
        {
            //元画像の画素値の配列作成
            int sourceWidth = source.PixelWidth;
            int sourceHeight = source.PixelHeight;
            int sourceStride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
            byte[] sourcePixels = new byte[sourceHeight * sourceStride];
            source.CopyPixels(sourcePixels, sourceStride, 0);

            //変換後の画像の画素値の配列用
            double widthScale = (double)sourceWidth / width;//横倍率
            double heightScale = (double)sourceHeight / height;
            int stride = (width * source.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[height * stride];
            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    //参照点
                    double rx = (x + 0.5) * widthScale;
                    double ry = (y + 0.5) * heightScale;
                    //参照点四捨五入で基準
                    int xKijun = (int)(rx + 0.5);
                    int yKijun = (int)(ry + 0.5);

                    double vv = 0;
                    //参照範囲は基準から左へ2、右へ1の範囲
                    for (int yy = -2; yy <= 1; yy++)//
                    {
                        //+0.5しているのは中心座標で計算するため
                        double dy = Math.Abs(ry - (yy + yKijun + 0.5));//距離
                        double yw = GetWeightCubic(dy, a);//重み
                        int yc = yKijun + yy;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                        for (int xx = -2; xx <= 1; xx++)
                        {
                            double dx = Math.Abs(rx - (xx + xKijun + 0.5));
                            double xw = GetWeightCubic(dx, a);
                            int xc = xKijun + xx;
                            xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                            byte value = sourcePixels[yc * sourceStride + xc];
                            vv += value * yw * xw;
                        }
                    }
                    //0～255の範囲を超えることがあるので、修正
                    vv = vv < 0 ? 0 : vv > 255 ? 255 : vv;
                    pixels[y * stride + x] = (byte)(vv + 0.5);
                }
            });


            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;
        }




        /// <summary>
        /// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        /// a指定版
        /// </summary>
        /// <param name="source">PixelFormats.Gray8のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="a">-0.5～-1.0くらいを指定するのがいい、小さくするとシャープ、大きくするとぼかし</param>
        /// <returns></returns>
        private static BitmapSource BicubicGray8Test0(BitmapSource source, int width, int height, double a = -1.0)
        {
            //元画像の画素値の配列作成
            int sourceWidth = source.PixelWidth;
            int sourceHeight = source.PixelHeight;
            int sourceStride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
            byte[] sourcePixels = new byte[sourceHeight * sourceStride];
            source.CopyPixels(sourcePixels, sourceStride, 0);

            //変換後の画像の画素値の配列用
            double widthScale = (double)sourceWidth / width;//横倍率
            double heightScale = (double)sourceHeight / height;
            int stride = (width * source.Format.BitsPerPixel + 7) / 8;
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

                    double vv = 0;
                    //参照範囲は基準から左へ2、右へ1の範囲
                    for (int yy = -2; yy <= 1; yy++)//
                    {
                        //+0.5しているのは中心座標で計算するため
                        double dy = Math.Abs(ry - (yy + yKijun + 0.5));//距離
                        double yw = GetWeightCubic(dy, a);//重み
                        int yc = yKijun + yy;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                        for (int xx = -2; xx <= 1; xx++)
                        {
                            double dx = Math.Abs(rx - (xx + xKijun + 0.5));
                            double xw = GetWeightCubic(dx, a);
                            int xc = xKijun + xx;
                            xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                            byte value = sourcePixels[yc * sourceStride + xc];
                            vv += value * yw * xw;
                        }
                    }
                    //0～255の範囲を超えることがあるので、修正
                    vv = vv < 0 ? 0 : vv > 255 ? 255 : vv;
                    pixels[y * stride + x] = (byte)(vv + 0.5);
                }
            };

            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;
        }


        /// <summary>
        /// 画像ファイルパスからPixelFormats.Gray8のBitmapSource作成
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dpiX"></param>
        /// <param name="dpiY"></param>
        /// <returns></returns>
        private static BitmapSource MakeBitmapSourceGray8FromFile(string filePath, double dpiX = 96, double dpiY = 96)
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
        private static void ClipboardSetImageWithPng(BitmapSource source)
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
        private static BitmapSource GetImageFromClipboardWithPNG()
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
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth * MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight * MySliderScale.Value);
            MyExe(BicubicGray8Test1, MyBitmapOrigin, yoko, tate, MySlider.Value);
        }


        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth * MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight * MySliderScale.Value);
            MyExe(BicubicGray8Test2, MyBitmapOrigin, yoko, tate, MySlider.Value);
        }

        private void MyButton3_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth * MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight * MySliderScale.Value);
            MyExe(BicubicGray8Test3, MyBitmapOrigin, yoko, tate, MySlider.Value);
        }

        private void MyButton4_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth * MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight * MySliderScale.Value);
            MyExe(BicubicGray8Test4, MyBitmapOrigin, yoko, tate, MySlider.Value);
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
                var gray8 = new FormatConvertedBitmap(bitmap, PixelFormats.Gray8, null, 0);
                MyBitmapOrigin = gray8;
                MyImage.Source = gray8;
            }
        }

        private void MySlider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) MySlider.Value += MySlider.SmallChange;
            else MySlider.Value -= MySlider.SmallChange;
        }


        private void MyButtonToOrigin_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = MyBitmapOrigin;
        }


        #endregion コピペ

        private void MyButton0_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth * MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight * MySliderScale.Value);
            MyExe(BicubicGray8Test0, MyBitmapOrigin, yoko, tate, MySlider.Value);
        }

        private void MyButton5_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth * MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight * MySliderScale.Value);
            MyExe(BicubicGray8Test5, MyBitmapOrigin, yoko, tate, MySlider.Value);
        }
    }
}