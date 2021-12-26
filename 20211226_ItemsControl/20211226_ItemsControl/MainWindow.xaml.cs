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

            for (int i = 0; i < 100; i++)
            {
                Data d = new() { Left = i * 5, Top = i * 10, Name = $"test{i}" };
                MyData.Add(d);
            }
            DataContext = MyData;

        }
    }
    public class Data
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public string Name { get; set; }
    }

}
