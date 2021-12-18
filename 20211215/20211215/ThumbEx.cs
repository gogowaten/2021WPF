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
using System.ComponentModel;
using System.Collections.ObjectModel;

//DependencyProperty INotifyPropertyChanged

//DPはBindingのソースとターゲットどちらでにもできる
//INPCはソースには使えるけど、ターゲットには使えない
namespace _20211215
{
    public class ThumbEx : Thumb
    {
        private Canvas RootCanvas;
        private static readonly string RootName = "canvas";
        public ThumbData ThumbData;
        private MainWindow MyMainWindow;
        private ObservableCollection<UIElement> Children = new();


        public ThumbEx(UIElement element, MainWindow mainWindow)
        {
            //ThumbData = new();
            //DataContext = ThumbData;
            Focusable = true;
            MyMainWindow = mainWindow;

            
            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Canvas), RootName);
            Template = template;
            _ = ApplyTemplate();
            RootCanvas = (Canvas)Template.FindName(RootName, this);

            Canvas.SetLeft(this, 0);
            Canvas.SetTop(this, 0);

            //_ = RootCanvas.Children.Add(element);
            AddChildren(element);


            Binding binding = new();
            binding.Source = this;
            binding.Path = new PropertyPath(Canvas.LeftProperty);
            binding.Mode = BindingMode.TwoWay;
            this.SetBinding(LeftProperty, binding);

            //↑↓は同じ、BindingModeを双方向だから？

            //Binding binding = new();
            //binding.Source = this;
            //binding.Path = new PropertyPath(LeftProperty);
            //binding.Mode = BindingMode.TwoWay;
            //this.SetBinding(Canvas.LeftProperty, binding);


            //binding = new();
            //binding.Source = this;
            //binding.Path = new PropertyPath(LeftProperty);
            //binding.Mode = BindingMode.TwoWay;
            //SetBinding(this.ThumbData.Left, binding);
            this.GotFocus += ThumbEx_GotFocus;
        }

        private void ThumbEx_GotFocus(object sender, RoutedEventArgs e)
        {
            MyMainWindow.DataContext = this;
        }


        public void AddChildren(UIElement element)
        {
            RootCanvas.Children.Add(element);
            Children.Add(element);

            

        }


        //
            #region DependencyProperty
            //public double MyLeft { get; set; }

        public double Left
        {
            get { return (double)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Left.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftProperty =
            DependencyProperty.Register("Left", typeof(double), typeof(ThumbEx), new PropertyMetadata(0.0));


        public double Top
        {
            get { return (double)GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Top.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TopProperty =
            DependencyProperty.Register("Top", typeof(double), typeof(ThumbEx), new PropertyMetadata(0.0));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ThumbEx), new PropertyMetadata(""));



        public List<ThumbData> ThumbDatas
        {
            get { return (List<ThumbData>)GetValue(ThumbDatasProperty); }
            set { SetValue(ThumbDatasProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThumbDatas.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThumbDatasProperty =
            DependencyProperty.Register("ThumbDatas", typeof(List<ThumbData>), typeof(ThumbEx), new PropertyMetadata(null));


        #endregion DependencyProperty

    }

    public class ThumbData
    {
        public List<ThumbData> ThumbDatas { get; set; } = new();
        public double Left { get; set; } = 0;
        public double Top { get; set; } = 0;
        public string Text { get; set; } = "";




    }

    public class ThumbDataNotify : System.ComponentModel.INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public List<ThumbData> ThumbDatas { get; set; } = new();
        public double Left { get; set; }
        public double Top { get; set; }
        public string Text { get; set; } = "";


    }



    public class ThumbDataDP : DependencyObject
    {
        public List<ThumbData> ThumbDatas { get; set; } = new();
        public double Left { get; set; } = 0;
        public double Top { get; set; } = 0;
        public string Text { get; set; } = "";



        public double MyLeft
        {
            get { return (double)GetValue(MyLeftProperty); }
            set { SetValue(MyLeftProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyLeft.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyLeftProperty =
            DependencyProperty.Register("MyLeft", typeof(double), typeof(ThumbEx), new PropertyMetadata(0.0));


    }


}
