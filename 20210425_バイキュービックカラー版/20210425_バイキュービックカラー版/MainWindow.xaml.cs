using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.Diagnostics;

namespace _20210425_バイキュービックカラー版
{
    public partial class MainWindow : Window
    {
        private BitmapSource MyBitmapOrigin;
        private BitmapSource MyBitmapOrigin32bit;
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.Top = 0;
            this.Left = 0;
#endif
            this.Background = MakeTileBrush(MakeCheckeredPattern(16, Colors.WhiteSmoke, Colors.LightGray));
        }

        //処理時間計測
        private void MyExe(Func<BitmapSource, int, int, double, BitmapSource> func,
    BitmapSource source, int width, int height, double a)
        {
            var sw = new Stopwatch();
            sw.Start();
            var bitmap = func(source, width, height, a);
            sw.Stop();
            MyStatusItem.Content = $"処理時間：{sw.Elapsed.TotalSeconds:000.000}秒, {func.Method.Name}";
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




        /// <summary>
        /// 画像の縮小、バイキュービック法で補完、PixelFormats.Bgra32専用)
        /// セパラブルとParallelによる高速化
        /// </summary>
        /// <param name="source">PixelFormats.Bgra32のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="a">-0.5～-1.0くらいを指定する、小さくするとシャープ、大きくするとぼかし</param>
        /// <returns></returns>
        private BitmapSource BicubicBgra32(BitmapSource source, int width, int height, double a = -1.0)
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

                      double bv = 0, gv = 0, rv = 0, av = 0;
                      double alphaFix = 0;//完全透明ピクセルによる修正用
                      int pp;
                      for (int xx = -2; xx <= 1; xx++)
                      {
                          int xc = xKijun + xx;
                          //マイナス座標や画像サイズを超えていたら、収まるように修正
                          xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                          //距離計算、+0.5しているのは中心座標で計算するため
                          double dx = Math.Abs(rx - (xx + xKijun + 0.5));
                          //重み取得してrgb各値と掛け算して加算
                          double weight = GetWeightCubic(dx, a);
                          pp = (y * sourceStride) + (xc * pByte);
                          //完全透明ピクセル(a=0)だった場合はRGBは計算しないで
                          //重みだけ足し算して後で使う
                          if (sourcePixels[pp + 3] == 0)
                          {
                              alphaFix += weight;
                              continue;
                          }
                          bv += sourcePixels[pp] * weight;
                          gv += sourcePixels[pp + 1] * weight;
                          rv += sourcePixels[pp + 2] * weight;
                          av += sourcePixels[pp + 3] * weight;
                      }

                      //                    C#、WPF、バイリニア法での画像の拡大縮小変換、半透明画像(32bit画像)対応版 - 午後わてんのブログ
                      //https://gogowaten.hatenablog.com/entry/2021/04/17/151803#32bit%E3%81%A824bit%E3%81%AF%E9%81%95%E3%81%A3%E3%81%9F

                      //完全透明ピクセルによるRGB値の修正
                      //16ピクセルすべて完全透明だった場合は計算しない0のまま
                      if (alphaFix == 1) continue;
                      //完全透明ピクセルが混じっていた場合は、その分を差し引いてRGB修正する
                      double rgbFix = 1 / (1 - alphaFix);
                      bv *= rgbFix;
                      gv *= rgbFix;
                      rv *= rgbFix;

                      pp = y * stride + x * pByte;
                      xResult[pp] = bv;
                      xResult[pp + 1] = gv;
                      xResult[pp + 2] = rv;
                      xResult[pp + 3] = av;
                  }
              });

            //縦処理
            _ = Parallel.For(0, height, y =>
              {
                  for (int x = 0; x < width; x++)
                  {
                      double ry = (y + 0.5) * heightScale;
                      int yKijun = (int)(ry + 0.5);

                      double bv = 0, gv = 0, rv = 0, av = 0;
                      double alphaFix = 0;
                      int pp;
                      for (int yy = -2; yy <= 1; yy++)//
                      {
                          double dy = Math.Abs(ry - (yy + yKijun + 0.5));//距離
                          double weight = GetWeightCubic(dy, a);//重み
                          int yc = yKijun + yy;
                          yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                          pp = (yc * stride) + (x * pByte);

                          //完全透明ピクセル(a=0)だった場合はRGBは計算しないで
                          //重みだけ足し算して後で使う
                          if (xResult[pp + 3] == 0)
                          {
                              alphaFix += weight;
                              continue;
                          }
                          bv += xResult[pp] * weight;
                          gv += xResult[pp + 1] * weight;
                          rv += xResult[pp + 2] * weight;
                          av += xResult[pp + 3] * weight;
                      }
                      //完全透明ピクセルによるRGB値の修正
                      //16ピクセルすべて完全透明だった場合は計算しない0のまま
                      if (alphaFix == 1) continue;
                      //完全透明ピクセルが混じっていた場合は、その分を差し引いてRGB修正する
                      double rgbFix = 1 / (1 - alphaFix);
                      bv *= rgbFix;
                      gv *= rgbFix;
                      rv *= rgbFix;
                      //0～255の範囲を超えることがあるので、修正
                      bv = bv < 0 ? 0 : bv > 255 ? 255 : bv;
                      gv = gv < 0 ? 0 : gv > 255 ? 255 : gv;
                      rv = rv < 0 ? 0 : rv > 255 ? 255 : rv;
                      av = av < 0 ? 0 : av > 255 ? 255 : av;
                      int ap = (y * stride) + (x * pByte);
                      pixels[ap] = (byte)(bv + 0.5);
                      pixels[ap + 1] = (byte)(gv + 0.5);
                      pixels[ap + 2] = (byte)(rv + 0.5);
                      pixels[ap + 3] = (byte)(av + 0.5);
                  }
              });


            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;
        }



        //未使用
        /// <summary>
        /// 画像の縮小、バイキュービック法で補完、PixelFormats.Bgra32専用)
        /// 通常版
        /// </summary>
        /// <param name="source">PixelFormats.Bgra32のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="a">-0.5～-1.0くらいを指定する、小さくするとシャープ、大きくするとぼかし</param>
        /// <returns></returns>
        private BitmapSource BicubicBgra32Old(BitmapSource source, int width, int height, double a = -1.0)
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

            int pp;
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

                    double bv = 0, gv = 0, rv = 0, av = 0;
                    double alphaFix = 0;
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
                            double weight = yw * xw;
                            pp = (yc * sourceStride) + (xc * pByte);
                            //完全透明ピクセル(a=0)だった場合はRGBは計算しないで
                            //重みだけ足し算して後で使う
                            if (sourcePixels[pp + 3] == 0)
                            {
                                alphaFix += weight;
                                continue;
                            }
                            bv += sourcePixels[pp] * weight;
                            gv += sourcePixels[pp + 1] * weight;
                            rv += sourcePixels[pp + 2] * weight;
                            av += sourcePixels[pp + 3] * weight;
                        }
                    }

                    //                    C#、WPF、バイリニア法での画像の拡大縮小変換、半透明画像(32bit画像)対応版 - 午後わてんのブログ
                    //https://gogowaten.hatenablog.com/entry/2021/04/17/151803#32bit%E3%81%A824bit%E3%81%AF%E9%81%95%E3%81%A3%E3%81%9F

                    //完全透明ピクセルによるRGB値の修正
                    //16ピクセルすべて完全透明だった場合は計算しない0のまま
                    if (alphaFix == 1) continue;
                    //完全透明ピクセルが混じっていた場合は、その分を差し引いてRGB修正する
                    double rgbFix = 1 / (1 - alphaFix);
                    bv *= rgbFix;
                    gv *= rgbFix;
                    rv *= rgbFix;

                    //0～255の範囲を超えることがあるので、修正
                    bv = bv < 0 ? 0 : bv > 255 ? 255 : bv;
                    gv = gv < 0 ? 0 : gv > 255 ? 255 : gv;
                    rv = rv < 0 ? 0 : rv > 255 ? 255 : rv;
                    av = av < 0 ? 0 : av > 255 ? 255 : av;
                    pp = (y * stride) + (x * pByte);
                    pixels[pp] = (byte)(bv + 0.5);
                    pixels[pp + 1] = (byte)(gv + 0.5);
                    pixels[pp + 2] = (byte)(rv + 0.5);
                    pixels[pp + 3] = (byte)(av + 0.5);
                }
            };

            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;
        }




        /// <summary>
        /// 画像の縮小、バイキュービック法で補完、PixelFormats.Bgr24専用)
        /// セパラブルとParallelで高速化
        /// </summary>
        /// <param name="source">PixelFormats.Bgr24のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="a">-0.5～-1.0くらいを指定する、小さくするとシャープ、大きくするとぼかし</param>
        /// <returns></returns>
        private BitmapSource BicubicBgr24(BitmapSource source, int width, int height, double a = -1.0)
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
            Parallel.For(0, sourceHeight, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    //参照点
                    double rx = (x + 0.5) * widthScale;
                    //参照点四捨五入で基準
                    int xKijun = (int)(rx + 0.5);

                    double bv = 0, gv = 0, rv = 0;
                    int pp;
                    for (int xx = -2; xx <= 1; xx++)
                    {
                        int xc = xKijun + xx;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                        //距離計算、+0.5しているのは中心座標で計算するため
                        double dx = Math.Abs(rx - (xx + xKijun + 0.5));
                        //重み取得してrgb各値と掛け算して加算
                        double weight = GetWeightCubic(dx, a);
                        pp = (y * sourceStride) + (xc * pByte);
                        bv += sourcePixels[pp] * weight;
                        gv += sourcePixels[pp + 1] * weight;
                        rv += sourcePixels[pp + 2] * weight;
                    }
                    pp = y * stride + x * pByte;
                    xResult[pp] = bv;
                    xResult[pp + 1] = gv;
                    xResult[pp + 2] = rv;
                }
            });

            //縦処理
            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    double ry = (y + 0.5) * heightScale;
                    int yKijun = (int)(ry + 0.5);

                    double bv = 0, gv = 0, rv = 0;
                    int pp;
                    for (int yy = -2; yy <= 1; yy++)//
                    {
                        double dy = Math.Abs(ry - (yy + yKijun + 0.5));//距離
                        double weight = GetWeightCubic(dy, a);//重み
                        int yc = yKijun + yy;
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                        pp = (yc * stride) + (x * pByte);
                        bv += xResult[pp] * weight;
                        gv += xResult[pp + 1] * weight;
                        rv += xResult[pp + 2] * weight;
                    }
                    //0～255の範囲を超えることがあるので、修正
                    bv = bv < 0 ? 0 : bv > 255 ? 255 : bv;
                    gv = gv < 0 ? 0 : gv > 255 ? 255 : gv;
                    rv = rv < 0 ? 0 : rv > 255 ? 255 : rv;
                    int ap = (y * stride) + (x * pByte);
                    pixels[ap] = (byte)(bv + 0.5);
                    pixels[ap + 1] = (byte)(gv + 0.5);
                    pixels[ap + 2] = (byte)(rv + 0.5);
                }
            });


            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;
        }



        //未使用
        /// <summary>
        /// 画像の縮小、バイキュービック法で補完、PixelFormats.Bgr24専用)
        /// 通常版
        /// </summary>
        /// <param name="source">PixelFormats.Bgr24のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="a">-0.5～-1.0くらいを指定する、小さくするとシャープ、大きくするとぼかし</param>
        /// <returns></returns>
        private BitmapSource BicubicBgr24Old(BitmapSource source, int width, int height, double a = -1.0)
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

                    double bv = 0, gv = 0, rv = 0;
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
                            double weight = yw * xw;
                            int pp = (yc * sourceStride) + (xc * pByte);
                            bv += sourcePixels[pp] * weight;
                            gv += sourcePixels[pp + 1] * weight;
                            rv += sourcePixels[pp + 2] * weight;
                        }
                    }
                    //0～255の範囲を超えることがあるので、修正
                    bv = bv < 0 ? 0 : bv > 255 ? 255 : bv;
                    gv = gv < 0 ? 0 : gv > 255 ? 255 : gv;
                    rv = rv < 0 ? 0 : rv > 255 ? 255 : rv;
                    int ap = (y * stride) + (x * pByte);
                    pixels[ap] = (byte)(bv + 0.5);
                    pixels[ap + 1] = (byte)(gv + 0.5);
                    pixels[ap + 2] = (byte)(rv + 0.5);
                }
            };

            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;
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
            MyBitmapOrigin = MakeBitmapSourceBgr24FromFile(paths[0]);
            MyBitmapOrigin32bit = MakeBitmapSourceBgra32FromFile(paths[0]);
            MyImage.Source = MyBitmapOrigin32bit;
        }

        //ボタンクリック
        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);            
            MyExe(BicubicBgr24, MyBitmapOrigin, yoko, tate, MySlider.Value);
        }


        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth * MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight * MySliderScale.Value);
            MyExe(BicubicBgr24, MyBitmapOrigin, yoko, tate, MySlider.Value);
        }

        private void MyButton3_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe(BicubicBgra32, MyBitmapOrigin32bit, yoko, tate, MySlider.Value);
        }

        private void MyButton4_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth * MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight * MySliderScale.Value);
            MyExe(BicubicBgra32, MyBitmapOrigin32bit, yoko, tate, MySlider.Value);
        }

        //画像をクリップボードにコピー
        private void MyButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            ClipboardSetImageWithPng((BitmapSource)MyImage.Source);
        }


        //クリップボードから画像追加
        private void MyButtonPaste_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource img = GetImageFromClipboardWithPNG();
            if (img != null)
            {
                FormatConvertedBitmap bitmap = new(img, PixelFormats.Bgr24, null, 0);
                MyBitmapOrigin = bitmap;
                FormatConvertedBitmap bitmap32 = new(img, PixelFormats.Bgra32, null, 0);
                MyImage.Source = bitmap32;
            }
        }

        private void MySlider_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0) MySlider.Value += MySlider.SmallChange;
            else MySlider.Value -= MySlider.SmallChange;
        }


        private void MyButtonToOrigin_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = MyBitmapOrigin32bit;
        }


        #endregion コピペ
    }
}