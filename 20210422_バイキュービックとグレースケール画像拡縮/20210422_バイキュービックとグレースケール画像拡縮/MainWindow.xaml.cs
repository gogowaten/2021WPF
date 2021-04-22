using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

namespace _20210422_バイキュービックとグレースケール画像拡縮
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
            //this.Background = MakeTileBrush(MakeCheckeredPattern(10, Colors.WhiteSmoke, Colors.LightGray));
        }



        /// <summary>
        /// バイキュービックをつかった重み計算
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
        /// 画像の縮小、バイキュービック法で補完、PixelFormats.Gray8専用)
        /// a指定版
        /// </summary>
        /// <param name="source">PixelFormats.Gray8のBitmap</param>
        /// <param name="width">変換後の横ピクセル数を指定</param>
        /// <param name="height">変換後の縦ピクセル数を指定</param>
        /// <param name="a">-0.5～-1.0くらいを指定する、基準は-1.0、小さくするとシャープ、大きくするとぼかし</param>
        /// <returns></returns>
        private BitmapSource BicubicGray8Test(BitmapSource source, int width, int height, double a = -1.0)
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
            MyImage.Source = BicubicGray8Test(MyBitmapOrigin, yoko, tate, MySlider.Value);
        }


        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / 3.0);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / 3.0);
            //MyImage.Source = BicubicGray8Test(MyBitmapOrigin, yoko, 3, MySlider.Value);
            MyImage.Source = BicubicGray8Test(MyBitmapOrigin, yoko, tate, MySlider.Value);
        }

        private void MyButton3_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = BicubicGray8Test(MyBitmapOrigin,
                MyBitmapOrigin.PixelWidth * 2,
                MyBitmapOrigin.PixelHeight * 2,
                MySlider.Value);
        }

        private void MyButton4_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = BicubicGray8Test(MyBitmapOrigin,
                MyBitmapOrigin.PixelWidth * 3,
                MyBitmapOrigin.PixelHeight * 3,
                MySlider.Value);
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

        private void MySlider_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0) MySlider.Value += MySlider.SmallChange;
            else MySlider.Value -= MySlider.SmallChange;
        }
    

        private void MyButtonToOrigin_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = MyBitmapOrigin;
        }


        #endregion コピペ
    }
}