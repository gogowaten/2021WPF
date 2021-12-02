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

namespace _20211202_textbox_onrender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MyClass2.DragDelta += MyClass2_DragDelta;
        }

        private void MyClass2_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Canvas.SetLeft(MyClass2, Canvas.GetLeft(MyClass2) + e.HorizontalChange);
            Canvas.SetTop(MyClass2, Canvas.GetTop(MyClass2) + e.VerticalChange);
        }
    }
}
