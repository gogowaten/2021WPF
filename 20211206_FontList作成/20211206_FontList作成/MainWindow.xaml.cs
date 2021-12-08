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
using System.IO;

//SystemFontFamiliesを表示する part1 : Win32 & wpf メモ
//http://blog.livedoor.jp/oans/archives/54753149.html

namespace _20211206_FontList作成
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<FontData> FontDatas = new();

        public MainWindow()
        {
            InitializeComponent();

            //今のPCで使っている言語(日本語)取得
            var language =
                System.Windows.Markup.XmlLanguage.GetLanguage(
                CultureInfo.CurrentCulture.IetfLanguageTag);
            var culture = CultureInfo.CurrentCulture;
            CultureInfo cultureUS = new("en-US");





            List<Uri> uris = new();
            List<string> absUri = new();
            List<string> absPath = new();
            List<string> origin = new();
            List<string> fragment = new();

            List<string> uOrigin = new();//重複なしのUri
            List<string> uName = new();//重複なしのフォント名
            List<string> uName2 = new();//重複なしのフォント名
            List<FontFamily> fontFamilies1 = new();
            Dictionary<string, FontFamily> fontDict = new();

            //SystemFontFamiliesだけでは全てのフォントが取得できないので
            //GlyphTypefaceのWin32FamilyNames
            foreach (var item in Fonts.SystemFontFamilies)
            {
                var typefaces = item.GetTypefaces();
                foreach (var typeface in typefaces)
                {
                    typeface.TryGetGlyphTypeface(out GlyphTypeface gType);
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
                            FontData fontData1 = new();
                            fontData1.FontFamily = new(uri, fontName);
                            fontData1.FamilyName = fontName;
                            fontData1.Uri = uri;
                            FontDatas.Add(fontData1);
                            fontDict.Add(fontName, new(uri, fontName));
                        }
                    }
                }
            }
            SortedDictionary<string, FontFamily> sorted = new(fontDict);

            var neko2 = FontDatas;//376種類

            ////フォントファイル一覧からも追加していく、けど全て含まれていた
            //var allUris = GetFontUris();
            //List<FontFamily> ffl = new();
            //List<Uri> errors = new();
            ////フォントファイルからフォント名を取得するためにGlyphTypefaceを作成
            ////フォントによってはGlyphTypefaceを作成できないものがあったのでtryを使っている
            //foreach (var uri in allUris)
            //{
            //    try
            //    {
            //        GlyphTypeface typeface = new(uri);                    
            //        var fintName = typeface.FamilyNames[culture] ?? typeface.FamilyNames[cultureUS];

            //        ffl.Add(new FontFamily(uri, fintName));//414
            //    }
            //    catch (Exception ex)
            //    {
            //        errors.Add(uri);
            //    }
            //}

            //foreach (var item in ffl)
            //{
            //    var typefaces = item.GetTypefaces();
            //    foreach (var typeface in typefaces)
            //    {
            //        typeface.TryGetGlyphTypeface(out GlyphTypeface gType);
            //        if (gType != null)
            //        {
            //            var uri = gType.FontUri;
            //            string fontName = gType.Win32FamilyNames[culture] ?? gType.Win32FamilyNames[cultureUS];
            //            origin.Add(uri.OriginalString);
            //            if (uName.Contains(fontName) == false)
            //            {
            //                uName.Add(fontName);
            //                FontData fontData1 = new();
            //                fontData1.FontFamily = new(uri, fontName);
            //                fontData1.FamilyName = fontName;
            //                fontData1.Uri = uri;
            //                FontDatas.Add(fontData1);
            //            }
            //        }
            //    }
            //}


            neko2 = FontDatas;

            //foreach (var item in Fonts.SystemFontFamilies)
            //{
            //    var typefaces = item.GetTypefaces();
            //    foreach (var typeface in typefaces)
            //    {
            //        typeface.TryGetGlyphTypeface(out GlyphTypeface gType);
            //        if (gType != null)
            //        {
            //            //AbsoluteUriとOriginalStringは同じで末尾に#1とかがついている
            //            var uri = gType.FontUri;
            //            absUri.Add(uri.AbsoluteUri);//パスに#1とかが付く
            //            absPath.Add(uri.AbsolutePath);//パス
            //            origin.Add(uri.OriginalString);//パスに#1とかが付く
            //            fragment.Add(uri.Fragment);//#1とか#2など
            //            uris.Add(gType.FontUri);

            //            //重複なしで追加していく
            //            if (uOrigin.Contains(uri.OriginalString) == false)
            //            {
            //                uOrigin.Add(uri.OriginalString);//パス(fragment付き)

            //                //test
            //                var ext = System.IO.Path.GetExtension(uri.LocalPath);

            //                if(ext == ".ttc")
            //                {

            //                }
            //                else
            //                {

            //                }

            //                string name = gType.FamilyNames[culture];//日本語名
            //                if (name == null) { name = item.Source; }//なければ英語名
            //                string faceN;
            //                FontData fontData = new();
            //                //フェイス名、ボールドとかイタリックとか
            //                if (uName.Contains(name))
            //                {
            //                    faceN = gType.FaceNames[culture];
            //                    if (faceN == null)
            //                    {
            //                        faceN = gType.FaceNames[cultureUS];
            //                    }
            //                    name += " " + faceN;
            //                    uName.Add(name);
            //                    fontData.FamilyName = name;
            //                }
            //                else
            //                {
            //                    uName.Add(name);
            //                    fontData.FamilyName = name;
            //                }

            //                FontFamily fontFamily = new(uri, name);
            //                fontFamilies1.Add(fontFamily);
            //                fontData.FontFamily = fontFamily;
            //                fontData.Uri = uri;
            //                FontDatas.Add(fontData);

            //                continue;
            //            }
            //        }
            //    }

            //}




            //MyListBox.ItemsSource = FontDatas;
            MyListBox.ItemsSource = sorted;

            List<string> fontPath = origin.Distinct().ToList();
            List<Uri> uris2 = new();
            List<FontFamily> fontFamilies = new();
            List<Glyphs> gList = new();
            List<GlyphTypeface> gTypes = new();
            foreach (var item in fontPath)
            {
                Uri u = new(item);
                uris2.Add(new(item));
                fontFamilies.Add(new(u, ""));
                Glyphs glyphs = new();
                glyphs.FontUri = u;
                gList.Add(glyphs);
                GlyphTypeface glyphTypeface = new(u);
                gTypes.Add(glyphTypeface);
            }

            var neko = uris;




            //Uri uri = new("file:///C:/WINDOWS/FONTS/MEIRYO.TTC#2");
            //Uri uri = new(@"C:/WINDOWS/FONTS/MEIRYO.TTC#2");//これは後で使うときにエラーになる
            //Uri uri = new(@"file:///C:/WINDOWS/FONTS/MEIRYO.TTC#2");
            //Uri uri = new(@"C:\Windows\Fonts\meiryo.ttc");
            //Glyphs glyphs = new();
            //glyphs.FontUri = uri;
            //GlyphTypeface glyphTypeface = new(uri);
            //var fns = glyphTypeface.FamilyNames;
            //var facens = glyphTypeface.FaceNames;
            //MyTextBlock.FontFamily = new(uri, "Meiryo Ui");

            //以下は同じ効果
            //this.FontFamily = new(new Uri(@"C:/Windows/Fonts/Meiryo.ttc"), "Meiryo UI");            
            //this.FontFamily = new("Meiryo UI");

            //
            ////インストールされていないフォントは使えないみたい
            ////以下はエラーにはならないけど反映されず初期値のFontFamilyが使われる
            //Uri uri = new(@"I:\4D080K4\hdd0\Program Files\Pinnacle\Pinnacle Express\Fonts\Pretext_.ttf");
            //FontFamily fontFamily1 = new(uri, "PRETEXT");
            //MyTextBlock.FontFamily = fontFamily1;
            //



            ////今のPCで使っている言語(日本語)取得
            //var language =
            //    System.Windows.Markup.XmlLanguage.GetLanguage(
            //    CultureInfo.CurrentCulture.IetfLanguageTag);
            //var culture = CultureInfo.CurrentCulture;

            ////Fonts.SystemFontFamiliesの
            //foreach (var item in Fonts.SystemFontFamilies)
            //{
            //    var neko = item.FamilyTypefaces;
            //    var inu = item.GetTypefaces();

            //    ICollection<Typeface> types = item.GetTypefaces();
            //    foreach (var type in types)
            //    {
            //        type.TryGetGlyphTypeface(out GlyphTypeface gType);
            //        var uri = gType.FontUri;
            //        FontDatas.Add(new()
            //        {
            //            Uri = uri,
            //            LocalPath = uri.LocalPath,
            //            FamilyName = gType.FamilyNames[culture],
            //            TypeName = gType.FaceNames[culture],
            //            FontWeight = type.Weight,
            //            FontStyle = type.Style
            //        });

            //    }

            //}


            //#region インストールされているフォントのDictionary作成、Key：日本語名、Value：FontFamiry

            ////フォント全部
            //ICollection<FontFamily> sysFonts = Fonts.SystemFontFamilies;

            ////test、FontFamiryのリスト作成
            //var lang4 = sysFonts
            //    .Select(a => a.FamilyNames).ToArray();
            ////test、フォント名で日本語名があればそれを取得、なければnull
            ////FirstOrDefaultを使って日本語名取得、なければnullを返す
            //var lang5 = sysFonts
            //    .Select(a => a.FamilyNames.FirstOrDefault(b => b.Key == language).Value).ToArray();
            ////test、フォント名で日本語名があればそれを取得、なければ初期名取得
            //var lang6 = sysFonts
            //    .Select(a => a.FamilyNames.FirstOrDefault(b => b.Key == language).Value ?? a.Source).ToArray();
            ////Dictionary作成、Keyは日本語名、なければ初期名、ValueはFontFamiry
            //var lang7 = sysFonts
            //    .ToDictionary(a => a.FamilyNames.FirstOrDefault(b => b.Key == language).Value ?? a.Source);
            ////名前でソートしたいのでDortedDictionaryを使って作成してるけど、名前に重複があるとエラーになるから対策したほうがいい
            //SortedDictionary<string, FontFamily> keyValuePairs7 = new(lang7);
            ////↑と同じ結果
            //var lang8 = sysFonts
            //    .ToDictionary(a => a.FamilyNames.FirstOrDefault(b => b.Key == language).Value ?? a.Source, v => v);
            //SortedDictionary<string, FontFamily> keyValuePairs8 = new(lang8);

            //#endregion インストールされているフォントのDictionary作成、Key：日本語名、Value：FontFamiry


            //var tFaces1 = keyValuePairs7
            //    .Select(a => a.Value.GetTypefaces()).ToArray();
            //var tFaces2 = keyValuePairs7
            //    .Select(a => a.Value.GetTypefaces()
            //    .Select(a => a.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface2))).ToArray();
            //GlyphTypeface gtf = null;
            //Dictionary<string, Uri> myDict2 = new();
            //Dictionary<string, Typeface> myDict3 = new();
            //var cul = CultureInfo.CurrentCulture;
            //foreach (var item in keyValuePairs7.Select(a => a.Value.GetTypefaces()))
            //{
            //    item.FirstOrDefault(a => a.TryGetGlyphTypeface(out gtf));
            //    if (gtf != null)
            //    {
            //        string key = System.IO.Path.GetFileNameWithoutExtension(gtf.FontUri.LocalPath);
            //        var str = gtf.FaceNames.FirstOrDefault(a => a.Key == cul).Value;
            //        var sst = gtf.FaceNames.Select(a => a).FirstOrDefault(a => a.Key.Name == cul.Name);
            //        foreach (var names in gtf.FaceNames)
            //        {
            //            if (names.Key.Name == cul.Name)
            //            {
            //                var nekko = names.Value;
            //            }
            //        }
            //        if (myDict2.TryAdd(
            //            System.IO.Path.GetFileNameWithoutExtension(gtf.FontUri.LocalPath),
            //            gtf.FontUri) == false)
            //        {
            //            myDict3.Add(
            //                item.FirstOrDefault(a => a.FaceNames != null).FontFamily.Source,
            //                item.FirstOrDefault(a => a.FontFamily != null));
            //        };
            //        gtf = null;
            //    }
            //    else
            //    {
            //        myDict3.Add(
            //            item.FirstOrDefault(a => a.FaceNames != null).FontFamily.Source,
            //            item.FirstOrDefault(a => a.FontFamily != null));
            //    }
            //}

            //List<Uri> uris = new();
            //GlyphTypeface glyphTypeface1 = null;
            //Dictionary<string, Uri> myDict = new();
            //List<FontFamily> ffList = new();
            //foreach (var aaa in Fonts.SystemFontFamilies.Select(a => a.GetTypefaces()))
            //{
            //    _ = aaa.FirstOrDefault(a => a.TryGetGlyphTypeface(out glyphTypeface1));
            //    //_ = aaa.Select(a => a.TryGetGlyphTypeface(out glyphTypeface1));//空っぽ
            //    if (glyphTypeface1 != null)
            //    {
            //        myDict.TryAdd(System.IO.Path.GetFileNameWithoutExtension(glyphTypeface1.FontUri.LocalPath), glyphTypeface1.FontUri);
            //        uris.Add(glyphTypeface1.FontUri);
            //        var neko = aaa.Select(a => a.FontFamily).ToList();
            //        var inu = aaa.Select(a => a.FontFamily.FamilyNames.FirstOrDefault(b => b.Key == language).Value);
            //        var inu2 = aaa.Select(a => a.FontFamily.FamilyNames.FirstOrDefault(b => b.Key == language).Value).ToArray();
            //        var inu3 = aaa.Select(a => a.FontFamily.FamilyNames.FirstOrDefault(b => b.Key == language).Value).ToArray().FirstOrDefault(a => a != null);
            //        var inu4 = aaa.FirstOrDefault(a => a.FontFamily.FamilyNames != null);
            //        var inu5 = aaa.FirstOrDefault(a => a.FontFamily != null);
            //        glyphTypeface1 = null;
            //    }
            //}
            //var group = uris.GroupBy(a => System.IO.Path.GetFileNameWithoutExtension(a.LocalPath)).ToArray();
            ////
            //Dictionary<string, Uri> groupFirst = uris
            //    .GroupBy(a => System.IO.Path.GetFileNameWithoutExtension(a.LocalPath))
            //    .ToDictionary(a => a.Key, a => a.First());
            //SortedDictionary<string, Uri> fontDict = new(groupFirst);
            ////
            //Dictionary<string, Uri> groupFirst2 = uris
            //    .GroupBy(a => System.IO.Path.GetFileNameWithoutExtension(a.LocalPath))
            //    .ToDictionary(a => a.Key, a => a.First());



            //var f17 = uris[17];
            //var f18 = uris[18];
            //GlyphTypeface glyphTypeface = new(uris[17]);
            //string str = "aゆっくり";
            //double emSize = 50;
            //List<ushort> indices = new();
            //List<double> widths = new();
            //List<Point> points = new();
            //List<double> heights = new();
            //double totalH = 0;
            //double totalW = 0;
            //for (int i = 0; i < str.Length; i++)
            //{
            //    char key = str[i];
            //    //文字のインデックスが存在した場合のみ処理
            //    if (glyphTypeface.CharacterToGlyphMap.ContainsKey(key))
            //    {
            //        ushort index = glyphTypeface.CharacterToGlyphMap[key];
            //        indices.Add(index);
            //        double w = glyphTypeface.AdvanceWidths[index] * emSize;
            //        widths.Add(w);
            //        points.Add(new(30, -emSize));
            //        totalW += w;
            //        double h = glyphTypeface.AdvanceHeights[index];
            //        heights.Add(h);

            //    };

            //}
            //GlyphRun glyphRun = new(glyphTypeface,
            //    0,
            //    false,
            //    50,
            //    96,
            //    indices,
            //    new Point(),
            //    widths,
            //    points,
            //    null, null, null, null, language);

            //DrawingVisual dv = new();
            //using (var dc = dv.RenderOpen())
            //{
            //    dc.DrawGlyphRun(Brushes.MediumAquamarine, glyphRun);
            //}
            //RenderTargetBitmap bitmap = new(500, 200, 96, 96, PixelFormats.Pbgra32);
            //bitmap.Render(dv);
            //GlyphRunDrawing glyphRunDrawing = new(Brushes.MediumAquamarine, glyphRun);
            //DrawingBrush brush = new(glyphRunDrawing);
            ////MyGrid.Background = brush;

            //Glyphs glyphs = new();
            //glyphs.FontRenderingEmSize = 50;
            //glyphs.FontUri = uris[18];
            //glyphs.Fill = Brushes.MediumBlue;
            ////glyphs.Indices = "！！";
            //glyphs.OriginX = 50;
            //glyphs.OriginY = 50;
            //glyphs.StyleSimulations = StyleSimulations.BoldItalicSimulation;
            //glyphs.UnicodeString = "abc";
            //MyGrid.Children.Add(glyphs);
        }

        /// <summary>
        /// インストールされている全てのフォントファイルのUriリスト作成
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Uri> GetFontUris()
        {
            //systemのフォントフォルダから一覧
            IEnumerable<Uri> result = MakeList(
                Environment.GetFolderPath(Environment.SpecialFolder.Fonts));

            //ユーザーのフォントフォルダが存在する場合は、
            //それも取得してConcatで付け加える
            string userPath = System.IO.Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\Windows\Fonts");
            if (System.IO.Directory.Exists(userPath))
            {
                result = result.Concat(MakeList(userPath));
            }

            //指定フォルダのフォントファイルすべてをUriにして取得
            static IEnumerable<Uri> MakeList(string folderPath)
            {
                IEnumerable<Uri> result =
                    System.IO.Directory.GetFiles(folderPath, "*.ttf").Concat
                    (System.IO.Directory.GetFiles(folderPath, "*.ttc")).Concat
                    (System.IO.Directory.GetFiles(folderPath, "*.otf")).Select
                    (x => new Uri(x));
                return result;
            }
            return result;
        }

    }

    public class FontData
    {
        public string FamilyName { get; set; }
        public Uri Uri { get; set; }
        public string TypeName { get; set; }
        public string LocalPath { get; set; }

        public StyleSimulations StyleSimulations { get; set; }
        public FontWeight FontWeight { get; set; }
        public FontStyle FontStyle { get; set; }
        public FontFamily FontFamily { get; set; }
        public override string ToString()
        {
            string str = FamilyName + "_" + FontWeight + "_" + FontStyle;
            return str;
            //return base.ToString();
        }
    }
}
