using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;

//WPF、マス目に敷き詰めたThumb、マウスドラッグ移動で入れ替え - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2021/03/01/150751


//左上から右下へ敷き詰められた3x3個のThumb
//マウスドラッグ移動で移動先のThumbと入れ替える感じの動き
//移動先にあるThumbから後ろのThumbすべてを1個づつ右下へ移動させる

namespace _20210227_thumb移動入れ替え
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<FlatThumb> MyThumbs;
        //ドラッグ移動中のThumbのRectと、MyThumbsのIndexに対応するRectとの重なり合う面積保持用
        private double moto重なり面積;
        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();

        }

        //Thumbを9個作成、表示
        private void MyInitialize()
        {
            MyThumbs = new();
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    FlatThumb t = new($"{(y * 3) + x}")
                    {
                        Width = 100,
                        Height = 100,
                        Opacity = 0.7,
                        FontSize = 40
                    };
                    MyCanvas.Children.Add(t);
                    Canvas.SetLeft(t, x * 100);
                    Canvas.SetTop(t, y * 100);
                    t.DragDelta += Thumb_DragDelta;
                    t.DragCompleted += Thumb_DragCompleted;
                    MyThumbs.Add(t);
                }
            }
        }

        //ドラッグ移動終了イベント時、
        //最寄りというか空いている(あるべき)場所に移動させる
        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            FlatThumb t = sender as FlatThumb;
            int imaIndex = MyThumbs.IndexOf(t);
            int x = imaIndex % 3 * 100;//元のx座標
            int y = imaIndex / 3 * 100;
            Canvas.SetLeft(t, x);
            Canvas.SetTop(t, y);

            MyStatus.Content = "";

        }

        /// <summary>
        /// ドラッグ移動中のThumbとその他のThumbとの重なり合う部分の面積を計算、
        /// 一定以上の面積があった場合、場所を入れ替えて整列
        /// </summary>
        /// <param name="t">ドラッグ移動中のThumb</param>
        private void Idou移動中処理(FlatThumb t)
        {
            t.Opacity = 0.7;
            var p = MyCanvas.PointToScreen(new Point());
            RectangleGeometry geometry = GetThumbGeometry(t, p);
            int imaIndex = MyThumbs.IndexOf(t);//ドラッグ移動中ThumbのIndex
            MyStatus.Content = imaIndex.ToString();

            //重なり面積のリスト作成
            List<double> kasanai面積 = MakeList(geometry, p);

            //面積最大のindex取得、これはIndexになる
            int sakiIndex = 0;
            double max = 0;
            for (int i = 0; i < kasanai面積.Count; i++)
            {
                if (max < kasanai面積[i] && i != imaIndex)
                {
                    max = kasanai面積[i];
                    sakiIndex = i;
                }
            }

            //移動中ThumbのIndexに対応するRectと、移動中ThumbのRect重なり面積
            int x = imaIndex % 3 * 100;//元のx座標
            int y = imaIndex / 3 * 100;
            var idxGeo = new RectangleGeometry(new Rect(new Point(x, y), new Size(100, 100)));
            var ima重なりbound = Geometry.Combine(idxGeo, geometry, GeometryCombineMode.Intersect, null).Bounds;
            var ima重なり面積 = ima重なりbound.Width * ima重なりbound.Height;

            //重なり面積が4000(4割)以上のThumbがある
            //and indexRectとの重なり面積が直前より減っていたら
            //移動中Thumbと入れ替える
            if (max > 4000 && moto重なり面積 > ima重なり面積)
            {
                //Thumbのindexと場所並べ替え
                SortIndexPlace(sakiIndex, imaIndex, t, geometry);
            }
            else
            {
                moto重なり面積 = ima重なり面積;
            }
        }


        /// <summary>
        /// 移動中Thumbとの重なり面積のリスト作成
        /// </summary>
        /// <param name="geometry">移動中ThumbのRectangleGeometry</param>
        /// <param name="p">基準になる座標</param>
        /// <returns></returns>
        private List<double> MakeList(RectangleGeometry geometry, Point p)
        {
            List<double> kasanai面積 = new();
            for (int i = 0; i < MyThumbs.Count; i++)
            {
                Rect bound = Geometry.Combine(
                    geometry,
                    GetThumbGeometry(MyThumbs[i], p),
                    GeometryCombineMode.Intersect, null).Bounds;

                if (bound == Rect.Empty)
                    kasanai面積.Add(0);
                else
                    kasanai面積.Add(bound.Width * bound.Height);
            }
            return kasanai面積;
        }

        /// <summary>
        /// Thumbのindexと場所並べ替え
        /// </summary>
        /// <param name="saki">移動中Thumbの新しいindex</param>
        /// <param name="tID">移動中Thumbのindex</param>
        /// <param name="t">移動中Thumb</param>
        /// <param name="tg">移動中ThumbのRectangleGeometry</param>
        private void SortIndexPlace(int saki, int tID, FlatThumb t, RectangleGeometry tg)
        {
            //今のindexより大きくなる場合は、そのindexより後ろのThumbを並べ替える
            if (saki > tID)
            {
                for (int tt = tID + 1; tt <= saki; tt++)
                {
                    FlatThumb target = MyThumbs[tt];
                    double left = Canvas.GetLeft(target);
                    double top = Canvas.GetTop(target);
                    //左端以外なら
                    if (left != 0)
                    {
                        //左へ移動
                        Canvas.SetLeft(target, left - 100);
                    }
                    else
                    {
                        //一段上の右端へ移動
                        if (top >= 100)
                        {
                            Canvas.SetTop(target, top - 100);
                            Canvas.SetLeft(target, 200);
                        }
                    }
                }

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

            }
            //index変更
            MyThumbs.RemoveAt(tID);
            MyThumbs.Insert(saki, t);

            //移動中Thumbのindexが変更されたので、対応するRectと今のThumbのRectとの重なり面積を再計算
            int x = saki % 3 * 100;
            int y = saki / 3 * 100;
            var imaRect = new RectangleGeometry(new Rect(new Point(x, y), new Size(100, 100)));
            var ima重なりbound = Geometry.Combine(imaRect, tg, GeometryCombineMode.Intersect, null).Bounds;
            moto重なり面積 = ima重なりbound.Width * ima重なりbound.Height;

        }

        //
        /// <summary>
        /// ThumbのRectanleGeometry作成
        /// </summary>
        /// <param name="t">Thumb</param>
        /// <param name="p">Thumbの親パネルの座標</param>
        /// <returns></returns>
        private RectangleGeometry GetThumbGeometry(FlatThumb t, Point p)
        {
            //親パネル上での座標なので、Thumbと親パネル座標で引き算Subtract
            return new(new Rect((Point)Point.Subtract(t.PointToScreen(new Point()), p), new Size(100, 100)));
        }

        //ドラッグ移動イベント時
        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            //移動
            FlatThumb t = sender as FlatThumb;
            Canvas.SetLeft(t, Canvas.GetLeft(t) + e.HorizontalChange);
            Canvas.SetTop(t, Canvas.GetTop(t) + e.VerticalChange);
            t.Opacity = 0.5;

            Idou移動中処理(t);
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
