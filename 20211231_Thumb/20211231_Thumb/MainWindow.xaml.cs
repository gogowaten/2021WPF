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

namespace _20211231_Thumb
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
            //Layer1.AddChildren(Test4());//グループ化
            //Layer1.Children.Add(Test4());//グループ化
            //Layer1.Children.Add(Test5());//グループに追加
            //Layer1.AddChildren(Test5());//グループに追加
            Test6();
            Test7();
            //Test8();
        }

        //グループAにグループBを追加、これはグループAとグループBからグループC作成と同じように見えるけど中身が違う
        //この追加はしないほうがいいかも？
        private void Test7()
        {
            var listA = Enumerable.Range(0, 3).
                Select(a => new ReThumb(MakeTextBlock($"GroupA-{a}"), a * 20 + 10, a * 50 + 10)).ToList();
            listA.ForEach(a => a.GotFocus += MyReThumb_GotFocus);
            ReThumb groupA = new(listA, "グループA");

            var listB = Enumerable.Range(0, 3).
                Select(a => new ReThumb(MakeTextBlock($"GroupB-{a}"), a * 20 + 200, a * 50 + 20)).ToList();

            listB.ForEach(a => a.GotFocus += MyReThumb_GotFocus);
            ReThumb groupB = new(listB, "グループB");

            groupA.AddChildren(groupB);


            Layer1.AddChildren(groupA);
        }
        //グループAとグループBからグループC作成
        private void Test6()
        {
            var listA = Enumerable.Range(0, 3).
                Select(a => new ReThumb(MakeTextBlock($"GroupA-{a}"), a * 20 + 10, a * 50 + 10)).ToList();
            listA.ForEach(a => a.GotFocus += MyReThumb_GotFocus);
            ReThumb groupA = new(listA, "グループA");

            var listB = Enumerable.Range(0, 3).
                Select(a => new ReThumb(MakeTextBlock($"GroupB-{a}"), a * 20 + 200, a * 50 + 20)).ToList();

            listB.ForEach(a => a.GotFocus += MyReThumb_GotFocus);
            ReThumb groupB = new(listB, "グループB");

            List<ReThumb> listC = new() { groupA, groupB };
            ReThumb groupC = new(listC, $"グループC");

            Layer1.AddChildren(groupC);
            //Layer1.ChildrenOld.Add(groupC);
        }
      

        //既存グループに1要素を追加
        private ReThumb Test5()
        {
            //Group作成
            ReThumb group = new(Enumerable.Range(0, 3).
                Select(a => new ReThumb(MakeTextBlock($"Test5-{a}"), a * 20 + 20, a * 50 + 30, $"Test5-{a}")).
                ToList(),"テストグループ");
            group.GotFocus += MyReThumb_GotFocus;

            ReThumb reThumb = new(MakeTextBlock("追加要素"), 150, 20, $"追加要素");
            //reThumb.GotFocus += MyReThumb_GotFocus;
            //group.ChildrenOld.Add(reThumb);//追加
            group.AddChildren(reThumb);//追加
            return group;
        }
        //新規でグループ化
        private ReThumb Test4()
        {
            List<ReThumb> list = new();//要素作成
            for (int i = 0; i < 5; i++)
            {
                ReThumb re = new(MakeTextBlock($"test4の{i}"), i * 20 + 20, i * 50 + 30, $"テスト4の{i}");
                re.GotFocus += MyReThumb_GotFocus;
                list.Add(re);
            }
            ReThumb group = new(list, "グループ");//Group作成
            return group;
        }


        private void Test1()
        {
            MyReThumb1 = new ReThumb(MakeTextBlock("test1"), "test1");
            //Layer1.ChildrenOld.Add(MyReThumb1);
            Layer1.AddChildren(MyReThumb1);
            MyReThumb1.GotFocus += MyReThumb_GotFocus;
        }
        private void Test2()
        {
            MyReThumb2 = new ReThumb(MakeTextBlock("test2"), 100, 0, "Test2");
            Layer1.AddChildren(MyReThumb2);
            //Layer1.ChildrenOld.Add(MyReThumb2);
            MyReThumb2.GotFocus += MyReThumb_GotFocus;
        }
        private void Test3()
        {
            for (int i = 0; i < 5; i++)
            {
                ReThumb re = new(MakeTextBlock($"test3-{i}"), i * 20 + 20, i * 50 + 100, $"テスト3-{i}");
                re.GotFocus += MyReThumb_GotFocus;
                //Layer1.ChildrenOld.Add(re);
                Layer1.AddChildren(re);
            }
            //Enumerable.Range(0, 5)
            //          .Select(a => new ReThumb(MakeTextBlock($"test3-{a}"), a * 20 + 20, a * 50 + 100, $"テスト3-{a}"))
            //          .ToList()
            //          .ForEach(a => { a.GotFocus += MyReThumb_GotFocus; Layer1.Children.Add(a); });
        }

        private void MyReThumb_GotFocus(object sender, RoutedEventArgs e)
        {
            ReThumb item = sender as ReThumb;

            //if (item.IsRoot)
            //{
            //    MyStackPanel.DataContext = item;
            //}
            //else
            //{
            //    ReThumb origin = e.OriginalSource as ReThumb;
            //    MyStackPanel.DataContext = origin?.RootReThumb;
            //}
            if (item == item.RootReThumb)
            {
                MyStackPanel.DataContext = item;
            }
            else
            {
                MyStackPanel.DataContext = item.RootReThumb;
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
