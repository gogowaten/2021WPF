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

//無理
//Thumbの管理にItemsControl系のListBox、TreeViewを利用するのは諦めた、無理、できない
//Listの中の要素を管理するのがめんどくさくてここまで試したけど
//ItemsControl系は単一の形式データをどう表現するかには使えるけど、
//多様なデータの表現には向かない感じ


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

            ThumbItemsControl thumbItemsControl = new();
        }
    }

    public class ThumbItemsControl : Thumb
    {
        public ThumbItemsControl()
        {
            ControlTemplate template = new();
            FrameworkElementFactory fCanvas = new(typeof(Canvas));
            template.VisualTree = fCanvas;

            FrameworkElementFactory fItemsControl = new(typeof(ItemsControl));
            fCanvas.AppendChild(fItemsControl);

            DataTemplate dataTemplate = new();
            FrameworkElementFactory dtCanvas = new(typeof(Canvas));
            dataTemplate.VisualTree = dtCanvas;



            this.Template = template;
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
