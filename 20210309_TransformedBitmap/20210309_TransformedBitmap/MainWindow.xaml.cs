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

//画像の縮小テスト
//TransformedBitmapを使って縮小
//画像ファイルから直接は使えない、なにかのBitmapSource形式にしてからになる、今回はBitmapImageにした
//縮小アルゴリズムはバイリニア法でほぼ確定、ニアレストネイバー法ではないのは確かめた
//バイキュービック法ではなさそうな感じ

namespace _20210309_TransformedBitmap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string path = @"D:\ブログ用\チェック用2\WP_20210228_11_25_51_Pro_2021_02_28_午後わてん.jpg";
            path = @"D:\ブログ用\テスト用画像\border_row.bmp";
            BitmapImage bmpImage = new BitmapImage(new Uri(path));
            bmpImage.Freeze();//必須、しないとエラー
            TransformedBitmap tBmp = new(bmpImage, new ScaleTransform(0.5, 0.5));
            tBmp.Freeze();//要る？

            MyStackPanel.Children.Add(new Image() { Source = bmpImage, Stretch = Stretch.None });
            //MyCanvas.Children.Add(new Image() { Source = tBmp, Stretch = Stretch.None });
            //Clipboard.SetImage(tBmp);


            //            TransformedBitmapを使用してサムネイルを作成するとメモリ使用量が多い - Qiita
            //https://qiita.com/rot-z/items/fde058a4bca236e59c47

            //TransformedBitmapをそのまま使わずにWriteableBitmapにしてから使うほうがいい？
            WriteableBitmap wBmp = new WriteableBitmap(tBmp);
            wBmp.Freeze();//要る？
            MyStackPanel.Children.Add(new Image() { Source = wBmp, Stretch = Stretch.None });
            Clipboard.SetImage(wBmp);
            

        }
    }
}
