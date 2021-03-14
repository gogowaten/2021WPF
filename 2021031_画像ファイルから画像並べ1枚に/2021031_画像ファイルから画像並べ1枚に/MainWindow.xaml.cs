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
        private double MasuSize = 200;
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
                Image img = new()
                {
                    Source = source,
                    Stretch = Stretch.Uniform,
                    Width = MasuSize,
                    Height = MasuSize,
                };
                source.Freeze();

                int count = MyThumbs.Count;
                double x = count % MasuYoko * MasuSize;
                double y = count / MasuYoko * MasuSize;
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
            MyCanvas.Width = MasuSize * MasuYoko;
            MyCanvas.Height = (MyThumbs.Count + 1) / MasuYoko * MasuSize;// ((int)Math.Ceiling(MyThumbsCount / (double)MyData.MasuX)) * MyHeight;
        }
        private void Replace()
        {
            for (int i = 0; i < MyThumbs.Count; i++)
            {
                MyThumbs[i].MyXLocate = (i % MasuYoko) * MasuSize;
                MyThumbs[i].MyYLocate = (i / MasuYoko) * MasuSize;
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
            DrawingVisual dv = new();
            using (var dc = dv.RenderOpen())
            {
                for (int i = 0; i < MyThumbs.Count; i++)
                {
                    FlatThumb t = MyThumbs[i];
                    //アスペクト比保持でのサイズ計算
                    double rate;
                    int pw = t.MyBitmap.PixelWidth;
                    int ph = t.MyBitmap.PixelHeight;
                    if (pw > ph)
                        rate = MasuSize / pw;
                    else
                        rate = MasuSize / ph;

                    int w = (int)(pw * rate);
                    int h = (int)(ph * rate);

                    //中央揃えの座標計算
                    int x = (int)t.MyXLocate + (int)(MasuSize - w) / 2;
                    int y = (int)t.MyYLocate + (int)(MasuSize - h) / 2;
                    //描画座標と描画サイズRect
                    Rect r = new(x, y, w, h);
                    //描画
                    dc.DrawImage(t.MyBitmap, r);
                }
            }
            //最終的な画像サイズ計算
            int rw = (int)(MasuYoko * MasuSize);
            int rh = (int)((MyThumbs.Count + 1) / MasuYoko * MasuSize);
            RenderTargetBitmap rb = new(rw, rh, 96, 96, PixelFormats.Pbgra32);
            rb.Render(dv);
            return rb;
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
