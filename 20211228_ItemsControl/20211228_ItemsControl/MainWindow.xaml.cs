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

namespace _20211228_ItemsControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var dd = Enumerable.Range(0, 3).Select(a => new Data() { Name = "data", Left = 10 * a, Top = 20 * a }).ToList();
            DataContext = dd;

        }

        private void MyThumb1_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Thumb thumb = sender as Thumb;
            Canvas.SetLeft(thumb, Canvas.GetLeft(thumb) + e.HorizontalChange);
            Canvas.SetTop(thumb, Canvas.GetTop(thumb) + e.VerticalChange);
        }
    }
    public class Data
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public string Name { get; set; }

    }
}
