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

namespace _20211213_FormattedTextと文字の装飾
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BitmapSource MyBitmap { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            MyTextBox.TextChanged += MyTextBox_TextChanged;
            MyImage.DataContext = this;
        }

        private void MyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //throw new NotImplementedException();
            if (MyTextBox.Text.Trim().Length == 0) { return; }

            FormattedText formattedText = new(
                MyTextBox.Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    MyTextBox.FontFamily,
                    MyTextBox.FontStyle,
                    MyTextBox.FontWeight,
                    MyTextBox.FontStretch),
                MyTextBox.FontSize,
                Brushes.Red,
                96);

            DrawingVisual dv = new();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawText(formattedText, new Point());
            }

            int w = (int)formattedText.Width;
            int h = (int)formattedText.Height;
            RenderTargetBitmap bitmap = new(w, h, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(dv);
            MyImage.Source = bitmap;
        }
    }

    public class FormattedTextBox : TextBox
    {
        public FormattedTextBox()
        {
            Foreground = Brushes.Transparent;
            Background = Brushes.Transparent;
        }
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            InvalidateVisual();
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            //base.OnRender(drawingContext);
            if (Text.Trim().Length == 0) { return; }

            FormattedText formattedText = new(
                Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Brushes.Gray,
                96);
            TextDecoration textDecoration = new();
            textDecoration.Pen = new Pen(Brushes.Red, 1);
            textDecoration.Location = TextDecorationLocation.Underline;
            textDecoration.PenOffset = 0;
            TextDecorationCollection decorations = new();
            decorations.Add(textDecoration);
            textDecoration = new();
            textDecoration.Pen = new Pen(Brushes.MediumVioletRed, 1);
            textDecoration.Location = TextDecorationLocation.Underline;
            textDecoration.PenOffset = 1;
            decorations.Add(textDecoration);
            
            formattedText.SetTextDecorations(decorations);
            drawingContext.DrawText(formattedText, new Point());
        }
    }
}
