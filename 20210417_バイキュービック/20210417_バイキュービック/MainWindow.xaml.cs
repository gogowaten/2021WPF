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


        ////縮小拡大対応完成版
        ///// <summary>
        ///// 画像の拡大縮小、バイリニア法で補完(PixelFormats.Bgra32専用)
        ///// </summary>
        ///// <param name="source">PixelFormats.Bgra32のBitmap</param>
        ///// <param name="yoko">横ピクセル数を指定</param>
        ///// <param name="tate">縦ピクセル数を指定</param>
        ///// <returns></returns>
        //private BitmapSource BilinearBgra32専用(BitmapSource source, int yoko, int tate)
        //{
        //    //元画像の画素値の配列作成
        //    int sourceWidth = source.PixelWidth;
        //    int sourceHeight = source.PixelHeight;
        //    int stride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
        //    byte[] pixels = new byte[sourceHeight * stride];
        //    source.CopyPixels(pixels, stride, 0);

        //    //縮小後の画像の画素値の配列用
        //    double yokoScale = (double)sourceWidth / yoko;//横倍率
        //    double tateScale = (double)sourceHeight / tate;
        //    //1ピクセルあたりのバイト数、Byte / Pixel
        //    int pByte = (source.Format.BitsPerPixel + 7) / 8;

        //    int scaledStride = yoko * pByte;
        //    byte[] resultPixels = new byte[tate * scaledStride];
        //    Parallel.For(0, tate, y =>
        //    {
        //        for (int x = 0; x < yoko; x++)
        //        {
        //            //参照範囲の左上座標bp
        //            double bpX = ((x + 0.5) * yokoScale) - 0.5;
        //            //画像範囲内チェック、参照範囲が画像から外れていたら修正(収める)
        //            if (bpX < 0) { bpX = 0; }
        //            if (bpX > sourceWidth - 1) { bpX = sourceWidth - 1; }

        //            double bpY = (y + 0.5) * tateScale - 0.5;
        //            if (bpY < 0) { bpY = 0; }
        //            if (bpY > sourceHeight - 1) { bpY = sourceHeight - 1; }

        //            //小数部分s
        //            double sx = bpX % 1;
        //            double sy = bpY % 1;

        //            //面積
        //            double d = sx * sy;
        //            double c = (1 - sx) * sy;
        //            double b = sx * (1 - sy);
        //            double a = 1 - (d + c + b);// (1 - sx) * (1 - sy)

        //            //左上ピクセルの座標は
        //            //参照範囲の左上座標の小数部分を切り捨て(整数部分)
        //            //左上ピクセルのIndex
        //            int i = ((int)bpY * stride) + ((int)bpX * pByte);


        //            //値*面積
        //            double aBlue = pixels[i] * a;
        //            double aGreen = pixels[i + 1] * a;
        //            double aRed = pixels[i + 2] * a;
        //            double aAlpha = pixels[i + 3] * a;

        //            //Alphaが0の区画のRGB値は無視したいので初期値1.0から面積を引き算して
        //            //有効面積率を計算
        //            double effectiveAreaRatio = 1.0;
        //            if (pixels[i + 3] == 0) effectiveAreaRatio -= a;

        //            double bB = 0;
        //            double bG = 0;
        //            double bR = 0;
        //            double bA = 0;

        //            double cB = 0;
        //            double cG = 0;
        //            double cR = 0;
        //            double cA = 0;

        //            double dB = 0;
        //            double dG = 0;
        //            double dR = 0;
        //            double dA = 0;

        //            int pp;
        //            //B区以降は面積が0より大きいときだけ計算
        //            if (b != 0)
        //            {
        //                //Aの右ピクセル*Bの面積
        //                pp = i + pByte;
        //                bB = pixels[pp] * b;
        //                bG = pixels[pp + 1] * b;
        //                bR = pixels[pp + 2] * b;
        //                bA = pixels[pp + 3] * b;
        //                if (pixels[pp + 3] == 0) effectiveAreaRatio -= b;
        //            }
        //            if (c != 0)
        //            {
        //                //下側ピクセル
        //                pp = i + stride;
        //                cB = pixels[pp] * c;
        //                cG = pixels[pp + 1] * c;
        //                cR = pixels[pp + 2] * c;
        //                cA = pixels[pp + 3] * c;
        //                if (pixels[pp + 3] == 0) effectiveAreaRatio -= c;
        //            }
        //            if (d != 0)
        //            {
        //                //Aの右下ピクセル、仮にAが画像右下ピクセルだったとしても
        //                //そのときは面積が0のはずだからここは計算されない
        //                pp = i + stride + pByte;
        //                dB = pixels[pp] * d;
        //                dG = pixels[pp + 1] * d;
        //                dR = pixels[pp + 2] * d;
        //                dA = pixels[pp + 3] * d;
        //                if (pixels[pp + 3] == 0) effectiveAreaRatio -= d;
        //            }

        //            //Alpha0の面積によって倍率変更
        //            //有効面積率はRGBそれぞれに掛け算
        //            effectiveAreaRatio = 1 / effectiveAreaRatio;

        //            //4区を合計して四捨五入で完成
        //            resultPixels[(y * scaledStride) + (x * pByte)] = (byte)(((aBlue + bB + cB + dB) * effectiveAreaRatio) + 0.5);
        //            resultPixels[(y * scaledStride) + (x * pByte) + 1] = (byte)(((aGreen + bG + cG + dG) * effectiveAreaRatio) + 0.5);
        //            resultPixels[(y * scaledStride) + (x * pByte) + 2] = (byte)(((aRed + bR + cR + dR) * effectiveAreaRatio) + 0.5);
        //            resultPixels[(y * scaledStride) + (x * pByte) + 3] = (byte)(aAlpha + bA + cA + dA + 0.5);
        //        }
        //    });

        //    BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
        //    return bitmap;
        //}



        private void TTT(byte[] values, double sx, double sy)
        {

            for (int i = 0; i < 16; i++)
            {

            }
        }
        private static double Cubic(double d)
        {
            return d switch
            {
                2 => 0,
                <= 1 => 1 - 2 * (d * d) + d * d * d,
                < 2 => 4 - 8 * d + 5 * (d * d) - (d * d * d),
                _ => 0
            };
        }
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
                    int kLeft = (int)(rx - 0.5)-1;
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
                        Cubic(sx+1),Cubic(sx),Cubic(1-sx),Cubic(2-sx)
                    };
                    double[] yw = new double[4]
                    {
                        Cubic(sy+1),Cubic(sy),Cubic(1-sy),Cubic(2-sy)
                    };

                    int p = (int)(ry-0.5)* stride + (int)(rx-0.5);
                    
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
            MyImage.Source = BicubicGray8(MyBitmapOrigin, yoko, tate);
        }


        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / 3.0);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / 3.0);
            MyImage.Source = BicubicGray8(MyBitmapOrigin, yoko, tate);
        }

        private void MyButton3_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = BicubicGray8(MyBitmapOrigin,
                MyBitmapOrigin.PixelWidth * 2,
                MyBitmapOrigin.PixelHeight * 2);
        }

        private void MyButton4_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = BicubicGray8(MyBitmapOrigin,
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