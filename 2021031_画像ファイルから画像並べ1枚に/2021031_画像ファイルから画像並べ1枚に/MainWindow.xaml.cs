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


namespace _2021031_画像ファイルから画像並べ1枚に
{
    public partial class MainWindow : Window
    {
        private int MasuYoko = 2;
        private int MasuWidth = 200;//ある程度固定
        //private int MasuHeight = 200;//最初の画像のアスペクト比に追従
        //private double MyAspectRatio = 0;//1枚目のアスペクト比
        private List<FlatThumb> MyThumbs = new();
        public MainWindow()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;
            this.Top = 0; this.Left = 0;


        }

        //private void BindingTest()
        //{
        //    TextBlock tb = new();
        //    MyCanvas.Children.Add(tb);
        //    Canvas.SetLeft(tb, 0);
        //    Canvas.SetTop(tb, 20);

        //    Slider s = new();
        //    s.Value = 20;
        //    Canvas.SetLeft(s, 0);
        //    Canvas.SetTop(s, 40);
        //    s.Width = 100;
        //    MyCanvas.Children.Add(s);

        //    //Binding、ソースがスライダー、ターゲットがテキストブロックの場合
        //    var b = new Binding();
        //    b.Source = s;
        //    b.Path = new PropertyPath(Slider.ValueProperty);

        //    //セット方法1、SetBindingを使う
        //    //tb.SetBinding(TextBlock.TextProperty, b);

        //    //セット方法2、BindingOperationsを使う
        //    BindingOperations.SetBinding(tb, TextBlock.TextProperty, b);
        //}

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) return;
            var datas = (string[])e.Data.GetData(DataFormats.FileDrop);
            var paths = datas.ToList();
            paths.Sort();

            for (int i = 0; i < paths.Count; i++)
            {
                BitmapSource source = MakeBitmapSource(paths[i]);
                //if (MasuHeight == 0)
                //{
                //    MasuHeight = source.PixelHeight * (MasuWidth / source.PixelWidth);
                //    MyAspectRatio = source.PixelWidth / (double)source.PixelHeight;
                //}
                Image img = new()
                {
                    Source = source,
                    Stretch = Stretch.Uniform,
                    Width = MasuWidth,
                    Height = MasuWidth,
                };
                source.Freeze();

                int count = MyThumbs.Count;
                double x = count % MasuYoko * MasuWidth;
                double y = count / MasuYoko * MasuWidth;
                var t = new FlatThumb(img, x, y);
                MyThumbs.Add(t);
                MyCanvas.Children.Add(t);
                t.DragDelta += T_DragDelta;
            }
            SetCanvasSize();
        }

        //スクロールバー表示できるようにCanvasのサイズを指定する
        private void SetCanvasSize()
        {
            if (MyCanvas == null) return;
            MyCanvas.Width = MasuWidth * MasuYoko;
            MyCanvas.Height = (MyThumbs.Count + 1) / MasuYoko * MasuWidth;// ((int)Math.Ceiling(MyThumbsCount / (double)MyData.MasuX)) * MyHeight;
        }
        private void Replace()
        {
            for (int i = 0; i < MyThumbs.Count; i++)
            {
                MyThumbs[i].MyXLocate = (i % MasuYoko) * MasuWidth;
                MyThumbs[i].MyYLocate = (i / MasuYoko) * MasuWidth;
            }
        }


        private void T_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var t = sender as FlatThumb;
            t.MySetLocate(e.HorizontalChange, e.VerticalChange);
        }

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

        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            var neko = MyThumbs;
            var t = MyThumbs[0];
            t.MyXLocate = 222;
            var x = Canvas.GetLeft(t);
        }

        private void MyButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var path = MakeSavePath();
            var source = MakeSaveBitmapSource();
            Save(source, path);
        }
        private BitmapSource MakeSaveBitmapSource()
        {
            //描画する座標とサイズを取得
            List<Rect> drawRects = MakeRects();

            DrawingVisual dv = new();
            using (DrawingContext dc = dv.RenderOpen())
            {
                for (int i = 0; i < MyThumbs.Count; i++)
                {
                    //DrawingContextに描画
                    FlatThumb t = MyThumbs[i];
                    dc.DrawImage(t.MyBitmap, drawRects[i]);
                }
            }
            //最終的な全体画像サイズ計算、RectのUnionを使う
            Rect dRect = new();
            for (int i = 0; i < drawRects.Count; i++)
            {
                dRect = Rect.Union(dRect, drawRects[i]);
            }
            //RenderTargetBitmapに描画、BitmapSource完成
            int rw = (int)dRect.Width;
            int rh = (int)dRect.Height;
            RenderTargetBitmap rb = new(rw, rh, 96, 96, PixelFormats.Pbgra32);
            rb.Render(dv);
            return rb;
        }

        //描画サイズと座標の計算
        //E:\オレ\エクセル\作りたいアプリのメモ.xlsm_2021031_$A$214
        private List<Rect> MakeRects()
        {
            List<Rect> drawRects = new();

            //サイズとX座標
            //指定横幅に縮小、アスペクト比は保持
            for (int i = 0; i < MyThumbs.Count; i++)
            {
                //サイズ
                BitmapSource bmp = MyThumbs[i].MyBitmap;
                double width = bmp.PixelWidth;
                double ratio = MasuWidth / width;
                if (ratio > 1) ratio = 1;
                width *= ratio;

                //X座標、中央揃え
                double x = (i % MasuYoko) * MasuWidth;
                x = x + (MasuWidth - width) / 2;

                //Y座標は後で計算
                drawRects.Add(new(x, 0, width, bmp.PixelHeight * ratio));
            }

            //Y座標計算
            //Y座標はその行にある画像の中で最大の高さを求めて、中央揃えのY座標を計算
            //行ごとに計算する必要がある

            //今の行の基準Y座標、次の行へは今の行の高さを加算していく
            double kijun = 0;            
            int count = 0;
            while (count < MyThumbs.Count)
            {
                int end = count + MasuYoko;
                if (end > MyThumbs.Count) end = MyThumbs.Count;
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
        private void Save(BitmapSource source, string path)
        {
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(source));
            using (var fs = new System.IO.FileStream(
                path, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write))
            {
                encoder.Save(fs);
            }

        }

        private void MyButtonClear_Click(object sender, RoutedEventArgs e)
        {
            MyThumbs.Clear();
            MyCanvas.Children.Clear();
            //MyAspectRatio = 0;
        }
    }



    public class FlatThumb : Thumb
    {
        private Canvas MyPanel;
        public BitmapSource MyBitmap;
        //public System.Collections.ObjectModel.ObservableCollection<ElementData> MyElementDatas = new();

        public double MyXLocate
        {
            get { return (double)GetValue(MyXLocateProperty); }
            set { SetValue(MyXLocateProperty, value); }
        }
        public static readonly DependencyProperty MyXLocateProperty =
            DependencyProperty.Register(nameof(MyXLocate), typeof(double), typeof(FlatThumb), new PropertyMetadata(0D));

        public double MyYLocate
        {
            get { return (double)GetValue(MyYLocateProperty); }
            set { SetValue(MyYLocateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyYLocate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyYLocateProperty =
            DependencyProperty.Register(nameof(MyYLocate), typeof(double), typeof(FlatThumb), new PropertyMetadata(0d));




        public FlatThumb(Image img, double x = 0, double y = 0, double ex = 0, double ey = 0)
        {
            this.Loaded += (s, e) => { this.VisualBitmapScalingMode = BitmapScalingMode.Fant; };

            //MyElementDatas.CollectionChanged += ElementDatas_CollectionChanged;

            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Canvas), "panel");
            this.Template = template;
            this.ApplyTemplate();
            MyPanel = (Canvas)template.FindName("panel", this);
            MyPanel.Background = Brushes.Aqua;
            MyPanel.Width = img.Width;
            MyPanel.Height = img.Height;

            //var data = new ElementData(element, ex, ey);
            //MyElementDatas.Add(data);

            MyAddElement(img, ex, ey);
            MyBitmap = img.Source as BitmapSource;


            MyXLocate = x;
            var b = new Binding();
            b.Source = this;
            b.Path = new PropertyPath(FlatThumb.MyXLocateProperty);
            BindingOperations.SetBinding(this, Canvas.LeftProperty, b);

            MyYLocate = y;
            b = new Binding();
            b.Source = this;
            b.Path = new PropertyPath(FlatThumb.MyYLocateProperty);
            BindingOperations.SetBinding(this, Canvas.TopProperty, b);

        }

        private void ElementDatas_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var ac = e.Action;
            var n = e.NewItems;
            var o = e.OldItems;

        }
        public void MySetLocate(double hChanged, double vChanged)
        {
            //Canvas.SetLeft(this, Canvas.GetLeft(this) + hChanged);
            //Canvas.SetTop(this, Canvas.GetTop(this) + vChanged);

            MyXLocate += hChanged;
            MyYLocate += vChanged;

        }


        public void MyElementSetLocate(UIElement element, double x, double y)
        {
            //ElementDatas.IndexOf(element)
            //Canvas.SetLeft(this, x);
            //Canvas.SetTop(this, y);
        }

        public void MyAddElement(UIElement element, double x = 0, double y = 0)
        {
            MyPanel.Children.Add(element);
            Canvas.SetLeft(element, x);
            Canvas.SetTop(element, y);

        }

        public override string ToString()
        {
            string str = $"x={MyXLocate}, y={MyYLocate}";
            return str;
            //return base.ToString();
        }
    }

    //public class ElementData
    //{
    //    public ElementData(UIElement uIElement, double x = 0, double y = 0)
    //    {
    //        UIElement = uIElement;
    //        Point = new Point(x, y);
    //    }

    //    public UIElement UIElement { get; set; }
    //    public Point Point { get; set; }

    //}


}
