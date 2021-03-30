using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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

using System.Collections.ObjectModel;
using System.Globalization;


//無理
//できるだけBindingで処理しようとしてみたけど無理
//座標決定に自身のListの中でのIndexが必要になるけど、それをXAMLで指定する方法がわからん

namespace _20210330_panel系へのBinding3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Datas MyDatas = new();
        public MainWindow()
        {
            InitializeComponent();

            //Data3 d3 = new();
            //d3.Datas.Add(new(0, 0, Brushes.Cyan));
            //d3.Size = 40;
            //d3.Datas.Add(new(100, 0, Brushes.Red));
            //this.DataContext = d3;


            //MyDatas.Col = 4;
            //MyDatas.Row = 3;
            //MyDatas.Size = 80;
            //MyDatas.Add(new Data(Brushes.Green));
            //MyDatas.Add(new Data(Brushes.Blue));
            //MyDatas.Add(new Data(Brushes.Green));
            //MyDatas.Add(new Data(Brushes.Blue));
            //MyDatas.Add(new Data(Brushes.Green));
            //MyDatas.Add(new Data(Brushes.Blue));
            //MyDatas.Add(new Data(Brushes.Green));
            //MyDatas.Add(new Data(Brushes.Blue));
            //MyDatas.Add(new Data(Brushes.Green));
            //MyDatas.Add(new Data(Brushes.Blue));
            //this.DataContext = MyDatas;

            PanelData panelData = new();
            this.DataContext = panelData;
            panelData.Datas.Add(new Data(Brushes.Green));
            panelData.Datas.Add(new Data(Brushes.Black));

        }

        private void MySliderSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void MyButtonTest_Click(object sender, RoutedEventArgs e)
        {
            var d = this.DataContext;
        }
    }

    public class Data
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public SolidColorBrush Brush { get; set; }


        public Data(double top, double left, SolidColorBrush brush)
        {
            Top = top;
            Left = left;
            Brush = brush;
        }

        public Data() { }
        public Data(SolidColorBrush b) { Brush = b; }
    }
    public class DataList : ObservableCollection<Data>
    {
      
    }
    public class PanelData:INotifyPropertyChanged
    {
        public DataList Datas { get; set; }
        public int Row { get; set; } = 3;
        public int Col { get; set; } = 4;

        private double _Size = 80;
        public double Size
        {
            get => _Size;
            set
            {
                if (_Size == value)
                    return;
                _Size = value;
                RaisePropertyChanged(nameof(Size));
                MyChange();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string pName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(pName));
        }

        public PanelData()
        {
            Datas = new();
            Datas.CollectionChanged += Datas_CollectionChanged;
        }


        private void Datas_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            MyChange();
        }
        public void MyChange()
        {
            for (int i = 0; i < Datas.Count; i++)
            {
                //Datas[i].Top = i / Col * Size;
                //Datas[i].Left = i % Col * Size;
                Datas[i].Width = Size;
                Datas[i].Height = Size;
            }
        }
    }

    public class Data3
    {
        public ObservableCollection<Data> Datas { get; set; } = new();
        public double Size = 40;

        public Data3(double size)
        {
            Size = size;
        }
        public Data3() { }

    }
    public class Datas : ObservableCollection<Data>
    {
        public double Size { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            MyChange();
            base.OnCollectionChanged(e);
        }
        public void MyChange()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Top = i / Col * Size;
                Items[i].Left = i % Col * Size;
            }
        }
    }
    public class Datas2 : INotifyPropertyChanged,INotifyCollectionChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private double _MySize;
        public double MySize
        {
            get => _MySize;
            set
            {
                if (_MySize == value)
                    return;
                _MySize = value;
                RaisePropertyChanged(nameof(MySize));
            }
        }

        private void RaisePropertyChanged(string pName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(pName));
        }
    }


    public class MyConverterTop : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int i = (int)values[0];
            int Col = (int)values[1];
            double Size = (double)values[2];
            return i / Col * Size;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class MyConverterLeft : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int i = (int)values[0];
            int Col = (int)values[1];
            double Size = (double)values[2];
            return i % Col * Size;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



}
