using System;
using System.Collections.Generic;
using System.Globalization;
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

using System.ComponentModel;

namespace _20210328_MultiBindingテスト
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Data MyData;
        public MainWindow()
        {
            InitializeComponent();
            //MyData = new() { Col = 4, Row = 3, Size = 80 };
            //this.DataContext = MyData;
        }

        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            var n = MyData;

        }
    }

    public class Data : INotifyPropertyChanged
    {
        //public int Row { get; set; }
        //public int Col { get; set; }
        //public int Size { get; set; }

        public Data()
        {
            Row = 3;
            Col = 3;
            Size = 40;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler d = PropertyChanged;
            d?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //if (d != null)
            //{
            //    d(this, new PropertyChangedEventArgs(propertyName));
            //}
        }

        private int _Row;
        public int Row
        {
            get => _Row;
            set
            {
                if (_Row == value)
                    return;
                _Row = value;
                RaisePropertyChanged(nameof(Row));
            }
        }

        private int _Col;
        public int Col
        {
            get => _Col;
            set
            {
                if (_Col == value) return;
                _Col = value;
                RaisePropertyChanged(nameof(Col));
            }
        }

        private int _Size;
        public int Size
        {
            get => _Size;
            set
            {
                if (_Size == value)
                    return;
                _Size = value;
                RaisePropertyChanged(nameof(Size));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }


    public class ConveterRectangleSize : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double i = (double)values[0];
            double size = (double)values[1];
            return i * size;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class ConveterRectangleSizeInt : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int i = (int)values[0];
            int size = (int)values[1];
            return i * size;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
