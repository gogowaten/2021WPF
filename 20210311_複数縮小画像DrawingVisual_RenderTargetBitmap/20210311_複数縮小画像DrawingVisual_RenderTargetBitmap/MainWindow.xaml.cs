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

//縮小アルゴリズムがバイリニア法固定になるけど
//この方法がラク

//2つの画像ファイルの画像から
//100x100に縮小して、横に並べて200ｘ100のBitmapSourceを取得する

//画像ファイルからBitmapImage作成
//DrawingVisualでBitmapImageをDrawImage
//RenderTargetBitmapでDrawingVisualをRender

namespace _20210311_複数縮小画像DrawingVisual_RenderTargetBitmap
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //これは今回の場合意味なかった
            this.Loaded += (s, e) => { this.VisualBitmapScalingMode = BitmapScalingMode.Fant; };

            //画像ファイルからBitmapImage作成
            string path1 = @"D:\ブログ用\チェック用2\WP_20210228_11_25_51_Pro_2021_02_28_午後わてん.jpg";
            BitmapImage bmpImage1 = new BitmapImage(new Uri(path1));
            //bmpImage1.Freeze();//必須、しないとエラー
            
            string path2 = @"D:\ブログ用\テスト用画像\border_row.bmp";
            BitmapImage bmpImage2 = new(new Uri(path2));


            DrawingVisual dv = new();
            //RenderOptions.SetBitmapScalingMode(dv, BitmapScalingMode.Fant);
            using (var dc = dv.RenderOpen())
            {
                dc.DrawImage(bmpImage1, new Rect(0, 0, 100, 100));
                dc.DrawImage(bmpImage2, new Rect(100, 0, 100, 100));
            }

            var rb = new RenderTargetBitmap(200, 100, 96, 96, PixelFormats.Pbgra32);
            rb.Render(dv);

            MyStackPanel.Children.Add(new Image() { Source = rb, Stretch = Stretch.None });

        }
    }
}
