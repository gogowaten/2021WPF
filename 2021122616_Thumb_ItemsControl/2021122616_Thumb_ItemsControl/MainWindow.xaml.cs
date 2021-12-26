using System;
using System.Collections.Generic;
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

namespace _2021122616_Thumb_ItemsControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Data> MyData { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();

            MyData.Add(new Data() { Left = 10, Name = "test" });
            MyData.Add(new Data() { Left = 220, Name = "test" });
            DataContext = MyData;

            MThumb mThumb = new();
            MyCanvas.Children.Add(mThumb);
            //AItemsControl aItemsControl = new();
            //MyGrid.Children.Add(aItemsControl);
        }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            MyData[0].Left = 60;
        }
    }

    public class Data : System.ComponentModel.INotifyPropertyChanged
    {
        private double left;

        public string Name { get; set; }
        public double Left { get => left; set { left = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
