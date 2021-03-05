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

//GridにCanvasを設置、水平と垂直位置を中央に設定
//Canvasの縦横サイズ指定が必須
//CanvasのサイズがGridより大きくなると、隣のパネル要素に被さってくるけど
//ScrollViwerで囲めばスクロールバー表示できる


namespace _20210305_レイアウトCanvasを中央
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            int yoko = 3;
            int tate = 3;
            int w = 200;
            int h = 100;
            RightCanvas.Width = w * yoko;
            RightCanvas.Height = h * tate;

            for (int i = 0; i < 9; i++)
            {
                Button b = new Button() { Width = w, Height = h, Content = $"b{i}", FontSize = 40 };
                RightCanvas.Children.Add(b);
                int left = i % yoko * w;
                int top = i / tate * h;
                SetLocate(b, left, top);

            }
        }
        private void SetLocate(FrameworkElement e, double left, double top)
        {
            Canvas.SetLeft(e, left);
            Canvas.SetTop(e, top);
        }
    }
}
