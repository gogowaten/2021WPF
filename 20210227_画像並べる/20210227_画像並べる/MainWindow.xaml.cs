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

//マウスドラッグで画像の並び替えのテスト
//失敗
//UniformgridとWrappanelでは無理そう、やっぱり地道にCanvas？ヒット確認？
namespace _20210227_画像並べる
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();
        }

        //Uniformgridは行列数を指定できるけど等間隔で並ぶ
        private void MyInitialize()
        {
            for (int i = 0; i < 10; i++)
            {
                Button element = new() { Content = $"textBlock{i}", Width = 100, Height = 50 };
                MyGrid.Children.Add(element);
            }
            MyGrid.FlowDirection = FlowDirection.RightToLeft;
            MyGrid.Columns = 2;
            MyGrid.Rows = 2;

            this.Loaded += MainWindow_Loaded;
        }

        //Wrappanelは詰めて並ぶけど行列数を指定できない
        //ThumbでもCanvas.Set系では移動しないし、Margin系でも移動しないかも
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = new() { Width = 400, Height = 300 };
            WrapPanel panel = new();
            w.Content = panel;
            for (int i = 0; i < 10; i++)
            {
                Button element = new() { Content = $"element{i}", Height = 50 };
                panel.Children.Add(element);
            }
            panel.Orientation = Orientation.Vertical;
            w.Show();

            w = new() { Width = 500, Height = 300 };
            panel = new();
            w.Content = panel;
            for (int i = 0; i < 10; i++)
            {
                Thumb element = new() { Width = 100, Height = 50 };
                panel.Children.Add(element);
                element.DragDelta += Element_DragDelta;
            }
            panel.Orientation = Orientation.Vertical;
            w.Show();

        }

        private void Element_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Thumb t = sender as Thumb;
            Canvas.SetLeft(t, Canvas.GetLeft(t) + e.HorizontalChange);
            Canvas.SetTop(t, Canvas.GetTop(t) + e.VerticalChange);
        }
    }


}
