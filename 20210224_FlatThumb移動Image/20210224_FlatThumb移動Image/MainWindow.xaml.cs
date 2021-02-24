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

namespace _20210224_FlatThumb移動Image
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MyInitialize();

        }

        private void MyInitialize()
        {
            BitmapSource source = null;            
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream("_20210224_FlatThumb移動Image.collection_1.png"))
            {
                if (stream != null) source = BitmapFrame.Create(stream);
            }
            Image img = new() { Source = source };
            FlatThumbFromCanvasBase thumb = new(0, 0);
            thumb.TemplateBase.Children.Add(img);
            MyCanvas.Children.Add(thumb);


            using (var stream = assembly.GetManifestResourceStream("_20210224_FlatThumb移動Image.collection_2.png"))
            {
                if (stream != null) source = BitmapFrame.Create(stream);
            }
            img = new() { Source = source };
            FlatThumbFromGridBase thumb2 = new(10, 10);
            thumb2.TemplateBase.Children.Add(img);
            MyCanvas.Children.Add(thumb2);



            using (var stream = assembly.GetManifestResourceStream("_20210224_FlatThumb移動Image.collection_1.png"))
            {
                if (stream != null) source = BitmapFrame.Create(stream);
            }
            img = new() { Source = source };
            FlatThumbFromCanvasBase thumb3 = new(50, 0);
            thumb3.TemplateBase.Children.Add(img);
            thumb3.Name = "thumb3";

            using (var stream = assembly.GetManifestResourceStream("_20210224_FlatThumb移動Image.collection_2.png"))
            {
                if (stream != null) source = BitmapFrame.Create(stream);
            }
            img = new() { Source = source };
            FlatThumbFromCanvasBase thumb4 = new(100, 100);
            thumb4.TemplateBase.Children.Add(thumb3);
            thumb4.TemplateBase.Children.Add(img);
            thumb4.Name = "thumb4";
            MyCanvas.Children.Add(thumb4);

            
        }


    }
}
