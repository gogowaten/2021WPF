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

namespace _20210318_画像連結テスト
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Image> MyImages = new();
        private double MyImageSize = 200;


        public MainWindow()
        {
            InitializeComponent();

            this.Top = 0;
            this.Left = 0;

        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            var datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            var paths = datas.ToList();
            paths.Sort();

            for (int i = 0; i < paths.Count; i++)
            {
                //BitmapSource source = FileToBitmapSource(paths[i],PixelFormats.Indexed2);                
                BitmapSource source = FileToBitmapSourceBgra32(paths[i]);
                Image img = new()
                {
                    Source = source,
                    Stretch = Stretch.Uniform,
                    Width = MyImageSize,
                    Height = MyImageSize,
                };
                source.Freeze();
                MyImages.Add(img);
                MyPanel.Children.Add(img);

            }

        }


        ///// <summary>
        ///// PixelFormatsやdpiなどは元の画像のまま画像読み込み
        ///// </summary>
        ///// <param name="filePath">フルパス</param>
        ///// <returns></returns>
        //private BitmapSource FileToBitmapSource(string filePath)
        //{
        //    BitmapSource source = null;
        //    try
        //    {
        //        using (var stream = System.IO.File.OpenRead(filePath))
        //        {
        //            source = BitmapFrame.Create(stream);
        //        }
        //        //using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        //        //{
        //        //    source = BitmapFrame.Create(stream);
        //        //};
        //    }
        //    catch (Exception)
        //    { }
        //    return source;
        //}

        //
        /// <summary>
        /// dpiを指定してファイルから画像読み込み
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dpiX"></param>
        /// <param name="dpiY"></param>
        /// <returns></returns>
        private BitmapSource FileToBitmapSource(string filePath, double dpiX = 96, double dpiY = 96)
        {
            BitmapSource source = null;
            try
            {
                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
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
        private BitmapSource FileToBitmapSource(string filePath, PixelFormat format, double dpiX = 96, double dpiY = 96)
        {
            BitmapSource source = null;
            try
            {
                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
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
        private BitmapSource FileToBitmapSourceBgra32(string filePath, double dpiX = 96, double dpiY = 96)
        {
            BitmapSource source = null;
            try
            {
                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
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


        private void MyButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }
        private void SaveFile()
        {
            //描画する座標とサイズを取得
            List<Rect> drawRects = MakeRects();

            DrawingVisual dv = new();
            using (var dc = dv.RenderOpen())
            {
                for (int i = 0; i < MyImages.Count; i++)
                {
                    BitmapSource source = MyImages[i].Source as BitmapSource;
                    dc.DrawImage(source, drawRects[i]);
                }
            }
            //最終的な全体画像サイズ計算、RectのUnionを使う
            Rect dRect = new();
            for (int i = 0; i < drawRects.Count; i++)
            {
                dRect = Rect.Union(dRect, drawRects[i]);
            }
            int width = (int)dRect.Width;
            int height = (int)dRect.Height;
            RenderTargetBitmap render = new(width, height, 96, 96, PixelFormats.Pbgra32);
            render.Render(dv);
            SaveBitmapToPngFile(render, MakeSavePath());
        }

        //描画サイズと座標の計算
        //E:\オレ\エクセル\作りたいアプリのメモ.xlsm_2021031_$A$214
        private List<Rect> MakeRects()
        {
            List<Rect> drawRects = new();
            //横に並べる個数
            int MasuYoko = (int)(MyPanel.Width / MyImageSize);
            if (MasuYoko == 0) MasuYoko = 1;
            int imageCount = MyImages.Count;
            //サイズとX座標
            //指定横幅に縮小、アスペクト比は保持
            for (int i = 0; i < imageCount; i++)
            {
                //サイズ
                BitmapSource bmp = MyImages[i].Source as BitmapSource;
                double width = bmp.PixelWidth;
                double ratio = MySliderCellWidth.Value / width;
                if (ratio > 1) ratio = 1;
                width *= ratio;

                //X座標、中央揃え
                double x = (i % MasuYoko) * MySliderCellWidth.Value;
                x = x + (MySliderCellWidth.Value - width) / 2;

                //Y座標は後で計算
                drawRects.Add(new(x, 0, width, bmp.PixelHeight * ratio));
            }

            //Y座標計算
            //Y座標はその行にある画像の中で最大の高さを求めて、中央揃えのY座標を計算
            //行ごとに計算する必要がある

            //今の行の基準Y座標、次の行へは今の行の高さを加算していく
            double kijun = 0;
            int count = 0;
            while (count < imageCount)
            {
                int end = count + MasuYoko;
                if (end > imageCount) end = imageCount;
                //Y座標計算
                kijun += SubFunc(count, end, kijun);
                //横に並べる個数が3なら0 3 6…
                count += MasuYoko;
            }

            //Y座標計算
            //開始と終了Index指定、基準値
            double SubFunc(int begin, int end, double kijun)
            {
                //行の高さを求める(最大の画像が収まる)
                double max = 0;
                for (int i = begin; i < end; i++)
                {
                    if (drawRects[i].Height > max) max = drawRects[i].Height;
                }
                //Y座標 = 基準値 + (行の高さ - 画像の高さ) / 2
                for (int i = begin; i < end; i++)
                {
                    Rect temp = drawRects[i];
                    temp.Y = kijun + (max - drawRects[i].Height) / 2;
                    drawRects[i] = temp;
                }
                return max;
            }
            return drawRects;
        }


        private string MakeSavePath()
        {
            DateTime time = DateTime.Now;
            string ts = time.ToString("yyyyMMdd_hhmmssfff");
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = System.IO.Path.Combine(path, ts);
            path += ".png";
            return path;
        }
        private void SaveBitmapToPngFile(BitmapSource source, string path)
        {
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(source));
            using (var fs = new System.IO.FileStream(
                path, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write))
            {
                encoder.Save(fs);
            }
        }





    }
}
