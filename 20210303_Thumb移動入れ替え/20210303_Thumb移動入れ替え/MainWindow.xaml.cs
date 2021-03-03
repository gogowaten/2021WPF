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
using System.Windows.Controls.Primitives;


namespace _20210303_Thumb移動入れ替え
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<FlatThumb> MyWrapPanelThumbs = new();
        private List<FlatThumb> MyCanvasThumbs = new();
        private List<FlatThumb> MyThumbs = new();
        private List<Point> MyPoints = new();
        private const int MASU_X = 2;
        private const int MASU_Y = 2;
        private const int MASU_W = 100;
        private const int MASU_H = 100;
        private int MyThumbsCount = 8;

        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();




            MyListBox1.AllowDrop = true;
            MyListBox1.DragOver += MyListBox1_DragOver;
            MyListBox1.PreviewMouseLeftButtonDown += MyListBox1_PreviewMouseLeftButtonDown;
        }

        private void MyInitialize()
        {
            for (int i = 0; i < MyThumbsCount; i++)
            {
                FlatThumb t = new($"t{i}")
                {
                    FontSize = 40,
                    Opacity = 0.7,
                    Width = MASU_W,
                    Height = MASU_H,
                };
                MyThumbs.Add(t);
                t.DragDelta += T_DragDelta;
                t.DragCompleted += T_DragCompleted;

                int count = MASU_X * MASU_Y;
                if (i < count)
                {
                    MyCanvas.Children.Add(t);
                    int x = i % MASU_X * MASU_W;
                    int y = i / MASU_Y * MASU_H;
                    MyPoints.Add(new Point(x, y));
                }
                else
                {
                    MyStockCanvas.Children.Add(t);
                    int x = (i - count) % MASU_X * MASU_W;
                    int y = (i - count) / MASU_Y * MASU_H;
                    MyPoints.Add(new Point(x, y));
                }
            }

            Replace();
        }

        private double GetDistance(Point a, Point b)
        {
            return Math.Sqrt((a - b) * (a - b));
        }

        private void T_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var t = sender as FlatThumb;
            if (t != null)
            {
                t.Opacity = 0.7;
            }
        }

        private void T_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var t = sender as FlatThumb;
            if (t != null)
            {
                t.Opacity = 0.5;
                double x = Canvas.GetLeft(t) + e.HorizontalChange;
                double y = Canvas.GetTop(t) + e.VerticalChange;
                Canvas.SetLeft(t, x);
                Canvas.SetTop(t, y);

                Idou移動中処理(t, x, y);
            }
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

            //最寄りのPointIndex取得
            int moyoriIndex = GetMoyoriIndex(imaIndex, x, y);

            //移動中のThumbのIndexと最寄りのIndexが異なれば交換か挿入を発生
            int count = MASU_X * MASU_Y;
            //最寄りのIndexが別のCanvasなら交換、
            //同じCanvasなら挿入            
            if (moyoriIndex != imaIndex)
            {
                if ((imaIndex < count && moyoriIndex < count) || (imaIndex >= count && moyoriIndex >= count))
                {
                    //同じCanvas
                    //Thumbリストのindexを入れ替え
                    MyThumbs.RemoveAt(imaIndex);
                    MyThumbs.Insert(moyoriIndex, t);

                    //indexに従って表示位置変更
                    Replace(moyoriIndex);
                }
                else
                {

                    //異なるCanvas
                    int min = Math.Min(imaIndex, moyoriIndex);
                    int max = Math.Max(imaIndex, moyoriIndex);
                    FlatThumb tMin = MyThumbs[min];
                    FlatThumb tMax = MyThumbs[max];


                    MyCanvas.Children.Remove(MyThumbs[min]);
                    MyStockCanvas.Children.Remove(MyThumbs[max]);
                    MyCanvas.Children.Add(tMax);
                    MyStockCanvas.Children.Add(tMin);



                    MyThumbs.Remove(tMin);
                    MyThumbs.Insert(max, tMin);
                    MyThumbs.Remove(tMax);
                    MyThumbs.Insert(min, tMax);

                    if (imaIndex < count)
                    {
                        Replace(max);
                    }
                    else Replace(min);

                }


            }
            //最短距離のPointを取得
        }

        private int GetMoyoriIndex(int imaIndex, double x, double y)
        {
            int count = MASU_X * MASU_Y;
            //2つのCanvasの位置を考慮して計算
            var migi = MyCanvas.PointToScreen(new Point());
            var hidari = MyStockCanvas.PointToScreen(new Point());
            //移動中のThumbが左Canvasの場合
            double xDiff = 0;
            double yDiff = 0;

            xDiff = migi.X - hidari.X;
            yDiff = migi.Y - hidari.Y;

            //右のThumbなら右と比較ならそのまま比較
            //右Thumbと左の比較はThumbにDiffを足して比較
            //左Thumbと左の比較はそのまま比較
            //左Thumbと右の比較はThumbにDiffを足して比較
            //最寄りのPoint
            int moyoriIndex = 0;
            double moyori距離 = double.MaxValue;

            for (int i = 0; i < MyPoints.Count; i++)
            {
                double distance;
                if ((imaIndex < count && i < count) || (imaIndex >= count && i >= count))
                {
                    distance = GetDistance(MyPoints[i], new Point(x, y));
                }
                else if (imaIndex < count && i >= count)
                {
                    distance = GetDistance(MyPoints[i], new Point(x + xDiff, y + yDiff));
                }
                else
                {
                    distance = GetDistance(MyPoints[i], new Point(x - xDiff, y - yDiff));
                }


              
                if (distance < moyori距離)
                {
                    moyori距離 = distance;
                    moyoriIndex = i;
                }
            }
            return moyoriIndex;
        }

        private void Replace(int avoid = -1)
        {
            for (int i = 0; i < MyPoints.Count; i++)
            {
                if (i == avoid) continue;
                SetCanvasLocate(MyThumbs[i], MyPoints[i]);
            }
        }

        private void SetCanvasLocate(FlatThumb t, Point p)
        {
            Canvas.SetLeft(t, p.X);
            Canvas.SetTop(t, p.Y);
        }



        private void MyListBox1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var lb = sender as ListBox;
            if (lb != null && lb.SelectedItem != null)
                DragDrop.DoDragDrop(lb, lb.SelectedItem, DragDropEffects.Move);
        }

        private void MyListBox1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListBoxItem)))
                e.Effects = DragDropEffects.Move;
        }
    }



    public class FlatThumb : Thumb
    {
        private Grid MyPanel;
        private string Text;

        public FlatThumb(string text)
        {
            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Grid), "panel");
            this.Template = template;
            this.ApplyTemplate();
            MyPanel = (Grid)template.FindName("panel", this);
            MyPanel.Background = Brushes.Aqua;
            this.Text = text;

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

        public override string ToString()
        {
            return Text;
            //return base.ToString();
        }
    }








}
