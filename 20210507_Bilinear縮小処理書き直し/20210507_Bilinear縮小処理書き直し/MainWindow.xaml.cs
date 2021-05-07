using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.Diagnostics;

//縦横ピクセル数を指定できるけど、横ピクセル数しか考慮していないので、縦ピクセル数はアスペクト比を考えないと正確な色じゃなくなるはず
//バイリニアとバイキュービックでの縮小処理に対応

//バイリニアとバイキュービックの縮小処理も書き直した - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2021/05/07/162232


namespace _20210507_Bilinear縮小処理書き直し
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






        ///// <summary>
        ///// 画像のリサイズ、バイキュービック法で補完、PixelFormats.Bgra32専用)
        ///// </summary>
        ///// <param name="source">PixelFormats.Bgra32のBitmap</param>
        ///// <param name="width">変換後の横ピクセル数を指定</param>
        ///// <param name="height">変換後の縦ピクセル数を指定</param>
        ///// <param name="a">係数、-1.0～-0.5がいい</param>
        ///// <param name="sharp">縮小時に参照距離の小数以下切り捨てすることで画質をシャープにする</param>
        ///// <returns></returns>
        //private BitmapSource BicubicBgra32(BitmapSource source, int width, int height, double a, bool sharp = false)
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
        //    double widthScale = (double)sourceWidth / width;//横倍率(逆倍率)
        //    double heightScale = (double)sourceHeight / height;
        //    int stride = width * pByte;
        //    byte[] pixels = new byte[height * stride];

        //    //横処理用配列
        //    double[] xResult = new double[sourceHeight * stride];

        //    //倍率
        //    double scale = width / (double)sourceWidth;
        //    //実際の参照距離、縮小時だけに関係する、切り下げだと画質はシャープになるかも            
        //    int actN = ((int)widthScale) * 2;//切り捨て
        //    if (sharp == false) actN = ((int)Math.Ceiling(widthScale)) * 2;
        //    //逆倍率
        //    double inScale = 1.0 / scale;

        //    //拡大時の調整(これがないと縮小専用)
        //    if (1.0 < scale)
        //    {
        //        scale = 1.0;//重み計算に使う、拡大時は1固定
        //        inScale = 1.0;//逆倍率、拡大時は1固定
        //        actN = 2;//拡大時のバイキュービックの参照距離は2で固定
        //    }

        //    //横処理
        //    _ = Parallel.For(0, sourceHeight, y =>
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            //参照点
        //            double rx = (x + 0.5) * widthScale;
        //            //参照点四捨五入で基準
        //            int xKijun = (int)(rx + 0.5);
        //            //修正した重み取得
        //            var ws = GetFixWeihgts(rx, a, actN, scale, inScale);

        //            double bSum = 0, gSum = 0, rSum = 0, aSum = 0;
        //            double alphaFix = 0;
        //            int pp;
        //            for (int xx = -actN; xx < actN; xx++)
        //            {
        //                int xc = xKijun + xx;
        //                //マイナス座標や画像サイズを超えていたら、収まるように修正
        //                xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
        //                pp = (y * sourceStride) + (xc * pByte);
        //                double weight = ws[xx + actN];
        //                //完全透明ピクセル(a=0)だった場合はRGBは計算しないで
        //                //重みだけ足し算して後で使う
        //                if (sourcePixels[pp + 3] == 0)
        //                {
        //                    alphaFix += weight;
        //                    continue;
        //                }
        //                bSum += sourcePixels[pp] * weight;
        //                gSum += sourcePixels[pp + 1] * weight;
        //                rSum += sourcePixels[pp + 2] * weight;
        //                aSum += sourcePixels[pp + 3] * weight;
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

        //            pp = y * stride + x * pByte;
        //            xResult[pp] = bSum;
        //            xResult[pp + 1] = gSum;
        //            xResult[pp + 2] = rSum;
        //            xResult[pp + 3] = aSum;
        //        }
        //    });

        //    //縦処理
        //    _ = Parallel.For(0, height, y =>
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            double ry = (y + 0.5) * heightScale;
        //            int yKijun = (int)(ry + 0.5);
        //            double[] ws = GetFixWeihgts(ry, a, actN, scale, inScale);
        //            double bSum = 0, gSum = 0, rSum = 0, aSum = 0;
        //            double alphaFix = 0;
        //            int pp;
        //            for (int yy = -actN; yy < actN; yy++)
        //            {
        //                int yc = yKijun + yy;
        //                yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
        //                pp = (yc * stride) + (x * pByte);
        //                double weight = ws[yy + actN];

        //                if (xResult[pp + 3] == 0)
        //                {
        //                    alphaFix += weight;
        //                    continue;
        //                }
        //                bSum += xResult[pp] * weight;
        //                gSum += xResult[pp + 1] * weight;
        //                rSum += xResult[pp + 2] * weight;
        //                aSum += xResult[pp + 3] * weight;
        //            }
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
        //    });



        //    BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
        //    return bitmap;



        //    //修正した重み取得
        //    double[] GetFixWeihgts(double r, double a, int actN, double scale, double inScale)
        //    {
        //        int nn = actN * 2;//全体の参照距離
        //        //基準距離
        //        double s = r - (int)r;
        //        double d = (s < 0.5) ? 0.5 - s : 0.5 - s + 1;

        //        //各重みと重み合計
        //        double[] ws = new double[nn];
        //        double sum = 0;
        //        for (int i = -actN; i < actN; i++)
        //        {
        //            double w = GetWeightCubic(Math.Abs(d + i) * scale, inScale, a);
        //            sum += w;
        //            ws[i + actN] = w;
        //        }

        //        //重み合計で割り算して修正、全体で100%(1.0)にする
        //        for (int i = 0; i < nn; i++)
        //        {
        //            ws[i] /= sum;
        //        }
        //        return ws;
        //    }
        //}



        /// <summary>
        /// 画像のリサイズ、バイキュービック法で補完、PixelFormats.Bgra32専用)
        /// セパラブルなので縦横別々処理とParallelで高速化したもの
        /// </summary>
        /// <param name="source">PixelFormats.Bgra32のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="a">係数、-1.0～-0.5がいい</param>
        /// <param name="soft">trueで縮小時に画質がソフトになる？</param>
        /// <returns></returns>
        private BitmapSource BicubicBgra32Ex(BitmapSource source, int width, int height, double a, bool soft = false)
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

            //横処理用配列
            double[] xResult = new double[sourceHeight * stride];

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
                    double[] ws = GetFixWeights(rx, actD, a, scale, inScale);
                    double bSum = 0, gSum = 0, rSum = 0, aSum = 0;
                    double alphaFix = 0;

                    int pp;
                    for (int xx = -actD; xx < actD; xx++)
                    {
                        int xc = xKijun + xx;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                        pp = (y * sourceStride) + (xc * pByte);
                        double weight = ws[xx + actD];
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
                        double[] ws = GetFixWeights(ry, actD, a, scale, inScale);

                        double bSum = 0, gSum = 0, rSum = 0, aSum = 0;
                        double alphaFix = 0;
                        int pp;
                        for (int yy = -actD; yy < actD; yy++)
                        {
                            int yc = yKijun + yy;
                            yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                            pp = (yc * stride) + (x * pByte);
                            double weight = ws[yy + actD];

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

            double[] GetFixWeights(double r, int actD, double a, double scale, double inScale)
            {
                //全体の参照距離
                int nn = actD * 2;
                //基準になる距離計算
                double s = r - (int)r;
                double d = (s < 0.5) ? 0.5 - s : 0.5 - s + 1;
                //各重みと重み合計
                double[] ws = new double[nn];
                double sum = 0;
                for (int i = -actD; i < actD; i++)
                {
                    double w = GetWeightCubic(Math.Abs(d + i), a, scale, inScale);
                    sum += w;
                    ws[i + actD] = w;
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



        /// <summary>
        /// 画像のリサイズ、バイリニア法で補完、PixelFormats.Bgra32専用)
        /// </summary>
        /// <param name="source">PixelFormats.Bgra32のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="sharp">縮小時に参照距離の小数以下切り捨てすることで画質をシャープにする</param>
        /// <returns></returns>
        private BitmapSource BilinearBgra32(BitmapSource source, int width, int height, bool sharp = false)
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
            //実際の参照距離、縮小時だけに関係する、切り下げだと画質はシャープになるかも
            //int actN = (int)(widthScale + 0.5);//四捨五入
            int actN = (int)widthScale;//切り捨て
            if (sharp == false) actN = (int)Math.Ceiling(widthScale);


            //拡大時の調整(これがないと縮小専用)
            if (1.0 < scale)
            {
                scale = 1.0;//重み計算に使う、拡大時は1固定
                actN = 1;//拡大時の実際の参照距離は指定距離と同じ
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
                    double[,] ws = GetFixWeights(rx, ry, actN, scale);

                    double bSum = 0, gSum = 0, rSum = 0, aSum = 0;
                    double alphaFix = 0;
                    //参照範囲は基準から上(xは左)へnn、下(xは右)へnn-1の範囲
                    for (int yy = -actN; yy < actN; yy++)
                    {
                        int yc = yKijun + yy;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                        for (int xx = -actN; xx < actN; xx++)
                        {
                            int xc = xKijun + xx;
                            xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                            int pp = (yc * sourceStride) + (xc * pByte);
                            double weight = ws[xx + actN, yy + actN];
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
            double[,] GetFixWeights(double rx, double ry, int actN, double scale)
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
                    double x = GetBilinearWeight(Math.Abs(dx + i), scale);
                    xSum += x;
                    xw[i + actN] = x;
                    double y = GetBilinearWeight(Math.Abs(dy + i), scale);
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
        /// バイリニアでの重み計算
        /// </summary>
        /// <param name="d">距離</param>
        /// <param name="scale">縮小倍率、拡大時は1で固定</param>
        /// <returns></returns>
        private double GetBilinearWeight(double d, double scale)
        {
            return 1 - (d * scale);
        }






        //処理時間計測
        private void MyExe(
            Func<BitmapSource, int, int, bool, BitmapSource> func,
            BitmapSource source, int width, int height, bool sharp = false)
        {
            var sw = new Stopwatch();
            sw.Start();
            var bitmap = func(source, width, height, sharp);
            sw.Stop();
            MyStatusItem.Content = $"処理時間：{sw.Elapsed.TotalSeconds:000.000}秒, {func.Method.Name}";
            MyImage.Source = bitmap;
        }
        private void MyExe(
            Func<BitmapSource, int, int, double, bool, BitmapSource> func,
            BitmapSource source, int width, int height, double a, bool sharp = false)
        {
            var sw = new Stopwatch();
            sw.Start();
            var bitmap = func(source, width, height, a, sharp);
            sw.Stop();
            MyStatusItem.Content = $"処理時間：{sw.Elapsed.TotalSeconds:000.000}秒, {func.Method.Name}";
            MyImage.Source = bitmap;
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

        //ボタンクリック
        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe(BilinearBgra32, MyBitmapOrigin32bit, yoko, tate);
        }


        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth * MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight * MySliderScale.Value);
            MyExe(BilinearBgra32, MyBitmapOrigin32bit, yoko, tate);
        }

        private void MyButton3_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe(BilinearBgra32, MyBitmapOrigin32bit, yoko, tate, true);
        }

        private void MyButton4_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe(BicubicBgra32, MyBitmapOrigin32bit, yoko, tate, MySliderCubic.Value);
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
        #endregion コピペ

        private void MyButton5_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth * MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight * MySliderScale.Value);
            MyExe(BicubicBgra32, MyBitmapOrigin32bit, yoko, tate, MySliderCubic.Value);
        }

        private void MyButton6_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe(BicubicBgra32, MyBitmapOrigin32bit, yoko, tate, MySliderCubic.Value, true);
        }

        private void MyButton7_Click(object sender, RoutedEventArgs e)
        {
            if (MyBitmapOrigin == null) return;
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth * MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight * MySliderScale.Value);
            MyExe(BicubicBgra32Ex, MyBitmapOrigin32bit, yoko, tate, MySliderCubic.Value);
        }
    }
}
