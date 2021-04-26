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


namespace _20210426_Lanczosで拡大縮小
{
    public partial class MainWindow : Window
    {
        private BitmapSource MyBitmapOrigin;
        //private BitmapSource MyBitmapOrigin32bit;
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.Top = 0;
            this.Left = 0;
#endif
            //this.Background = MakeTileBrush(MakeCheckeredPattern(16, Colors.WhiteSmoke, Colors.LightGray));
        }

        //処理時間計測
        private void MyExe(Func<BitmapSource, int, int, int, BitmapSource> func,
    BitmapSource source, int width, int height, int a)
        {
            var sw = new Stopwatch();
            sw.Start();
            var bitmap = func(source, width, height, a);
            sw.Stop();
            MyStatusItem.Content = $"処理時間：{sw.Elapsed.TotalSeconds:000.000}秒, {func.Method.Name}";
            MyImage.Source = bitmap;
        }

        private double Sinc(double d)
        {
            //double pix = Math.PI * d;
            //return Math.Sin(pix) / pix;
            return (Math.Sin(Math.PI * d)) / (Math.PI * d);
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



        /// <summary>
        /// 画像の拡大縮小、ランチョス法で補完、PixelFormats.Gray8専用)
        /// セパラブルとParallelで高速化
        /// </summary>
        /// <param name="source">PixelFormats.Gray8のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="n">最大参照距離、3か4がいい</param>
        /// <returns></returns>
        private BitmapSource LanczosGray8Kai(BitmapSource source, int width, int height, int n)
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

                      double vv = 0;
                      int pp;
                      for (int xx = -n; xx < n; xx++)
                      //for (int xx = -2; xx <= 1; xx++)
                      {
                          int xc = xKijun + xx;
                          //マイナス座標や画像サイズを超えていたら、収まるように修正
                          xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                          //距離計算、+0.5しているのは中心座標で計算するため
                          double dx = Math.Abs(rx - (xx + xKijun + 0.5));
                          //重み取得してrgb各値と掛け算して加算
                          double weight = GetLanczosWeight(dx, n);
                          pp = (y * sourceStride) + (xc * pByte);
                          vv += sourcePixels[pp] * weight;
                      }
                      pp = y * stride + x * pByte;
                      xResult[pp] = vv;
                  }
              });

            //縦処理
            _ = Parallel.For(0, height, y =>
              {
                  for (int x = 0; x < width; x++)
                  {
                      double ry = (y + 0.5) * heightScale;
                      int yKijun = (int)(ry + 0.5);

                      double vv = 0;
                      int pp;
                      for (int yy = -n; yy < n; yy++)
                      //for (int yy = -2; yy <= 1; yy++)
                      {
                          double dy = Math.Abs(ry - (yy + yKijun + 0.5));//距離
                          double weight = GetLanczosWeight(dy, n);//重み
                          int yc = yKijun + yy;
                          yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                          pp = (yc * stride) + (x * pByte);
                          vv += xResult[pp] * weight;
                      }
                      //0～255の範囲を超えることがあるので、修正
                      vv = vv < 0 ? 0 : vv > 255 ? 255 : vv;
                      int ap = (y * stride) + (x * pByte);
                      pixels[ap] = (byte)(vv + 0.5);
                  }
              });


            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;
        }



        //未使用
        /// <summary>
        /// 画像の拡大縮小、ランチョス法で補完、PixelFormats.Gray8専用)
        /// 通常版
        /// </summary>
        /// <param name="source">PixelFormats.Gray8のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="n">最大参照距離、3か4がいい</param>
        /// <returns></returns>
        private BitmapSource LanczosGray8(BitmapSource source, int width, int height, int n)
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

                    var ws = GetWS(rx, ry,n);
                    double vv = 0;
                    //参照範囲は基準から左へ2、右へ1の範囲
                    for (int yy = -n; yy < n; yy++)
                    //for (int yy = -2; yy <= 1; yy++)
                    {
                        //+0.5しているのは中心座標で計算するため
                        //double dy = Math.Abs(ry - (yy + yKijun + 0.5));//距離
                        //double yw = GetLanczosWeight(dy, n);//重み
                        int yc = yKijun + yy;
                        //マイナス座標や画像サイズを超えていたら、収まるように修正
                        yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;
                        for (int xx = -n; xx < n; xx++)
                        //for (int xx = -2; xx <= 1; xx++)
                        {
                            //double dx = Math.Abs(rx - (xx + xKijun + 0.5));
                            //double xw = GetLanczosWeight(dx, n);
                            int xc = xKijun + xx;
                            xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
                            //double weight = yw * xw;
                            int pp = (yc * sourceStride) + (xc * pByte);
                            vv += sourcePixels[pp] * ws[xx+n,yy+n];
                            //vv += sourcePixels[pp] * weight;
                        }
                    }
                    //0～255の範囲を超えることがあるので、修正
                    vv = vv < 0 ? 0 : vv > 255 ? 255 : vv;
                    int ap = (y * stride) + (x * pByte);
                    pixels[ap] = (byte)(vv + 0.5);
                }
            };

            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            return bitmap;

            double[,] GetWS(double rx, double ry, int n)
            {
                double sx = rx - (int)rx;
                double sy = ry - (int)ry;
                double dx = (sx < 0.5) ? 0.5 - sx : 0.5 - sx + 1;
                double dy = (sy < 0.5) ? 0.5 - sy : 0.5 - sy + 1;
                int nn = n * 2;
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
                for (int i = 0; i < nn; i++)
                {
                    xw[i] /= xSum;
                    yw[i] /= ySum;
                }
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

            ////未使用
            ///// <summary>
            ///// 画像の拡大縮小、ランチョス法で補完、PixelFormats.Gray8専用)
            ///// 通常版
            ///// </summary>
            ///// <param name="source">PixelFormats.Gray8のBitmap</param>
            ///// <param name="width">変換後の横ピクセル数を指定</param>
            ///// <param name="height">変換後の縦ピクセル数を指定</param>
            ///// <param name="n">最大参照距離、3か4がいい</param>
            ///// <returns></returns>
            //private BitmapSource LanczosGray8(BitmapSource source, int width, int height, int n)
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

            //            double vv = 0;
            //            //参照範囲は基準から左へ2、右へ1の範囲
            //            for (int yy = -n; yy < n; yy++)
            //            //for (int yy = -2; yy <= 1; yy++)
            //            {
            //                //+0.5しているのは中心座標で計算するため
            //                double dy = Math.Abs(ry - (yy + yKijun + 0.5));//距離
            //                double yw = GetLanczosWeight(dy, n);//重み
            //                int yc = yKijun + yy;
            //                //マイナス座標や画像サイズを超えていたら、収まるように修正
            //                yc = yc < 0 ? 0 : yc > sourceHeight - 1 ? sourceHeight - 1 : yc;                        
            //                for (int xx = -n; xx < n; xx++)                        
            //                //for (int xx = -2; xx <= 1; xx++)
            //                {
            //                    double dx = Math.Abs(rx - (xx + xKijun + 0.5));
            //                    double xw = GetLanczosWeight(dx, n);
            //                    int xc = xKijun + xx;
            //                    xc = xc < 0 ? 0 : xc > sourceWidth - 1 ? sourceWidth - 1 : xc;
            //                    double weight = yw * xw;
            //                    int pp = (yc * sourceStride) + (xc * pByte);
            //                    vv += sourcePixels[pp] * weight;
            //                }
            //            }
            //            //0～255の範囲を超えることがあるので、修正
            //            vv = vv < 0 ? 0 : vv > 255 ? 255 : vv;
            //            int ap = (y * stride) + (x * pByte);
            //            pixels[ap] = (byte)(vv + 0.5);
            //        }
            //    };

            //    BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, source.Format, null, pixels, stride);
            //    return bitmap;

            //    //double[] GetWS(double rx,double ry,int n)
            //    //{
            //    //    double sx = rx - (int)rx;
            //    //    double sy = ry - (int)ry;
            //    //    double dx = (sx < 0.5) ? 0.5 - sx : 0.5 - sx + 1;
            //    //    double dy = (sy < 0.5) ? 0.5 - sy : 0.5 - sy + 1;
            //    //    double[] xw = new double[n];
            //    //    for (int x = -n; x < n; x++)
            //    //    {
            //    //        xw[x]=()
            //    //    }
            //}
        }

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


        ///// <summary>
        ///// 市松模様の元になる画像作成、2色を2マスずつ合計4マス交互に並べた画像、
        ///// □■
        ///// ■□
        ///// </summary>
        ///// <param name="cellSize">1マスの1辺の長さ、作成される画像はこれの2倍の1辺になる</param>
        ///// <param name="c1">色1</param>
        ///// <param name="c2">色2</param>
        ///// <returns>画像のピクセルフォーマットはBgra32</returns>
        //private WriteableBitmap MakeCheckeredPattern(int cellSize, Color c1, Color c2)
        //{
        //    int width = cellSize * 2;
        //    int height = cellSize * 2;
        //    var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        //    int stride = 4 * width;// wb.Format.BitsPerPixel / 8 * width;
        //    byte[] pixels = new byte[stride * height];
        //    //すべてを1色目で塗る
        //    for (int i = 0; i < pixels.Length; i += 4)
        //    {
        //        pixels[i] = c1.B;
        //        pixels[i + 1] = c1.G;
        //        pixels[i + 2] = c1.R;
        //        pixels[i + 3] = c1.A;
        //    }

        //    //2色目で市松模様にする
        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            //左上と右下に塗る
        //            if ((y < cellSize & x < cellSize) | (y >= cellSize & x >= cellSize))
        //            {
        //                int p = y * stride + x * 4;
        //                pixels[p] = c2.B;
        //                pixels[p + 1] = c2.G;
        //                pixels[p + 2] = c2.R;
        //                pixels[p + 3] = c2.A;
        //            }
        //        }
        //    }
        //    wb.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        //    return wb;
        //}

        ///// <summary>
        ///// BitmapからImageBrush作成
        ///// 引き伸ばし無しでタイル状に敷き詰め
        ///// </summary>
        ///// <param name="bitmap"></param>
        ///// <returns></returns>
        //private ImageBrush MakeTileBrush(BitmapSource bitmap)
        //{
        //    var imgBrush = new ImageBrush(bitmap);
        //    imgBrush.Stretch = Stretch.None;//これは必要ないかも
        //    //タイルモード、タイル
        //    imgBrush.TileMode = TileMode.Tile;
        //    //タイルサイズは元画像のサイズ
        //    imgBrush.Viewport = new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
        //    //タイルサイズ指定方法は絶対値、これで引き伸ばされない
        //    imgBrush.ViewportUnits = BrushMappingMode.Absolute;
        //    return imgBrush;
        //}



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
            //MyBitmapOrigin = MakeBitmapSourceBgr24FromFile(paths[0]);
            //MyImage.Source = MyBitmapOrigin;
            //MyBitmapOrigin32bit = MakeBitmapSourceBgra32FromFile(paths[0]);
            //MyImage.Source = MyBitmapOrigin32bit;
        }

        //ボタンクリック
        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe(LanczosGray8, MyBitmapOrigin, yoko, tate, (int)MySlider.Value);
        }


        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth * MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight * MySliderScale.Value);
            MyExe(LanczosGray8, MyBitmapOrigin, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton3_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / MySliderScale.Value);
            MyExe(LanczosGray8Kai, MyBitmapOrigin, yoko, tate, (int)MySlider.Value);
        }

        private void MyButton4_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth * MySliderScale.Value);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight * MySliderScale.Value);
            MyExe(LanczosGray8Kai, MyBitmapOrigin, yoko, tate, (int)MySlider.Value);
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
                FormatConvertedBitmap bitmap = new(img, PixelFormats.Gray8, null, 0);
                MyBitmapOrigin = bitmap;
                MyImage.Source = bitmap;

                //FormatConvertedBitmap bitmap = new(img, PixelFormats.Bgr24, null, 0);
                //MyBitmapOrigin = bitmap;
                //FormatConvertedBitmap bitmap32 = new(img, PixelFormats.Bgra32, null, 0);
                //MyImage.Source = bitmap32;
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
            MyImage.Source = MyBitmapOrigin;
            //MyImage.Source = MyBitmapOrigin32bit;
        }


        #endregion コピペ
    }
}