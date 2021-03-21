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

namespace _20210321_画像読み込み
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //MyImage.Loaded += (s, e) => { RenderOptions.SetBitmapScalingMode(MyImage, BitmapScalingMode.NearestNeighbor); };
        }

        //ドロップされたファイル群の先頭ファイルから画像読み込み
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            var datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            var paths = datas.ToList();
            //昇順ソート
            paths.Sort();

            //BitmapSource source = MakeBitmapSourceFromFile(paths[0]);
            //BitmapSource source = MakeBitmapSourceFromFile(paths[0], 48, 48);
            //BitmapSource source = MakeBitmapSourceFromFile(paths[0], 120, 120);            
            //BitmapSource source = MakeBitmapSourceFromFile(paths[0], PixelFormats.Indexed2);
            BitmapSource source = MakeBitmapSourceFromFile(paths[0], PixelFormats.Indexed2, 192, 192);
            //BitmapSource source = MakeBitmapSourceFromFile(paths[0], PixelFormats.Indexed2, 48, 48);
            //BitmapSource source = MakeBitmapSourceFromFile(paths[0], PixelFormats.Gray8);
            //BitmapSource source = MakeBitmapSourceBgra32FromFile(paths[0]);

            source.Freeze();//要る？
            MyImage.Source = source;
            MyStatus.Content = $"dpi = {source.DpiX}, PixelFormat = {source.Format}";
        }

        /// <summary>
        /// PixelFormatsやdpiなどは元の画像のまま画像読み込み
        /// </summary>
        /// <param name="filePath">フルパス</param>
        /// <returns></returns>
        private BitmapSource MakeBitmapSourceFromFile(string filePath)
        {
            BitmapSource source = null;
            try
            {
                //using (var stream = System.IO.File.OpenRead(filePath))
                //{
                //    source = BitmapFrame.Create(stream);
                //}
                //↑この時点では画像取得できているけど、returnのところでは真っ白になる
                //↓はBitmapCacheOption.OnLoadを指定、これなら期待通りに取得できる
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }

                //以前の方法、FileStreamクラスを使っていた
                //using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                //{
                //    source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                //};
            }
            catch (Exception)
            { }
            return source;
        }

        //
        /// <summary>
        /// dpiを指定してファイルから画像読み込み
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dpiX"></param>
        /// <param name="dpiY"></param>
        /// <returns></returns>
        private BitmapSource MakeBitmapSourceFromFile(string filePath, double dpiX = 96, double dpiY = 96)
        {
            BitmapSource source = null;
            try
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    var frame = BitmapFrame.Create(stream);
                    int w = frame.PixelWidth;
                    int h = frame.PixelHeight;
                    int stride = (w * frame.Format.BitsPerPixel + 7) / 8;
                    var pixels = new byte[h * stride];
                    frame.CopyPixels(pixels, stride, 0);
                    source = BitmapSource.Create(w, h, dpiX, dpiY, frame.Format, frame.Palette, pixels, stride);
                };
            }
            catch (Exception)
            { }
            return source;
        }


        /// <summary>
        /// PixelFormatとdpiを指定してファイルから画像読み込み
        /// </summary>
        /// <param name="filePath">フルパス</param>
        /// <param name="format">ピクセルフォーマット、画像と違ったときは指定フォーマットにコンバートする</param>
        /// <param name="dpiX"></param>
        /// <param name="dpiY"></param>
        /// <returns></returns>
        private BitmapSource MakeBitmapSourceFromFile(string filePath, PixelFormat format, double dpiX = 96, double dpiY = 96)
        {
            BitmapSource source = null;
            try
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    source = BitmapFrame.Create(stream);
                    //画像と違ったときは指定フォーマットににコンバートする
                    if (source.Format != format)
                    {
                        source = new FormatConvertedBitmap(source, format, null, 0);
                    }
                    int w = source.PixelWidth;
                    int h = source.PixelHeight;
                    int stride = (w * source.Format.BitsPerPixel + 7) / 8;
                    var pixels = new byte[h * stride];
                    source.CopyPixels(pixels, stride, 0);
                    source = BitmapSource.Create(w, h, dpiX, dpiY, format, source.Palette, pixels, stride);
                };
            }
            catch (Exception)
            { }
            return source;
        }


        //
        /// <summary>
        /// PixelFormatをBgar32固定、dpiは指定してファイルから画像読み込み
        /// </summary>
        /// <param name="filePath">フルパス</param>
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
                    var pixels = new byte[h * stride];
                    source.CopyPixels(pixels, stride, 0);
                    source = BitmapSource.Create(w, h, dpiX, dpiY, PixelFormats.Bgra32, source.Palette, pixels, stride);
                };
            }
            catch (Exception)
            { }
            return source;
        }


    }
}
