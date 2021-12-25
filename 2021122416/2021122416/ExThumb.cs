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

namespace _2021122416
{
    public class ExThumb : Thumb, INotifyPropertyChanged
    {
        private readonly string ROOT_PANEL_NAME = "rootPanel";
        private Canvas RootCanvas;
        private MainWindow MyMainWindow;
        public bool IsGroup;
        //public bool IsRootExThumb;
        public bool IsLayer;
        public ExThumb ParentExThumb;

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

            //ControlTemplate template = new(typeof(Thumb));
            //template.VisualTree = new FrameworkElementFactory(typeof(Canvas), ROOT_PANEL_NAME);
            //Template = template;
            //ApplyTemplate();
            //RootCanvas = Template.FindName(ROOT_PANEL_NAME, this) as Canvas;
            //RootCanvas.Background = Brushes.Transparent;
            //Focusable = true;

            //RootExThumb = this;

            //Left = left;
            //Top = top;
            //Name = name;
            //DragDelta += ExThumb_DragDelta;
            //GotFocus += ExThumb_GotFocus;
            //SetBinding(Canvas.LeftProperty, MakeBinding("Left"));
            //SetBinding(Canvas.TopProperty, MakeBinding("Top"));


        }

        private void ExThumb_GotFocus(object sender, RoutedEventArgs e)
        {
            MyMainWindow.CurrentExThumb = this.RootExThumb;
            //オリジナルソース以外には通知しないで、ここで切断
            e.Handled = true;
        }

        public ExThumb(MainWindow mainWindow, string name, UIElement element, double left = 0, double top = 0) : this(mainWindow, name, left, top)
        {
            AddChildrenElement(element);
        }

        private void ExThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Left += e.HorizontalChange;
            Top += e.VerticalChange;
        }


        private Binding MakeBinding(string pName)
        {
            Binding b = new();
            b.Source = this;
            b.Path = new PropertyPath(pName);
            b.Mode = BindingMode.TwoWay;
            return b;
        }


        //グループ化のときに使う
        public void AddChildrenExThumb(ExThumb exThumb)
        {
            RootCanvas.Children.Add(exThumb);
            Children.Add(exThumb);
            IsGroup = true;
            exThumb.ParentExThumb = this;

            //ルートを取得、設定
            ExThumb rootEx = GetRootExThumb(exThumb);
            exThumb.RootExThumb = rootEx;
            //追加Thumbがルートじゃないときは動かないようにDragdeltaを外す
            if (exThumb != rootEx)
            {
                exThumb.DragDelta -= exThumb.ExThumb_DragDelta;
            }



        }
        //ルートThumbを取得
        private ExThumb GetRootExThumb(ExThumb ex)
        {
            //Parentを辿る、Parentが無くなるかParentがLayer用Thumbになるまで辿る
            if (ex.ParentExThumb == null || ex.ParentExThumb.IsLayer) { return ex; }
            while (true)
            {
                GetRootExThumb(ex.ParentExThumb);
            }
        }

        public void RemoveChildrenExThumb(ExThumb exThumb)
        {
            exThumb.ParentExThumb.RootCanvas.Children.Remove(exThumb);
            exThumb.RootExThumb = exThumb;
            exThumb.DragDelta += exThumb.ExThumb_DragDelta;
        }
        public void ChangeParent(ExThumb newExThumb)
        {
            ParentExThumb.RootCanvas.Children.Remove(this);
            ParentExThumb.Children.Remove(this);
            newExThumb.RootCanvas.Children.Add(this);
            newExThumb.Children.Add(this);
            this.ParentExThumb = newExThumb;
            this.RootExThumb = GetRootExThumb(this);

        }
        //グループ化できるのはParentが同じ者同士
        public void MakeGroup(List<ExThumb> exThumbs)
        {
            //要素全てがIsGroupではなかった場合は、新たにGroup用ExThumbを作成して、全てを詰め込む
            //要素の中にIsGroupがある場合は、一番上にあるものに詰め込む

        }
        public void AddChildrenElement(UIElement element)
        {
            RootCanvas.Children.Add(element);

        }
        public void AddDragDeltaEvent()
        {
            DragDelta += ExThumb_DragDelta;
        }
        public void RemoveDragDeltaEvent()
        {
            DragDelta -= ExThumb_DragDelta;
        }

        public override string ToString()
        {
            //return base.ToString();
            return $"name({Name}), (x,y) = ({Left},{Top})";
        }
    }
}
