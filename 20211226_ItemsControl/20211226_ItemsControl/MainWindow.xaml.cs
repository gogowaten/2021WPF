using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
//WPF ItemsControlをDataGridみたいに使う - No more Death March
//https://nomoredeathmarch.hatenablog.com/entry/2019/01/21/003825
//マツオソフトウェアブログ: Canvasにリストの中身をBindingする方法
//http://my-clip-devdiary.blogspot.com/2011/01/canvasbinding.html
//ItemsControl 攻略 ～ 外観のカスタマイズ | grabacr.nét
//http://grabacr.net/archives/1240


namespace _20211226_ItemsControl
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

            for (int i = 0; i < 10; i++)
            {
                Data d = new() { Left = i * 10, Top = i * 20, Name = $"test{i}" };
                MyData.Add(d);
            }

            Xamlをコードで();
            DataContext = MyData;

        }
        private void Xamlをコードで()
        {
            ItemsControl itemsControl = new();

            //ItemsPanel
            ItemsPanelTemplate itemsPanelTemplate = new();
            itemsPanelTemplate.VisualTree = new FrameworkElementFactory(typeof(Canvas));
            itemsControl.ItemsPanel = itemsPanelTemplate;

            //ItemTemplate
            FrameworkElementFactory stackPanel = new(typeof(StackPanel));
            stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            FrameworkElementFactory fTextBlock1 = new(typeof(TextBlock));
            fTextBlock1.SetBinding(TextBlock.TextProperty, new Binding("Left"));
            stackPanel.AppendChild(fTextBlock1);
            FrameworkElementFactory fTextBlock2 = new(typeof(TextBlock));
            fTextBlock2.SetBinding(TextBlock.TextProperty, new Binding("Name"));
            stackPanel.AppendChild(fTextBlock2);
            DataTemplate dataTemplate = new();
            dataTemplate.VisualTree = stackPanel;
            itemsControl.ItemTemplate = dataTemplate;

            //ItemContainerStyle
            Style style = new();
            style.Setters.Add(new Setter(Canvas.LeftProperty, new Binding("Left")));
            style.Setters.Add(new Setter(Canvas.TopProperty, new Binding("Top")));
            itemsControl.ItemContainerStyle = style;

            Grid.SetColumn(itemsControl, 1);
            MyGrid.Children.Add(itemsControl);

            itemsControl.SetBinding(ItemsControl.ItemsSourceProperty, new Binding());
        }
    }

    public class Data
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public string Name { get; set; }
    }

}
