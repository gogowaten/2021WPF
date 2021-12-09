using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Markup;



namespace _20211208_フォント一覧表示
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MyListBox1.ItemsSource = GetFontFamilies2();
            MyTextBlock1.Text = "Fonts.SystemFontFamilies\nフォント数：" + MyListBox1.Items.Count.ToString();
            //MyListBox1.ItemsSource = Fonts.SystemFontFamilies;

            MyListBox2.ItemsSource = GetFontFamilies();
            MyTextBlock2.Text = "Fonts.SystemFontFamilies＋ひと手間\nフォント数：" + MyListBox2.Items.Count.ToString();
        }

        /// <summary>
        /// SystemFontFamiliesから日本語フォント名で並べ替えたフォント一覧を返す、1ファイルに別名のフォントがある場合も取得
        /// </summary>
        /// <returns></returns>
        private SortedDictionary<string, FontFamily> GetFontFamilies()
        {
            //今のPCで使っている言語(日本語)のCulture取得
            //var language =
            //    System.Windows.Markup.XmlLanguage.GetLanguage(
            //    CultureInfo.CurrentCulture.IetfLanguageTag);
            CultureInfo culture = CultureInfo.CurrentCulture;//日本
            CultureInfo cultureUS = new("en-US");//英語？米国？

            List<string> uName = new();//フォント名の重複判定に使う
            Dictionary<string, FontFamily> tempDictionary = new();
            foreach (var item in Fonts.SystemFontFamilies)
            {
                var typefaces = item.GetTypefaces();
                foreach (var typeface in typefaces)
                {
                    _ = typeface.TryGetGlyphTypeface(out GlyphTypeface gType);
                    if (gType != null)
                    {
                        //フォント名取得はFamilyNamesではなく、Win32FamilyNamesを使う
                        //FamilyNamesだと違うフォントなのに同じフォント名で取得されるものがあるので
                        //Win32FamilyNamesを使う
                        //日本語名がなければ英語名
                        string fontName = gType.Win32FamilyNames[culture] ?? gType.Win32FamilyNames[cultureUS];
                        //string fontName = gType.FamilyNames[culture] ?? gType.FamilyNames[cultureUS];
                        
                        //フォント名で重複判定
                        var uri = gType.FontUri;
                        if (uName.Contains(fontName) == false)
                        {
                            uName.Add(fontName);
                            tempDictionary.Add(fontName, new(uri, fontName));
                        }
                    }
                }
            }
            SortedDictionary<string, FontFamily> fontDictionary = new(tempDictionary);
            return fontDictionary;
        }

        /// <summary>
        /// SystemFontFamiliesから日本語フォント名で並べ替えたフォント一覧を返す
        /// </summary>
        /// <returns></returns>
        private SortedDictionary<string, FontFamily> GetFontFamilies2()
        {
            //今のPCで使っている言語(日本語)取得
            XmlLanguage language =
                XmlLanguage.GetLanguage(
                CultureInfo.CurrentCulture.IetfLanguageTag);
            //英語のXmlLanguage取得
            XmlLanguage[] lang0 = FontFamily.FamilyNames.Select(a => a.Key).ToArray();
            
            List<string> uName = new();//フォント名の重複判定に使う
            Dictionary<string, FontFamily> tempDictionary = new();

            foreach (var item in Fonts.SystemFontFamilies)
            {
                //フォント名取得、日本語名がなければ英語名
                string name;
                if (item.FamilyNames.TryGetValue(language, out name) == false)
                {
                    name = item.FamilyNames[lang0[0]];//[0]は英語
                }
                //フォント名で重複判定
                if (uName.Contains(name) == false)
                {
                    uName.Add(name);
                    tempDictionary.Add(name, item);
                }
            }
            SortedDictionary<string, FontFamily> fontDictionary = new(tempDictionary);
            return fontDictionary;
        }

    }
}
