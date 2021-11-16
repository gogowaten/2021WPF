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

//[C# WPF] なんとかしてWPFの描画を速くしたい「Canvas.Childrenへのオブジェクト追加」 – Pelican Philosophy
//https://www.peliphilo.net/archives/2390

//速い canvas4 >> canvas2 > canvas3 >> canvas1 遅い
namespace _20211116_描画速度canvas
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Square> MySquareList = new();
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            List<Square> squares = new();
            Random randH = new Random(DateTime.Now.Millisecond);
            Random randV = new Random(DateTime.Now.Millisecond + 1);

            for (int i = 0; i < 110000; i++)
            {
                Square s = new();
                Canvas.SetLeft(s, randH.Next(100000));
                Canvas.SetTop(s, randV.Next(300));
                s.Width = 5;
                s.Height = 5;
                _ = xCanvas1.Children.Add(s);
                squares.Add(s);

                s = new();
                Canvas.SetLeft(s, randH.Next(100000));
                Canvas.SetTop(s, randV.Next(300));
                s.Width = 5;
                s.Height = 5;
                MySquareList.Add(s);
                
            }
            UpdateViewport();

            xSquares.MyList = squares;
            xSquares.InvalidateVisual();



            xCanvas4Squares.SV = MyScrollViewer2;
            xCanvas4Squares.SquareList = MySquareList;
            xCanvas4Squares.InvalidateVisual();
        }

        private void UpdateViewport()
        {
            Rect viewRect = new(
                MyScrollViewer1.HorizontalOffset, MyScrollViewer1.VerticalOffset,
                MyScrollViewer1.ViewportWidth, MyScrollViewer1.ViewportHeight);
            foreach (Square item in MySquareList)
            {
                Rect r = new(Canvas.GetLeft(item), Canvas.GetTop(item), item.Width, item.Height);
                if (viewRect.IntersectsWith(r))
                {
                    if (xCanvas3.Children.Contains(item) == false)
                    {
                        xCanvas3.Children.Add(item);
                    }
                }
                else
                {
                    if (xCanvas3.Children.Contains(item))
                    {
                        xCanvas3.Children.Remove(item);
                    }
                }
            }
        }
        private void MyScrollViewer1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateViewport();
        }

        private void MyScrollViewer2_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //再描画
            xCanvas4Squares.InvalidateVisual();
        }
    }

    public class Square : Control
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawRectangle(Brushes.MediumAquamarine, null, new Rect(0, 0, Width, Height));
        }
    }
    public class Squares : Control
    {
        public List<Square> MyList;
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (MyList == null) { return; }
            foreach (Square item in MyList)
            {
                Rect r = new(Canvas.GetLeft(item), Canvas.GetTop(item), item.Width, item.Height);
                drawingContext.DrawRectangle(Brushes.MediumOrchid, null, r);
            }
        }
    }

    public class Squares2 : Control
    {
        public ScrollViewer SV;
        public List<Square> SquareList;

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (SV == null || SquareList == null) { return; }

            Rect viewRect = new(SV.HorizontalOffset, SV.VerticalOffset, SV.ViewportWidth, SV.ViewportHeight);
            foreach (Square item in SquareList)
            {
                Rect r = new(Canvas.GetLeft(item), Canvas.GetTop(item), item.Width, item.Height);
                if (viewRect.IntersectsWith(r))
                {
                    drawingContext.DrawRectangle(Brushes.MediumSpringGreen, null, r);
                }
            }
        }
    }
}
