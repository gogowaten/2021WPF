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

namespace _20210323_画像連結テスト
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Data MyData;
        private ObservableCollection<FlatThumb> MyThumbs = new();

        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();
        }
        private void MyInitialize()
        {
            this.Left = 0;
            this.Top = 0;
            MyData = new() { Col = 2, Row = 2 };
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
                img.Width = 100;
                img.Height = 100;

                int x = i % MyData.Col * 100;
                int y = i / MyData.Col * 100;
                FlatThumb thumb = new(img, 0, 0, x, y);
                thumb.DragStarted += (s, e) =>
                {
                    thumb.Opacity = 0.5;
                    Panel.SetZIndex(thumb, MyThumbs.Count);
                };
                thumb.DragDelta += Thumb_DragDelta;
                thumb.DragCompleted += (s, e) =>
                {
                    thumb.Opacity = 1.0;
                    Panel.SetZIndex(thumb, MyThumbs.IndexOf(thumb));
                };
                thumb.Width = 100;
                thumb.Height = 100;
                Panel.SetZIndex(thumb, i);
                MyCanvas.Children.Add(thumb);
                MyThumbs.Add(thumb);

            }
        }

        //ドラッグ移動イベント時
        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            //移動
            FlatThumb t = sender as FlatThumb;
            double x = Canvas.GetLeft(t) + e.HorizontalChange;
            double y = Canvas.GetTop(t) + e.VerticalChange;
            Canvas.SetLeft(t, x);
            Canvas.SetTop(t, y);
            t.Opacity = 0.5;

            //Idou移動中処理(t, x, y);
        }

        //2点間距離
        private double GetDistance(Point a, Point b)
        {
            return Math.Sqrt((a - b) * (a - b));
        }


        private void MyButtonTest_Click(object sender, RoutedEventArgs e)
        {
            MyData.Row = 3;
            var neko = MyData;

        }
    }



    public class FlatThumb : Thumb
    {
        private Canvas MyPanel;

        public FlatThumb(UIElement element, int eX = 0, int eY = 0, int x = 0, int y = 0) : this()
        {
            MyPanel.Children.Add(element);
            Canvas.SetLeft(element, eX);
            Canvas.SetTop(element, eY);

            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);

        }
        public FlatThumb()
        {
            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Canvas), "panel");
            this.Template = template;
            this.ApplyTemplate();
            MyPanel = (Canvas)template.FindName("panel", this);
            MyPanel.Background = Brushes.Aqua;

        }


    }




    public class Data
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Size { get; set; }
    }

}
