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

namespace _20211222
{

    public class ExThumb : Thumb, INotifyPropertyChanged
    {
        private readonly string ROOT_PANEL_NAME = "rootPanel";
        private Canvas RootCanvas;
        private MainWindow MyMainWindow;
        public bool IsGroup;
        public ExThumb RootExThumb;
        private double left;
        private double top;

        public ObservableCollection<ExThumb> Children { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public double Left { get => left; set { left = value; OnPropertyChanged(); } }
        public double Top { get => top; set { top = value; OnPropertyChanged(); } }


        public ExThumb()
        {

        }
        public ExThumb(MainWindow mainWindow, string name, double left = 0, double top = 0)
        {
            MyMainWindow = mainWindow;

            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Canvas), ROOT_PANEL_NAME);
            Template = template;
            ApplyTemplate();
            RootCanvas = Template.FindName(ROOT_PANEL_NAME, this) as Canvas;
            RootCanvas.Background = Brushes.Transparent;
            Focusable = true;

            RootExThumb = this;

            Left = left;
            Top = top;
            Name = name;
            DragDelta += ExThumb_DragDelta;
            GotFocus += ExThumb_GotFocus;
            SetBinding(Canvas.LeftProperty, MakeBinding("Left"));
            SetBinding(Canvas.TopProperty, MakeBinding("Top"));


        }

        private void ExThumb_GotFocus(object sender, RoutedEventArgs e)
        {
            MyMainWindow.DataContext = this.RootExThumb;
            MyMainWindow.CurrentExThumb = this.RootExThumb;
        }

        public ExThumb(MainWindow mainWindow, string name, UIElement element, double left = 0, double top = 0) : this(mainWindow, name, left, top)
        {
            AddChildrenElement(element);
        }

        private void ExThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ExThumb ex = sender as ExThumb;
            if (ex.Name == ex.RootExThumb.Name)
            {
                Left += e.HorizontalChange;
                Top += e.VerticalChange;
            }
            else
            {

            }

        }


        private Binding MakeBinding(string pName)
        {
            Binding b = new();
            b.Source = this;
            b.Path = new PropertyPath(pName);
            b.Mode = BindingMode.TwoWay;
            return b;
        }
        public void AddChildrenExThumb(ExThumb exThumb)
        {
            RootCanvas.Children.Add(exThumb);
            Children.Add(exThumb);
            IsGroup = true;
            exThumb.RootExThumb = this;
            exThumb.DragDelta -= ExThumb_DragDelta;
        }
        public void AddChildrenElement(UIElement element)
        {
            RootCanvas.Children.Add(element);

        }
        public override string ToString()
        {
            //return base.ToString();
            return $"name({Name}), (x,y) = ({Left},{Top})";
        }
    }
}
