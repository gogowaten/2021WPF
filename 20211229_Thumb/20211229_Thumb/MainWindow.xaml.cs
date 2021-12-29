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

namespace _20211229_Thumb
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ReThumb MyReThumb;
        ReThumb MyGroupReThumb;
        public MainWindow()
        {
            InitializeComponent();

            Test1();
            //ReThumb reThumb = new();
            //for (int i = 0; i < 2; i++)
            //{
            //    ReThumb rr = new();
            //    reThumb.Children.Add(rr);
            //}
            //reThumb.Children.RemoveAt(0);
        }

        private void Test1()
        {
            MyReThumb = new ReThumb(MakeTextBlock("test1"));
            MyCanvas.Children.Add(MyReThumb);
            this.DataContext = MyReThumb;
        }
        private TextBlock MakeTextBlock(string text)
        {
            TextBlock tb = new();
            tb.Text = text;
            tb.Background = Brushes.MediumAquamarine;
            tb.FontSize = 30;
            return tb;
        }
    }
}
