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

namespace _20210305_Thumb移動入れ替え
{
    public partial class MainWindow : Window
    {
        private List<FlatThumb> MyLeftThumbs = new();
        private List<FlatThumb> MyRightThumbs = new();
        private List<Point> MyLeftPoint = new();
        private List<Point> MyRightPoint = new();
        private const int MASU_X = 2;
        private const int MASU_Y = 2;
        private const int MASU_W = 100;
        private const int MASU_H = 100;
        private int MyThumbsCount = 11;

        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();

        }
        private void MyInitialize()
        {
            int displayCount = MASU_X * MASU_Y;
            int stockCount = MyThumbsCount - displayCount;
            for (int i = 0; i < displayCount; i++)
            {
                FlatThumb t = MakeThumb($"t{i}");
                MyRightCanvas.Children.Add(t);
                MyRightThumbs.Add(t);
                MyRightPoint.Add(new Point(i % MASU_X * MASU_W, i / MASU_Y * MASU_H));
            }
            for (int i = displayCount; i < MyThumbsCount; i++)
            {
                FlatThumb t = MakeThumb($"t{i}");
                MyLeftThumbs.Add(t);
                MyLeftCanvas.Children.Add(t);
                int ii = i - displayCount;
                MyLeftPoint.Add(new Point(ii % MASU_X * MASU_W, ii / MASU_Y * MASU_H));
            }

            Replace();

            MyRightCanvas.Width = MASU_W * MASU_X;
            MyRightCanvas.Height = MASU_H * MASU_Y;
            MyLeftCanvas.Width = MASU_W * MASU_X;
            MyLeftCanvas.Height = (int)((stockCount / (double)MASU_X + 0.5) * MASU_H);

        }


        private void Replace()
        {
            for (int i = 0; i < MyRightThumbs.Count; i++)
            {
                SetOnCanvas(MyRightThumbs[i], MyRightPoint[i]);
            }
            for (int i = 0; i < MyLeftThumbs.Count; i++)
            {
                SetOnCanvas(MyLeftThumbs[i], MyLeftPoint[i]);
            }
        }
        private void SetOnCanvas(FlatThumb t, Point p)
        {
            Canvas.SetLeft(t, p.X);
            Canvas.SetTop(t, p.Y);
        }
        private FlatThumb MakeThumb(string text)
        {
            FlatThumb t = new(text)
            {
                FontSize = 40,
                Opacity = 0.7,
                Width = MASU_W,
                Height = MASU_H,
            };
            return t;
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
