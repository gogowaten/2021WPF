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
using System.Collections.ObjectModel;

namespace _20211223
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ExThumb currentExThumb;
        //private ExThumb MyExThumb1;
        //private ExThumb MyExThumb2;
        //private ExThumb MyGroupExThumb1;
        private int CountForName;
        private ObservableCollection<ExThumb> MyLayers = new();
        private ExThumb MyCurrentLayer;
        private ExThumb MyMainPanel;
        public ExThumb CurrentExThumb
        {
            get => currentExThumb; set
            {
                currentExThumb = value;
                DataContext = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();

            ExThumb layer = MakeLayer("Layer1");
            MyMainPanel.AddChildrenExThumb(layer);
            MyLayers.Add(layer);
            MyCurrentLayer = layer;
            //Test1();
            //GroupTest1();

        }
        private void MyInitialize()
        {
            //最上位Thumb、ここに全てを追加していくことになる
            ExThumb panel = new(this, "MainPanel");
            panel.RemoveDragDeltaEvent();
            panel.IsLayer = true;
            panel.Focusable = false;
            MyGrid.Children.Add(panel);
            //Grid.SetColumn(panel, 0);
            MyMainPanel = panel;
        }
        private ExThumb MakeLayer(string name)
        {
            //レイヤー、最上位の直下のChildren
            ExThumb layer = new(this, name);
            layer.RemoveDragDeltaEvent();
            layer.IsLayer = true;
            layer.Focusable = false;
            return layer;
        }


        //private void Test1()
        //{
        //    ExThumb exThumb1 = new(this, "textBlock1", MakeTextBloxk("textBlock1", 30, Brushes.Cyan), 50, 0);
        //    ExThumb exThumb2 = new(this, "textBlock2", MakeTextBloxk("textBlock2", 30, Brushes.Cyan), 0, 100);
        //    MyCanvas.Children.Add(exThumb1);
        //    MyCanvas.Children.Add(exThumb2);
        //    MyExThumb1 = exThumb1;
        //    MyExThumb2 = exThumb2;
        //}
        //private void GroupTest1()
        //{
        //    TextBlock t1 = MakeTextBloxk("GroupItem1", 30, Brushes.Magenta);
        //    ExThumb ex1 = new(this, "GroupItem1", t1, 0, 0);
        //    TextBlock t2 = MakeTextBloxk("GroupItem2", 30, Brushes.MediumAquamarine);
        //    ExThumb ex2 = new(this, "GroupItem2", t2, 100, 100);
        //    ExThumb exGroup = new(this, "Group", 200, 100);
        //    exGroup.AddChildrenExThumb(ex1);
        //    exGroup.AddChildrenExThumb(ex2);
        //    MyCanvas.Children.Add(exGroup);
        //    MyGroupExThumb1 = exGroup;
        //}
        private TextBlock MakeTextBloxk(string text, double fontsize, Brush brush)
        {
            TextBlock textBlock = new();
            textBlock.Text = text;
            textBlock.FontSize = fontsize;
            textBlock.Background = brush;
            return textBlock;
        }

        private void ButtonTest1_Click(object sender, RoutedEventArgs e)
        {
            var dc = DataContext;
            var cex = CurrentExThumb;
            var panel = MyMainPanel;
            var layer = MyCurrentLayer;

            var panelParent = MyMainPanel.ParentExThumb;
            var layerParent = layer.ParentExThumb;

        }

        private void ButtonGroup_Click(object sender, RoutedEventArgs e)
        {
            ////1と2をグループ化
            ////グループ化Thumbの座標決定
            //double x = MyExThumb1.Left;
            //if (x > MyExThumb2.Left) { x = MyExThumb2.Left; }
            //double y = MyExThumb1.Top;
            //if (y > MyExThumb2.Top) { y = MyExThumb2.Top; }
            //ExThumb group = new(this, "Group1", x, y);
            ////Itemの座標修正
            //MyExThumb1.Left -= x; MyExThumb1.Top -= y;
            //MyExThumb2.Left -= x; MyExThumb2.Top -= y;
            ////ItemをCanvasから削除
            //MyCanvas.Children.Remove(MyExThumb1);
            //MyCanvas.Children.Remove(MyExThumb2);
            ////ItemをグループThumbに追加
            //group.AddChildrenExThumb(MyExThumb1);
            //group.AddChildrenExThumb(MyExThumb2);
            //MyCanvas.Children.Add(group);
            //MyGroupExThumb1 = group;
        }

        private void ButtonUnGroup_Click(object sender, RoutedEventArgs e)
        {
            //double x = MyGroupExThumb1.Left;
            //double y = MyGroupExThumb1.Top;
            //foreach (var item in MyGroupExThumb1.Children)
            //{
            //    MyGroupExThumb1.RemoveChildrenExThumb(item);
            //    item.Left += x;
            //    item.Top += y;
            //    MyCanvas.Children.Add(item);
            //}
            //MyCanvas.Children.Remove(MyGroupExThumb1);
            //MyGroupExThumb1 = null;
        }

        private void ButtonAddThumb_Click(object sender, RoutedEventArgs e)
        {
            ExThumb ex = new(this, $"textBlock{CountForName}");
            ex.AddChildrenElement(MakeTextBloxk($"Test{CountForName}", 30, Brushes.Magenta));
            CountForName++;
            MyCurrentLayer.AddChildrenExThumb(ex);
        }
    }
}
