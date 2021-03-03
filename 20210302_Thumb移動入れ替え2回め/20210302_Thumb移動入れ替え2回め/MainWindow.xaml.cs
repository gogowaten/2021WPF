using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;

// _20210227_thumb移動入れ替え
//↑の改良版、入れ替え処理判定を面積から距離にした

namespace _20210302_Thumb移動入れ替え2回め
{
    public partial class MainWindow : Window
    {
        private const int MASU_X = 3;//横に並べる個数指定
        private const int MASU_Y = 3;//縦
        private const int MASU_WIDTH = 80;//Thumbの横幅指定
        private const int MASU_HEIGHT = 80;//縦幅
        //Thumbのリスト
        private ObservableCollection<FlatThumb> MyThumbs;
        //座標リスト
        private List<Point> MyPoints;

        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();
        }

        private void MyInitialize()
        {
            MyPoints = new List<Point>();
            MyThumbs = new ObservableCollection<FlatThumb>();

            //Thumbを作成、表示
            for (int y = 0; y < MASU_Y; y++)
            {
                for (int x = 0; x < MASU_X; x++)
                {
                    FlatThumb t = new($"{(y * MASU_X) + x}")
                    {
                        Width = MASU_WIDTH,
                        Height = MASU_HEIGHT,
                        Opacity = 0.7,
                        FontSize = 40
                    };
                    MyCanvas.Children.Add(t);
                    Canvas.SetLeft(t, x * MASU_WIDTH);
                    Canvas.SetTop(t, y * MASU_HEIGHT);
                    t.DragDelta += Thumb_DragDelta;
                    t.DragCompleted += Thumb_DragCompleted;
                    MyThumbs.Add(t);

                    //基本座標セット
                    Point p = new(x * MASU_WIDTH, y * MASU_HEIGHT);
                    MyPoints.Add(p);
                }
            }
        }

        //ドラッグ移動終了イベント時、
        //Indexに対応した位置に移動させる
        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            FlatThumb t = sender as FlatThumb;
            int imaIndex = MyThumbs.IndexOf(t);
            //元のx座標へ移動
            Canvas.SetLeft(t, MyPoints[imaIndex].X);
            Canvas.SetTop(t, MyPoints[imaIndex].Y);

            MyStatus.Content = "";

        }

        /// <summary>
        /// ドラッグ移動中のThumbとその他のThumbとの重なり合う部分の面積を計算、
        /// 一定以上の面積があった場合、場所を入れ替えて整列
        /// </summary>
        /// <param name="t">ドラッグ移動中のThumb</param>
        /// <param name="x">Canvas上でのX座標</param>
        /// <param name="y">Canvas上でのY座標</param>
        private void Idou移動中処理(FlatThumb t, double x, double y)
        {
            t.Opacity = 0.7;
            int imaIndex = MyThumbs.IndexOf(t);//ドラッグ移動中ThumbのIndex
            MyStatus.Content = imaIndex.ToString();

            //最寄りのPoint
            int moyoriIndex = 0;
            double moyori距離 = double.MaxValue;
            for (int i = 0; i < MyPoints.Count; i++)
            {
                double distance = GetDistance(MyPoints[i], new Point(x,y));
                if (distance < moyori距離)
                {
                    moyori距離 = distance;
                    moyoriIndex = i;
                }
            }

            //最短距離のIndexと移動中のThumbのIndexが違うなら入れ替え処理
            if (moyoriIndex != imaIndex)
            {
                //Thumbリストのindexを入れ替え
                MyThumbs.RemoveAt(imaIndex);
                MyThumbs.Insert(moyoriIndex, t);

                //indexに従って表示位置変更
                ReplaceThumb表示位置更新(moyoriIndex);
            }
        }

        //2点間距離
        private double GetDistance(Point a, Point b)
        {
            return Math.Sqrt((a - b) * (a - b));
        }


        /// <summary>
        /// すべてのThumbの表示位置を更新
        /// </summary>
        /// <param name="avoidIndex">ドラッグ移動中のThumbのindex</param>
        private void ReplaceThumb表示位置更新(int avoidIndex)
        {
            for (int i = 0; i < MyThumbs.Count; i++)
            {
                if (i == avoidIndex) continue;//移動中のThumbは変更しない
                Canvas.SetLeft(MyThumbs[i], MyPoints[i].X);
                Canvas.SetTop(MyThumbs[i], MyPoints[i].Y);
            }
        }

        //ドラッグ移動イベント時
        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            //移動
            FlatThumb t = sender as FlatThumb;
            double x = Canvas.GetLeft(t) + e.HorizontalChange;
            double y = Canvas.GetTop(t) + e.VerticalChange;
            Canvas.SetLeft(t, x);
            Canvas.SetTop(t, y);
            t.Opacity = 0.5;

            Idou移動中処理(t, x, y);
        }
    }



    public class FlatThumb : Thumb
    {
        private Grid MyPanel;

        public FlatThumb(string text)
        {
            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Grid), "panel");
            this.Template = template;
            this.ApplyTemplate();
            MyPanel = (Grid)template.FindName("panel", this);
            MyPanel.Background = Brushes.Aqua;

            MyPanel.Children.Add(new TextBlock()
            {
                Text = text,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
            MyPanel.Children.Add(new Border()
            {
                BorderBrush = Brushes.MediumBlue,
                BorderThickness = new Thickness(1)
            });
        }
    }
}
