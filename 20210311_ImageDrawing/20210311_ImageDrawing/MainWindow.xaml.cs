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


//ImageDrawingではBitmapSourceは取得できない、縮小画像は得られない
//
namespace _20210311_ImageDrawing
{
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string path = @"D:\ブログ用\チェック用2\WP_20210228_11_25_51_Pro_2021_02_28_午後わてん.jpg";
            path = @"D:\ブログ用\テスト用画像\border_row.bmp";
            BitmapImage bmpImage = new BitmapImage(new Uri(path));
            //bmpImage.Freeze();//必須、しないとエラー
            MyStackPanel.Children.Add(new Image() { Source = bmpImage, Stretch = Stretch.None });

            //BitmapSource系からImageDrawing作成
            //ImageDrawingからDrawingImage作成
            //DrawingImageをImageのSourceにして表示
            //表示はできるけど、このまま画像取得はできない
            ImageDrawing id = new ImageDrawing(bmpImage, new Rect(0, 0, 100, 100));
            DrawingImage di = new DrawingImage(id);
            MyStackPanel.Children.Add(new Image() { Source = di, Stretch = Stretch.None });
            Image img = new Image() { Source = di };
            ImageSource source = img.Source;            
            
            //DrawingVisualとRenderTargetBitmapを使って画像取得
            //できるけど、これならBitmapImageから直接取得変形したほうが早い
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawImage(di, new Rect(0, 0, 100, 100));
                //これはエラー
                //dc.DrawImage(id, new Rect(0, 0, 100, 100));
            }
            var rtb = new RenderTargetBitmap(100, 100, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);

        }
    }
}
