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

//ItemsControl 攻略 ～ 外観のカスタマイズ | grabacr.nét
//http://grabacr.net/archives/1240

//ListBoxのTemplate変更をC#で
namespace _2021122014
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();




            ListBox listBox = new();
            MyGrid.Children.Add(listBox);

            ////FrameworkElementFactory rootCanvas = new(typeof(Canvas), "root");
            //ItemsPanelTemplate panelTemplate = new(new FrameworkElementFactory(typeof(Canvas)));
            //listBox.ItemsPanel = panelTemplate;

            ////1. Template = ControlTemplate はアイテム表示欄全体の設定になる
            //FrameworkElementFactory factory = new(typeof(Border));
            //factory.SetValue(Border.BorderBrushProperty, Brushes.Magenta);
            //factory.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            //factory.SetValue(Border.BackgroundProperty, Brushes.LightGray);
            //factory.SetValue(Border.MarginProperty, new Thickness(10));
            //factory.AppendChild(new FrameworkElementFactory(typeof(ItemsPresenter)));//これがよくわからん、アイテム表示になるみたい
            //ControlTemplate controlTemplate = new(typeof(ItemsControl));
            //controlTemplate.VisualTree = factory;
            //ItemsControl itemsControl = new();
            //itemsControl.Template = controlTemplate;
            //listBox.Template = controlTemplate;

            ////2. ItemsPanel = ItemsPanelTemplate、アイテム表示欄のベースになるパネルの設定
            //FrameworkElementFactory root = new(typeof(VirtualizingStackPanel));
            //root.SetValue(VirtualizingStackPanel.OrientationProperty, Orientation.Horizontal);
            //ItemsPanelTemplate template = new(root);
            //listBox.ItemsPanel = template;

            //3. 1と2をあわせて
            FrameworkElementFactory factory = new(typeof(Border));
            factory.SetValue(Border.BorderBrushProperty, Brushes.Magenta);
            factory.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            factory.SetValue(Border.BackgroundProperty, Brushes.LightGray);
            factory.SetValue(Border.MarginProperty, new Thickness(10));
            //factory.AppendChild(new FrameworkElementFactory(typeof(ItemsPresenter)));//これがよくわからん、アイテム表示になるみたい
            FrameworkElementFactory root = new(typeof(VirtualizingStackPanel));
            root.SetValue(VirtualizingStackPanel.OrientationProperty, Orientation.Horizontal);            
            root.SetValue(VirtualizingStackPanel.IsItemsHostProperty, true);
            factory.AppendChild(root);//ここが違う
            ControlTemplate controlTemplate = new(typeof(ItemsControl));
            controlTemplate.VisualTree = factory;
            ItemsControl itemsControl = new();
            itemsControl.Template = controlTemplate;
            listBox.Template = controlTemplate;


            listBox.ItemsSource = new List<int>() { 1, 2, 3 };

        }
    }

    public class ExListBox : ListBox
    {
        public ExListBox()
        {
            DataTemplate template = new(typeof(ListBox));
            template.VisualTree = new FrameworkElementFactory(typeof(Canvas), "root");
            this.ItemTemplate = template;
            ApplyTemplate();
            Canvas canvas = (Canvas)template.FindName("root", this);

            Thumb t = new();
            t.Width = 100;
            t.Height = 20;
            canvas.Children.Add(t);
            ItemsControl itemsControl = new();
            itemsControl.ItemTemplate = template;

        }
    }
}
