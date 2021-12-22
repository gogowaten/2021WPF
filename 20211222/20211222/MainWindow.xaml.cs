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

namespace _20211222
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ExThumb CurrentExThumb = new();
        public MainWindow()
        {
            InitializeComponent();

       
            Test1();
            GroupTest1();

        }

       

        private void Test1()
        {
            TextBlock textBlock1 = new();
            textBlock1.Text = "textBlock1";
            textBlock1.FontSize = 30;
            textBlock1.Background = Brushes.Cyan;
            ExThumb exThumb1 = new(this, "test1", textBlock1, 10, 10);
            MyCanvas.Children.Add(exThumb1);

        }
        private void GroupTest1()
        {
            TextBlock t1 = MakeTextBloxk("textBlock1", 30, Brushes.Magenta);
            ExThumb ex1 = new(this, "text1", t1, 0, 0);
            TextBlock t2 = MakeTextBloxk("textBlock2", 30, Brushes.MediumAquamarine);
            ExThumb ex2 = new(this, "text2", t2, 100, 100);
            ExThumb exGroup = new(this, "Group", 100, 100);
            exGroup.AddChildrenExThumb(ex1);
            exGroup.AddChildrenExThumb(ex2);
            MyCanvas.Children.Add(exGroup);
            
        }
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
        }
    }
}
