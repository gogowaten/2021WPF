﻿using System;
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
using System.Collections.ObjectModel;

namespace _20210227_thumb移動入れ替え
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<FlatThumb> MyThumbs;
        private double moto重なり面積;
        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();

        }

        private void MyInitialize()
        {
            MyThumbs = new();
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    FlatThumb t = new($"{y * 3 + x}") { Width = 100, Height = 100, Opacity = 0.7, FontSize = 40 };
                    MyCanvas.Children.Add(t);
                    Canvas.SetLeft(t, x * 100);
                    Canvas.SetTop(t, y * 100);
                    t.DragDelta += Thumb_DragDelta;
                    t.DragCompleted += Thumb_DragCompleted;
                    MyThumbs.Add(t);
                }
            }
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            FlatThumb t = sender as FlatThumb;
            Idoutyuu(t);
            var p = MyCanvas.PointToScreen(new Point());
            RectangleGeometry tg = GetGeometry(t, p);
            int left = (int)((tg.Bounds.Left / 100.0) + 0.5) * 100;
            int top = (int)((tg.Bounds.Top / 100.0) + 0.5) * 100;
            Canvas.SetLeft(t, left);
            Canvas.SetTop(t, top);
            MyStatus.Content = "";

        }

        private void Idoutyuu(FlatThumb t)
        {
            t.Opacity = 0.7;
            var p = MyCanvas.PointToScreen(new Point());
            RectangleGeometry tg = GetGeometry(t, p);
            List<double> kasanai面積 = new();
            int tID = MyThumbs.IndexOf(t);//ドラッグ移動中ThumbのIndex
            MyStatus.Content = tID.ToString();

            //重なり面積のリスト作成
            for (int i = 0; i < MyThumbs.Count; i++)
            {
                Rect bound = Geometry.Combine(tg, GetGeometry(MyThumbs[i], p), GeometryCombineMode.Intersect, null).Bounds;
                if (bound == Rect.Empty)
                {
                    kasanai面積.Add(0);
                }
                else
                {
                    kasanai面積.Add(bound.Width * bound.Height);
                }
            }

            //重なり面積が4000以上のThumbがあれば、移動中Thumbと入れ替える
            int saki = 0;
            double max = 0;
            for (int i = 0; i < kasanai面積.Count; i++)
            {
                if (max < kasanai面積[i] && i != tID)
                {
                    max = kasanai面積[i];
                    saki = i;
                }
            }

            //元の位置との重なり面積
            int x = tID % 3 * 100;
            int y = tID / 3 * 100;
            var motoRect = new RectangleGeometry(new Rect(new Point(x, y), new Size(100, 100)));
            var moto重なりbound = Geometry.Combine(motoRect, tg, GeometryCombineMode.Intersect, null).Bounds;
            var moto重なり面積temp = moto重なりbound.Width * moto重なりbound.Height;

            if (max > 4000 && moto重なり面積 > moto重なり面積temp)
            {
                Idou(saki, tID, t, tg);
            }
            else
            {
                moto重なり面積 = moto重なり面積temp;
            }
        }

        //Thumbの場所入れ替え
        private void Idou(int saki, int tID, FlatThumb t, RectangleGeometry tg)
        {
            if (saki > tID)
            {
                for (int tt = tID + 1; tt <= saki; tt++)
                {
                    FlatThumb target = MyThumbs[tt];
                    double left = Canvas.GetLeft(target);
                    double top = Canvas.GetTop(target);
                    if (left >= 100)
                    {
                        Canvas.SetLeft(target, left - 100);
                    }
                    else
                    {
                        if (top >= 100)
                        {
                            Canvas.SetTop(target, top - 100);
                            Canvas.SetLeft(target, 200);
                        }
                    }
                }
                MyThumbs.RemoveAt(tID);
                MyThumbs.Insert(saki, t);
            }
            else
            {
                for (int tt = saki; tt < tID; tt++)
                {
                    FlatThumb target = MyThumbs[tt];
                    double left = Canvas.GetLeft(target);
                    double top = Canvas.GetTop(target);
                    if (left + 100 <= 200)
                    {
                        Canvas.SetLeft(target, left + 100);
                    }
                    else
                    {
                        if (top + 100 <= 200)
                        {
                            Canvas.SetTop(target, top + 100);
                            Canvas.SetLeft(target, 0);
                        }
                    }
                }
                MyThumbs.RemoveAt(tID);
                MyThumbs.Insert(saki, t);
            }

            //元の位置との重なり面積
            int x = saki % 3 * 100;
            int y = saki / 3 * 100;
            var motoRect = new RectangleGeometry(new Rect(new Point(x, y), new Size(100, 100)));
            var moto重なりbound = Geometry.Combine(motoRect, tg, GeometryCombineMode.Intersect, null).Bounds;
            moto重なり面積 = moto重なりbound.Width * moto重なりbound.Height;

        }

        private RectangleGeometry GetGeometry(FlatThumb t, Point p)
        {
            Rect tr = new((Point)Point.Subtract(t.PointToScreen(new Point()), p), new Size(100, 100));
            RectangleGeometry tg = new(tr);
            return tg;
        }


        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            FlatThumb t = sender as FlatThumb;
            Canvas.SetLeft(t, Canvas.GetLeft(t) + e.HorizontalChange);
            Canvas.SetTop(t, Canvas.GetTop(t) + e.VerticalChange);
            t.Opacity = 0.5;

            Idoutyuu(t);
        }
    }

    public class FlatThumb : Thumb
    {
        private Grid MyPanel;
        private static int ElementCount;
        public FlatThumb(string text)
        {
            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Grid), "panel");
            this.Template = template;
            this.ApplyTemplate();
            MyPanel = (Grid)template.FindName("panel", this);
            MyPanel.Background = Brushes.MediumAquamarine;

            MyPanel.Children.Add(new TextBlock()
            {
                Text = text,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
            ElementCount++;
        }
    }
}
