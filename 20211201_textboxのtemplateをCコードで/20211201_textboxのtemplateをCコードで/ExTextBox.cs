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
//ムリ
//xamlでできることがC#だとどう書けばいいのかわからん
//名前付きパーツってのを指定するのがわからんPART_ContentHost

namespace _20211201_textboxのtemplateをCコードで
{
    internal class ExTextBox : TextBox
    {
        static string TEMPLATE_NAME = "root";
        Grid rootGrid;

        public ExTextBox()
        {

            var s = Style;


            var pa = VisualTreeHelper.GetParent(this);
            //var c0 = VisualTreeHelper.GetChild(this, 0);
            //var c1 = VisualTreeHelper.GetChild(this, 1);
            var cc = VisualTreeHelper.GetChildrenCount(this);

            ControlTemplate template = new(typeof(TextBox));
            template.VisualTree = new FrameworkElementFactory(typeof(Grid), TEMPLATE_NAME);
            Template = template;
            ApplyTemplate();
            rootGrid = (Grid)template.FindName(TEMPLATE_NAME, this);

            ScrollViewer scrollViewer = new();
            scrollViewer.Name = "PART_ContentHost";
            _ = rootGrid.Children.Add(scrollViewer);


            //TextBlock textblock = new();
            //textblock.Text = "test";
            //textblock.Visibility = Visibility.Collapsed;
            //_ = rootGrid.Children.Add(textblock);

            //これはデザイン画面でエラーになる
            //FrameworkElementFactory vt = template.VisualTree;
            //vt.AppendChild(new FrameworkElementFactory(typeof(TextBox)));

            //StyleTypedPropertyAttribute styleTypedPropertyAttribute = new();
            //styleTypedPropertyAttribute.StyleTargetType = typeof(TextBox);
            //Setter setter = new(TemplateProperty, template);

            //Style style = new(typeof(TextBox));
            
            
        }
    }
}
