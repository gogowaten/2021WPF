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


namespace _20211122_textbox_template
{
    public class MyTextBox : TextBox
    {
        public MyTextBox()
        {
            Foreground = Brushes.Transparent;
            Background = Brushes.Transparent;
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
