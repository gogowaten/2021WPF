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

//WPFで雑にウォーターマーク付きのテキストボックスを作る - プログラムの事とか
//https://puni-o.hatenablog.com/entry/2016/12/15/110619
//WPF でテンプレートを作成する -WPF.NET Framework | Microsoft Docs
//https://docs.microsoft.com/ja-jp/dotnet/desktop/wpf/controls/how-to-create-apply-template?view=netframeworkdesktop-4.8

//これは難しい、わからん
namespace _20211201_textboxUserControl
{
    /// <summary>
    /// UserControl2.xaml の相互作用ロジック
    /// </summary>
    public partial class UserControl2 : UserControl
    {
        public UserControl2()
        {
            InitializeComponent();
            DataContext = this;
        }


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(UserControl2), new PropertyMetadata("test"));


    }
}
