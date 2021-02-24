
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;


//
//ControlTemplateを使って見た目がフラットなThumb
//ベースにするPanelにCanvasとGridを使った場合の違いを見てみたけど、ほとんど同じだった
//Canvasのほうがいいかなあ、Thumbの中にThumbを入れたときも表示位置をかんたんに指定できる

namespace _20210224_FlatThumb移動Image
{

    /// <summary>
    /// ControlTemplateのVisualTreeのトップがCanvasなThumb
    /// </summary>
    class FlatThumbFromCanvasBase : Thumb
    {
        private const string BASE_CANVAS_NAME = "canvas";
        public Canvas TemplateBase;

        public FlatThumbFromCanvasBase(double left, double top)
        {
            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Canvas), BASE_CANVAS_NAME);
            this.Template = template;
            this.ApplyTemplate();//テンプレート再構築、これで中の要素を名前で検索取得できるようになる            
            TemplateBase = (Canvas)this.Template.FindName(BASE_CANVAS_NAME, this);
            //TemplateBaseCanvas.Background = new SolidColorBrush(SystemColors.ControlColor);
            Canvas.SetLeft(this, left);
            Canvas.SetTop(this, top);


            this.DragDelta += FlatThumb_DragDelta;
        }

        private void FlatThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            //var t = sender as FlatThumbFromCanvasBase;
            //var name = t.Name;
            //var flag1 = t.IsDragging;
            //var flag2 = t.IsEnabled;
            //var flag3 = t.IsEnabledCore;
            //var flag4 = t.IsMouseCaptureWithin;
            //if (flag1 == false) return;
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
            Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
        }
      
    }

    /// <summary>
    /// ControlTemplateのVisualTreeのトップがGridなThumb
    /// </summary>
    class FlatThumbFromGridBase : Thumb
    {
        string BASE_NAME = "grid";
        public Grid TemplateBase;

        public FlatThumbFromGridBase(double left, double top)
        {
            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Grid), BASE_NAME);
            this.Template = template;
            this.ApplyTemplate();//テンプレート再構築、これで中の要素を名前で検索取得できるようになる            
            TemplateBase = (Grid)this.Template.FindName(BASE_NAME, this);
            //TemplateBaseCanvas.Background = new SolidColorBrush(SystemColors.ControlColor);
            Canvas.SetLeft(this, left);
            Canvas.SetTop(this, top);


            this.DragDelta += FlatThumb_DragDelta;
        }

        private void FlatThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.HorizontalChange);
            Canvas.SetTop(this, Canvas.GetTop(this) + e.VerticalChange);
        }
    }







}
