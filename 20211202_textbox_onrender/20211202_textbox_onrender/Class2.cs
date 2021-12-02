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
using System.Diagnostics;
using System.Windows.Controls.Primitives;

namespace _20211202_textbox_onrender
{
    class Class2 : Thumb
    {
        static string ROOT_NAME = "root";
        public Canvas RootCanvas;
        public Class2()
        {
            ControlTemplate template = new(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Canvas), ROOT_NAME);
            Template = template;
            ApplyTemplate();
            RootCanvas = (Canvas)template.FindName(ROOT_NAME, this);
            RootCanvas.Background = Brushes.Red;
            Class1 class1 = new();
            class1.Text = "class1";
            class1.BorderThickness = new Thickness(10);
            
            RootCanvas.Children.Add(class1);

            Border border = new();
            border.Background = Brushes.Transparent;
            border.Width = 50;border.Height = 20;
            RootCanvas.Children.Add(border);

            this.Focusable = true;
        }
    }
}
