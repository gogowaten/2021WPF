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

        public MainWindow()
        {
            InitializeComponent();

            TextDecoration textDecoration = new(TextDecorationLocation.Underline, new Pen(Brushes.Red, 1), 0, TextDecorationUnit.FontRecommended, TextDecorationUnit.FontRecommended);

            MyTextBox.TextDecorations = new() { textDecoration };

        }


    }

    public class BaseFormattedTextBox : TextBox
    {
        public BaseFormattedTextBox()
        {
            Foreground = Brushes.Transparent;
            Background = Brushes.Transparent;
        }
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            InvalidateVisual();
        }

    }

    /// <summary>
    /// アンダーライン
    /// </summary>
    public class FormattedTextBox1 : BaseFormattedTextBox
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            //base.OnRender(drawingContext);
            if (Text.Trim().Length == 0) { return; }

            FormattedText formattedText = new(
                Text,
                CultureInfo.CurrentCulture,
                FlowDirection.RightToLeft,
                new Typeface(FontFamily, FontStyles.Italic, FontWeights.Bold, FontStretch),
                FontSize,
                Brushes.Gray,
                96);formattedText.TextAlignment = TextAlignment.Right;
            drawingContext.DrawText(formattedText, new Point());
        }
    }

    /// <summary>
    /// 二重アンダーライン
    /// </summary>
    public class FormattedTextBox2 : BaseFormattedTextBox
    {
        
        protected override void OnRender(DrawingContext drawingContext)
        {
            //base.OnRender(drawingContext);
            if (Text.Trim().Length == 0) { return; }

            FormattedText formattedText = new(
                Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeights.Black, FontStretch),
                FontSize,
                Brushes.Gray,
                96);
            drawingContext.DrawText(formattedText, new Point());
        }
    }

    /// <summary>
    /// ベースライン
    /// </summary>
    public class FormattedTextBox3 : BaseFormattedTextBox
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            //base.OnRender(drawingContext);
            if (Text.Trim().Length == 0) { return; }

            FormattedText formattedText = new(
                Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeights.DemiBold, FontStretch),
                FontSize,
                Brushes.Gray,
                96);
            drawingContext.DrawText(formattedText, new Point());
        }
    }

    /// <summary>
    /// オーバーライン
    /// </summary>
    public class FormattedTextBox4 : BaseFormattedTextBox
    {
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
            textDecoration.Pen = new Pen(Brushes.Magenta, 1);
            textDecoration.Location = TextDecorationLocation.OverLine;
            textDecoration.PenOffset = 0;
            TextDecorationCollection decorations = new();
            decorations.Add(textDecoration);

            formattedText.SetTextDecorations(decorations);
            drawingContext.DrawText(formattedText, new Point());
        }
    }

    /// <summary>
    /// ストライクスルー打ち消し線
    /// </summary>
    public class FormattedTextBox5 : BaseFormattedTextBox
    {
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
            textDecoration.Pen = new Pen(Brushes.DeepPink, 1);
            textDecoration.Location = TextDecorationLocation.Strikethrough;
            textDecoration.PenOffset = 0;
            TextDecorationCollection decorations = new();
            decorations.Add(textDecoration);

            formattedText.SetTextDecorations(decorations);
            drawingContext.DrawText(formattedText, new Point());
        }
    }

    /// <summary>
    /// 全部
    /// </summary>
    public class FormattedTextBox6 : BaseFormattedTextBox
    {
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
            TextDecorationCollection decorations = new();
            decorations.Add(MakeTextDecoration(
                TextDecorationLocation.Strikethrough, new Pen(Brushes.DeepPink, 1), 0));
            decorations.Add(MakeTextDecoration(
                TextDecorationLocation.Baseline, new Pen(Brushes.DeepPink, 1), 0));
            decorations.Add(MakeTextDecoration(
                TextDecorationLocation.OverLine, new Pen(Brushes.DeepPink, 1), 0));
            decorations.Add(MakeTextDecoration(
                TextDecorationLocation.Underline, new Pen(Brushes.DeepPink, 1), 0));
            formattedText.SetTextDecorations(decorations);
            drawingContext.DrawText(formattedText, new Point());
        }
        private static TextDecoration MakeTextDecoration(TextDecorationLocation location, Pen pen, double offset = 0.0)
        {
            TextDecoration textDecoration = new();
            textDecoration.Pen = pen;
            textDecoration.Location = location;
            textDecoration.PenOffset = offset;
            return textDecoration;
        }
    }

    /// <summary>
    /// 範囲指定
    /// </summary>
    public class FormattedTextBox7 : BaseFormattedTextBox
    {
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
            textDecoration.Pen = new Pen(Brushes.DeepPink, 1);
            textDecoration.Location = TextDecorationLocation.Underline;
            textDecoration.PenOffset = 0;
            TextDecorationCollection decorations = new();
            decorations.Add(textDecoration);
            formattedText.SetTextDecorations(decorations, 3, 4);
            drawingContext.DrawText(formattedText, new Point());
        }
    }
}
