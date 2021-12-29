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
        ReThumb MyReThumb1;
        ReThumb MyReThumb2;
        ReThumb MyGroupReThumb;
        public MainWindow()
        {
            InitializeComponent();

            //Test1();
            //Test2();
            //Test3();
            //MyCanvas.Children.Add(Test4());
            MyCanvas.Children.Add(Test5());
        }


        //既存グループに追加
        private ReThumb Test5()
        {
            ReThumb group = Test4();//Group作成
            ReThumb reThumb = new(MakeTextBlock("追加要素"), 100, 20);
            reThumb.GotFocus += MyReThumb_GotFocus;
            group.Children.Add(reThumb);//追加
            return group;
        }
        //新規でグループ化
        private ReThumb Test4()
        {
            List<ReThumb> list = new();//要素作成
            for (int i = 1; i < 5; i++)
            {
                ReThumb re = new(MakeTextBlock($"test{i}"), i * 20, i * 30);
                re.GotFocus += MyReThumb_GotFocus;
                list.Add(re);
            }
            ReThumb group = new(list);//Group作成
            return group;
        }


        private void Test1()
        {
            MyReThumb1 = new ReThumb(MakeTextBlock("test1"));
            MyCanvas.Children.Add(MyReThumb1);
            MyReThumb1.GotFocus += MyReThumb_GotFocus;
        }
        private void Test2()
        {
            MyReThumb2 = new ReThumb(MakeTextBlock("test2"), 100, 100);
            MyCanvas.Children.Add(MyReThumb2);
            MyReThumb2.GotFocus += MyReThumb_GotFocus;
        }
        private void Test3()
        {
            for (int i = 0; i < 5; i++)
            {
                ReThumb re = new(MakeTextBlock($"test{i}"), i * 20, i * 40);
                re.GotFocus += MyReThumb_GotFocus;
                MyCanvas.Children.Add(re);
            }
        }

        private void MyReThumb_GotFocus(object sender, RoutedEventArgs e)
        {
            ReThumb item = sender as ReThumb;

            if (item.IsRoot)
            {
                MyStackPanel.DataContext = item;
            }
            else
            {
                ReThumb origin = e.OriginalSource as ReThumb;
                MyStackPanel.DataContext = origin?.ParentReThumb;
            }

        }

        private TextBlock MakeTextBlock(string text)
        {
            TextBlock tb = new();
            tb.Text = text;
            tb.Background = Brushes.MediumAquamarine;
            tb.Foreground = Brushes.White;
            tb.FontSize = 30;
            return tb;
        }
    }
}
