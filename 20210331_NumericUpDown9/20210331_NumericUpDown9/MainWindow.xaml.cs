using System.Windows;

namespace _20210331_NumericUpDown9
{
    public partial class MainWindow : Window
    {
        private Data MyData = new();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = MyData;
        }

        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            var inu = MyUpDown.MyValue;
            var neko = MyData.Value;
        }
    }


    public class Data
    {
        public decimal Value { get; set; } = 100m;
    }
}
