using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

//WPF、画像から複数箇所を矩形(Rect)に切り抜いて、それぞれ位置を合わせて1枚の画像にしてファイルに保存する - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2021/01/24/233657

namespace _20210124_画像の切り抜き_複数画像を1枚にする
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
            //元の画像の読み込みと表示
            var img = new BitmapImage(new Uri(
                @"D:\ブログ用\チェック用2\WP_20201222_10_21_40_Pro_2020_12_22_午後わてん_ラーメン.jpg"));
            MyOriginImage.Source = img;

            //切り抜き範囲のリスト作成
            List<Rect> MyRectList = new()
            {
                new Rect(5, 110, 85, 60),
                new Rect(65, 135, 130, 130),
                new Rect(270, 50, 135, 130),
            };

            //切り抜いて
            BitmapSource bmp = CroppedBitmapFromRects(img, MyRectList);
            MyImage.Source = bmp;//表示
            SaveImage(bmp);//保存
        }

        /// <summary>
        /// 複数Rect範囲を組み合わせた形にbitmapを切り抜く
        /// </summary>
        /// <param name="source">元の画像</param>
        /// <param name="rectList">Rectのコレクション</param>
        /// <returns></returns>
        private BitmapSource CroppedBitmapFromRects(BitmapSource source, List<Rect> rectList)
        {
            var dv = new DrawingVisual();

            using (DrawingContext dc = dv.RenderOpen())
            {
                //それぞれのRect範囲で切り抜いた画像を描画していく
                foreach (var rect in rectList)
                {
                    dc.DrawImage(new CroppedBitmap(source, RectToIntRectWith切り捨て(rect)), rect);
                }
            }

            //描画位置調整
            dv.Offset = new Vector(-dv.ContentBounds.X, -dv.ContentBounds.Y);

            //bitmap作成、縦横サイズは切り抜き後の画像全体がピッタリ収まるサイズにする
            //PixelFormatsはPbgra32で決め打ち、これ以外だとエラーになるかも、
            //画像を読み込んだbitmapImageのPixelFormats.Bgr32では、なぜかエラーになった
            var bmp = new RenderTargetBitmap(
                (int)Math.Ceiling(dv.ContentBounds.Width),
                (int)Math.Ceiling(dv.ContentBounds.Height),
                96, 96, PixelFormats.Pbgra32);

            bmp.Render(dv);
            return bmp;
        }

        //RectからInt32Rect作成、小数点以下切り捨て編
        private Int32Rect RectToIntRectWith切り捨て(Rect re)
        {
            return new Int32Rect((int)re.X, (int)re.Y, (int)re.Width, (int)re.Height);
        }

        //今回は未使用
        //RectからInt32Rect作成、小数点以下四捨五入
        private Int32Rect RectToIntRectWith簡易四捨五入(Rect re)
        {
            return new Int32Rect(
                My簡易四捨五入(re.X),
                My簡易四捨五入(re.Y),
                My簡易四捨五入(re.Width),
                My簡易四捨五入(re.Height));

            //double型からint型の変換は切り捨てなので、
            //0.5足してから変換すると簡易四捨五入になる
            int My簡易四捨五入(double value)
            {
                return (int)(value + 0.5);
            }
        }

        //bitmapをpng画像ファイルで保存
        private void SaveImage(BitmapSource source)
        {
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add( BitmapFrame.Create(source));
            string path = DateTime.Now.ToString("HH時mm分ss秒");
            path = "cropped_" + path + ".png";
            using (var pp = new System.IO.FileStream(
                path, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                encoder.Save(pp);
            }
        }

    }
}
