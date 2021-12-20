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
using System.ComponentModel;

namespace _20211220
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

        }
        private void AddThumb()
        {
            ExThumb thumb = new(10, 10);
            thumb.Width = 100;
            thumb.Height = 60;

            thumb.DragDelta += Thumb_DragDelta;

            MyCanvas.Children.Add(thumb);

            DataContext = thumb;
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ExThumb thumb = sender as ExThumb;
            thumb.Left += e.HorizontalChange;
            thumb.Top += e.VerticalChange;

        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            AddThumb();
        }

        private void ButtonCheck_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    //public class Data : ObservableCollection<Data>, System.ComponentModel.INotifyPropertyChanged
    //{
    //    private double left;

    //    //public ObservableCollection<Data> Children { get; set; } = new();
    //    public double Left
    //    {
    //        get => left; set
    //        {
    //            left = value;
    //            OnPropertyChanged();
    //        }
    //    }

    //    public event PropertyChangedEventHandler PropertyChanged;
    //    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberNameAttribute] string name = null)
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    //    }
    //}

    public class ExThumb : Thumb, System.ComponentModel.INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private double left;
        public double Left
        {
            get => left; set
            { left = value; OnPropertyChanged(); }
        }

        private double _Top;
        public double Top
        {
            get => _Top;
            set { _Top = value; OnPropertyChanged(); }
        }


        public ExThumb(double left, double top)
        {
            Left = left;
            Top = top;

            Binding b = new();
            b.Source = this;
            b.Path = new PropertyPath("Left");
            b.Mode = BindingMode.TwoWay;//必須
            SetBinding(Canvas.LeftProperty, b);

            //SetBinding(Canvas.LeftProperty, new Binding("Left"));//TwoWayがないから動かない
            //this.SetBinding(Canvas.LeftProperty, "Left");//TwoWayがないから動かない

            b = new();
            b.Source = this;
            b.Path = new PropertyPath("Top");
            b.Mode = BindingMode.TwoWay;
            SetBinding(Canvas.TopProperty, b);

        }

    }
}
