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

//WPF - Sample / FormattedTextBox / FormattedTextBox at master · gesource/WPF-Sample
//https://github.com/gesource/WPF-Sample/tree/master/FormattedTextBox/FormattedTextBox
//ここのを少し改変

namespace FormattedTextBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IReadOnlyCollection<object> FontStyles { get { return fontStyles; } }
        private IReadOnlyCollection<object> fontStyles = new[] {
            new {Name="Normal", Value=System.Windows.FontStyles.Normal},
            new {Name="Italic", Value=System.Windows.FontStyles.Italic},
            new {Name="Oblique", Value=System.Windows.FontStyles.Oblique},
        };
        public IReadOnlyCollection<object> FontWeights { get { return fontWeights; } }
        private IReadOnlyCollection<object> fontWeights = new[] {
            new {Name="Thin",Value=System.Windows.FontWeights.Thin},
            new {Name="ExtraLight",Value=System.Windows.FontWeights.ExtraLight},
            new {Name="Light",Value=System.Windows.FontWeights.Light},
            new {Name="Normal",Value=System.Windows.FontWeights.Normal},
            new {Name="Medium",Value=System.Windows.FontWeights.Medium},
            new {Name="DemiBold",Value=System.Windows.FontWeights.DemiBold},
            new {Name="Bold",Value=System.Windows.FontWeights.Bold},
            new {Name="ExtraBold",Value=System.Windows.FontWeights.ExtraBold},
            new {Name="Black",Value=System.Windows.FontWeights.Black},
            new {Name="ExtraBlack",Value=System.Windows.FontWeights.ExtraBlack},
        };
        public IReadOnlyCollection<object> FontStretches { get { return fontStretches; } }
        private IReadOnlyCollection<object> fontStretches = new[] {
            new {Name="UltraCondensed", Value=System.Windows.FontStretches.UltraCondensed},
            new {Name="ExtraCondensed", Value=System.Windows.FontStretches.ExtraCondensed},
            new {Name="Condensed", Value=System.Windows.FontStretches.Condensed},
            new {Name="SemiCondensed", Value=System.Windows.FontStretches.SemiCondensed},
            new {Name="Medium", Value=System.Windows.FontStretches.Medium},
            new {Name="SemiExpanded", Value=System.Windows.FontStretches.SemiExpanded},
            new {Name="Expanded", Value=System.Windows.FontStretches.Expanded},
            new {Name="ExtraExpanded", Value=System.Windows.FontStretches.ExtraExpanded},
            new {Name="UltraExpanded", Value=System.Windows.FontStretches.UltraExpanded},
        };
        //public IReadOnlyCollection<object> TextBoxColors { get { return textBoxColors; } }
        //private IReadOnlyCollection<object> textBoxColors = new[] {
        //    new { Name="Blue", Value=Colors.Blue},
        //    new { Name="Green", Value=Colors.Green},
        //    new { Name="Red", Value=Colors.Red},
        //    new { Name="Black", Value=Colors.Black},
        //    new { Name="White", Value=Colors.White},
        //};
        public Dictionary<string, Color> TextBoxColors { get { return textBoxColors; } }
        private Dictionary<string, Color> textBoxColors = new();
        


        public double TextBoxFontSize { get { return textBox.FontSize; } set { textBox.FontSize = value; } }
        public int TextBoxPenSize { get { return textBox.PenSize; } set { textBox.PenSize = value; } }
        public FontFamily FontFamilyValue { get { return textBox.FontFamily; } set { textBox.FontFamily = value; } }
        public FontStyle FontStyleValue { get { return textBox.FontStyle; } set { textBox.FontStyle = value; } }
        public FontWeight FontWeightValue { get { return textBox.FontWeight; } set { textBox.FontWeight = value; } }
        public FontStretch FontStretchValue { get { return textBox.FontStretch; } set { textBox.FontStretch = value; } }
        public Color BrushColor { get { return textBox.BrushColor; } set { textBox.BrushColor = value; } }
        public Color PenColor { get { return textBox.PenColor; } set { textBox.PenColor = value; } }
        private MyTextBox textBox = new MyTextBox()
        {
            Text = "Sample",
            FontSize = 84,
        };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            grid.Children.Add(textBox);


            //色の一覧を取得、コンボボックスが参照しているtextBoxColorsに入れる
            System.Reflection.PropertyInfo[]? infos = typeof(Colors).GetProperties(
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            for (int i = 0; i < infos.Length; i++)
            {
                System.Reflection.PropertyInfo info = infos[i];
                object? obj = info.GetValue(null);
                if (obj == null) { continue; }
                textBoxColors.Add(info.Name, (Color)obj);
            }

        }
    }
    public class MyTextBox : TextBox
    {
        /// <summary>
        /// 線の太さ
        /// </summary>
        public int PenSize { get { return penSize; } set { penSize = value; InvalidateVisual(); } }
        /// <summary>
        /// 文字の塗りの色
        /// </summary>
        public Color BrushColor { get { return brushColor; } set { brushColor = value; InvalidateVisual(); } }
        /// <summary>
        /// 文字の線の色
        /// </summary>
        public Color PenColor { get { return penColor; } set { penColor = value; InvalidateVisual(); } }

        private int penSize { get; set; } = 2;
        private Color brushColor { get; set; } = Colors.Green;
        private Color penColor { get; set; } = Colors.Red;

        public MyTextBox()
        {
            Foreground = new SolidColorBrush(Colors.Transparent);
            Background = new SolidColorBrush(Colors.Transparent);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (string.IsNullOrEmpty(Text.Trim()))
                return;

            FormattedText formattedText = new FormattedText(
                Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                new SolidColorBrush(),
                96
            );

            drawingContext.DrawGeometry(
              new SolidColorBrush(BrushColor),
              new Pen(new SolidColorBrush(PenColor), PenSize),
              formattedText.BuildGeometry(new Point(2, 0)));
        }
    }
}
