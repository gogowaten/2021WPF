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

namespace _20210510_640x480リサイズ
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


        #region 縮小用

        #region Bicubic

        /// <summary>
        /// バイキュービックで重み計算、縮小にも使えるけど画質がイマイチになるので拡大専用
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


        /// <summary>
        /// バイキュービックで重み計算、縮小対応版
        /// </summary>
        /// <param name="d">距離</param>
        /// <param name="actN">参照最大距離、拡大時は2固定</param>
        /// <param name="a">定数、-1.0 ～ -0.5 が丁度いい</param>
        /// <returns></returns>
        private static double GetWeightCubic(double d, double a, double scale, double inScale)
        {
            double dd = d * scale;

            if (d == inScale * 2) return 0;
            else if (d <= inScale) return ((a + 2) * (dd * dd * dd)) - ((a + 3) * (dd * dd)) + 1;
            else if (d < inScale * 2) return (a * (dd * dd * dd)) - (5 * a * (dd * dd)) + (8 * a * dd) - (4 * a);
            else return 0;
        }


        /// <summary>
        /// 画像のリサイズ、バイキュービック法で補完、PixelFormats.Bgra32専用)
        /// Parallelだけで高速化
        /// </summary>
        /// <param name="source">PixelFormats.Bgra32のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="a">係数、-1.0～-0.5がいい</param>
        /// <param name="soft">trueで縮小時に画質がソフトになる？</param>
        /// <returns></returns>
        private BitmapSource BicubicBgra32(BitmapSource source, int width, int height, double a, bool soft = false)
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
            double widthScale = (double)sourceWidth / width;//横倍率(逆倍率)
            double heightScale = (double)sourceHeight / height;
            int stride = width * pByte;
            byte[] pixels = new byte[height * stride];

            //倍率
            double scale = width / (double)sourceWidth;
            double inScale = 1.0 / scale;//逆倍率
                                         //実際の参照距離、縮小時だけに関係する、倍率*2の切り上げが正しいはず
                                         //切り捨てするとぼやけるソフト画質になる
            int actD = (int)Math.Ceiling(widthScale * 2);
            if (soft == true) actD = ((int)widthScale) * 2;//切り捨て

            //拡大時の調整(これがないと縮小専用)
            if (1.0 < scale)
            {
                scale = 1.0;//重み計算に使う、拡大時は1固定
                inScale = 1.0;//逆倍率、拡大時は1固定
                actD = 2;//拡大時のバイキュービックの参照距離は2で固定
            }


            _ = Parallel.For(0, height, y =>
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
                    double[,] ws = GetFixWeights(rx, ry, actD, a, scale, inScale);

                    double bSum = 0, gSum = 0, rSum = 0, aSum = 0;
                    double alphaFix = 0;
                    //参照範囲は基準から上(xは左)へnn、下(xは右)へnn-1の範囲
                    for (int yy = -actD; yy < actD; yy++)
                    {
                        int yc = yKijun + yy;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                        for (int xx = -actD; xx < actD; xx++)
                        {
                            int xc = xKijun + xx;
                            xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                            int pp = (yc * sourceStride) + (xc * pByte);
                            double weight = ws[xx + actD, yy + actD];
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
            });

            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;

            //修正した重み取得
            double[,] GetFixWeights(double rx, double ry, int actN, double a, double scale, double inScale)
            {
                int nn = actN * 2;//全体の参照距離
                                  //基準になる距離計算
                double sx = rx - (int)rx;
                double sy = ry - (int)ry;
                double dx = (sx < 0.5) ? 0.5 - sx : 0.5 - sx + 1;
                double dy = (sy < 0.5) ? 0.5 - sy : 0.5 - sy + 1;

                //各ピクセルの重みと、重み合計を計算
                double[] xw = new double[nn];
                double[] yw = new double[nn];
                double xSum = 0, ySum = 0;

                for (int i = -actN; i < actN; i++)
                {
                    double x = GetWeightCubic(Math.Abs(dx + i), a, scale, inScale);
                    xSum += x;
                    xw[i + actN] = x;
                    double y = GetWeightCubic(Math.Abs(dy + i), a, scale, inScale);
                    ySum += y;
                    yw[i + actN] = y;
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

        #endregion Bicubic



        #region Lanczos

        //窓関数
        private double Sinc(double d)
        {
            return Math.Sin(Math.PI * d) / (Math.PI * d);
        }
        //窓関数
        private double SincA(double d)
        {
            if (d == 0) return 1.0;
            return Math.Sin(Math.PI * d) / (Math.PI * d);
        }

        /// <summary>
        /// ランチョス補完法での重み計算、拡大専用
        /// </summary>
        /// <param name="d">距離</param>
        /// <param name="n">最大参照距離</param>
        /// <returns></returns>
        //private double GetLanczosWeight(double d, int n)
        //{
        //    if (d == 0) return 1.0;
        //    else if (d > n) return 0.0;
        //    else return Sinc(d) * Sinc(d / n);
        //}

        /// <summary>
        /// 縮小拡大両対応重み計算、Lanczos
        /// </summary>
        /// <param name="d">距離</param>
        /// <param name="n">最大参照距離</param>
        /// <param name="scale">倍率</param>
        /// <returns></returns>
        private double GetLanczosWeight(double d, int n, double limitD)
        {
            if (d == 0) return 1.0;
            else if (d > limitD) return 0.0;
            else return Sinc(d) * Sinc(d / n);
        }



        /// <summary>
        /// 画像のリサイズ、ランチョス法で補完、PixelFormats.Bgra32専用)
        /// 高速化なし
        /// </summary>
        /// <param name="source">PixelFormats.Bgra32のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="n">最大参照距離、3か4がいい</param>
        /// <returns></returns>
        private BitmapSource LanczosBgra32(BitmapSource source, int width, int height, int n)
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
            double widthScale = (double)sourceWidth / width;//横倍率(逆倍率)
            double heightScale = (double)sourceHeight / height;
            int stride = width * pByte;
            byte[] pixels = new byte[height * stride];

            //倍率
            double scale = width / (double)sourceWidth;
            //最大参照距離 = 逆倍率 * n
            double limitD = widthScale * n;
            //実際の参照距離、は指定距離*逆倍率の切り上げにしたけど、切り捨てでも見た目の変化なし            
            int actD = (int)Math.Ceiling(limitD);
            //int actD = (int)(limitD);


            //拡大時の調整(これがないと縮小専用)
            if (1.0 < scale)
            {
                scale = 1.0;//重み計算に使うようで、拡大時は1固定
                actD = n;//拡大時の実際の参照距離は指定距離と同じ
            }

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
                    //double[,] ws = GetFixWeights(rx, ry, actN, scale*0.1, n);
                    double[,] ws = GetFixWeights(rx, ry, actD, scale, n, limitD);

                    double bSum = 0, gSum = 0, rSum = 0, aSum = 0;
                    double alphaFix = 0;
                    //参照範囲は基準から上(xは左)へnn、下(xは右)へnn-1の範囲
                    for (int yy = -actD; yy < actD; yy++)
                    {
                        int yc = yKijun + yy;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                        for (int xx = -actD; xx < actD; xx++)
                        {
                            int xc = xKijun + xx;
                            xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                            int pp = (yc * sourceStride) + (xc * pByte);
                            double weight = ws[xx + actD, yy + actD];
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
            }

            //_ = Parallel.For(0, height, y =>
            //  {

            //  });

            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;

            //修正した重み取得
            double[,] GetFixWeights(double rx, double ry, int actN, double scale, int n, double limitD)
            {
                //全体の参照距離
                int nn = actN * 2;
                //基準になる距離計算
                double sx = rx - (int)rx;
                double sy = ry - (int)ry;
                double dx = (sx < 0.5) ? 0.5 - sx : 0.5 - sx + 1;
                double dy = (sy < 0.5) ? 0.5 - sy : 0.5 - sy + 1;

                //各ピクセルの重みと、重み合計を計算
                double[] xw = new double[nn];
                double[] yw = new double[nn];
                double xSum = 0, ySum = 0;
                for (int i = -actN; i < actN; i++)
                {
                    //距離に倍率を掛け算したのをLanczosで重み計算
                    double x = GetLanczosWeight(Math.Abs(dx + i) * scale, n, limitD);
                    xSum += x;
                    xw[i + actN] = x;
                    double y = GetLanczosWeight(Math.Abs(dy + i) * scale, n, limitD);
                    ySum += y;
                    yw[i + actN] = y;
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

        #endregion Lanczos


        //これのn=4が一番キレイに見える
        /// <summary>
        /// 画像のリサイズ、窓関数で補完、PixelFormats.Bgra32専用)
        /// 高速化なし
        /// </summary>
        /// <param name="source">PixelFormats.Bgra32のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="n">最大参照距離、3か4がいい</param>
        /// <returns></returns>
        private BitmapSource SincBgra32(BitmapSource source, int width, int height, int n)
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
            double widthScale = (double)sourceWidth / width;//横倍率(逆倍率)
            double heightScale = (double)sourceHeight / height;
            int stride = width * pByte;
            byte[] pixels = new byte[height * stride];

            //倍率
            double scale = width / (double)sourceWidth;
            //逆倍率
            double inScale = widthScale;
            //最大参照距離 = 逆倍率 * n
            double limitD = widthScale * n;
            //実際の参照距離、は指定距離*逆倍率の切り上げにしたけど、切り捨てでも見た目の変化なし            
            int actD = (int)Math.Ceiling(limitD);
            //int actD = (int)(limitD);


            //拡大時の調整(これがないと縮小専用)
            if (1.0 < scale)
            {
                scale = 1.0;//重み計算に使う、拡大時は1固定
                actD = n;//拡大時の実際の参照距離は指定距離と同じ
                inScale = 1.0;
            }

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
                    double[,] ws = GetFixWeights(rx, ry, actD, inScale);

                    double bSum = 0, gSum = 0, rSum = 0, aSum = 0;
                    double alphaFix = 0;
                    //参照範囲は基準から上(xは左)へnn、下(xは右)へnn-1の範囲
                    for (int yy = -actD; yy < actD; yy++)
                    {
                        int yc = yKijun + yy;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                        for (int xx = -actD; xx < actD; xx++)
                        {
                            int xc = xKijun + xx;
                            xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                            int pp = (yc * sourceStride) + (xc * pByte);
                            double weight = ws[xx + actD, yy + actD];
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
            }

            //_ = Parallel.For(0, height, y =>
            //  {

            //  });

            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;

            //修正した重み取得
            double[,] GetFixWeights(double rx, double ry, int actN, double inScale)
            {
                //全体の参照距離
                int nn = actN * 2;
                //基準になる距離計算
                double sx = rx - (int)rx;
                double sy = ry - (int)ry;
                double dx = (sx < 0.5) ? 0.5 - sx : 0.5 - sx + 1;
                double dy = (sy < 0.5) ? 0.5 - sy : 0.5 - sy + 1;

                //各ピクセルの重みと、重み合計を計算
                double[] xw = new double[nn];
                double[] yw = new double[nn];
                double xSum = 0, ySum = 0;
                for (int i = -actN; i < actN; i++)
                {
                    //距離に倍率を掛け算したのをLanczosで重み計算
                    double x = SincA(Math.Abs(dx + i) * inScale);
                    xSum += x;
                    xw[i + actN] = x;
                    double y = SincA(Math.Abs(dy + i) * inScale);
                    ySum += y;
                    yw[i + actN] = y;
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
        /// 画像のリサイズ、窓関数で補完、PixelFormats.Bgra32専用)
        /// 高速化なし
        /// </summary>
        /// <param name="source">PixelFormats.Bgra32のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="n">最大参照距離、3か4がいい</param>
        /// <returns></returns>
        private BitmapSource SincTypeGBgra32(BitmapSource source, int width, int height, int n)
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
            double widthScale = (double)sourceWidth / width;//横倍率(逆倍率)
            double heightScale = (double)sourceHeight / height;
            int stride = width * pByte;
            byte[] pixels = new byte[height * stride];

            //倍率
            double scale = width / (double)sourceWidth;
            //逆倍率
            double inScale = widthScale;
            //最大参照距離 = 逆倍率 * n
            double limitD = widthScale * n;
            //実際の参照距離、は指定距離*逆倍率の切り上げにしたけど、切り捨てでも見た目の変化なし            
            int actD = (int)Math.Ceiling(limitD);
            //int actD = (int)(limitD);


            //拡大時の調整(これがないと縮小専用)
            if (1.0 < scale)
            {
                scale = 1.0;//重み計算に使うようで、拡大時は1固定
                actD = n;//拡大時の実際の参照距離は指定距離と同じ
                inScale = 1.0;
            }

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
                    //double[,] ws = GetFixWeights(rx, ry, actN, scale*0.1, n);
                    double[,] ws = GetFixWeights(rx, ry, actD, n);

                    double bSum = 0, gSum = 0, rSum = 0, aSum = 0;
                    double alphaFix = 0;
                    //参照範囲は基準から上(xは左)へnn、下(xは右)へnn-1の範囲
                    for (int yy = -actD; yy < actD; yy++)
                    {
                        int yc = yKijun + yy;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                        for (int xx = -actD; xx < actD; xx++)
                        {
                            int xc = xKijun + xx;
                            xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                            int pp = (yc * sourceStride) + (xc * pByte);
                            double weight = ws[xx + actD, yy + actD];
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
            }

            //_ = Parallel.For(0, height, y =>
            //  {

            //  });

            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;

            //修正した重み取得
            double[,] GetFixWeights(double rx, double ry, int actN, int n)
            {
                //全体の参照距離
                int nn = actN * 2;
                //基準になる距離計算
                double sx = rx - (int)rx;
                double sy = ry - (int)ry;
                double dx = (sx < 0.5) ? 0.5 - sx : 0.5 - sx + 1;
                double dy = (sy < 0.5) ? 0.5 - sy : 0.5 - sy + 1;

                //各ピクセルの重みと、重み合計を計算
                double[] xw = new double[nn];
                double[] yw = new double[nn];
                double xSum = 0, ySum = 0;
                for (int i = -actN; i < actN; i++)
                {
                    //距離に倍率を掛け算したのをLanczosで重み計算
                    //double x = SincA(Math.Abs(dx + i) / (limitD / 2.0));//n=2より大きいとボケる
                    //double x = SincA(Math.Abs(dx + i) / (inScale / 2.0));//ノイズ多め
                    double x = SincA(Math.Abs(dx + i) / (actN / 2.0));
                    //double x = SincA(Math.Abs(dx + i) / actN);//ボケる
                    //double x = SincA.Abs(dx + i) / (n / 2));//ノイズ多め
                    xSum += x;
                    xw[i + actN] = x;
                    //double y = SincA(Math.Abs(dy + i) / (limitD / 2.0));
                    //double y = SincA(Math.Abs(dy + i) / (inScale / 2.0));
                    double y = SincA(Math.Abs(dy + i) / (actN / 2.0));
                    //double y = SincA(Math.Abs(dy + i) / actN);
                    //double y = SincA(Math.Abs(dy + i) / (n / 2));
                    ySum += y;
                    yw[i + actN] = y;
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
        /// 画像のリサイズ、ニアレストネイバー法で補完、PixelFormats.Bgra32専用)
        /// 高速化なし
        /// </summary>
        /// <param name="source">PixelFormats.Bgra32のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <returns></returns>
        private BitmapSource NearestNeighborBgra32(BitmapSource source, int width, int height)
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
            double widthScale = (double)sourceWidth / width;//横倍率(逆倍率)
            double heightScale = (double)sourceHeight / height;
            int stride = width * pByte;
            byte[] pixels = new byte[height * stride];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //参照点四捨五入で基準
                    int rx = (int)((x + 0.5) * widthScale);
                    int ry = (int)((y + 0.5) * heightScale);

                    //int xKijun = (int)(rx + 0.5);
                    //int yKijun = (int)(ry + 0.5);

                    int sp = (ry * sourceStride) + (rx * pByte);
                    int ap = (y * stride) + (x * pByte);
                    pixels[ap] = sourcePixels[sp];
                    pixels[ap + 1] = sourcePixels[sp + 1];
                    pixels[ap + 2] = sourcePixels[sp + 2];
                    pixels[ap + 3] = sourcePixels[sp + 3];
                }
            }

            //_ = Parallel.For(0, height, y =>
            //  {

            //  });

            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;


        }



        #endregion 縮小用


        private BitmapSource BitmapHeightX2(BitmapSource source)
        {
            //1ピクセルあたりのバイト数、Byte / Pixel
            int pByte = (source.Format.BitsPerPixel + 7) / 8;

            //元画像の画素値の配列作成
            int motoWidth = source.PixelWidth;
            int motoHeight = source.PixelHeight;
            int motoStride = motoWidth * pByte;//1行あたりのbyte数
            byte[] sourcePixels = new byte[motoHeight * motoStride];
            source.CopyPixels(sourcePixels, motoStride, 0);

            //変換後の画像の画素値の配列用
            int height = motoHeight * 2;
            byte[] pixels = new byte[height * motoStride];

            for (int y = 0; y < motoHeight; y++)
            {
                for (int x = 0; x < motoWidth; x++)
                {
                    int pp = (y * motoStride) + (x * pByte);
                    int pp2 = pp + (motoStride * y);
                    pixels[pp2] = sourcePixels[pp];
                    pixels[pp2 + 1] = sourcePixels[pp + 1];
                    pixels[pp2 + 2] = sourcePixels[pp + 2];
                    pixels[pp2 + 3] = sourcePixels[pp + 3];

                    pp2 += motoStride;
                    pixels[pp2] = sourcePixels[pp];
                    pixels[pp2 + 1] = sourcePixels[pp + 1];
                    pixels[pp2 + 2] = sourcePixels[pp + 2];
                    pixels[pp2 + 3] = sourcePixels[pp + 3];
                }
            }

            BitmapSource bitmap = BitmapSource.Create(motoWidth, height, 96, 96, source.Format, null, pixels, motoStride);
            return bitmap;
        }


        private BitmapSource BitmapWidthX2(BitmapSource source)
        {
            //1ピクセルあたりのバイト数、Byte / Pixel
            int pByte = (source.Format.BitsPerPixel + 7) / 8;

            //元画像の画素値の配列作成
            int motoWidth = source.PixelWidth;
            int motoHeight = source.PixelHeight;
            int motoStride = motoWidth * pByte;//1行あたりのbyte数
            byte[] sourcePixels = new byte[motoHeight * motoStride];
            source.CopyPixels(sourcePixels, motoStride, 0);

            //変換後の画像の画素値の配列用
            int sakiWidth = motoWidth * 2;
            int sakiStride = motoStride * 2;
            byte[] pixels = new byte[motoHeight * sakiStride];

            for (int y = 0; y < motoHeight; y++)
            {
                for (int x = 0; x < motoWidth; x++)
                {
                    int pp = (y * motoStride) + (x * pByte);
                    int pp2 = (y * sakiStride) + (x * 2 * pByte);
                    pixels[pp2] = sourcePixels[pp];
                    pixels[pp2 + 1] = sourcePixels[pp + 1];
                    pixels[pp2 + 2] = sourcePixels[pp + 2];
                    pixels[pp2 + 3] = sourcePixels[pp + 3];

                    pixels[pp2 + 4] = sourcePixels[pp];
                    pixels[pp2 + 5] = sourcePixels[pp + 1];
                    pixels[pp2 + 6] = sourcePixels[pp + 2];
                    pixels[pp2 + 7] = sourcePixels[pp + 3];
                }
            }
            BitmapSource bitmap = BitmapSource.Create(sakiWidth, motoHeight, 96, 96, source.Format, null, pixels, sakiStride);
            return bitmap;
        }


        private BitmapSource BitmapX2(BitmapSource source)
        {
            //1ピクセルあたりのバイト数、Byte / Pixel
            int pByte = (source.Format.BitsPerPixel + 7) / 8;

            //元画像の画素値の配列作成
            int motoWidth = source.PixelWidth;
            int motoHeight = source.PixelHeight;
            int motoStride = motoWidth * pByte;//1行あたりのbyte数
            byte[] sourcePixels = new byte[motoHeight * motoStride];
            source.CopyPixels(sourcePixels, motoStride, 0);

            //変換後の画像の画素値の配列用
            int sakiWidth = motoWidth * 2;
            int sakiHeight = motoHeight * 2;
            int sakiStride = motoStride * 2;
            byte[] pixels = new byte[sakiHeight * sakiStride];

            for (int y = 0; y < motoHeight; y++)
            {
                for (int x = 0; x < motoWidth; x++)
                {
                    int pp = (y * motoStride) + (x * pByte);
                    byte p0 = sourcePixels[pp];
                    byte p1 = sourcePixels[pp + 1];
                    byte p2 = sourcePixels[pp + 2];
                    byte p3 = sourcePixels[pp + 3];

                    int pp2 = (y * 2 * sakiStride) + (x * 2 * pByte);
                    pixels[pp2] = p0;
                    pixels[pp2 + 1] = p1;
                    pixels[pp2 + 2] = p2;
                    pixels[pp2 + 3] = p3;
                    pixels[pp2 + 4] = p0;
                    pixels[pp2 + 5] = p1;
                    pixels[pp2 + 6] = p2;
                    pixels[pp2 + 7] = p3;

                    pp2 += sakiStride;
                    pixels[pp2] = p0;
                    pixels[pp2 + 1] = p1;
                    pixels[pp2 + 2] = p2;
                    pixels[pp2 + 3] = p3;
                    pixels[pp2 + 4] = p0;
                    pixels[pp2 + 5] = p1;
                    pixels[pp2 + 6] = p2;
                    pixels[pp2 + 7] = p3;

                }
            }
            BitmapSource bitmap = BitmapSource.Create(sakiWidth, sakiHeight, 96, 96, source.Format, null, pixels, sakiStride);
            return bitmap;
        }




        private BitmapSource BitmapTrimWidth(BitmapSource source)
        {
            int width = source.PixelWidth;
            if (width <= 640) return source;
            int diff = width - 640;
            int x = diff / 2;
            int w = width - diff;
            CroppedBitmap cropped = new(source, new(x, 0, w, source.PixelHeight));
            return cropped;
        }

        private BitmapSource BitmapTrimWidth2(BitmapSource source)
        {
            int width = source.PixelWidth;
            if (width <= 640) return source;
            int diff = width - 640;
            int x = diff / 2;
            //x -= 14;//グランツーリスモ2セッティング画面
            x -= 16;//グランツーリスモ2タイトル画面、これを汎用としてもいいかも
            if (x < 0) x = 0;
            int w = width - diff;
            CroppedBitmap cropped = new(source, new(x, 0, w, source.PixelHeight));
            return cropped;
        }

        private BitmapSource Bitmap640x480(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            bool isHalfWidth = 320 >= width;
            bool isHalfHeight = 240 >= height;
            if (height > width) isHalfWidth = true;
            BitmapSource bitmap;
            if (isHalfHeight && isHalfWidth)
            {
                bitmap = BitmapX2(source);
            }
            else if (isHalfWidth) bitmap = BitmapWidthX2(source);
            else if (isHalfHeight) bitmap = BitmapHeightX2(source);
            else bitmap = source;

            if (bitmap.PixelWidth > 640) bitmap = BitmapTrimWidth2(bitmap);
            return bitmap;
        }

        private BitmapSource BitmapCenter(BitmapSource source)
        {
            int w = source.PixelWidth;
            int h = source.PixelHeight;
            if (w == 640 && h == 480) return source;

            int x = (640 - w) / 2;
            int y = (480 - h) / 2;

            DrawingVisual dv = new();
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawImage(source, new Rect(x, y, w, h));
            }
            var render = new RenderTargetBitmap(640, 480, 96, 96, PixelFormats.Pbgra32);
            render.Render(dv);
            return render;
        }

        private BitmapSource CroppedBitmapFromRects(BitmapSource source, List<Int32Rect> rectList)
        {
            var dv = new DrawingVisual();

            using (DrawingContext dc = dv.RenderOpen())
            {
                //それぞれのRect範囲で切り抜いた画像を描画していく
                foreach (var rect in rectList)
                {
                    dc.DrawImage(new CroppedBitmap(source, rect), new Rect(rect.X, rect.Y, rect.Width, rect.Height));
                }
            }

            //描画位置調整
            dv.Offset = new Vector(-dv.ContentBounds.X, -dv.ContentBounds.Y);

            //bitmap作成、縦横サイズは切り抜き後の画像全体がピッタリ収まるサイズにする
            //PixelFormatsはPbgra32で決め打ち、これ以外だとエラーになるかも、
            //画像を読み込んだbitmapImageのPixelFormats.Bgr32では、なぜかエラーになった
            var bmp = new RenderTargetBitmap(
                (int)Math.Ceiling(dv.ContentBounds.Width),
                (int)Math.Ceiling(dv.ContentBounds.Height),
                96, 96, PixelFormats.Pbgra32);

            bmp.Render(dv);
            //bmp.Freeze();
            return bmp;
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
        /// クリップボードからBitmapSourceを取り出して返す、PNG形式を優先
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
                ////エクセルのグラフとかはAlphaがおかしいのでBgr32にコンバート
                //source = new FormatConvertedBitmap(Clipboard.GetImage(), PixelFormats.Bgr32, null, 0);
            }
            return source;
        }

        /// <summary>
        /// クリップボードからBitmapSourceを取り出して返す、エクセルのオブジェクト用、bmp優先してAlphaを0にして取り出す、
        /// 次点でにpng(アルファ値保持)形式に対応
        /// </summary>
        /// <returns></returns>
        private BitmapSource GetImageFromClipboardBmpNextPNG()
        {
            BitmapSource source = null;
            BitmapSource img = Clipboard.GetImage();

            //BMP
            if (img != null)
            {
                //エクセル系はそのままだとAlphaがおかしいのでBgr32にコンバート
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

        #region 市松模様作成

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


        #endregion 市松模様作成

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

        //ボタンクリック
        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            MyImage.Source = BitmapHeightX2(MyBitmapOrigin32bit);
        }


        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            MyImage.Source = BitmapWidthX2(MyBitmapOrigin32bit);
        }

        private void MyButton3_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            BitmapSource source = BitmapWidthX2(MyBitmapOrigin32bit);
            MyImage.Source = BitmapTrimWidth(source);
        }

        private void MyButton4_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            BitmapSource source = BitmapWidthX2(MyBitmapOrigin32bit);
            MyImage.Source = BitmapTrimWidth2(source);
        }

        private void MyButton5_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            MyImage.Source = BitmapX2(MyBitmapOrigin32bit);
        }

        private void MyButton6_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            var bmp = Bitmap640x480(MyBitmapOrigin32bit);
            bmp = Bitmap640x480(bmp);
            MyImage.Source = bmp;
            MyStatusItem.Content = $"{bmp.PixelWidth}, {bmp.PixelHeight}";
        }



        #endregion コピペ

        private void MyButton7_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            var bmp = BitmapX2(MyBitmapOrigin32bit);
            if (bmp.PixelWidth > 640)
            {
                bmp = BicubicBgra32(bmp, 640, 480, MySliderCubic.Value);
            }
            MyImage.Source = bmp;
            MyStatusItem.Content = $"{bmp.PixelWidth}, {bmp.PixelHeight}";
        }

        private void MyButton8_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            var bmp = BitmapX2(MyBitmapOrigin32bit);
            if (bmp.PixelWidth > 640)
            {
                bmp = LanczosBgra32(bmp, 640, 480, (int)MySliderLanczosN.Value);
            }
            MyImage.Source = bmp;
            MyStatusItem.Content = $"{bmp.PixelWidth}, {bmp.PixelHeight}";
        }

        private void MyButton9_Click(object sender, RoutedEventArgs e)
        {
            //4倍拡大してLanczosで縮小640x480
            if (MyBitmapOrigin == null) return;
            var bmp = BitmapX2(MyBitmapOrigin32bit);
            bmp = BitmapX2(bmp);
            bmp = LanczosBgra32(bmp, 640, 480, (int)MySliderLanczosN.Value);
            MyImage.Source = bmp;
            MyStatusItem.Content = $"{bmp.PixelWidth}, {bmp.PixelHeight}";
        }

        private void MyButton10_Click(object sender, RoutedEventArgs e)
        {
            //そのまま、ニアレストネイバー
            if (MyBitmapOrigin == null) return;
            BitmapSource bmp = NearestNeighborBgra32(MyBitmapOrigin32bit, 640, 480);
            MyImage.Source = bmp;
            MyStatusItem.Content = $"{bmp.PixelWidth}, {bmp.PixelHeight}";
        }

        private void MyButton11_Click(object sender, RoutedEventArgs e)
        {
            //そのままLanczos
            if (MyBitmapOrigin == null) return;
            BitmapSource bmp = LanczosBgra32(MyBitmapOrigin32bit, 640, 480, (int)MySliderLanczosN.Value);
            MyImage.Source = bmp;
            MyStatusItem.Content = $"{bmp.PixelWidth}, {bmp.PixelHeight}";
        }

        //ランチョス、n=4がいい、安定、
        private void MyButton12_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            BitmapSource bmp = MyBitmapOrigin32bit;
            if (bmp.PixelHeight <= 240)
            {
                bmp = BitmapHeightX2(MyBitmapOrigin32bit);
            }
            if (bmp.PixelWidth < 512) bmp = BitmapWidthX2(bmp);
            if (bmp.PixelWidth > 640 || bmp.PixelWidth < 624)
            {
                bmp = LanczosBgra32(bmp, 640, 480, (int)MySliderLanczosN.Value);
            }
            //640x480の中央配置
            bmp = BitmapCenter(bmp);

            MyImage.Source = bmp;
            MyStatusItem.Content = $"{bmp.PixelWidth}, {bmp.PixelHeight}";
        }

        //派手、シャープ、
        private void MyButton13_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            BitmapSource bmp = MyBitmapOrigin32bit;
            if (bmp.PixelHeight <= 240)
            {
                bmp = BitmapHeightX2(MyBitmapOrigin32bit);
            }
            if (bmp.PixelWidth < 512) bmp = BitmapWidthX2(bmp);
            if (bmp.PixelWidth > 640 || bmp.PixelWidth < 624)
            {
                bmp = SincBgra32(bmp, 640, 480, (int)MySliderLanczosN.Value);
            }
            bmp = BitmapCenter(bmp);
            MyImage.Source = bmp;
            MyStatusItem.Content = $"{bmp.PixelWidth}, {bmp.PixelHeight}";
        }

        private void MyButton14_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            BitmapSource bmp = MyBitmapOrigin32bit;
            if (bmp.PixelHeight <= 240)
            {
                bmp = BitmapHeightX2(MyBitmapOrigin32bit);
            }
            if (bmp.PixelWidth < 512) bmp = BitmapWidthX2(bmp);
            if (bmp.PixelWidth > 640 || bmp.PixelWidth < 624)
            {
                bmp = SincTypeGBgra32(bmp, 640, 480, (int)MySliderLanczosN.Value);
            }
            bmp = BitmapCenter(bmp);
            MyImage.Source = bmp;
            MyStatusItem.Content = $"{bmp.PixelWidth}, {bmp.PixelHeight}";
        }
    }
}