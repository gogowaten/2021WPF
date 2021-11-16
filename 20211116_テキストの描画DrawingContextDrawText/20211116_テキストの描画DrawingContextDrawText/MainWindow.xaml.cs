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

namespace _20211116_テキストの描画DrawingContextDrawText
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string text = "ゆっくりしていってね！！！";
            CultureInfo cultureInfo = CultureInfo.CurrentUICulture;
            FlowDirection flowDirection = FlowDirection.LeftToRight;
            Typeface typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
            FormattedText fText = new(text, cultureInfo, flowDirection, typeface, 50, new SolidColorBrush(), 96);
            //FormattedText fText = new(text, cultureInfo, flowDirection, new Typeface("Meiryo UI"), 20, Brushes.Red, 96);

            var textWidth = fText.GetMaxTextWidths();
           var geo = fText.BuildGeometry(new Point(0, 0));

            MyPath.Data = geo;
            MyPath.Fill = Brushes.Red;
            //MyPath.Stroke = Brushes.Black;
        }
    }
}
