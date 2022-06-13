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


using System.ComponentModel;

namespace _20211122_textbox_template
{
    public class MyTextBox : TextBox, INotifyPropertyChanged
    {
        private Brush _foregroundBrush;
        public Brush ForegroundBrush
        {
            get => _foregroundBrush;
            set
            {
                if (value == _foregroundBrush) { return; }
                _foregroundBrush = value;
                OnPropertyChanged();
                InvalidateVisual();
            }
        }
        private Brush _penBrush;
        public Brush PenBrush
        {
            get => _penBrush;
            set
            {
                if (value == _penBrush) { return; }
                _penBrush = value;
                OnPropertyChanged();
                InvalidateVisual();
            }
        }

        public MyTextBox()
        {
            Foreground = Brushes.Transparent;
            Background = Brushes.Transparent;
            ForegroundBrush = Brushes.White;
            PenBrush = Brushes.Gray;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
            drawingContext.DrawGeometry(ForegroundBrush, new Pen(PenBrush, 1), geo);
            //drawingContext.DrawGeometry(Brushes.Green, new Pen(Brushes.Cyan, 2), geo);

        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            InvalidateVisual();
        }
    }
}
