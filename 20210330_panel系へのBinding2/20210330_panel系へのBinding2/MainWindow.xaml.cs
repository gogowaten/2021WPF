using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace _20210330_panel系へのBinding2
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
                new {Color = Colors.Red, Top = 0, Left = 0},
                new {Color = Colors.Red, Top = 100, Left = 0},
                new {Color = Colors.Red, Top = 0, Left = 100},
                new {Color = Colors.Cyan, Top = 100, Left = 100},
            };
        }
    }
    public class MyConverter : IValueConverter
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
