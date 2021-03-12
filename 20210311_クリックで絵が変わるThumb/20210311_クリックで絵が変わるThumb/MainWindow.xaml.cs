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

//これは失敗、よくわからん

namespace _20210311_クリックで絵が変わるThumb
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FlatThumb MyThumb;
        public MainWindow()
        {
            InitializeComponent();

            MyThumb = new FlatThumb(@"D:\ブログ用\チェック用2\WP_20210228_11_25_51_Pro_2021_02_28_午後わてん.jpg");
            MyCanvas.Children.Add(MyThumb);
            MyThumb.SetLocate(0, 0);
            MyThumb.DragDelta += MyThumb_DragDelta;
        }

        private void MyThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var x = Canvas.GetLeft(MyThumb) + e.HorizontalChange;
            var y = Canvas.GetTop(MyThumb) + e.VerticalChange;
            MyThumb.SetLocate(x, y);
        }
    }


    public class FlatThumb : Thumb
    {
        public string FilePath { get; }
        public Canvas MyPanel;
        public Image MyImage;
        private Label MyLabel;

        public FlatThumb(string filePath)
        {
            FilePath = filePath;

            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Canvas), "panel");
            this.Template = template;
            this.ApplyTemplate();
            MyPanel = (Canvas)template.FindName("panel", this);
            MyPanel.Background = Brushes.Aqua;

            MyImage = MakeImage(filePath);
            MyPanel.Children.Add(MyImage);
            MyImage.Visibility = Visibility.Collapsed;

            MyLabel = new Label() { Content = filePath };
            MyPanel.Children.Add(MyLabel);

            MyLabel.IsMouseDirectlyOverChanged += MyLabel_IsMouseDirectlyOverChanged;
            //this.PreviewMouseLeftButtonDown += FlatThumb_MouseDown;
            //this.PreviewMouseLeftButtonUp += FlatThumb_MouseUp;
            
        }

        //マウスオーバーで表示を変化させようとしたけど、無反応
        private void MyLabel_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var mdo = this.IsMouseDirectlyOver;
            if (mdo)
            {
                MyImage.Visibility = Visibility.Visible;
                MyLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                MyImage.Visibility = Visibility.Collapsed;
                MyLabel.Visibility = Visibility.Visible;
            }

        }

        private void FlatThumb_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            var mo = this.IsMouseOver;
        }

        private void FlatThumb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MyImage.Visibility = Visibility.Collapsed;
            MyLabel.Visibility = Visibility.Visible;
        }

        private void FlatThumb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MyImage.Visibility = Visibility.Visible;
            MyLabel.Visibility = Visibility.Collapsed;
        }

        private Image MakeImage(string path)
        {
            return new Image() { Source = new BitmapImage(new Uri(path)), Stretch = Stretch.Uniform, Width = 100, Height = 100 };
        }
        public void SetLocate(Point p)
        {
            Canvas.SetLeft(this, p.X);
            Canvas.SetTop(this, p.Y);
        }
        public void SetLocate(double x, double y)
        {
            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);
        }

    }
}
