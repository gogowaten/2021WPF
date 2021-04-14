using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

//画像を拡大縮小、バイリニア法で補完、
//グレースケール専用(PixelFormats.Gray8専用)
//カラー画像をドロップした場合はグレースケール画像に変換する
//倍率は1/2、1/3、2、3
namespace _20210414_Bilinearで画像拡大縮小
{
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

        //縮小拡大対応完成版
        /// <summary>
        /// バイリニア法で画像の拡大縮小、グレースケール専用(PixelFormats.Gray8専用)
        /// </summary>
        /// <param name="source">PixelFormats.Gray8のBitmap</param>
        /// <param name="yoko">横ピクセル数を指定</param>
        /// <param name="tate">縦ピクセル数を指定</param>
        /// <returns></returns>
        private BitmapSource BilinearGray8専用(BitmapSource source, int yoko, int tate)
        {
            //元画像の画素値の配列作成
            int sourceWidth = source.PixelWidth;
            int sourceHeight = source.PixelHeight;
            int stride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[sourceHeight * stride];
            source.CopyPixels(pixels, stride, 0);

            //縮小後の画像の画素値の配列用
            double yokoScale = (double)sourceWidth / yoko;//横倍率
            double tateScale = (double)sourceHeight / tate;
            int scaledStride = (yoko * source.Format.BitsPerPixel + 7) / 8;
            byte[] resultPixels = new byte[tate * scaledStride];

            for (int y = 0; y < tate; y++)
            {
                for (int x = 0; x < yoko; x++)
                {
                    //参照点r
                    double rx = (x + 0.5) * yokoScale;
                    double ry = (y + 0.5) * tateScale;

                    //参照範囲の左上座標bp
                    double bpX = rx - 0.5;
                    //画像範囲内チェック、参照範囲が画像から外れていたら修正(収める)
                    if (bpX < 0) { bpX = 0; }
                    if (bpX > sourceWidth - 1) { bpX = sourceWidth - 1; }

                    double bpY = ry - 0.5;
                    if (bpY < 0) { bpY = 0; }
                    if (bpY > sourceHeight - 1) { bpY = sourceHeight - 1; }

                    //小数部分s
                    double sx = bpX % 1;
                    double sy = bpY % 1;

                    //面積
                    double d = sx * sy;
                    double c = (1 - sx) * sy;
                    double b = sx * (1 - sy);
                    double a = 1 - (d + c + b);// (1 - sx) * (1 - sy)

                    //左上ピクセルの座標は
                    //参照範囲の左上座標の小数部分を切り捨て(整数部分)
                    //左上ピクセルのIndex
                    int ia = ((int)bpY * stride) + (int)bpX;

                    //値*面積
                    double aa = pixels[ia] * a;
                    double bb = 0, cc = 0, dd = 0;
                    //B以降は面積が0より大きいときだけ計算
                    if (b != 0)
                    {
                        //Aの右ピクセル*Bの面積
                        bb = pixels[ia + 1] * b;
                    }
                    if (c != 0)
                    {
                        cc = pixels[ia + stride] * c;
                    }
                    if (d != 0)
                    {
                        //Aの右下ピクセル、仮にAが画像右下ピクセルだったとしても
                        //そのときは面積が0のはずだからここは計算されない
                        dd = pixels[ia + stride + 1] * d;
                    }


                    //4区を合計して四捨五入で完成
                    double add = aa + bb + cc + dd;
                    resultPixels[y * scaledStride + x] = (byte)(add + 0.5);

                }
            }
            BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
            return bitmap;
        }


        //E:\オレ\エクセル\画像処理.xlsm_バイリニア法_$A$599
        //縮小専用完成版
        /// <summary>
        /// バイリニア法で画像の縮小、グレースケール専用(PixelFormats.Gray8専用)
        /// </summary>
        /// <param name="source">PixelFormats.Gray8のBitmap</param>
        /// <param name="yoko">変換後の横ピクセル数を指定</param>
        /// <param name="tate">変換後の縦ピクセル数を指定</param>
        /// <returns></returns>
        private BitmapSource BilinearGray8縮小専用(BitmapSource source, int yoko, int tate)
        {
            //元画像の画素値の配列作成
            int sourceWidth = source.PixelWidth;
            int sourceHeight = source.PixelHeight;
            int stride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[sourceHeight * stride];
            source.CopyPixels(pixels, stride, 0);

            //縮小後の画像の画素値の配列用
            double yokoScale = (double)sourceWidth / yoko;//横倍率
            double tateScale = (double)sourceHeight / tate;
            int scaledStride = (yoko * source.Format.BitsPerPixel + 7) / 8;
            byte[] resultPixels = new byte[tate * scaledStride];

            for (int y = 0; y < tate; y++)
            {
                for (int x = 0; x < yoko; x++)
                {
                    //参照点r
                    double rx = (x + 0.5) * yokoScale;
                    double ry = (y + 0.5) * tateScale;

                    //参照範囲の左上座標bp
                    double bpX = rx - 0.5;
                    double bpY = ry - 0.5;

                    //小数部分s
                    double sx = bpX % 1;
                    double sy = bpY % 1;

                    ////面積
                    double d = sx * sy;
                    double c = (1 - sx) * sy;
                    double b = sx * (1 - sy);
                    double a = 1 - (d + c + b);// (1 - sx) * (1 - sy)


                    //左上ピクセルの座標は
                    //参照範囲の左上座標の小数部分を切り捨て(整数部分)
                    //左上ピクセルのIndex
                    int i = ((int)bpY * stride) + (int)bpX;

                    //値*面積
                    double aa = pixels[i] * a;
                    double bb = pixels[i + 1] * b;
                    double cc = pixels[i + stride] * c;
                    double dd = pixels[i + stride + 1] * d;

                    //4区を合計して四捨五入で完成
                    double add = aa + bb + cc + dd;
                    resultPixels[y * scaledStride + x] = (byte)(add + 0.5);

                }
            }
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
            MyImage.Source = BilinearGray8専用(MyBitmapOrigin, yoko, tate);
        }


        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / 3.0);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / 3.0);
            MyImage.Source = BilinearGray8専用(MyBitmapOrigin, yoko, tate);
        }

        private void MyButton3_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = BilinearGray8専用(MyBitmapOrigin,
                MyBitmapOrigin.PixelWidth * 2,
                MyBitmapOrigin.PixelHeight * 2);
        }

        private void MyButton4_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = BilinearGray8専用(MyBitmapOrigin,
                MyBitmapOrigin.PixelWidth * 3,
                MyBitmapOrigin.PixelHeight * 3);
        }

        //画像をクリップボードにコピー
        private void MyButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage((BitmapSource)MyImage.Source);
        }
    }
}
