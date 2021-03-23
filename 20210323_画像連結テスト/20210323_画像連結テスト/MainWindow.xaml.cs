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

using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using System.Globalization;

namespace _20210323_画像連結テスト
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Data MyData;
        private ObservableCollection<ImageThumb> MyThumbs = new();
        private List<Point> MyLocate = new();

        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();
        }
        private void MyInitialize()
        {
            this.Left = 0;
            this.Top = 0;
            MyData = new() { Col = 2, Row = 3, Size = 80 };
            this.DataContext = MyData;


        }


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

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            var datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            var paths = datas.ToList();
            paths.Sort();

            for (int i = 0; i < paths.Count; i++)
            {
                BitmapSource source = MakeBitmapSourceBgra32FromFile(paths[i]);
                Image img = new() { Source = source, StretchDirection = StretchDirection.DownOnly };
                img.Width = MyData.Size;
                img.Height = MyData.Size;

                int x = i % MyData.Col * MyData.Size;
                int y = (int)((double)i / MyData.Col) * MyData.Size;
                MyLocate.Add(new Point(x, y));
                ImageThumb thumb = new(img, 0, 0, x, y);
                thumb.DragStarted += (s, e) =>
                {
                    thumb.Opacity = 0.5;
                    Panel.SetZIndex(thumb, MyThumbs.Count);
                };
                thumb.DragDelta += Thumb_DragDelta;
                thumb.DragCompleted += (s, e) =>
                {
                    thumb.Opacity = 1.0;
                    int index = MyThumbs.IndexOf(thumb);
                    Panel.SetZIndex(thumb, index);
                    thumb.SetLocateTopLeft(MyLocate[index]);
                };
                thumb.Width = MyData.Size;
                thumb.Height = MyData.Size;
                Panel.SetZIndex(thumb, i);
                MyCanvas.Children.Add(thumb);
                MyThumbs.Add(thumb);
            }
            Panel.SetZIndex(MyRectangle, MyThumbs.Count + 1);
            SetMyCanvasSize();
        }

        #region 保存
        private void SaveFile()
        {
            //描画する座標とサイズを取得
            List<Rect> drawRects = MakeRects();

            DrawingVisual dv = new();
            using (var dc = dv.RenderOpen())
            {
                for (int i = 0; i < drawRects.Count; i++)
                {
                    BitmapSource source = MyThumbs[i].MyImage.Source as BitmapSource;
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
            int MasuYoko = MyData.Col;

            int saveImageCount = MyData.Row * MyData.Col;
            if (saveImageCount > MyThumbs.Count) saveImageCount = MyThumbs.Count;

            //サイズとX座標
            //指定横幅に縮小、アスペクト比は保持
            for (int i = 0; i < saveImageCount; i++)
            {
                //サイズ
                BitmapSource bmp = MyThumbs[i].MyImage.Source as BitmapSource;
                double width = bmp.PixelWidth;
                double ratio = MyData.Size / width;
                if (ratio > 1) ratio = 1;
                width *= ratio;

                //X座標、中央揃え
                double x = (i % MasuYoko) * MyData.Size;
                x = x + (MyData.Size - width) / 2;

                //Y座標は後で計算
                drawRects.Add(new(x, 0, width, bmp.PixelHeight * ratio));
            }

            //Y座標計算
            //Y座標はその行にある画像の中で最大の高さを求めて、中央揃えのY座標を計算
            //行ごとに計算する必要がある

            //今の行の基準Y座標、次の行へは今の行の高さを加算していく
            double kijun = 0;
            int count = 0;
            while (count < saveImageCount)
            {
                int end = count + MasuYoko;
                if (end > saveImageCount) end = saveImageCount;
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
            string ts = time.ToString("yyyyMMdd_HHmmssfff");
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

        #endregion 保存

        /// <summary>
        /// ドラッグ移動中のThumbとその他のThumbとの重なり合う部分の面積を計算、
        /// 一定以上の面積があった場合、場所を入れ替えて整列
        /// </summary>
        /// <param name="t">ドラッグ移動中のThumb</param>
        /// <param name="x">Canvas上でのX座標</param>
        /// <param name="y">Canvas上でのY座標</param>
        private void Idou移動中処理(ImageThumb t, double x, double y)
        {
            int imaIndex = MyThumbs.IndexOf(t);//ドラッグ移動中ThumbのIndex            

            //最寄りのPoint
            int moyoriIndex = 0;
            double moyori距離 = double.MaxValue;
            for (int i = 0; i < MyLocate.Count; i++)
            {
                double distance = GetDistance(MyLocate[i], new Point(x, y));
                if (distance < moyori距離)
                {
                    moyori距離 = distance;
                    moyoriIndex = i;
                }
            }

            //最短距離のIndexと移動中のThumbのIndexが違うなら入れ替え処理
            if (moyoriIndex != imaIndex)
            {
                //Thumbリストのindexを入れ替え
                MyThumbs.RemoveAt(imaIndex);
                MyThumbs.Insert(moyoriIndex, t);

                //indexに従って表示位置変更
                SetLocate(moyoriIndex);
            }
        }

        //ドラッグ移動イベント時
        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            //移動
            ImageThumb t = sender as ImageThumb;
            double x = Canvas.GetLeft(t) + e.HorizontalChange;
            double y = Canvas.GetTop(t) + e.VerticalChange;
            Canvas.SetLeft(t, x);
            Canvas.SetTop(t, y);
            t.Opacity = 0.5;

            Idou移動中処理(t, x, y);
        }

        //2点間距離
        private double GetDistance(Point a, Point b)
        {
            return Math.Sqrt((a - b) * (a - b));
        }


        //座標リスト更新
        private void ChangeLocate()
        {
            for (int i = 0; i < MyThumbs.Count; i++)
            {
                int x = i % MyData.Col * MyData.Size;
                int y = (int)((double)i / MyData.Col) * MyData.Size;
                MyLocate[i] = new Point(x, y);
            }
        }
        //
        /// <summary>
        /// すべてのThumbを再配置、移動中のThumbは変更しない
        /// </summary>
        /// <param name="avoidIndex">位置変更したくないThumbのIndex</param>
        private void SetLocate(int avoidIndex = -1)
        {
            for (int i = 0; i < avoidIndex; i++)
            {
                MyThumbs[i].SetLocateTopLeft(MyLocate[i]);
            }
            for (int i = avoidIndex + 1; i < MyThumbs.Count; i++)
            {
                MyThumbs[i].SetLocateTopLeft(MyLocate[i]);
            }
        }
        private void ChangeSize()
        {
            for (int i = 0; i < MyThumbs.Count; i++)
            {
                ImageThumb t = MyThumbs[i];
                int size = MyData.Size;
                t.Width = size;
                t.Height = size;
                t.MyImage.Width = size;
                t.MyImage.Height = size;
            }
        }
        private void SetMyCanvasSize()
        {
            if (MyThumbs.Count == 0) return;
            int c = MyData.Col;
            int r = MyData.Row;
            int size = MyData.Size;
            int w = c * size;
            int h = r * size;
            int hh = (int)Math.Ceiling((double)MyThumbs.Count / c) * size;
            if (hh > h) h = hh;

            MyCanvas.Width = w;
            MyCanvas.Height = h;

        }


        private void MyButtonTest_Click(object sender, RoutedEventArgs e)
        {
            MyData.Row = 3;
            var neko = MyRectangle.Width;
            var t = MyThumbs[0];

        }

        private void MySliderSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ChangeLocate();
            SetLocate();
            ChangeSize();
            SetMyCanvasSize();
        }
        private void MySliderCol_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ChangeLocate();
            SetLocate();
            SetMyCanvasSize();
        }

        private void MySliderRow_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ChangeLocate();
            SetLocate();
            SetMyCanvasSize();
        }

        private void MyButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }
    }



    public class ImageThumb : Thumb
    {
        private Canvas MyPanel;
        public Image MyImage;

        public ImageThumb(Image img, int eX = 0, int eY = 0, int x = 0, int y = 0) : this()
        {
            MyPanel.Children.Add(img);
            MyImage = img;

            Canvas.SetLeft(img, eX);
            Canvas.SetTop(img, eY);

            SetLocateTopLeft(new Point(x, y));

        }
        public ImageThumb()
        {
            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Canvas), "panel");
            this.Template = template;
            this.ApplyTemplate();
            MyPanel = (Canvas)template.FindName("panel", this);
            MyPanel.Background = Brushes.Aqua;
            SetLocateTopLeft(new Point(0, 0));
        }
        public void SetLocateTopLeft(Point p)
        {
            Canvas.SetLeft(this, p.X);
            Canvas.SetTop(this, p.Y);

        }

    }



    public class ConveterRectangleSize : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double i = (double)values[0];
            double size = (double)values[1];
            return i * size;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class Data
    {
        public int Row { get; set; } = 2;
        public int Col { get; set; } = 2;
        public int Size { get; set; } = 80;

    }

}
