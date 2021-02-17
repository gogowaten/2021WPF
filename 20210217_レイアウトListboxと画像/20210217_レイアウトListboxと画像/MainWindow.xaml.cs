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

namespace _20210217_レイアウトListboxと画像
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Collections.ObjectModel.ObservableCollection<MyData> MyDatas;
        
        public MainWindow()
        {
            InitializeComponent();

            MyDatas = new();
            MyDatas.Add(new MyData("no1", new BitmapImage(new Uri(@"D:\ブログ用\テスト用画像\collection_2.png")), true));
            MyDatas.Add(new MyData("no2", new BitmapImage(new Uri(@"D:\ブログ用\テスト用画像\collection_3.png")), false));
            this.DataContext = MyDatas;




        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var neko = MyDatas;
            
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            MyDatas.Add(new MyData("addData1", new BitmapImage(new Uri(@"D:\ブログ用\テスト用画像\collection_4.png")), false));
        }

        private void MyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var data = (MyData)MyListBox.SelectedItem;
            MyImage.Source = data.Image;
        }
    }




    public class MyData
    {
        public MyData(string name, BitmapSource image, bool? isSave)
        {
            Name = name;
            Image = image;
            IsSave = isSave;
        }

        public string Name { get; set; }
        public BitmapSource Image { get; set; }
        public bool? IsSave { get; set; }

    }

}
