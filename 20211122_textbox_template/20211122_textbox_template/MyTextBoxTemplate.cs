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

using System.Globalization;
//テキストボックスの枠表示を消すだけならControlTemplateは必要なかった

namespace _20211122_textbox_template
{
    internal class MyTextBoxTemplate : TextBox
    {
        public Grid RootPanel;

        public MyTextBoxTemplate()
        {
            Foreground = Brushes.Transparent;
            Background = Brushes.Transparent;

            //ControlTemplate template = new(typeof(TextBox));
            //template.VisualTree = new FrameworkElementFactory(typeof(Grid), "root");
            //Template = template;
            //ApplyTemplate();
            //RootPanel = (Grid)template.FindName("root", this);

            //RootPanel.Background = Brushes.GreenYellow;

            //ScrollViewer scrollViewer = new();
            //scrollViewer.VerticalAlignment = VerticalAlignment.Center;

            //ContentPresenter contentPresenter = new();
            //contentPresenter.Content = new TextBox();
            //RootPanel.Children.Add(scrollViewer);
            //RootPanel.Children.Add(contentPresenter);


            //枠表示なし
            this.BorderThickness = new Thickness(0);
            //this.BorderBrush = null;
            


        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            FormattedText formatted = new(
                Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    FontFamily,
                    FontStyle,
                    FontWeight,
                    FontStretch),
                FontSize,
                Brushes.Red,
                96);

            //縁取りが必要なければ
            //drawingContext.DrawText(formatted, new Point());

            //縁取りありはGeometry
            Geometry geo = formatted.BuildGeometry(new Point());
            drawingContext.DrawGeometry(Brushes.Green, new Pen(Brushes.Cyan, 2), geo);

        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            InvalidateVisual();
        }



    }
}
