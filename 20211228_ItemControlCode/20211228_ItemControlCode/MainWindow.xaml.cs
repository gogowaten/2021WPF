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


namespace _20211228_ItemControlCode
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
    }

    public class ThumbItemsControl : Thumb
    {
        public ThumbItemsControl()
        {
            ControlTemplate template = new();
            FrameworkElementFactory fCanvas = new(typeof(Canvas));
            template.VisualTree = fCanvas;

        }
    }

    public class Data
    {
        public ObservableCollection<Data> Children { get; set; } = new();
        public double Left { get; set; }
        public double Top { get; set; }
        public string Name { get; set; }

    }
}
