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
            MyImage.Source = Bitmap640x480(MyBitmapOrigin32bit);
        }



        #endregion コピペ
    }
}