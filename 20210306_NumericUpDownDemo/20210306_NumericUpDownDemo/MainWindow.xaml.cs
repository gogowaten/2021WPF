using System.Windows;
using System.Windows.Media;

//WPFにもNumericUpDownみたいなのをユーザーコントロールで、その8、ValueChangedイベント追加した - 午後わてんのブログ
//https://gogowaten.hatenablog.com/entry/2021/03/06/131111

namespace _20210306_NumericUpDownDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Nume_MyValueChanged(object sender, ControlLibraryCore20200620.MyValuechangedEventArgs e)
        {
            MyBorder.Background = new SolidColorBrush(
                Color.FromRgb((byte)NumeR.MyValue, (byte)NumeG.MyValue, (byte)NumeB.MyValue));
        }

    }
}
