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

//参照したところ
//【WPF】フォント一覧を取得してCOMBOBOXやLISTで選択するには？ | 趣味や仕事に役立つ初心者DIYプログラミング入門
//https://resanaplaza.com/2021/03/28/%E3%80%90wpf%E3%80%91%E3%83%95%E3%82%A9%E3%83%B3%E3%83%88%E4%B8%80%E8%A6%A7%E3%82%92%E5%8F%96%E5%BE%97%E3%81%97%E3%81%A6combobox%E3%82%84list%E3%81%A7%E9%81%B8%E6%8A%9E%EF%BC%81/

namespace _20211204_Font一覧リスト
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<FontData> fontDatas = new();
        public MainWindow()
        {
            InitializeComponent();


            //日本語のフォント名があればそれを選択
            string ietf = System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag;
            var language = System.Windows.Markup.XmlLanguage.GetLanguage(ietf);
            foreach (var item in Fonts.SystemFontFamilies)
            {
                FontData data = new() { FontFamily = item, Name = item.Source };

                foreach (var c in item.FamilyNames)
                {
                    if (c.Key == language)
                    {
                        data.Name = c.Value;
                        continue;
                    }
                }

                fontDatas.Add(data);
            }
            MyListBox.ItemsSource = fontDatas;
        }
    }

    public class FontData
    {
        public string Name { get; set; }
        public FontFamily FontFamily { get; set; }
    }
}
