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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<FlatThumb> MyThumbs = new();
        public MainWindow()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;


            TextBlock tb = new();
            MyCanvas.Children.Add(tb);
            Canvas.SetLeft(tb, 0);
            Canvas.SetTop(tb, 20);

            Slider s = new();
            s.Value = 20;
            Canvas.SetLeft(s, 0);
            Canvas.SetTop(s, 40);
            s.Width = 100;
            MyCanvas.Children.Add(s);

            //Binding、ソースがスライダー、ターゲットがテキストブロックの場合
            var b = new Binding();
            b.Source = s;
            b.Path = new PropertyPath(Slider.ValueProperty);

            //セット方法1、SetBindingを使う
            //tb.SetBinding(TextBlock.TextProperty, b);

            //セット方法2、BindingOperationsを使う
            BindingOperations.SetBinding(tb, TextBlock.TextProperty, b);
        }

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
                    Width = 200,
                    Height = 200,
                };
                var t = new FlatThumb(img, 100, 0);
                MyThumbs.Add(t);
                MyCanvas.Children.Add(t);
                //t.MySetLocate(0, i * 20);
                t.DragDelta += T_DragDelta;
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
    }



    public class FlatThumb : Thumb
    {
        private Canvas MyPanel;

        public System.Collections.ObjectModel.ObservableCollection<ElementData> MyElementDatas = new();

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




        public FlatThumb(UIElement element, double x = 0, double y = 0, double ex = 0, double ey = 0)
        {
            this.Loaded += (s, e) => { this.VisualBitmapScalingMode = BitmapScalingMode.Fant; };
            
            MyElementDatas.CollectionChanged += ElementDatas_CollectionChanged;

            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Canvas), "panel");
            this.Template = template;
            this.ApplyTemplate();
            MyPanel = (Canvas)template.FindName("panel", this);
            MyPanel.Background = Brushes.Aqua;

            var data = new ElementData(element, ex, ey);
            MyElementDatas.Add(data);

            MyAddElement(element, ex, ey);

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

    public class ElementData
    {
        public ElementData(UIElement uIElement, double x = 0, double y = 0)
        {
            UIElement = uIElement;
            Point = new Point(x, y);
        }

        public UIElement UIElement { get; set; }
        public Point Point { get; set; }

    }


}
