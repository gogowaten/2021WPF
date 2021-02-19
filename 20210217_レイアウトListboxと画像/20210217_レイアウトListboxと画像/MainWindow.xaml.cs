using System;
using System.Collections.Generic;
using System.Globalization;
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


            this.VisualBitmapScalingMode = BitmapScalingMode.Fant;

            MyDatas = new();
            this.DataContext = MyDatas;

            AddItem(GetNowText(), new BitmapImage(new Uri(@"D:\ブログ用\テスト用画像\collection_1.png")), true);
            AddItem(GetNowText(), new BitmapImage(new Uri(@"D:\ブログ用\テスト用画像\collection_2.png")), true);
            AddItem(GetNowText(), new BitmapImage(new Uri(@"D:\ブログ用\テスト用画像\collection_3.png")), true);
            AddItem(GetNowText(), new BitmapImage(new Uri(@"D:\ブログ用\テスト用画像\collection_4.png")), true);
            AddItem(GetNowText(), new BitmapImage(new Uri(@"D:\ブログ用\テスト用画像\collection_5.png")), true);
            AddItem(GetNowText(), new BitmapImage(new Uri(@"D:\ブログ用\20210216_Pixcren124_13.png")), true);
            AddItem(GetNowText(), new BitmapImage(new Uri(@"D:\ブログ用\20210216_Pixcren124_14.png")), true);
            AddItem(GetNowText(), new BitmapImage(new Uri(@"D:\ブログ用\20210216_Pixcren124_15.png")), true);
            AddItem(GetNowText(), new BitmapImage(new Uri(@"D:\ブログ用\20210216_Pixcren124_16.png")), true);



        }

        private string GetNowText()
        {
            DateTime ima = DateTime.Now;
            return ima.ToString("yyyyMMdd_hhmmss_fff");
        }
        private void AddItem(string name, BitmapSource image, bool isSaved)
        {
            MyDatas.Add(new MyData(name, image, isSaved, "dammyPath"));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var neko = MyDatas;

          
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            AddItem(GetNowText(), new BitmapImage(new Uri(@"D:\ブログ用\テスト用画像\collection_1.png")), false);
        }

        private void MyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var data = (MyData)MyListBox.SelectedItem;
            MyImage.Source = data.Image;
            //MyImageList.ScrollIntoView(MyDatas[MyListBox.SelectedIndex]);
        }

        private void MyButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var neko = (Button)sender;
            var dc = neko.DataContext;

        }


    }




    public class MyData
    {
        public MyData(string name, BitmapSource image, bool isSave, string savePath)
        {
            Name = name;
            Image = image;
            IsSaved = isSave;
            SavePath = savePath;
        }

        public string Name { get; set; }
        public BitmapSource Image { get; set; }
        public bool IsSaved { get; set; }
        public string SavePath { get; set; }
    }


    public class MyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double len = (double)value;
            return len / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
