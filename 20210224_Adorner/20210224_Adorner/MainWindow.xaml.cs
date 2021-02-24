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


namespace _20210224_Adorner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }
    }

    public class SimpleCircleAdorner : Adorner
    {
        // Be sure to call the base class constructor.
        public SimpleCircleAdorner(UIElement adornedElement)
          : base(adornedElement)
        {
        }

        // A common way to implement an adorner's rendering behavior is to override the OnRender
        // method, which is called by the layout system as part of a rendering pass.
        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);

            // Some arbitrary drawing implements.
            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Green);
            renderBrush.Opacity = 0.2;
            Pen renderPen = new Pen(new SolidColorBrush(Colors.Navy), 1.5);
            double renderRadius = 5.0;

            // Draw a circle at each corner.
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopLeft, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopRight, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomLeft, renderRadius, renderRadius);
            drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomRight, renderRadius, renderRadius);
        }
    }





//    C#のWPFでサイズ変更できるTextBoxを作る - Ararami Studio
//https://araramistudio.jimdo.com/2016/12/08/wpf%E3%81%A7%E3%82%B5%E3%82%A4%E3%82%BA%E5%A4%89%E6%9B%B4%E3%81%A7%E3%81%8D%E3%82%8Btextbox%E3%82%92%E4%BD%9C%E3%82%8B/

    public class ResizingAdorner : Adorner
    {
        private Thumb resizeGrip;
        private VisualCollection visualChildren;


        public ResizingAdorner(UIElement adornedElement) : base(adornedElement)
        {
            //サイズ変更用コントロールを作成
            resizeGrip = new Thumb();
            resizeGrip.Cursor = Cursors.SizeNWSE;
            resizeGrip.Width = 18;
            resizeGrip.Height = 18;
            resizeGrip.DragDelta += new DragDeltaEventHandler(ResizeGripDragDelta);

            //Thumbコントロールの見た目を変える
            var p1 = new FrameworkElementFactory(typeof(Path));
            p1.SetValue(Path.FillProperty, new SolidColorBrush(Colors.White));
            p1.SetValue(Path.DataProperty, Geometry.Parse("M0,14L14,0L14,14z"));
            var p2 = new FrameworkElementFactory(typeof(Path));
            p2.SetValue(Path.StrokeProperty, new SolidColorBrush(Colors.LightGray));
            p2.SetValue(Path.DataProperty, Geometry.Parse("M0,14L14,0"));
            var p3 = new FrameworkElementFactory(typeof(Path));
            p3.SetValue(Path.StrokeProperty, new SolidColorBrush(Colors.LightGray));
            p3.SetValue(Path.DataProperty, Geometry.Parse("M4,14L14,4"));
            var p4 = new FrameworkElementFactory(typeof(Path));
            p4.SetValue(Path.StrokeProperty, new SolidColorBrush(Colors.LightGray));
            p4.SetValue(Path.DataProperty, Geometry.Parse("M8,14L14,8"));
            var p5 = new FrameworkElementFactory(typeof(Path));
            p5.SetValue(Path.StrokeProperty, new SolidColorBrush(Colors.LightGray));
            p5.SetValue(Path.DataProperty, Geometry.Parse("M12,14L14,12"));

            var grid = new FrameworkElementFactory(typeof(Grid));
            grid.SetValue(Grid.MarginProperty, new Thickness(2));
            grid.AppendChild(p1);
            grid.AppendChild(p2);
            grid.AppendChild(p3);
            grid.AppendChild(p4);
            grid.AppendChild(p5);

            var template = new ControlTemplate(typeof(Thumb));
            template.VisualTree = grid;
            resizeGrip.Template = template;

            //作成したコントロールの管理
            visualChildren = new VisualCollection(this);
            visualChildren.Add(resizeGrip);
        }

        //ドラッグされたらTextBoxのサイズを変える
        private void ResizeGripDragDelta(object sender, DragDeltaEventArgs e)
        {
            var element = this.AdornedElement as FrameworkElement;

            var w = element.Width;
            var h = element.Height;
            if (w.Equals(Double.NaN))
                w = element.DesiredSize.Width;
            if (h.Equals(Double.NaN))
                h = element.DesiredSize.Height;

            w += e.HorizontalChange;
            h += e.VerticalChange;
            w = Math.Max(resizeGrip.Width, w);
            h = Math.Max(resizeGrip.Height, h);
            w = Math.Max(element.MinWidth, w);
            h = Math.Max(element.MinHeight, h);
            w = Math.Min(element.MaxWidth, w);
            h = Math.Min(element.MaxHeight, h);

            element.Width = w;
            element.Height = h;
        }

        //サイズが変更されたらThumbコントロールの位置を変える
        protected override Size ArrangeOverride(Size finalSize)
        {
            var element = this.AdornedElement as FrameworkElement;

            var w = resizeGrip.Width;
            var h = resizeGrip.Height;
            var x = element.ActualWidth - w;
            var y = element.ActualHeight - h;

            resizeGrip.Arrange(new Rect(x, y, w, h));

            return finalSize;
        }

        //Adornerで既存コントロールを管理する為のお約束
        protected override int VisualChildrenCount
        {
            get { return visualChildren.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return visualChildren[index];
        }
    }


    public class ResizableTextBox : TextBox
    {
        private void InitializeAdorner(object sender, RoutedEventArgs e)
        {
            var layer = AdornerLayer.GetAdornerLayer(this);
            var adrner = new ResizingAdorner(this);
            layer.Add(adrner);
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Loaded += new RoutedEventHandler(InitializeAdorner);
        }
        
    }
    public class MyTextBox : TextBox
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Loaded += MyTextBox_Loaded;
        }

        private void MyTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var layer = AdornerLayer.GetAdornerLayer(this);
            var adrner = new ResizingAdorner(this);
            layer.Add(adrner);
        }
    }
    public class MyTextBox2 : TextBox
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Loaded += (s, e) =>
            {
                var layer = AdornerLayer.GetAdornerLayer(this);
                var adrner = new ResizingAdorner(this);
                layer.Add(adrner);
            };
        }

    }
    public class MyTextBox3 : TextBox
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Loaded += (s, e) =>
            {
                var layer = AdornerLayer.GetAdornerLayer(this);
                var adrner = new SimpleCircleAdorner(this);
                layer.Add(adrner);
            };
        }

    }



}
