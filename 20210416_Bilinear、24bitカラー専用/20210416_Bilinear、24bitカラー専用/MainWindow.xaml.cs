using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

//C#、WPF、バイリニア法での画像の拡大縮小変換、24bitカラー対応とマルチスレッド対応版 - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2021/04/16/201514

namespace _20210416_Bilinear_24bitカラー専用
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
        /// 画像の拡大縮小、バイリニア法で補完(PixelFormats.Bgr24専用)
        /// </summary>
        /// <param name="source">PixelFormats.Bgr24のBitmap</param>
        /// <param name="yoko">横ピクセル数を指定</param>
        /// <param name="tate">縦ピクセル数を指定</param>
        /// <returns></returns>
        private BitmapSource BilinearBgr24専用(BitmapSource source, int yoko, int tate)
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
            //1ピクセルあたりのバイト数、Byte / Pixel
            int pByte = (source.Format.BitsPerPixel + 7) / 8;

            int scaledStride = yoko * pByte;
            byte[] resultPixels = new byte[tate * scaledStride];

            _ = Parallel.For(0, tate, y =>
            {
                for (int x = 0; x < yoko; x++)
                {
                    //参照範囲の左上座標bp
                    double bpX = ((x + 0.5) * yokoScale) - 0.5;
                    //画像範囲内チェック、参照範囲が画像から外れていたら修正(収める)
                    if (bpX < 0) { bpX = 0; }
                    if (bpX > sourceWidth - 1) { bpX = sourceWidth - 1; }

                    double bpY = (y + 0.5) * tateScale - 0.5;
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
                    int i = ((int)bpY * stride) + ((int)bpX * pByte);

                    //値*面積
                    double aBlue = pixels[i] * a;
                    double aGreen = pixels[i + 1] * a;
                    double aRed = pixels[i + 2] * a;

                    double bBlue = 0;
                    double bGreen = 0;
                    double bRed = 0;

                    double cBlue = 0;
                    double cGreen = 0;
                    double cRed = 0;
                    
                    double dBlue = 0;
                    double dGreen = 0;
                    double dRed = 0;                    

                    //B区以降は面積が0より大きいときだけ計算
                    if (b != 0)
                    {
                        //Aの右ピクセル*Bの面積
                        bBlue = pixels[i + pByte] * b;
                        bGreen = pixels[i + pByte + 1] * b;
                        bRed = pixels[i + pByte + 2] * b;                        
                    }
                    if (c != 0)
                    {
                        cBlue = pixels[i + stride] * c;
                        cGreen = pixels[i + stride + 1] * c;
                        cRed = pixels[i + stride + 2] * c;

                    }
                    if (d != 0)
                    {
                        //Aの右下ピクセル、仮にAが画像右下ピクセルだったとしても
                        //そのときは面積が0のはずだからここは計算されない
                        dBlue = pixels[i + stride + pByte] * d;
                        dGreen = pixels[i + stride + pByte + 1] * d;
                        dRed = pixels[i + stride + pByte + 2] * d;
                    }

                    //4区を合計して四捨五入で完成
                    resultPixels[y * scaledStride + x * pByte] = (byte)(aBlue + bBlue + cBlue + dBlue + 0.5);
                    resultPixels[y * scaledStride + x * pByte + 1] = (byte)(aGreen + bGreen + cGreen + dGreen + 0.5);
                    resultPixels[y * scaledStride + x * pByte + 2] = (byte)(aRed + bRed + cRed + dRed + 0.5);
                }
            });

            BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
            return bitmap;
        }





        //E:\オレ\エクセル\画像処理.xlsm_バイリニア法_$A$599
        //縮小専用
        /// <summary>
        /// 画像の縮小、バイリニア法で補完、PixelFormats.Bgr24専用)
        /// </summary>
        /// <param name="source">PixelFormats.Bgr24のBitmap</param>
        /// <param name="yoko">変換後の横ピクセル数を指定</param>
        /// <param name="tate">変換後の縦ピクセル数を指定</param>
        /// <returns></returns>
        private BitmapSource BilinearBgr24縮小専用(BitmapSource source, int yoko, int tate)
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
            //1ピクセルあたりのバイト数、Byte / Pixel
            int pByte = (source.Format.BitsPerPixel + 7) / 8;

            int scaledStride = yoko * pByte;
            byte[] resultPixels = new byte[tate * scaledStride];

            _ = Parallel.For(0, tate, y =>
              {
                  for (int x = 0; x < yoko; x++)
                  {
                      //参照範囲の左上座標bp
                      double bpX = ((x + 0.5) * yokoScale) - 0.5;
                      double bpY = ((y + 0.5) * tateScale) - 0.5;

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
                      int i = ((int)bpY * stride) + ((int)bpX * pByte);

                      //各区の値*面積の合計を四捨五入して完成
                      //Blue
                      resultPixels[y * scaledStride + x * pByte] =
                            (byte)(pixels[i] * a
                            + pixels[i + pByte] * b
                            + pixels[i + stride] * c
                            + pixels[i + stride + pByte] * d
                            + 0.5);
                      //Green
                      resultPixels[y * scaledStride + x * pByte + 1] =
                            (byte)(pixels[i + 1] * a
                            + pixels[i + pByte + 1] * b
                            + pixels[i + stride + 1] * c
                            + pixels[i + stride + pByte + 1] * d
                            + 0.5);
                      //Red
                      resultPixels[y * scaledStride + x * pByte + 2] =
                            (byte)(pixels[i + 2] * a
                            + pixels[i + pByte + 2] * b
                            + pixels[i + stride + 2] * c
                            + pixels[i + stride + pByte + 2] * d
                            + 0.5);
                  }
              });


            BitmapSource bitmap = BitmapSource.Create(yoko, tate, 96, 96, source.Format, null, resultPixels, scaledStride);
            return bitmap;
        }




        /// <summary>
        /// 画像ファイルパスからPixelFormats.Bgr24のBitmapSource作成
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
            MyImage.Source = BilinearBgr24縮小専用(MyBitmapOrigin, yoko, tate);
        }


        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            int yoko = (int)Math.Ceiling(MyBitmapOrigin.PixelWidth / 3.0);
            int tate = (int)Math.Ceiling(MyBitmapOrigin.PixelHeight / 3.0);
            MyImage.Source = BilinearBgr24専用(MyBitmapOrigin, yoko, tate);
        }

        private void MyButton3_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = BilinearBgr24専用(MyBitmapOrigin,
                MyBitmapOrigin.PixelWidth * 2,
                MyBitmapOrigin.PixelHeight * 2);
        }

        private void MyButton4_Click(object sender, RoutedEventArgs e)
        {
            MyImage.Source = BilinearBgr24専用(MyBitmapOrigin,
                MyBitmapOrigin.PixelWidth * 3,
                MyBitmapOrigin.PixelHeight * 3);
        }

        //画像をクリップボードにコピー
        private void MyButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage((BitmapSource)MyImage.Source);            
        }

        //クリップボードから画像追加
        private void MyButtonPaste_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource img = Clipboard.GetImage();
            if (img != null)
            {
                MyBitmapOrigin = img;
                MyImage.Source = img;
            }
        }
    }
}
