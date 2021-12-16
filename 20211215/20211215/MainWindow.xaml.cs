using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace _20211215
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ThumbEx MyThunbEx;
        private ThumbEx MyThunbEx2;
        private MainData MainData = new();
        public MainWindow()
        {
            InitializeComponent();

            DataContext = MainData;

            Test1();
            Test2();

            MyThunbEx.DragDelta += MyThunbEx_DragDelta;
            MyThunbEx2.DragDelta += MyThunbEx_DragDelta;
            //DataContext = MyThunbEx.ThumbData;
        }

        private void MyThunbEx_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var t = sender as ThumbEx;
            Canvas.SetLeft(t, Canvas.GetLeft(t) + e.HorizontalChange);
            Canvas.SetTop(t, Canvas.GetTop(t) + e.VerticalChange);
        }


        private void Test1()
        {
            TextBlock textBlock = new();
            textBlock.Text = "TextBlock";
            textBlock.Foreground = Brushes.Gray;
            textBlock.Background = Brushes.Cyan;
            textBlock.FontSize = 50;

            MyThunbEx = new(textBlock);

            MyCanvas.Children.Add(MyThunbEx);
        }

        private void Test2()
        {
            TextBlock textBlock = new();
            textBlock.Text = "TextBlock1";
            ThumbEx t1 = new(textBlock);

            TextBlock textBlock2 = new();
            textBlock2.Text = "TextBlock2";
            ThumbEx t2 = new(textBlock2);
            Canvas.SetLeft(t2, 100);
            Canvas.SetTop(t2, 100);

            TextBox textBox = new();
            textBox.Text = "TextBox";
            ThumbEx t3 = new(textBox);
            Canvas.SetLeft(t3, 100);

            MyThunbEx2 = new(t1);
            MyThunbEx2.AddContent(t2);
            MyThunbEx2.AddContent(t3);

            MyCanvas.Children.Add(MyThunbEx2);
        }

        private void MyButtonTest_Click(object sender, RoutedEventArgs e)
        {
            //DataContext = MyThunbEx2;

            //UIElementCollection neko = MyThunbEx2.RootCanvas.Children;
            //Type type = neko[0].GetType();
            var data = MainData.Left;
            MainData.Left = 200;
            var left = MyThunbEx.Left;
            //MyThunbEx.Left = 100;
            //left = MyThunbEx.Left;
        }
    }

    public class MainData : System.ComponentModel.INotifyPropertyChanged
    {
        private double left;

        public double Left
        {
            get => left; set
            {
                if (left == value) { return; }
                left = value;
                RaisePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
