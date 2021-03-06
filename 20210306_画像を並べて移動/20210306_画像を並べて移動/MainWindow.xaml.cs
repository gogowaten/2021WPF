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

namespace _20210306_画像を並べて移動
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<FlatThumb> MyThumbs = new();
        private List<Point> MyPoints = new();
        private List<BitmapSource> MyBitmaps = new();
        private int MyThumbsCount;
        private Data MyData;

        private const int MASU_W = 100;
        //private const int MASU_H = 200;
        private int MyHeight;

        public MainWindow()
        {
            InitializeComponent();
            //this.VisualBitmapScalingMode = BitmapScalingMode.Fant;
            this.Loaded += (s, e) => { this.VisualBitmapScalingMode = BitmapScalingMode.Fant; };
            MyData = new Data() { MasuX = 2, MasuY = 2 };
            this.DataContext = MyData;

        }

        /// <summary>
        /// ドラッグ移動中のThumbとその他のThumbとの重なり合う部分の面積を計算、
        /// 一定以上の面積があった場合、場所を入れ替えて整列
        /// </summary>
        /// <param name="t">ドラッグ移動中のThumb</param>
        /// <param name="x">Canvas上でのX座標</param>
        /// <param name="y">Canvas上でのY座標</param>
        private void Idou移動中処理(FlatThumb t, double x, double y)
        {

            int imaIndex = MyThumbs.IndexOf(t);//ドラッグ移動中ThumbのIndex

            //最寄りのPoint
            int moyoriIndex = 0;
            double moyori距離 = double.MaxValue;
            for (int i = 0; i < MyPoints.Count; i++)
            {
                double distance = GetDistance(MyPoints[i], new Point(x, y));
                if (distance < moyori距離)
                {
                    moyori距離 = distance;
                    moyoriIndex = i;
                }
            }

            //最短距離のIndexと移動中のThumbのIndexが違うなら入れ替え処理
            if (moyoriIndex != imaIndex)
            {
                //Thumbリストの中での位置を変更
                MyThumbs.RemoveAt(imaIndex);
                MyThumbs.Insert(moyoriIndex, t);

                //indexに従って表示位置変更
                //ReplaceThumb表示位置更新(moyoriIndex);
                Replace();
            }
        }

        //2点間距離
        private double GetDistance(Point a, Point b)
        {
            return Math.Sqrt((a - b) * (a - b));
        }


        private void Window_Drop(object sender, DragEventArgs e)
        {
            MyThumbs.Clear();
            MyPoints.Clear();
            MyBitmaps.Clear();
            MyCanvas.Children.Clear();
            MyThumbsCount = 0;

            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            var data = (string[])e.Data.GetData(DataFormats.FileDrop);
            var paths = data.ToList();
            paths.Sort();

            for (int i = 0; i < paths.Count; i++)
            {
                MyBitmaps.Add(MakeBitmapSource(paths[i]));
            }
            if (MyBitmaps.Count == 0) return;
            MyHeight = (int)((MASU_W * (MyBitmaps[0].PixelHeight / (double)MyBitmaps[0].PixelWidth)) + 0.5);


            for (int i = 0; i < MyBitmaps.Count; i++)
            {
                //BitmapSource bmp = MakeBitmapSource(paths[i]);

                FlatThumb t = new FlatThumb(MyBitmaps[i], MASU_W, MyHeight);
                t.DragDelta += T_DragDelta;
                t.DragCompleted += T_DragCompleted;
                t.DragStarted += T_DragStarted;

                MyCanvas.Children.Add(t);
                t.Width = MASU_W;
                t.Height = MyHeight;
                MyThumbs.Add(t);
                MyPoints.Add(new Point(i % MyData.MasuX * MASU_W, i / MyData.MasuX * MyHeight));
                MyThumbsCount++;

                Panel.SetZIndex(t, i);
            }

            Replace();
            SetCanvasSize();
        }
        private void SetCanvasSize()
        {
            if (MyCanvas == null) return;
            MyCanvas.Width = MyData.MasuX * MASU_W;
            MyCanvas.Height = ((int)Math.Ceiling(MyThumbsCount / (double)MyData.MasuX)) * MyHeight;
            

        }

        private void T_DragStarted(object sender, DragStartedEventArgs e)
        {
            var t = sender as FlatThumb;
            t.Opacity = 0.7;
            Panel.SetZIndex(t, MyData.MasuX * MyData.MasuY);
        }

        private void T_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            FlatThumb t = sender as FlatThumb;
            t.Opacity = 1.0;
            int imaIndex = MyThumbs.IndexOf(t);
            //元のx座標へ移動
            Canvas.SetLeft(t, MyPoints[imaIndex].X);
            Canvas.SetTop(t, MyPoints[imaIndex].Y);

            Panel.SetZIndex(t, imaIndex);
        }

        private void Replace()
        {
            for (int i = 0; i < MyPoints.Count; i++)
            {
                MyThumbs[i].SetLocate(MyPoints[i]);
            }
        }

        private void T_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var t = sender as FlatThumb;
            double x = Canvas.GetLeft(t) + e.HorizontalChange;
            double y = Canvas.GetTop(t) + e.VerticalChange;
            Canvas.SetLeft(t, x);
            Canvas.SetTop(t, y);

            Idou移動中処理(t, x, y);
        }

        //private FlatThumb MakeThumb(string filePath)
        //{
        //    BitmapSource bmp = MakeBitmapSource(filePath);
        //    return new FlatThumb(bmp, MASU_W, MASU_H);
        //}

        private BitmapSource MakeBitmapSource(string filePath, double dpiX = 96, double dpiY = 96)
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
            {

            }
            return source;
        }

        private void MyNumeMasuCount_MyValueChanged(object sender, ControlLibraryCore20200620.MyValuechangedEventArgs e)
        {
            if (MyPoints.Count == 0) return;
            int count = MyPoints.Count;
            MyPoints.Clear();
            for (int i = 0; i < count; i++)
            {
                MyPoints.Add(new Point(i % MyData.MasuX * MASU_W, (i / MyData.MasuX) * MyHeight));
            }
            Replace();
            SetCanvasSize();
        }

        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            var neko = MyData.MasuX;
        }
    }



    public class FlatThumb : Thumb
    {
        private Canvas MyPanel;
        private Image MyImage;

        public FlatThumb(BitmapSource bitmap, double width, double height)
        {
            //this.VisualBitmapScalingMode = BitmapScalingMode.Fant;

            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Canvas), "panel");
            this.Template = template;
            this.ApplyTemplate();
            MyPanel = (Canvas)template.FindName("panel", this);
            MyPanel.Background = Brushes.Aqua;

            MyImage = new() { Source = bitmap, Stretch = Stretch.Uniform };
            MyPanel.Children.Add(MyImage);
            MyImage.Width = width;
            MyImage.Height = height;

        }
        public void SetLocate(Point p)
        {
            Canvas.SetLeft(this, p.X);
            Canvas.SetTop(this, p.Y);
        }


        //public override string ToString()
        //{            
        //    return base.ToString();
        //}
    }



    public class Data
    {
        public int MasuX { get; set; }
        public int MasuY { get; set; }


    }




}
