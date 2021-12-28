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

namespace _20211227_TreeView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Take1();
        }
        private void Take1()
        {
            List<Data> datas = new();
            for (int i = 0; i < 3; i++)
            {
                datas.Add(new Data() { Name = $"Data 1-{i}" });
            }
            var ds = Enumerable.Range(0, 3).Select(a => new Data() { Name = $"Data {a}", Left = a * 10, Top = a * 50 }).ToList();
            var ds0 = Enumerable.Range(0, 3).Select(a => new Data() { Name = $"Data 0-{a}", Left = a * 10, Top = a * 100 }).ToList();
            var ds1 = Enumerable.Range(0, 3).Select(a => new Data() { Name = $"Data 1-{a}", Left = a * 10, Top = a * 100 }).ToList();
            var ds2 = Enumerable.Range(0, 3).Select(a => new Data() { Name = $"Data 2-{a}", Left = a * 10, Top = a * 10 }).ToList();
            var ds00 = Enumerable.Range(0, 3).Select(a => new Data() { Name = $"Data 0-0-{a}", Left = a * 10, Top = a * 100 }).ToList();
            ds[0].Children = ds0;
            ds[1].Children = ds1;
            ds[2].Children = ds2;
            ds[0].Children[0].Children = ds00;
            MyTreeView1.ItemsSource = ds;
            MyTreeView2.ItemsSource = ds;
        }
    }

    public class Data
    {
        public string Name { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public List<Data> Children { get; set; } = new();
    }
}
