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

//Gridへのアイテムのバインド（WPF編） | 泥庭
//https://yone64.wordpress.com/2013/05/29/grid%E3%81%B8%E3%81%AE%E3%82%A2%E3%82%A4%E3%83%86%E3%83%A0%E3%81%AE%E3%83%90%E3%82%A4%E3%83%B3%E3%83%89%EF%BC%88wpf%E7%B7%A8%EF%BC%89/

namespace _20210330_panel系へのBinding
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new[]
            {
                new { Color = Colors.White, ColumnIndex = 3, RowIndex = 3 },
                new { Color = Colors.White, ColumnIndex = 4, RowIndex = 4 },
                new { Color = Colors.Black, ColumnIndex = 3, RowIndex = 4 },
                new { Color = Colors.Black, ColumnIndex = 4, RowIndex = 3 },
            };
        }


    }
    public class ColorToSolidColorBrushValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color v = (Color)value;
            return new SolidColorBrush(v);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
