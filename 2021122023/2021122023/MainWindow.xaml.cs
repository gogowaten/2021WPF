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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace _2021122023
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ExThumb ExGropup;
        public ExThumb MyExThumb;
        public MainWindow()
        {
            InitializeComponent();

            ExThumb ex1 = new(this, "test", 10, 10);
            MyCanvas.Children.Add(ex1);
            //DataContext = ex1.Data;
            //ExThumb ex2 = new("test2", 20, 20);
            //MyCanvas.Children.Add(ex2);


            ExThumb item1 = new(this, "item1", 100, 100);
            ExThumb item2 = new(this, "item2", 50, 50);
            //item1.DragDelta += DragDeltaEx;
            //item2.DragDelta += DragDeltaEx;
            ExGropup = new(this, new List<ExThumb>() { item1, item2 });
            MyCanvas.Children.Add(ExGropup);
            MyExThumb = ExGropup;

            //ExGropup.DragDelta += (o, e) => { ExGropup.Data.Left += e.HorizontalChange; ExGropup.Data.Top += e.VerticalChange; };
            //MyExThumb.DragDelta += (o, e) => { MyExThumb.Data.Left += e.HorizontalChange; MyExThumb.Data.Top += e.VerticalChange; };
        }

        private void DragDeltaEx(object sender, DragDeltaEventArgs e)
        {
            ExThumb ex = sender as ExThumb;
            ex.Data.Left += e.HorizontalChange;
            ex.Data.Top += e.VerticalChange;
        }

        private void ButtonGroup_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonUnGroup_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            var rect = ExGropup.RenderSize;
            var chi = ExGropup.Children;
            var focusThumb = MyExThumb;
            var dc = DataContext;
        }
    }

    public class Data : INotifyPropertyChanged
    {
        private double left;
        private double top;

        public double Left { get => left; set { left = value; OnPropertyChanged(); } }
        public double Top { get => top; set { top = value; OnPropertyChanged(); } }
        public BitmapSource BitmapSource { get; set; }
        public string Text { get; set; }
        public string Name { get; set; }
        public ExThumb RootExThumb { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class ExThumb : Thumb
    {
        private readonly string ROOT_PANEL_NAME = "rootPanel";
        private Canvas RootCanvas;
        private MainWindow RootWindow;
        //public ObservableCollection<ExThumb> Children { get; set; } = new();
        public UIElementCollection Children;
        public Data Data { get; set; } = new();

        public ExThumb(MainWindow mainWindow, double left = 0, double top = 0)
        {
            RootWindow = mainWindow;
            Data.RootExThumb = this;

            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Canvas), ROOT_PANEL_NAME);
            Template = template;
            ApplyTemplate();
            RootCanvas = Template.FindName(ROOT_PANEL_NAME, this) as Canvas;
            RootCanvas.Background = Brushes.Transparent;
            Focusable = true;

            Data.Left = left;
            Data.Top = top;
            DragDelta += ExThumb_DragDelta;
            //DragDelta -= ExThumb_DragDelta;
            PreviewMouseDown += ExThumb_PreviewMouseDown;
            Children = RootCanvas.Children;

            SetBinding(Canvas.LeftProperty, MakeBinding("Data.Left"));
            SetBinding(Canvas.TopProperty, MakeBinding("Data.Top"));


        }

        private void ExThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ExThumb ex = sender as ExThumb;
            if (ex == ex.Data.RootExThumb)
            {
                Data.Left += e.HorizontalChange;
                Data.Top += e.VerticalChange;
            }
            else
            {

            }
            
        }

        private void ExThumb_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            RootWindow.DataContext = Data.RootExThumb.Data;
            //e.Handled = true;
            //RootWindow.MyExThumb = Data.RootExThumb;
            //RootWindow.DataContext = Data;
            //RootWindow.MyExThumb = this;
        }

        private Rect GetRect()
        {
            Rect rect = new();
            foreach (var item in RootCanvas.Children)
            {
                ExThumb ex = item as ExThumb;
                rect = Rect.Union(rect, new Rect(ex.Data.Left, ex.Data.Top, ex.ActualWidth, ex.ActualHeight));
            }
            return rect;
        }


        public ExThumb(MainWindow mainWindow, string text, double left = 0, double top = 0) : this(mainWindow, left, top)
        {
            TextBlock textBlock = new();
            textBlock.Text = text;
            textBlock.FontSize = 30;
            _ = RootCanvas.Children.Add(textBlock);
        }
        public ExThumb(MainWindow mainWindow, List<ExThumb> exThumbs) : this(mainWindow)
        {
            this.Data.Left = exThumbs[0].Data.Left;
            this.Data.Top = exThumbs[0].Data.Top;
            //this.DragDelta -= ExThumb_DragDelta;
            for (int i = 0; i < exThumbs.Count; i++)
            {
                ExThumb ex = exThumbs[i];
                _ = RootCanvas.Children.Add(ex);
                ex.Data.RootExThumb = this;
                //ex.DragDelta -= ExThumb_DragDelta;
                //BindingOperations.ClearBinding(ex, Canvas.LeftProperty);
                //BindingOperations.ClearBinding(ex, Canvas.TopProperty);
            }

        }


        private Binding MakeBinding(string pName)
        {
            Binding b = new();
            b.Source = this;
            b.Path = new PropertyPath(pName);
            b.Mode = BindingMode.OneWay;
            return b;
        }
        public override string ToString()
        {
            //return base.ToString();
            string str = $"(x, y) = ({Data.Left}, {Data.Top})";
            return str;
        }

    }


}
