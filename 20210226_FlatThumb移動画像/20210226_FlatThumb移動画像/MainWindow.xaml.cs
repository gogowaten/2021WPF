using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;

//WPF、Image(画像)をマウスドラッグ移動、ThumbのTemplateを変更して作成 - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2021/02/27/005213

namespace _20210226_FlatThumb移動画像
{    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();
        }

        private void MyInitialize()
        {
            //画像1枚だけのThumb
            ImageThumb t1 = new();
            t1.DragDelta += Thumb_DragDelta;
            Canvas.SetLeft(t1, 0);
            Canvas.SetTop(t1, 0);
            //画像ファイルからBitmapImage作成してThumbにセット
            BitmapImage bmp1 =
                new(new Uri(@"D:\ブログ用\テスト用画像\collection_1.png"));
            bmp1.Freeze();
            t1.SetImage(new Image() { Source = bmp1 });
            //MainWindowに表示
            MyCanvas.Children.Add(t1);


            //画像2枚表示のThumb
            ImageThumb t2 = new();
            t2.DragDelta += Thumb_DragDelta;
            Canvas.SetLeft(t2, 200);
            Canvas.SetTop(t2, 0);
            //1枚めセット
            t2.SetImage(new Image() { Source = bmp1 });
            //2枚めセット
            BitmapImage bmp2 =
                new(new Uri(@"D:\ブログ用\テスト用画像\collection_2.png"));
            bmp2.Freeze();
            //少し位置をずらしてセットしてみた
            t2.SetImage(new Image() { Source = bmp2 }, 40, 40);

            MyCanvas.Children.Add(t2);
        }

        //マウスドラッグで移動
        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var t = sender as ImageThumb;
            //移動先の座標は、今の座標 + マウスの移動量
            Canvas.SetLeft(t, Canvas.GetLeft(t) + e.HorizontalChange);
            Canvas.SetTop(t, Canvas.GetTop(t) + e.VerticalChange);
        }
    }



    /// <summary>
    /// Image表示専用Thumb
    /// ControlTemplateを使ってCanvasだけで構成されたThumb
    /// </summary>
    public class ImageThumb : Thumb
    {
        private const string BASE_PANEL_NAME = "canvas";
        private Canvas BasePanel;//ThumbのベースパネルにするCanvas

        public ImageThumb()
        {
            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree =
                new FrameworkElementFactory(typeof(Canvas), BASE_PANEL_NAME);
            this.Template = template;
            //テンプレート再構築、これで中の要素を名前で検索取得できるようになる
            this.ApplyTemplate();
            BasePanel = (Canvas)this.Template.FindName(BASE_PANEL_NAME, this);
        }

        //画像をCanvasにセット
        public void SetImage(Image img, double left = 0, double top = 0)
        {
            BasePanel.Children.Add(img);
            Canvas.SetLeft(img, left);
            Canvas.SetTop(img, top);
        }

    }
}
